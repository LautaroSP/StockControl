# History — backlog web

Fuente de verdad del estado. **No re-auditar el repo** para “qué está hecho”: mirar esta tabla.

Al **cerrar** una tarjeta: tildar criterios en `backlog/NN-*.md`, pasar a **sí** acá, y una línea en el log.

Estados: **sí** · **no** · **parcial** (hay código, no cierra la tarjeta).

## Siguiente

**#20 · 17 Estadísticas**

Después: 19 Carteles → **26 Proveedores** → **27 Recibir orden** → 02 Pago por local → 22 Cobro del abono → 03 Admin → 04 Contratar local → **21 QR (última)**.

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
| 20 | 17 | [Estadísticas](17-estadisticas.md) | no | |
| 21 | 19 | [Carteles](19-carteles.md) | no | |
| 22 | 26 | [Proveedores y órdenes](26-proveedores.md) | no | Dueño/socio. Catálogo Excel + pedido WhatsApp/Excel/PDF |
| 23 | 27 | [Recibir orden](27-recibir-orden.md) | no | Marcar recibida y sumar stock. Depende de 26 |
| 24 | 02 | [Pago por local](02-pago-por-local.md) | no | Hay `EstadoAbono` / vence en el local; no bloquea caja vencida ni panel admin |
| 25 | 22 | [Cobro del abono](22-cobro-abono-servicio.md) | no | QR tuyo / “ya pagué”. Distinto del QR de mostrador (21) |
| 26 | 03 | [Admin: alta de locales](03-admin-alta-locales.md) | no | |
| 27 | 04 | [Dueño: contratar otro local](04-dueno-contratar-local.md) | no | Depende de 22 |
| 28 | 21 | [Pasarela pagos QR MP](21-pasarela-pagos-qr.md) | no | **Última.** Cliente del kiosco paga en caja |

## Log

- 2026-09-10 · 18 Ticket · sí · Impresión por diálogo del navegador; formatos POS 58/80 y A4
- 2026-09-10 · 08 Varios carritos · sí · 4 pestañas fijas F1–F4, extras con +/×
- 2026-09-10 · 10 / 25 · ajuste · Informes = ventas + cerrar caja; Cajas = historial de cierres y detalle de ventas
- 2026-09-10 · 26 Proveedores · no · alta de tarjeta (órdenes de compra, Excel, WhatsApp wa.me)
- 2026-09-10 · 27 Recibir orden · no · alta de tarjeta (marcar recibida + sumar stock)
