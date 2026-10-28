# Backlog web — StockControl

**Estado y orden:** [History.md](History.md). Consultar ahí, no rearmar el tablero.

Una tarjeta = un archivo. El dueño puede todo lo del empleado, y más.
Se paga **$ 14.999 / mes por local** (Argentina). El dueño **no** da de alta locales: paga o habla con ventas.

## Roles

| Rol | Quién |
|---|---|
| **admin** | Vos. Servicio: locales, cobros, altas. No opera la caja del cliente. |
| **dueño** | Cliente. Ve sus locales, empleados, productos, precios, estadísticas, caja. |
| **empleado** | Caja, stock básico, cierre de caja. No crea locales ni ve estadísticas. |

## Índice

| # | Tarjeta | Roles |
|---|---|---|
| 00 | [Stack y migración](00-stack.md) | decisión técnica |
| 01 | [Roles y permisos](01-roles-y-permisos.md) | los tres |
| 02 | [Pago por local](02-pago-por-local.md) | admin, dueño |
| 03 | [Admin: alta de locales](03-admin-alta-locales.md) | admin |
| 04 | [Dueño: contratar otro local](04-dueno-contratar-local.md) | dueño, admin |
| 22 | [Cobro del abono (te pagan a vos)](22-cobro-abono-servicio.md) | dueño paga, admin cobra — QR MP o ventas |
| 05 | [Empleados](05-empleados.md) | dueño |
| 06 | [Permiso: empleado modifica precios](06-permiso-empleado-precios.md) | dueño |
| 07 | [Venta en caja](07-venta-en-caja.md) | empleado, dueño |
| 08 | [Varios carritos](08-varios-carritos.md) | empleado, dueño |
| 09 | [Cierre de caja](09-cierre-de-caja.md) | empleado, dueño |
| 24 | [Puestos de caja](24-puestos-de-caja.md) | dueño (cantidad), empleado y dueño (elegir / cerrar) |
| 25 | [Consulta de cajas cerradas](25-consulta-cajas-cerradas.md) | empleado, dueño |
| 10 | [Informes de ventas](10-informes-de-ventas.md) | empleado*, dueño |
| 11 | [Productos](11-productos.md) | dueño |
| 12 | [Stock](12-stock.md) | empleado, dueño |
| 13 | [Producto sector](13-producto-sector.md) | dueño |
| 14 | [Grupos de precio](14-grupos-de-precio.md) | dueño |
| 15 | [Filtros de catálogo](15-filtros-catalogo.md) | empleado, dueño |
| 16 | [Medios de pago](16-medios-de-pago.md) | dueño |
| 17 | [Estadísticas](17-estadisticas.md) | dueño |
| 18 | [Ticket](18-ticket.md) | dueño (config), empleado (imprimir) |
| 19 | [Carteles](19-carteles.md) | dueño |
| 26 | [Proveedores y órdenes](26-proveedores.md) | dueño, socio |
| 27 | [Recibir orden](27-recibir-orden.md) | dueño, socio |
| 20 | [Configuración del local](20-configuracion-del-local.md) | dueño |
| 21 | [Pasarela de pagos (QR MP / Point)](21-pasarela-pagos-qr.md) | dueño (vincula), empleado (cobra) — antes de la tienda |
| 23 | [Modo oscuro](23-modo-oscuro.md) | todos — UI |
| 28 | [Landing](28-landing.md) | visitante — sitio público |
| 29 | [Tienda: URL pública y catálogo](29-tienda-online.md) | dueño, comprador — `/tienda/{alias}` única por local, sin login |
| 50 | [Tienda: panel](50-tienda-panel.md) | dueño, socio, empleado (solo pedidos) — grupo Tienda en la barra |
| 36 | [Tienda: imágenes](36-tienda-imagenes.md) | dueño |
| 37 | [Tienda: carrito y pedido](37-tienda-carrito-pedido.md) | comprador |
| 41 | [Tienda: panel de pedidos y avisos](41-tienda-panel-pedidos.md) | dueño, socio, empleado |
| 38 | [Tienda: Mercado Pago y gestión](38-tienda-mercado-pago.md) | comprador, dueño, admin |
| 39 | [Tienda: transferencia](39-tienda-transferencia.md) | comprador, dueño |
| 51 | [Tienda: gestión fuera de MP](51-tienda-gestion-fuera-mp.md) | dueño, comprador, admin — monto en pesos (inicial $ 1.000) por pedido en transferencia o efectivo, suma al abono |
| 40 | [Tienda: envío por km](40-tienda-envio-km.md) | comprador, dueño |
| 30 | [Mantenimiento y 404](30-mantenimiento-y-404.md) | visitante — Caddy si la API no responde; 404 de ruta en Angular |
| 31 | [Iconos](31-iconos.md) | dueño, empleado — barra y Configuración; Mercado Pago con check |
| 32 | [Teclado de la caja](32-teclado-caja.md) | empleado, dueño — foco en el lector y cobro con teclado |
| 33 | [Guardar ticket sin vender](33-ticket-sin-venta.md) | empleado, dueño — guardar el carrito y recargarlo si el cliente vuelve |
| 34 | [Configurar ofertas](34-ofertas.md) | dueño — descuento por producto con fecha, o llevás N y pagás M; la caja lo aplica sola |
| 35 | [Cuentas pendientes](35-cuentas-pendientes.md) | empleado, dueño — el cliente se lleva la mercadería y paga después; la ganancia entra al cobrar |

\*El empleado ve lo necesario para operar (ventas del día / reimprimir). No borra ventas ni ve costo.

Dos Mercados Pago distintos: la **22** es Jorge pagándote el mes (tu QR o WhatsApp). La **21** conecta la cuenta del local; el mostrador cobra ahí y la **29** usa esa misma cuenta (Checkout Pro; la gestión es un importe fijo de Configuración). Con pocos usuarios, la 22 puede ser QR + “ya pagué” / marca admin; no hace falta débito automático.
