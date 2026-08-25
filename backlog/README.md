# Backlog web — StockControl

Una tarjeta = un archivo. El dueño puede todo lo del empleado, y más.
Se paga **$ 9.999 / mes por local** (Argentina). El dueño **no** da de alta locales: paga o habla con ventas.

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
| 20 | [Configuración del local](20-configuracion-del-local.md) | dueño |
| 21 | [Pasarela de pagos (QR MP / Posnet)](21-pasarela-pagos-qr.md) | dueño (vincula), empleado (cobra) — **última** |
| 23 | [Modo oscuro](23-modo-oscuro.md) | todos — UI |

\*El empleado ve lo necesario para operar (ventas del día / reimprimir). No borra ventas ni ve costo.

Dos Mercados Pago distintos: la **22** es Jorge pagándote el mes (tu QR o WhatsApp). La **21** es el cliente del kiosco pagando en caja (última, spike). Con pocos usuarios, la 22 puede ser QR + “ya pagué” / marca admin; no hace falta débito automático.
