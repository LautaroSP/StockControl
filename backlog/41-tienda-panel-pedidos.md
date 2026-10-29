# Tienda online: panel de pedidos y avisos

- **Roles:** dueño · socio · empleado con acceso a pedidos
- **Tipo:** panel + notificaciones
- **Depende de:** 37 Carrito y pedido (y 38 / 39 / 40 para los estados de pago y envío)

## Panel

- Estados: Nuevo → Pagado / A confirmar transferencia → Preparando → Listo / Enviado → Entregado. También Cancelado.
- Al marcar **Entregado**, el pedido entra como venta del local: descuenta stock, va a informes y al cierre con el medio de pago real.
- Cancelar libera el stock reservado.

### Cómo quedó en v1

- Estados: Nuevo → Preparando → Listo → Entregado, y Cancelado. Se puede saltar pasos (Nuevo → Listo o Nuevo → Entregado). Pagado, A confirmar transferencia y Enviado los suman la 38, la 39 y la 40 en la tabla de transiciones.
- Reservan stock Nuevo, Preparando y Listo.
- El local cancela mientras el pedido esté abierto. El comprador, solo en Nuevo.
- **Entregar y cobrar**: el que entrega elige el medio de pago (no Mercado Pago). La venta entra en su puesto de caja con el precio congelado del pedido y sin ofertas. Si el local está vencido, no entrega.
- Anular esa venta repone el stock; el pedido queda Entregado.
- El empleado entra si el dueño tilda «El empleado ve y entrega los pedidos de la tienda» en Configuración del local.

## Avisos

El aviso que **siempre** llega es el del sistema.

- **Campanita** con contador de no leídas y lista. Clic abre el pedido y lo marca leída; “Marcar todas”.
- Se actualiza sola mientras el panel está abierto, con sonido opcional.
- **Web Push:** llega con la pestaña cerrada (compu y Android; iPhone solo con el panel en la pantalla de inicio). Claves VAPID en entorno, nunca en git.
- Cada usuario elige qué recibe y si quiere sonido. Las notificaciones son por local.
- **WhatsApp `wa.me`:** el comprador puede avisar al local con el pedido escrito; desde el panel, “Escribir al comprador”. El envío automático (API de Meta) queda fuera.

### Cómo quedó en v1

- Avisan: pedido nuevo y pedido que cancela el comprador. Un aviso por usuario del local que puede ver pedidos.
- La campanita consulta cada 30 s con la pestaña a la vista y suena (si el usuario lo eligió) cuando llega uno nuevo.
- «Mis avisos» en Pedidos: sonido, y qué llega al dispositivo (nuevos, cancelados). «Recibir avisos acá» suscribe el navegador.
- Un dispositivo avisa a quien lo activó último. El servidor solo escribe a servicios de push conocidos (Google, Mozilla, Apple, Windows).
- Claves: `WebPush__ClavePublica`, `WebPush__ClavePrivada`, `WebPush__Asunto`. El par sale de `dotnet run --project src/StockControl.Api -- vapid`. Sin claves, el push queda apagado y la campanita sigue.

## Criterios de aceptación

- [x] Panel con estados y transiciones válidas.
- [x] Entregado registra la venta (stock, informes, cierre).
- [x] Cancelar libera el stock.
- [x] Campanita con contador, lista y marcar leídas, por local y por usuario.
- [x] Web Push opcional por usuario.
- [x] Botones de WhatsApp (comprador → local, local → comprador).
- [x] Tests xUnit: transiciones válidas e inválidas, entregado = venta, notificación solo a usuarios de ese local, cancelar libera stock.
- [ ] Probado en el navegador con la base: panel, entregar y cobrar, campanita con sonido y Web Push en compu y Android.

## Fuera de v1

- WhatsApp automático con la API de Meta.
