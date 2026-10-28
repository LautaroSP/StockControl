# History — backlog web

Fuente de verdad del estado. **No re-auditar el repo** para “qué está hecho”: mirar esta tabla.

Al **cerrar** una tarjeta: tildar criterios en `backlog/NN-*.md`, pasar a **sí** acá, y una línea en el log.

Estados: **sí** · **no** · **parcial** (hay código, no cierra la tarjeta).

## Siguiente

**#40 · 37 Tienda: carrito y pedido** (falta probarla en el navegador con la base). Pendientes de prueba: 36 Imágenes y 49 Categorías

## Tablero

| #orden | # | Tarjeta | Estado | Nota |
|---:|---:|---|---|---|
| 1 | 00 | [Stack y migración](00-stack.md) | sí | API + Angular + Postgres + import `.db` |
| 2 | 01 | [Roles y permisos](01-roles-y-permisos.md) | sí | Login, local, UI según rol. Admin hoy puede operar caja (v1 decía que no) |
| 3 | 23 | [Modo oscuro](23-modo-oscuro.md) | sí | Toggle + `localStorage` + `prefers-color-scheme`. No hay sitio público aún |
| 4 | 20 | [Configuración del local](20-configuracion-del-local.md) | sí | |
| 5 | 11 | [Productos](11-productos.md) | sí | |
| 6 | 13 | [Producto sector](13-producto-sector.md) | sí | |
| 7 | 14 | [Grupos de precio](14-grupos-de-precio.md) | sí | |
| 8 | 12 | [Stock](12-stock.md) | sí | |
| 9 | 06 | [Permiso: empleado modifica precios](06-permiso-empleado-precios.md) | sí | |
| 10 | 16 | [Medios de pago](16-medios-de-pago.md) | sí | |
| 11 | 24 | [Puestos de caja](24-puestos-de-caja.md) | sí | |
| 12 | 07 | [Venta en caja](07-venta-en-caja.md) | sí | Hueco: código inexistente no ofrece alta al dueño. QR = 21. Imprimir al cobrar = 18 |
| 13 | 08 | [Varios carritos](08-varios-carritos.md) | sí | 4 fijas + extras |
| 14 | 15 | [Filtros de catálogo](15-filtros-catalogo.md) | sí | |
| 15 | 05 | [Empleados](05-empleados.md) | sí | Incluye socio |
| 16 | 09 | [Cierre de caja](09-cierre-de-caja.md) | sí | Cerrar hoy / anterior en Informes |
| 17 | 10 | [Informes de ventas](10-informes-de-ventas.md) | sí | Ventas por puesto + detalle. Sin listado de cierres (eso es 25) |
| 18 | 25 | [Consulta de cajas cerradas](25-consulta-cajas-cerradas.md) | sí | Desglose + ventas del cierre |
| 19 | 18 | [Ticket](18-ticket.md) | sí | Diálogo de impresión del navegador. POS-58 / POS-80 / A4. Sin agente ESC/POS |
| 20 | 17 | [Estadísticas](17-estadisticas.md) | sí | Rendimiento mensual/rango, top productos, margen, grupos, comparativa y ventas del día |
| 21 | 19 | [Carteles](19-carteles.md) | sí | Impresión por navegador; genérica CSS; plantilla propia por local. Requiere migración `PlantillasCartel` |
| 22 | 26 | [Proveedores y órdenes](26-proveedores.md) | sí | ClosedXML en API; PDF por navegador; wa.me con 549 (celular AR). Requiere migración `Proveedores` |
| 23 | 27 | [Recibir orden](27-recibir-orden.md) | sí | Recepción única; cantidad recibida editable; requiere migración `RecepcionOrden` |
| 24 | 02 | [Pago por local](02-pago-por-local.md) | sí | Estado calculado por fecha; bloquea solo cobrar; aviso 5 días; $ 14.999. Resumen admin pasó a 03 |
| 25 | 22 | [Cobro del abono](22-cobro-abono-servicio.md) | sí | Checkout Pro + débito automático (suscripción MP) con webhooks verificados; renovación desde el vencimiento; 3 días de gracia con débito; registro con trial. Requiere migración `Abonos` y secretos MP en entorno |
| 26 | 03 | [Admin: alta de locales](03-admin-alta-locales.md) | sí | Listado, alta con dueño nuevo, vencimiento, canal de pago, teléfono de ventas, baja del local y total del mes |
| 27 | 04 | [Dueño: contratar otro local](04-dueno-contratar-local.md) | sí | Aunque el primero siga en prueba. El nuevo nace pendiente, sin trial, y no opera hasta el pago |
| 28 | 28 | [Landing](28-landing.md) | sí | Inicio público con el recorrido, precio, contacto de ventas y términos. Ruta lazy |
| 29 | 21 | [Pasarela pagos QR MP](21-pasarela-pagos-qr.md) | sí | Probada en el mostrador con QR. Point y el QR impreso quedan en el producto |
| 30 | 30 | [Mantenimiento y 404](30-mantenimiento-y-404.md) | sí | Página estática si Kestrel no responde. El 404 de una ruta lo muestra Angular. Sin test de dominio: no hay regla nueva |
| 31 | 32 | [Teclado de la caja](32-teclado-caja.md) | sí | Foco al lector al volver y tras 10 s. Enter y + como en el escritorio. Sin test de dominio: es foco, no hay regla nueva |
| 32 | 31 | [Iconos](31-iconos.md) | sí | Barra y Configuración. Mercado Pago: check verde y «Conectar otra cuenta» |
| 33 | 33 | [Guardar ticket sin vender](33-ticket-sin-venta.md) | sí | Pedido en espera del puesto de caja. No mueve stock ni entra en la caja. Cobrar lo borra en la misma transacción; la otra pestaña recibe aviso. Volver a guardar un pedido cargado lo reemplaza. Requiere migración `PedidosEspera` |
| 34 | 34 | [Configurar ofertas](34-ofertas.md) | sí | Pantalla Ofertas del dueño. Descuento de 1 a 99 % o llevás N y pagás M, con fechas en día de Argentina. Cada oferta elige si el descuento de caja o al costo la deshabilitan, y si la caja puede no aplicarla. El detalle de la venta guarda la oferta. Requiere migraciones `Ofertas` y `OpcionesOferta` |
| 35 | 35 | [Cuentas pendientes](35-cuentas-pendientes.md) | sí | Dejar a cuenta desde la caja y pantalla Cuentas. Cada pago es una venta de una línea `CUENTA-` con ganancia proporcional. Anular ese pago devuelve el saldo. Incobrable queda en la cuenta. Requiere migración `CuentasPendientes` |
| 36 | 29 | [Tienda: URL pública y catálogo](29-tienda-online.md) | sí | `/tienda/{alias}` pública, alias en Configuración, publicar desde Productos, mapa OSM con Nominatim. Pausada y abono vencido tienen su página. Pie legal (términos, privacidad, arrepentimiento) diferido hasta tener textos: sin eso no se abre al público real. Requiere migración `Tienda` |
| 37 | 50 | [Tienda: panel](50-tienda-panel.md) | sí | Grupo Tienda en la barra (dueño y socio): Tienda online (estado, link, publicados y con imagen), Configuración (sale de Configuración del local), Imágenes de productos. Pedidos lo suma la 41. Rutas `/mi-tienda/...` lazy con `tiendaGuard` |
| 38 | 36 | [Tienda: imágenes](36-tienda-imagenes.md) | parcial | Propuesta por código de barras (Open Food Facts) con Aceptar o Subir propia. Hasta 2 por producto, 400 × 400 px, WebP, SkiaSharp. Se prueba ya en Tienda › Imágenes de productos (50). Falta probarla en el navegador. Requiere migración `ImagenesTienda` y el volumen `Imagenes__Ruta` |
| 39 | 49 | [Categorías de productos](49-categorias.md) | parcial | Pantalla Categorías del dueño. Opcional en el producto y en el grupo; el producto hereda la del grupo y la propia domina. Borrar deja productos y grupos sin categoría. En la tienda, chips con «Todas»; se pueden marcar varias. Reemplaza el filtro por grupo. Sin subcategorías. Requiere migración `Categorias`. Falta probar en el navegador con la base |
| 40 | 37 | [Tienda: carrito y pedido](37-tienda-carrito-pedido.md) | parcial | Carrito en `localStorage`, el servidor cotiza. Retiro en el local, pago a coordinar. Precio congelado, stock reservado (la caja con stock rígido no lo vende), sin vencimiento. Link con código no adivinable (se guarda el hash); el comprador consulta y cancela. Requiere migración `PedidosTienda`. Falta probar en el navegador con la base |
| 41 | 41 | [Tienda: panel de pedidos y avisos](41-tienda-panel-pedidos.md) | no | Estados, entregado = venta, campanita, Web Push, wa.me |
| 42 | 38 | [Tienda: Mercado Pago y gestión](38-tienda-mercado-pago.md) | no | Checkout Pro con la cuenta MP de la 21. Gestión: importe fijo en Configuración |
| 43 | 39 | [Tienda: transferencia](39-tienda-transferencia.md) | no | A confirmar con plazo del dueño; gestión suma al abono |
| 44 | 51 | [Tienda: gestión fuera de MP](51-tienda-gestion-fuera-mp.md) | no | Monto en pesos (no %), el configurable de la 38, inicial $ 1.000, en transferencia o efectivo; casilla para cobrarlo al comprador (tooltip ?) o lo paga el dueño; suma al próximo abono |
| 45 | 40 | [Tienda: envío por km](40-tienda-envio-km.md) | no | Precio por km, mínimo, radio, gratis desde |
| 46 | 45 | [Vista de cliente (web)](45-vista-cliente.md) | sí | Botón arriba del carrito: pestaña aparte para el cliente, al instante por `BroadcastChannel`. Muestra QR de MP y «Gracias» al cobrar |
| 47 | 46 | [Vista de cliente (escritorio)](46-vista-cliente-escritorio.md) | sí | Ventana normal en el segundo monitor (recuerda cuál), atada a `CarritoSesion` vía `CalcularTotal`. Lo nuevo arriba. Sin test: es WinForms |
| 48 | 42 | [Sesión: cortar el token](42-sesion-revocar-token.md) | no | Versión de sesión en el usuario y en el JWT. Cambiar rol o clave corta los tokens viejos. Emisor y audiencia |
| 49 | 43 | [SQL injection](43-sql-injection.md) | no | Inventario de SQL crudo, regla contra `*Raw` con datos, escapar comodines de `ILike`, usuario de base sin privilegios |
| 50 | 44 | [Auditoría de seguridad](44-auditoria-seguridad.md) | no | Última antes de producción. Informe con gravedad; lo crítico se corrige ahí. Incluye RLS, validación de contratos, CSP, CORS, secretos y dependencias |
| 51 | 47 | [Mejorar los tickets](47-mejorar-tickets.md) | parcial | Ahorros guardados por línea y ticket armado en `ServicioTicket`; diseño de impresión nuevo. Falta probar la impresión en POS-58, POS-80 y A4. Requiere migración `AhorrosVenta` |
| 52 | 48 | [Legales](48-legales.md) | no | Términos del servicio, privacidad (Ley 25.326), arrepentimiento y baja (consumidor), términos de la tienda, aceptación con versión. Faltan los datos del titular. Antes de producción y de abrir la tienda al público |

## Log

- 2026-10-28 · 37 Tienda: carrito y pedido · parcial · `PedidoTienda` y `LineaPedidoTienda` (tablas `PedidosTienda` y `LineasPedidoTienda`, número correlativo por local, hash SHA-256 del código). `ServicioPedidosTienda`: cotizar (precio de lista del servidor, avisa cuánto queda con stock rígido, marca lo que ya no está publicado), crear (tienda abierta, nombre, teléfono AR, mail, de 1 a 99 por producto, hasta 50 productos), cancelar (solo Nuevo), reservado y disponible. La reserva no toca `Cantidad`: es la suma de los pedidos nuevos. `ServicioCobro` con stock rígido descuenta lo reservado; el catálogo muestra «Sin stock» si todo está reservado. Sin vencimiento. API pública: `POST /tienda/{alias}/carrito`, `POST /tienda/{alias}/pedidos` (honeypot, 10 cada 10 min por IP, lock por local para número y reserva), `GET` y `POST .../pedidos/{codigo}/cancelar`. Angular: Agregar y más/menos en la tienda, `/tienda/:alias/carrito` con checkout, `/tienda/:alias/pedido/:codigo` con Cancelar y WhatsApp al local. Requiere migración `PedidosTienda`. Falta probar en el navegador con la base
- 2026-10-28 · 50 Tienda: panel · sí · Probada por el usuario

- 2026-10-27 · 50 Tienda: panel · parcial · Grupo Tienda en la barra, después de Reportes, para dueño y socio. `/mi-tienda` (resumen nuevo: estado, link con Copiar y Ver mi tienda, publicados y con imagen, aviso sin alias), `/mi-tienda/configuracion` (la sección que estaba en Configuración del local, sin imágenes) y `/mi-tienda/imagenes`. `tiendaGuard` en la ruta. Configuración del local deja un enlace a Tienda. `GET /configuracion/tienda` suma `conImagen`. Íconos tienda e imágenes. Sin test de dominio: no hay regla nueva. Falta probar en el navegador

- 2026-10-28 · 49 Categorías de productos · parcial · `Categoria` (tabla `Categorias`, nombre hasta 40, único por local sin mayúsculas). `IdCategoria` opcional en `Producto` y en `GrupoProductos`, FK con `SET NULL`. `ServicioCategorias`: alta, renombrar, borrar (limpia productos y grupos), asignar sin cruzar locales, categoría efectiva (propia, si no la del grupo) y conteo. `ServicioCatalogoTienda` filtra por varias categorías efectivas y lista solo las que tienen publicados (`CategoriaTienda` reemplaza a `GrupoTienda`). API: `/categorias` (dueño y socio), `idCategoria` en productos y grupos (null no toca, 0 saca), tienda con `categorias=1,2`. Angular: `/categorias` lazy con `duenoGuard`, entrada junto a Grupos, selector en Grupos y en la ficha del producto («La del grupo (X)»), columna Categoría en Productos con «(grupo)», chips en la tienda. `.gitignore` ignoraba `Datos/` entero en Windows (sin distinguir mayúsculas): pasa a `datos/imagenes/`; aparecen sin versionar las migraciones `AhorrosVenta`, `Tienda` e `ImagenesTienda`. Requiere migración `Categorias`. Falta probar en el navegador con la base
- 2026-10-28 · 49 Categorías de productos · ajuste · El grupo de precio también puede tener categoría. El producto hereda la del grupo si no tiene propia; si tiene las dos, domina la del producto
- 2026-10-26 · 51 Tienda: gestión fuera de MP · no · alta de tarjeta (monto en pesos, no %, configurable como en la 38 y con $ 1.000 inicial, por pedido en transferencia o efectivo; casilla «Incluir gasto de gestión» con tooltip (?): tildada se le suma al comprador, sin tildar la paga el dueño; la deuda se suma al próximo abono)

- 2026-10-26 · 50 Tienda: panel · no · alta de tarjeta (grupo Tienda en la barra: Tienda online, Pedidos, Configuración, Imágenes de productos y después lo que venga; rutas `/mi-tienda/...`; va antes de cerrar la 36)
- 2026-10-26 · 49 Categorías de productos · ajuste · La tienda permite marcar varias categorías (salen los productos de cualquiera). No va a haber subcategorías
- 2026-10-26 · 49 Categorías de productos · no · alta de tarjeta (pantalla Categorías del dueño, opcional en el producto, filtro con seleccionables en la tienda que reemplaza al de grupo)
- 2026-10-26 · 36 Tienda: imágenes · ajuste · Subir propia achica la foto en el navegador (canvas, 400 × 400, JPEG 85 %) antes de mandarla: acepta originales de hasta 20 MB y HEIC donde el navegador lo abre. El servidor mantiene el tope de 2 MB y vuelve a validar
- 2026-10-26 · 36 Tienda: imágenes · parcial · `ServicioImagenesTienda` en el dominio (tope de 2, tipo real por bytes, 2 MB, 400 × 400 sin deformar ni agrandar, propuesta solo con código de barras, rechazadas, disponibilidad). `ClienteOpenFoodFacts` (foto frontal, solo el servidor de imágenes de OFF, sin redirecciones, tope de peso; si cae devuelve «sin respuesta»). `ProcesadorImagen` con SkiaSharp (MIT): valida, corrige el giro, achica y guarda en WebP. Archivos en un volumen (`Imagenes:Ruta`, fuera de git y de la base). Dueño: `/productos/{id}/imagenes` (ver, propuesta, aceptar, subir, quitar). Público: `/tienda/{alias}/imagenes/{id}` solo si el producto está publicado y la tienda abierta. Angular: apartado «Imágenes de la tienda» en Configuración; foto, miniaturas y atribución en la tienda. Requiere migración `ImagenesTienda`. Falta probarla en el navegador
- 2026-10-26 · 48 Legales · no · alta de tarjeta (términos del servicio y de la tienda, privacidad, arrepentimiento, baja del abono, aceptación con versión; toda la web)
- 2026-10-26 · 29 Tienda: URL pública y catálogo · sí · Probada por el usuario. Rutas de Angular pasan a carga bajo demanda (inicial de 617 kB a 322 kB). Productos: fecha y usuario de modificación en una columna y scroll propio de la tabla. Queda diferido el pie legal
- 2026-10-26 · 29 Tienda: URL pública y catálogo · parcial · `Local` suma alias, estado, dirección, horario, WhatsApp, envíos y ubicación; `Producto` suma `Publicado`. `ServicioTienda` y `ServicioCatalogoTienda` con tests. Público: `GET /tienda/{alias}` y `/productos` (mismo 404, tope 60/min por IP, sin costo ni cantidad). Dueño: `/configuracion/tienda`, búsqueda en Nominatim (1 por segundo) y `PUT /productos/publicados`. Angular: ruta lazy `tienda/:alias`, sección Tienda en Configuración, columna Tienda en Productos, Leaflet lazy. Requiere migración `Tienda`. Falta probar en el navegador
- 2026-10-26 · 29 Tienda: URL pública y catálogo · ajuste · Filtro por grupo de producto. Dirección, horario, WhatsApp y envíos se agregan al local. Términos, privacidad y arrepentimiento quedan para cuando haya textos legales
- 2026-10-26 · 29 Tienda: URL pública y catálogo · ajuste · Precio de lista (ofertas con la 37). Abono vencido o pendiente: «Tienda temporalmente cerrada», sin catálogo
- 2026-10-26 · 29 Tienda: URL pública y catálogo · ajuste · Pausada por el dueño: sin catálogo, «En este momento la tienda no se encuentra disponible, intentá en los horarios {Horario}»
- 2026-10-26 · 47 Mejorar los tickets · parcial · La línea guarda `PrecioBase` y los ahorros de oferta, descuento y combo (con su nombre). `ServicioTicket` arma el ticket: oferta debajo de la línea, combos y descuento al final; ventas viejas como estaban. `/ventas/{id}` y la cuenta devuelven `ticket`. Informes y Cajas usan `sc-detalle-venta`. Ticket nuevo: encabezado con caja, separadores, importes alineados, total grande, medios aparte, pie con «Ticket no válido como factura». Requiere migración `AhorrosVenta`. Falta probar la impresión
- 2026-10-26 · 47 Mejorar los tickets · ajuste · Suma el diseño de impresión: encabezado, separadores, importes encolumnados, total destacado, medios aparte, sin código de barras y al pie «Ticket no válido como factura»
- 2026-10-26 · 47 Mejorar los tickets · ajuste · Análisis: se guarda precio de lista y ahorro de combo en la línea (migración `PreciosListaVenta`); el ticket lo arma `ServicioTicket` en el dominio. Oferta debajo de su línea; combos y descuento al final. Al costo muestra el precio cobrado, sin línea de ahorro
- 2026-10-26 · 46 Vista de cliente (escritorio) · sí · Probada en pantalla por el usuario
- 2026-10-25 · 45 Vista de cliente (web) · ajuste · Con «Cobrar al costo» muestra precio y subtotal cobrados. Lo último agregado queda arriba de la lista
- 2026-10-25 · 46 Vista de cliente (escritorio) · ajuste · Fuente de las líneas un poco más chica. Lo último agregado queda arriba de la lista
- 2026-10-25 · 46 Vista de cliente (escritorio) · ajuste · Siempre ventana normal (mover, achicar, cerrar), centrada en el monitor guardado o en el secundario. Con «Cobrar al costo» muestra precio y subtotal cobrados
- 2026-10-25 · 46 Vista de cliente (escritorio) · parcial · `frmVistaCliente`: botón arriba del carrito en `StockMain`; se refresca desde `CalcularTotal`. Con dos monitores abre maximizada sin bordes en el guardado (`VistaClienteMonitor` en Configuración) o en el secundario; doble clic o Esc pasa a ventana. Con «Cobrar al costo» solo muestra el total. «Gracias» 5 s. Falta probarla en pantalla
- 2026-10-25 · 45 Vista de cliente (web) · sí · Botón en la caja que abre `/vista-cliente?caja=N`. La caja emite el carrito por `BroadcastChannel` (canal por local y puesto, latido 2 s; sin latido 8 s vuelve a espera). Con «Cobrar al costo» la vista muestra solo el total, sin precios. Sin test de dominio: no hay regla nueva
- 2026-10-24 · 47 Mejorar los tickets · no · alta de tarjeta (el combo reparte el descuento dentro de cada línea y el ticket no se puede verificar; pasa a línea de ahorro aparte)
- 2026-10-24 · 26 Proveedores y órdenes · ajuste · Cada proveedor tiene su lista de productos vinculados (varios proveedores por producto), con buscador, «Agregar a la orden» y «Editar pack». La orden pide por unidad o por pack/caja/bulto (nombre y unidades); el mensaje dice «6 Pack de Coca 1.75 lt» y recibir suma packs × unidades al stock. Mandar un producto no vinculado crea el vínculo. La orden ya no lleva código de barras. Requiere migración `PacksProveedor`
- 2026-10-24 · 34 Configurar ofertas · ajuste · Tipos nuevos: precio por unidad desde N o exactamente N, y oferta sin fecha de fin. Combos de varios productos por un precio total (pantalla Combos): se repiten por cada set completo, no aplican con descuento de caja, al costo ni «No aplicar ofertas», y solo si le conviene al cliente. Requiere migración `OfertasPrecioYCombos`
- 2026-10-24 · 35 Cuentas pendientes · ajuste · Al abrir una cuenta se ve lo que el cliente debe en total y sus otras cuentas abiertas
- 2026-10-23 · 46 Vista de cliente (escritorio) · no · alta de tarjeta (ventana aparte en WinForms, segundo monitor, se actualiza con el carrito)
- 2026-10-23 · 45 Vista de cliente (web) · no · alta de tarjeta (pestaña aparte para el cliente, al instante con `BroadcastChannel`, QR de MP y «Gracias»)

- 2026-10-23 · 44 Auditoría de seguridad · no · alta de tarjeta (última antes de producción; informe con gravedad; RLS, validación, cabeceras, CORS, secretos, dependencias)
- 2026-10-23 · 43 SQL injection · no · alta de tarjeta (SQL crudo sin datos del usuario, comodines de `ILike`, usuario de base sin privilegios)
- 2026-10-23 · 42 Sesión: cortar el token · no · alta de tarjeta (cambiar rol o clave invalida los JWT anteriores)

- 2026-10-23 · 35 Cuentas pendientes · sí · Dejar a cuenta con cliente del local; baja stock y no es venta. Cada pago, parcial o total, crea una venta de una línea `CUENTA-{id}` en la caja de ese momento, con medio sin Mercado Pago y ganancia proporcional. Anular esa venta devuelve el saldo. Incobrable queda guardado en la cuenta, sin indicador en Estadísticas. Estadísticas: deuda actual por cliente; lo fiado suma unidades en top y ventas del día sin plata. Configuración «El empleado puede fiar». Requiere migración `CuentasPendientes`

- 2026-10-23 · 34 Configurar ofertas · ajuste · Cada oferta elige si el descuento de la caja la deshabilita, si al costo la deshabilita (viene marcado) y si la caja muestra el tilde «No aplicar ofertas» (dueño y empleado). Requiere migración `OpcionesOferta`

- 2026-10-23 · 34 Configurar ofertas · sí · Pantalla Ofertas (dueño): alta, edición y baja, sin fechas que se pisen por producto. La caja muestra la oferta en la línea y el ticket la imprime. El stock baja lo llevado y la venta guarda lo cobrado. Requiere migración `Ofertas`

- 2026-10-23 · 33 Guardar ticket sin vender · sí · Botones Guardar en espera y Pedidos en espera debajo de COBRAR. Al cargarlo, el común toma el precio actual y avisa en la línea; el producto borrado no entra. Con Mercado Pago se borra al acreditarse. Tope de 50 por puesto. Requiere migración `PedidosEspera`

- 2026-10-22 · 29 Tienda: URL pública y catálogo · ajuste · La tienda muestra un mapa de OpenStreetMap con la ubicación del local. La 40 reutiliza esa ubicación

- 2026-10-22 · 36 Tienda: imágenes · ajuste · El apartado propone la imagen por código de barras (Open Food Facts) con aviso de origen. El dueño elige Aceptar o Subir propia

- 2026-10-22 · 29 Tienda online · ajuste · Se divide en 7 tarjetas (29, 36–41). URL pública única `/tienda/{alias}` por local, sin login

- 2026-10-22 · 33 Guardar ticket sin vender · ajuste · El pedido en espera es del puesto de caja que lo guardó. Botones debajo de COBRAR y Cancelar
- 2026-10-22 · 35 Cuentas pendientes · ajuste · Tabla aparte, no es venta hasta el pago. Pagos parciales. El empleado fía si el dueño lo habilita. Incobrable queda como pérdida. Total por cliente
- 2026-10-22 · 33 Guardar ticket sin vender · ajuste · Se llama pedido en espera. Al cargarlo, el producto común toma el precio actual y avisa si cambió. Cobro y borrado van juntos
- 2026-10-22 · 17 Estadísticas · ajuste · El genérico sin factor propio estima el margen con el factor de ganancia del local
- 2026-10-22 · 35 Cuentas pendientes · no · alta de tarjeta (el cliente paga después; baja el stock; la ganancia entra al cobrar; total por cliente en Estadísticas)
- 2026-10-22 · 34 Configurar ofertas · no · alta de tarjeta (descuento por producto con fecha, o 2x1 / 3x2; el carrito cobra solo)
- 2026-10-22 · 26 Proveedores y órdenes · ajuste · Botón Nueva orden en el listado y botón Agregar en cada producto
- 2026-10-22 · 26 Proveedores y órdenes · ajuste · La orden también acepta productos ya cargados en el local, no solo los del Excel del proveedor
- 2026-10-22 · 33 Guardar ticket sin vender · no · alta de tarjeta (guardar el carrito sin cobrar; recargarlo si el cliente vuelve)
- 2026-10-22 · 30 Mantenimiento y 404 · sí · Si Kestrel no responde, Caddy muestra la página estática. Una ruta que no existe la muestra Angular, con vuelta al inicio
- 2026-10-22 · 32 Teclado de la caja · sí · El lector recupera el foco al volver, a los 10 s y al terminar de cobrar. + pasa al medio y después a Cobrar, como en el escritorio
- 2026-10-22 · 22 Cobro del abono · ajuste · El registro puede confirmar el mail por enlace. Sin dominio ni clave de correo, la cuenta entra directo
- 2026-10-22 · 22 Cobro del abono · ajuste · El registro pide captcha (Turnstile). Sin claves reales, en local usa el widget de prueba
- 2026-10-22 · 28 Landing · ajuste · El formulario de contacto queda en el inicio. Cebo, tope por IP y apertura firmada
- 2026-10-22 · 21 Pasarela pagos QR MP · sí · Cerrada tras la prueba en el mostrador. La 29 queda al final, después del mantenimiento
- 2026-10-22 · 29 Tienda online · ajuste · Pasa al final del tablero
- 2026-10-21 · 32 Teclado de la caja · no · alta de tarjeta (foco al lector al volver y a los 10 s; ritmo Enter y + hasta cobrar)
- 2026-10-21 · 31 Iconos · sí · Íconos en la barra y en Configuración. Cuenta de Mercado Pago en verde con check; el botón pasa a «Conectar otra cuenta»
- 2026-10-21 · 31 Iconos · no · alta de tarjeta (barra, Configuración, marca de Mercado Pago, cuenta conectada con check)
- 2026-10-20 · 21 Pasarela pagos QR MP · parcial · Cuenta del local cifrada, cobro que espera el pago y QR o Point en la caja. Falta probarlo con Mercado Pago
- 2026-10-20 · 29 Tienda online · ajuste · La gestión deja de ser 5 %: importe fijo por pedido, en Configuración del local
- 2026-10-20 · 29 Tienda online · ajuste · Cobra con la cuenta de Mercado Pago que el local conecta en la 21. El cartel del mostrador no recibe pedidos web
- 2026-10-20 · 21 Pasarela pagos QR MP · ajuste · El QR impreso del mostrador también cobra: la caja le carga el importe a ese cartel
- 2026-10-20 · 21 Pasarela pagos QR MP · ajuste · Pasa adelante de la tienda. El cobro de mostrador es QR en pantalla o Point del local
- 2026-10-20 · 30 Mantenimiento y 404 · no · alta de tarjeta (página estática si la API no responde; 404 de ruta en Angular)
- 2026-10-20 · 28 Landing · ajuste · El formulario de contacto se guarda; el admin lo marca leído y puede apagarlo
- 2026-10-20 · 28 Landing · ajuste · El contacto muestra un mail genérico, editable desde el admin
- 2026-10-20 · 05 Empleados · ajuste · Dueño y socio ven todos los usuarios de la cuenta, no solo los del local abierto
- 2026-10-20 · 03 Admin alta de locales · ajuste · Al crear otro local se pueden copiar los productos; el stock se repite o queda en 0
- 2026-10-20 · 03 Admin alta de locales · ajuste · El trial es uno por dueño; si ya tiene un local, el siguiente nace pendiente
- 2026-10-20 · 28 Landing · sí · Inicio público: recorrido de la app, precio, contacto de ventas y términos
- 2026-10-20 · 04 Dueño: contratar otro local · sí · El dueño pide otra sucursal sin trial; paga $ 14.999 o habla con ventas; no opera hasta que se acredite
- 2026-10-20 · 03 Admin alta de locales · sí · Listado de locales, dueño en el alta, vencimiento, canal de pago, teléfono de ventas, baja y total del mes
- 2026-10-20 · 29 Tienda online · no · alta de tarjeta (tienda pública, pedidos, 5 % de gestión, MP del local, transferencia, envío por km)
- 2026-10-20 · 03 Admin alta de locales · parcial · Pago, anulación, bloqueo y baja piden la clave maestra
- 2026-10-20 · 03 Admin alta de locales · parcial · Cambiar la clave pide la clave maestra
- 2026-10-20 · 03 Admin alta de locales · parcial · El admin cambia la clave de dueños, admins, empleados y socios
- 2026-10-20 · 03 Admin alta de locales · parcial · Anular el pago de admin y volver al vencimiento anterior
- 2026-10-20 · 03 Admin alta de locales · parcial · Marcar el abono de un local como pago (un mes, sin Mercado Pago)
- 2026-10-20 · 03 Admin alta de locales · parcial · Panel de dueños, alta de local, admin con clave secreta, bloqueo y baja
- 2026-10-20 · 28 Landing · no · alta de tarjeta; la maqueta queda en prototipo-web
- 2026-10-15 · 22 Cobro del abono · sí · Registro público (prueba 7 días o pagar); pago único Checkout Pro con QR; débito automático opcional con 3 días de gracia; webhook firmado
- 2026-10-10 · 02 Pago por local · sí · $ 14.999/mes por local; vence mismo día del mes siguiente; aviso 5 días antes; vencido no cobra
- 2026-09-28 · 27 Recibir orden · sí · Marcar recibida desde la ficha del proveedor; suma stock de productos comunes; guarda pedido vs recibido, quién y cuándo
- 2026-09-28 · 26 Proveedores · sí · ABM + catálogo Excel (revisar/confirmar, suma stock) + órdenes por WhatsApp/Excel/PDF con Repetir
- 2026-09-28 · 19 Carteles · sí · Grande 6 / chico 30 por A4 vía diálogo de impresión; plantilla genérica CSS o imagen propia por local
- 2026-09-12 · 17 Estadísticas · sí · Métricas de ventas, top productos, mix de pagos, margen, grupos, sucursales y ventas del día
- 2026-09-10 · 18 Ticket · sí · Impresión por diálogo del navegador; formatos POS 58/80 y A4
- 2026-09-10 · 08 Varios carritos · sí · 4 pestañas fijas F1–F4, extras con +/×
- 2026-09-10 · 10 / 25 · ajuste · Informes = ventas + cerrar caja; Cajas = historial de cierres y detalle de ventas
- 2026-09-10 · 26 Proveedores · no · alta de tarjeta (órdenes de compra, Excel, WhatsApp wa.me)
- 2026-09-10 · 27 Recibir orden · no · alta de tarjeta (marcar recibida + sumar stock)
