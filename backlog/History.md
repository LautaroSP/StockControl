# History — backlog web

Fuente de verdad del estado. **No re-auditar el repo** para “qué está hecho”: mirar esta tabla.

Al **cerrar** una tarjeta: tildar criterios en `backlog/NN-*.md`, pasar a **sí** acá, y una línea en el log.

Estados: **sí** · **no** · **parcial** (hay código, no cierra la tarjeta).

## Siguiente

**#33 · 33 Guardar ticket sin vender**

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
| 33 | 33 | [Guardar ticket sin vender](33-ticket-sin-venta.md) | no | Pedido en espera: el carrito se guarda sin cobrar y es del puesto de caja. Si cancelan y vuelven, se carga de nuevo. No mueve stock ni entra en la caja |
| 34 | 34 | [Configurar ofertas](34-ofertas.md) | no | Descuento por producto con fecha, o llevás N y pagás M. El carrito lo aplica solo. El descuento manual de la caja sigue |
| 35 | 35 | [Cuentas pendientes](35-cuentas-pendientes.md) | no | Fiado: baja el stock, no es venta hasta el pago. Pagos parciales. Empleado fía si el dueño lo habilita. Incobrable = pérdida. Total por cliente en Estadísticas |
| 36 | 29 | [Tienda: URL pública y catálogo](29-tienda-online.md) | no | Última etapa. `/tienda/{alias}` única por local, pública, sin login. Mapa OpenStreetMap con la ubicación del local. Publicar productos, activar/pausar |
| 37 | 36 | [Tienda: imágenes](36-tienda-imagenes.md) | no | Propuesta por código de barras (Open Food Facts) con Aceptar o Subir propia. Hasta 2 por producto, 400 × 400 px |
| 38 | 37 | [Tienda: carrito y pedido](37-tienda-carrito-pedido.md) | no | Retiro en el local, stock reservado, precio congelado, consulta sin cuenta |
| 39 | 41 | [Tienda: panel de pedidos y avisos](41-tienda-panel-pedidos.md) | no | Estados, entregado = venta, campanita, Web Push, wa.me |
| 40 | 38 | [Tienda: Mercado Pago y gestión](38-tienda-mercado-pago.md) | no | Checkout Pro con la cuenta MP de la 21. Gestión: importe fijo en Configuración |
| 41 | 39 | [Tienda: transferencia](39-tienda-transferencia.md) | no | A confirmar con plazo del dueño; gestión suma al abono |
| 42 | 40 | [Tienda: envío por km](40-tienda-envio-km.md) | no | Precio por km, mínimo, radio, gratis desde |

## Log

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
