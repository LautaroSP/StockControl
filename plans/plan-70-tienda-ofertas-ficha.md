# Plan 70 · Tienda: ofertas, dos columnas y ficha

Tarjeta: `backlog/70-tienda-ofertas-ficha.md`. Depende de 29 Tienda, 34 Ofertas, 37 Carrito y pedido. Solo plan: no hay código en este cambio.

## Resumen

Hoy la tienda cotiza con `producto.Precio` (`ServicioPedidosTienda.Cotizar` → `LineaCotizada`) y no conoce las ofertas. Las ofertas viven en `Oferta` / `ServicioOfertas` (descuento, llevaPaga, precioDesde, precioExacto) y se aplican en caja con `PedidoCobro`. `tienda.component.ts` lista productos en una columna, sin ficha ni orden.

Esta tarjeta hace seis cosas:

1. Campo `Oferta.EnTienda` («también en la tienda»). La tienda aplica solo esas ofertas.
2. Cotización, carrito y pedido de la tienda guardan precio de lista, oferta y ahorro.
3. `Producto.Descripcion` opcional y ficha del producto al tocarlo.
4. Dos columnas en el celular y orden por nombre o precio.
5. Vista previa del link (Open Graph) servida por la API.
6. Aviso al dueño en Tienda online cuando la tienda está cerrada por abono vencido.

## Decisiones

| Tema | Decisión |
|---|---|
| Ofertas en tienda | Solo corren si `EnTienda = true`. Por defecto `false`: las ofertas existentes no cambian. |
| Opciones de caja | `PermiteNoAplicarEnCaja`, `AnulaConDescuentoCaja` y `AnulaConAlCosto` no aplican en tienda (no hay caja, descuento ni al costo). |
| Quién aplica | Función nueva en `ServicioOfertas` (`AplicableEnTienda`) que filtra `EnTienda` y usa `Vigente`. No se toca `Aplicable` de caja. |
| Precio | Siempre por `PrecioUnitarioConOferta` y `CantidadCobrada` (reglas ya probadas). El servidor recalcula; el cliente nunca manda precios. |
| Línea cotizada | Suma precio de lista, descripción corta de la oferta (`ServicioOfertas.Descripcion`) y ahorro. `SubTotal` es lo que se cobra. |
| Pedido | `LineaPedidoTienda` guarda precio de lista, oferta y ahorro al confirmar. Un cambio posterior de la oferta no altera pedidos hechos. |
| Descripción | `Producto.Descripcion`, opcional, máximo 500 caracteres, texto plano (se escapa al mostrar). Se edita en el formulario de producto del panel. |
| Productos sin oferta | Sector y genéricos no tienen oferta (ya validado). No aparecen con precio tachado. |
| Móvil | Grilla de 2 columnas en anchos chicos; el CSS sale del prototipo / `estilos.css`, con variables de tema (oscuro incluido). |
| Ficha | Panel o ruta `/tienda/:alias/producto/:id`. Ruta propia para poder compartir. El plan asume ruta con lazy load. |
| Orden | `orden=nombre|precio` y `sentido=asc|desc` en el listado. Lista blanca en el servidor. Para precio, se ordena por el precio **con oferta** vigente. |
| Paginación | Se mantiene el tope de página. El orden se aplica antes de paginar. |
| Open Graph | Endpoint de la API que devuelve HTML mínimo con `og:title`, `og:image`, `og:description`, `og:url` y redirige al SPA. El reverse proxy manda los rastreadores (WhatsApp, etc.) a ese endpoint. |
| Logo en OG | Se usa la URL pública del logo de la tienda. Si no hay logo, sin `og:image`. |
| Aviso de cierre | Se usa `ServicioTienda.MotivoCierre` (`"abono"` / `"gestion"`). El panel muestra el aviso solo con motivo `"abono"`. No se duplica la lógica de `Vista`. |

## Migraciones

Una migración EF en `src/StockControl.Api/Datos/Migraciones/` (nombre con fecha, ej. `2026112xxxxxxx_TiendaOfertasFicha`):

- `Ofertas.EnTienda` boolean NOT NULL, default `false`.
- `Productos.Descripcion` varchar(500) NULL.
- `LineasPedidoTienda`: `PrecioLista` numeric (default = `PrecioUnitario` para filas viejas), `Oferta` varchar(60) NULL, `Ahorro` numeric NOT NULL default 0.

Notas:

- Sin tablas nuevas: no hacen falta políticas RLS nuevas. Las tablas existentes ya filtran por `IdLocal`.
- Los valores por defecto dejan intactos productos, ofertas y pedidos actuales.
- Actualizar el snapshot del `DbContext`.

## Archivos

**Dominio (`src/StockControl.Dominio/`)**
- `Oferta.cs`: propiedad `EnTienda`.
- `Producto.cs`: propiedad `Descripcion`.
- `ServicioOfertas.cs`: `AplicableEnTienda(IEnumerable<Oferta>, int idProducto, DateOnly hoy)`.
- `ServicioPedidosTienda.cs`: `Cotizar` recibe ofertas y `hoy`; `LineaCotizada` suma lista, oferta y ahorro; `ALinea` los copia.
- `PedidoTienda.cs` (`LineaPedidoTienda`): `PrecioLista`, `Oferta`, `Ahorro`.
- `ServicioTienda.cs`: orden permitido (constantes) si se valida en dominio.
- `ServicioProductos` (o el que valida productos): largo de `Descripcion`.

**API (`src/StockControl.Api/`)**
- `Datos/OfertasVigentes.cs`: consulta de ofertas `EnTienda` vigentes del local.
- `Tienda/EndpointsPedidosTienda.cs`: pasar ofertas a `Cotizar` y al crear pedido.
- Endpoint público de productos de la tienda: parámetros `orden` y `sentido`; el DTO suma `descripcion`, `precioLista`, `oferta`, `ahorro`.
- Endpoint de ficha: `GET /tienda/{alias}/productos/{id}`.
- Endpoint de ofertas del panel: leer y escribir `EnTienda`.
- Endpoint de productos del panel: leer y escribir `Descripcion`.
- `Tienda/EndpointsOpenGraphTienda.cs` (nuevo, un trabajo): `GET /compartir/tienda/{alias}`.
- Endpoint del panel de tienda: devolver el motivo de cierre.
- `Datos/Migraciones/…`: migración.

**Angular (`src/StockControl.Web/src/app/`)**
- `paginas/tienda.component.ts`: grilla de 2 columnas, orden, precio tachado y ahorro, aviso de oferta.
- `paginas/ficha-producto-tienda.component.ts` (nuevo): fotos grandes, nombre, precio, stock, descripción, agregar.
- `servicios/api.service.ts`: tipos (`ProductoTiendaDto`, `CotizacionTiendaDto`, `LineaCotizada`), orden y ficha.
- `paginas/carrito-tienda.component.ts` y `paginas/pedido-tienda.component.ts`: mostrar tachado y ahorro.
- `paginas/ofertas.component.ts`: casilla «También en la tienda».
- Formulario de producto del panel: campo Descripción.
- `paginas/config-tienda.component.ts` (Tienda online del dueño): aviso de cierre por abono con enlace a pagar.
- Rutas: agregar la de ficha con lazy load.
- CSS (`estilos.css`): grilla y ficha, solo con variables.

**Infra**
- Regla del reverse proxy (Caddy/Nginx) para User-Agent de rastreadores hacia `/compartir/tienda/{alias}`. Sin secretos en git.

## Pasos

1. **Dominio y migración.** `EnTienda`, `Descripcion`, campos de línea de pedido, migración. Compila y migra sin romper datos.
2. **Ofertas en tienda.** `AplicableEnTienda`, `Cotizar` con ofertas, `ALinea`, pedido guarda lista/oferta/ahorro. Tests del dominio.
3. **Panel.** Casilla «También en la tienda» en ofertas y campo Descripción en producto, con validaciones.
4. **API pública.** DTO con oferta y descripción, orden por nombre o precio, endpoint de ficha. Rate limit `tienda` y tope de página como hoy.
5. **Tienda Angular.** Dos columnas, orden, precio tachado y ahorro, carrito y pedido con ahorro.
6. **Ficha.** Componente, ruta lazy, agregar al carrito desde la ficha.
7. **Open Graph.** Endpoint de la API, enlace compartible, regla del proxy. Cabeceras de seguridad ya existentes se mantienen.
8. **Aviso de abono.** El panel devuelve el motivo de cierre; el dueño ve el aviso en Tienda online.
9. **Cierre.** Revisar en celular (360 px) y escritorio, tema claro y oscuro. Tildar criterios y actualizar History.

## Tests

xUnit en `src/StockControl.Tests/` (extender `ServicioOfertasTests.cs` y `ServicioPedidosTiendaTests.cs`). Un test, un comportamiento:

- `Tienda_aplica_oferta_marcada_en_tienda`
- `Tienda_ignora_oferta_sin_en_tienda`
- `Tienda_ignora_oferta_vencida_o_futura`
- `Tienda_no_aplica_opciones_de_caja_al_costo_ni_descuento` (la oferta corre aunque `AnulaConAlCosto`)
- `Cotizar_tienda_descuento_calcula_precio_y_ahorro`
- `Cotizar_tienda_lleva_paga_cobra_unidades_correctas`
- `Cotizar_tienda_precio_desde_y_exacto_segun_cantidad`
- `Cotizar_tienda_sin_oferta_no_tiene_ahorro`
- `Pedido_tienda_guarda_precio_lista_oferta_y_ahorro`
- `Pedido_tienda_no_cambia_si_la_oferta_se_edita_despues`
- `Cotizar_tienda_producto_sector_o_generico_sin_oferta`
- `Cotizar_tienda_stock_rigido_sigue_limitando_con_oferta`
- `Descripcion_producto_admite_vacio_y_rechaza_mas_de_500`
- `Orden_tienda_acepta_solo_nombre_o_precio`
- `Orden_por_precio_usa_precio_con_oferta`
- `Open_graph_escapa_nombre_y_omite_imagen_sin_logo` (generador de HTML como función pura)
- `MotivoCierre_abono_vencido_devuelve_abono` y `MotivoCierre_gestion_devuelve_gestion` (ya cubierto si existe; completar si falta)
- `Aviso_dueno_solo_con_motivo_abono`
- Permisos: `Oferta_en_tienda_solo_la_edita_el_dueno` (empleado sin permiso rechazado).
- Aislamiento: la consulta de ofertas de tienda no trae ofertas de otro local.

## Criterios

Los de la tarjeta, ya verificables:

- [ ] Casilla «también en la tienda» en la oferta; la tienda aplica solo esas.
- [ ] Dos columnas en el celular.
- [ ] Descripción opcional en el producto y ficha de producto.
- [ ] Ordenar por nombre o precio.
- [ ] Vista previa del link con nombre y logo.
- [ ] Aviso al dueño de tienda cerrada por abono.
- [ ] Tests xUnit.

Además:

- [ ] Carrito y pedido muestran precio tachado y ahorro, y el total coincide con lo guardado.
- [ ] Migración aplicada sin tocar datos existentes.
- [ ] Sin secretos en el diff.

## Fuera de alcance

- Cambios en las reglas de ofertas de caja o en el WinForms.
- Ofertas nuevas solo de tienda (no hay tipo propio: se marcan las existentes).
- Cupones, combos de varios productos en tienda (`OfertaCombo` no entra).
- Varias oferta simultáneas por producto.
- Reseñas, favoritos, productos relacionados, filtros por precio.
- Editor de texto enriquecido para la descripción.
- Vista previa dinámica para otras rutas (carrito, pedido, ficha).
- Pasarela de pago del abono nueva: el aviso enlaza a la existente.
- Prerenderizado (SSR) de Angular.
- Descuento de la descripción en el sitemap.

## History

Al cerrar la tarjeta, en `backlog/History.md`:

1. Fila `#orden` 71 (tarjeta 70): estado **no** → **sí**, sin huecos.
2. Actualizar **Siguiente** a la próxima tarjeta según `#orden`.
3. Línea en el **Log**: `2026-MM-DD · 70 Tienda: ofertas, dos columnas y ficha · sí · …` (qué quedó y cualquier hueco).
4. Tildar los criterios en `backlog/70-tienda-ofertas-ficha.md`.
5. Si hay datos nuevos de la tienda, actualizar `Manual-de-usuario.md`.
