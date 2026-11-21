# Plan 68 · Caja sin conexión

Tarjeta: `backlog/68-caja-sin-conexion.md` · History `#orden` 69 · Depende de 07 y 08 (ambas **sí**).

## Resumen

Si se corta internet, la caja sigue cobrando con el catálogo y los precios que ya tenía guardados en el navegador. Cada venta offline queda en IndexedDB con un id propio (Guid generado en el cliente) y se manda sola al volver la conexión. El servidor no duplica una venta cuyo id ya recibió. Si el stock rígido no alcanza al sincronizar, la venta entra igual y queda marcada para revisar. El cierre de caja se bloquea hasta que no quede nada por mandar.

Hoy `cobrar()` en `caja.component.ts` llama a `api.cobrar` → `POST /ventas` (`ServicioCobro.Cobrar` + `InformeVenta` en una transacción). Todo el cálculo (ofertas, combos, descuento sobre margen, descuento de medio) ya vive en el front y se repite en el servidor. Eso permite cobrar offline con lo cacheado y que el servidor recalcule al sincronizar.

## Decisiones

| Tema | Decisión |
|---|---|
| Medios offline | Solo **efectivo y medios manuales** (`integracion` vacía o `manual`). Se ocultan QR, Point y Mercado Pago. |
| No hay offline | Mercado Pago, «Dejar a cuenta» (cuentas), pedidos en espera nuevos, ofertas/combos nuevos (se usan los cacheados tal cual estaban). |
| Cierre | **Bloqueado** hasta sincronizar todo (también las ventas en conflicto ya sincronizadas no bloquean; solo las pendientes). |
| Almacenamiento | **IndexedDB** (no `localStorage`): catálogo, medios, ofertas, combos y cola de ventas. |
| Anti-duplicado | `IdVentaCliente` (Guid) generado al cobrar. Índice único `(IdLocal, IdVentaCliente)` en `InformeVenta`. Reenviar devuelve la venta ya creada (200), no un error. |
| Stock en conflicto | Al sincronizar se ignora el tope de stock rígido; la venta entra, descuenta stock (puede quedar negativo) y se marca `StockEnConflicto = true`. |
| Fecha de la venta | La del cobro en el cliente (`FechaCobro`), acotada: no futura y no más de 7 días atrás; si no, se usa la del servidor. |
| Precios | Al sincronizar el servidor **recalcula** con `ServicioCobro`. Si el total difiere del que vio el cajero, se guarda el del servidor y se anota en la marca de revisión. |
| Online normal | Sin cambios: si hay conexión, `POST /ventas` directo, con el mismo `IdVentaCliente` (reintentos por timeout también quedan seguros). |

## Arquitectura de cache

Servicio nuevo `CacheCajaService` (un trabajo: leer/escribir IndexedDB). Base `stockcontrol-caja`, una por usuario+local (`IdLocal` en el nombre) para no mezclar locales en la misma PC.

Almacenes (object stores):

- `productos` — catálogo completo del local (no la página de 50): id, código, nombre, precio, costo, cantidad, `productoSector`, grupo.
- `mediosPago` — solo `activo` y manuales.
- `ofertas` y `combos` — vigentes del día de la última carga.
- `ventasPendientes` — clave `idVentaCliente`; guarda `VentaPedido` + `fechaCobro` + `intentos` + `ultimoError`.
- `meta` — `ultimaCarga`, `umbralStockBajo`, `nroCaja`, `idLocal`.

Cuándo se llena:

1. Al abrir caja con conexión y en `alVolver()`: descargar catálogo completo (listado paginado por tramos, respetando el tope de página de la API) y refrescar ofertas/combos.
2. Si la descarga falla por red, la caja sigue con lo guardado. Si no hay cache (primera vez sin red), se muestra «Necesitás conexión la primera vez».

Detección de conexión: `navigator.onLine` + eventos `online`/`offline` + fallo real de red (status 0) en `cobrar()`. Un `ConexionService` expone `senal` (`enLinea`). No confiar solo en `onLine`.

Modo offline en `caja.component.ts`:

- `cargar()` / `escanear()` leen de `CacheCajaService` cuando `!enLinea`.
- `cobrar()`: si offline o status 0 → `encolarVenta()` en vez de `api.cobrar`. Se oculta/deshabilita lo no permitido (MP, fiar, espera, balanza si depende de red).
- Stock cacheado: se descuenta localmente de la copia cacheada para no vender de más en el mismo puesto (informativo; el servidor decide al sincronizar).
- El ticket offline se arma local con los datos de la venta y marca «Pendiente de sincronizar» (reutilizar `sc-ticket` si alcanza; si necesita `TicketDto` sin id de venta, mostrar el id corto del Guid).

## API sync

Sin endpoint nuevo si se puede: se extiende `POST /ventas`. Un solo lugar de verdad para cobrar.

**`VentaPedido`** suma (todos opcionales, compatibles con la caja actual):

- `IdVentaCliente` (Guid?) — requerido para offline; recomendado siempre.
- `FechaCobro` (DateTimeOffset?).
- `DesdeSinConexion` (bool) — activa «aceptar con conflicto de stock».

**`InformeVenta`** suma:

- `IdVentaCliente` (Guid?, índice único filtrado por `IdLocal`).
- `StockEnConflicto` (bool) y `NotaRevision` (string?, máx. 200).
- Migración EF + política RLS ya existente por `IdLocal` (sin cambios de política).

**Flujo en `POST /ventas`:**

1. Si llega `IdVentaCliente` y ya existe en el local → `200 OK` con `{ IdInformeVenta, Total, yaRecibida: true }` (idempotente). Sin tocar stock.
2. Si no existe: `ServicioCobro.Cobrar` con `StockRigido = rigido && !DesdeSinConexion`. Se agrega al resultado un `StockEnConflicto` calculado: «el rígido habría rechazado esta venta».
3. Guardar con `IdVentaCliente`, `FechaCobro` validada, `StockEnConflicto`, `NotaRevision`.
4. Carrera (dos pestañas mandan el mismo id): capturar `DbUpdateException` por índice único y devolver la venta existente.
5. `DesdeSinConexion` rechaza medios con integración MP (`400`). El abono vencido sigue dando `402`: la venta queda en cola con ese error y se informa al dueño.
6. Validación: `IdVentaCliente` no vacío, items con tope, importes ≥ 0 (DataAnnotations existentes).

Errores en la cola:

- `400` de negocio (producto borrado, medio inactivo): la venta pasa a estado **rechazada**, no se reintenta sola, se muestra en el aviso con botón «Revisar».
- `5xx`/red: reintento con espera creciente (5 s, 15 s, 60 s, tope 5 min).
- `401`: pausar la cola hasta que vuelva a iniciar sesión (las ventas quedan guardadas).

**Cierre de caja:** el bloqueo es en el cliente (la cola vive allí). Como defensa, el endpoint de cierre del día (`/cajas/cierre`, usado por `ServicioCierre.Cerrar`) no cambia, pero la pantalla de cierre lee la cola de **todas las pestañas del mismo navegador** (IndexedDB compartida) y deshabilita «Cerrar caja» si hay pendientes o rechazadas. Mensaje: «Hay N ventas sin mandar. Conectá y esperá a que se sincronicen.»

**Listado/venta:** `GET /ventas` y `GET /ventas/{id}` devuelven `stockEnConflicto` y `notaRevision`. Filtro `?soloRevisar=true` en el listado para el dueño. La revisión se da por resuelta con `PUT /ventas/{id}/revisada` (dueño y socio; el empleado no).

## UX banner

- Banner fijo arriba de la caja (debajo de la topbar), colores solo por variables CSS (modo oscuro).
- **Sin conexión:** «Sin conexión · N ventas por mandar» (N = pendientes). Singular: «1 venta por mandar».
- **Sincronizando:** «Mandando N ventas…».
- **Con rechazadas:** «N ventas para revisar» + botón Revisar (lista con motivo; reintentar o descartar con confirmación del dueño).
- **Todo mandado:** el banner desaparece; toast «Ventas sincronizadas».
- Cobro offline: toast «Cobrado $ X (sin conexión)».
- Medios no disponibles: el selector de medios solo ofrece manuales; «Dejar a cuenta», cobro MP y «Pedidos en espera» aparecen deshabilitados con título «Sin conexión».
- Pantalla de cierre: botón deshabilitado + el mismo texto de pendientes.
- Informes/lista de ventas: etiqueta «Revisar stock» en las ventas con `StockEnConflicto`.
- Vista de cliente (`vista-cliente`): sin cambios.

## Archivos

Back (`src/StockControl.Api/` y dominio):

- `Program.cs` — `POST /ventas`: idempotencia, `DesdeSinConexion`, `FechaCobro`; `GET /ventas` con `soloRevisar`; `PUT /ventas/{id}/revisada`.
- `VentaPedido` (donde esté declarado) — campos nuevos.
- `InformeVenta` — `IdVentaCliente`, `StockEnConflicto`, `NotaRevision`.
- `ServicioCobro` / `PedidoCobro` — `StockRigido` ya existe; sumar `StockEnConflicto` al resultado.
- Migración EF (`AgregarVentaSinConexion`).
- `Permisos` — `PuedeRevisarVentas` (dueño y socio).

Front (`src/StockControl.Web/src/app/`):

- `servicios/cache-caja.service.ts` — IndexedDB.
- `servicios/conexion.service.ts` — señal `enLinea`.
- `servicios/cola-ventas.service.ts` — encolar, reintentar, estados, bloqueo de cierre.
- `servicios/api.service.ts` — `VentaPedido` con campos nuevos; `cobrar` manda `IdVentaCliente`.
- `paginas/caja.component.ts` — `cobrar()` con camino offline, lectura del cache.
- `paginas/aviso-conexion.component.ts` — banner (un archivo, un trabajo).
- Pantalla de cierre de caja — bloquear botón.
- Lista de ventas — etiqueta y filtro «para revisar».

Docs:

- `Manual-de-usuario.md` — sección «Si se corta internet».
- `backlog/68-caja-sin-conexion.md` y `backlog/History.md`.

No se toca WinForms ni `prototipo-web/`.

## Pasos

1. **Dominio:** `StockEnConflicto` en `ServicioCobro` (rígido deja pasar y marca cuando `DesdeSinConexion`).
2. **Modelo + migración:** columnas e índice único `(IdLocal, IdVentaCliente)`.
3. **API:** idempotencia, fecha acotada, rechazo de MP offline, `GET` con marca y filtro, `PUT revisada`.
4. **Tests xUnit** del dominio (ver sección Tests) en el mismo cambio.
5. **Front base:** `ConexionService` y `CacheCajaService` (sin UI).
6. **Cola:** `ColaVentasService` con reintentos y estados; enchufarla al evento `online`.
7. **Caja:** carga desde cache, `cobrar()` offline, restricciones de medios, ticket offline.
8. **Banner** y bloqueo del botón de cierre.
9. **Revisión:** etiqueta, filtro y acción «revisada» en la lista de ventas.
10. **Manual** + prueba a mano en navegador (DevTools → Offline): cobrar 3 ventas, volver, verificar sin duplicados, cerrar caja.
11. **Cerrar tarjeta** (ver sección History).

## Tests

xUnit sobre el modelo/casos de uso, nombres en español, un comportamiento por test:

- `Cobrar_sin_conexion_con_stock_rigido_insuficiente_entra_y_marca_conflicto`
- `Cobrar_sin_conexion_con_stock_suficiente_no_marca_conflicto`
- `Cobrar_con_stock_no_rigido_no_marca_conflicto`
- `Cobrar_con_el_mismo_id_de_venta_no_duplica_la_venta`
- `Cobrar_con_el_mismo_id_de_venta_no_descuenta_stock_dos_veces`
- `Cobrar_con_el_mismo_id_en_otro_local_crea_otra_venta`
- `Cobrar_sin_conexion_rechaza_medio_de_mercado_pago`
- `Cobrar_sin_conexion_acepta_efectivo_y_medio_manual`
- `Cobrar_sin_conexion_recalcula_el_total_con_el_precio_del_servidor`
- `Cobrar_sin_conexion_con_fecha_futura_usa_la_del_servidor`
- `Cobrar_sin_conexion_con_fecha_de_mas_de_7_dias_usa_la_del_servidor`
- `Cobrar_sin_conexion_con_descuento_sobre_margen_aplica_igual_que_online`
- `Cobrar_sin_conexion_producto_sector_y_generico_entran`
- `Revisar_venta_solo_dueno_y_socio_no_empleado`
- `Venta_revisada_sale_del_listado_para_revisar`

El cierre bloqueado es lógica del front: se cubre con el test de la cola si el repo ya tiene runner de front; si no, queda en la prueba manual del paso 10. El servidor no cambia el cierre.

## Criterios

Los de la tarjeta, en orden:

- [ ] Catálogo y precios guardados en el navegador (IndexedDB).
- [ ] Cobrar en efectivo y medios manuales sin conexión.
- [ ] Sincronizar al volver, sin duplicar (id de la venta).
- [ ] Aviso «Sin conexión · N ventas por mandar».
- [ ] Cierre de caja bloqueado hasta sincronizar.
- [ ] Ventas con stock en conflicto marcadas para revisar.
- [ ] Tests xUnit.

Verificación extra: reenviar la misma venta dos veces no cambia stock ni totales; cerrar la pestaña con ventas en cola y volver las sigue mandando; el empleado no puede marcar revisada.

## Fuera de alcance

- Mercado Pago (QR, Point), «Dejar a cuenta», pedidos en espera nuevos, ofertas/combos creados mientras no hay red.
- Alta o edición de productos, stock, clientes offline.
- Sincronizar entre PCs distintas: la cola es por navegador.
- Service worker / PWA instalable (la app necesita haber cargado antes con red).
- Cierre de caja offline.
- Factura ARCA offline (tarjeta 67).
- Resolver automáticamente conflictos de precio o stock: solo se marcan.
- Cifrado de la cola local (las ventas no se cifran campo a campo, según regla de seguridad).

## History

Al cerrar, en el mismo cambio:

1. Tildar criterios en `backlog/68-caja-sin-conexion.md`.
2. En `backlog/History.md`: fila `#orden 69` (tarjeta 68) → **sí**, sin huecos; actualizar **Siguiente** (hoy apunta a `#55 · 52 Actualizar Angular`; revisar si el próximo por orden es la 70); y sumar una línea al **Log**:
   `2026-11-DD · 68 Caja sin conexión · sí · IndexedDB, cola con id anti-duplicado, stock en conflicto marcado, cierre bloqueado hasta sincronizar. Tests xUnit.`
3. Si queda algo sin probar en navegador, marcar **parcial** y anotarlo en **Siguiente**, como las demás.

Secretos: nada de claves ni cadenas de conexión en el cambio; la cola local no guarda tokens de MP.
