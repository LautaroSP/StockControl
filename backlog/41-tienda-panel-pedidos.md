# Tienda online: panel de pedidos y avisos

- **Roles:** dueño · socio · empleado con acceso a pedidos
- **Tipo:** panel + notificaciones
- **Depende de:** 37 Carrito y pedido (y 38 / 39 / 40 para los estados de pago y envío)

## Panel

- Estados: Nuevo → Pagado / A confirmar transferencia → Preparando → Listo / Enviado → Entregado. También Cancelado.
- Al marcar **Entregado**, el pedido entra como venta del local: descuenta stock, va a informes y al cierre con el medio de pago real.
- Cancelar libera el stock reservado.

## Avisos

El aviso que **siempre** llega es el del sistema.

- **Campanita** con contador de no leídas y lista. Clic abre el pedido y lo marca leída; “Marcar todas”.
- Se actualiza sola mientras el panel está abierto, con sonido opcional.
- **Web Push:** llega con la pestaña cerrada (compu y Android; iPhone solo con el panel en la pantalla de inicio). Claves VAPID en entorno, nunca en git.
- Cada usuario elige qué recibe y si quiere sonido. Las notificaciones son por local.
- **WhatsApp `wa.me`:** el comprador puede avisar al local con el pedido escrito; desde el panel, “Escribir al comprador”. El envío automático (API de Meta) queda fuera.

## Criterios de aceptación

- [ ] Panel con estados y transiciones válidas.
- [ ] Entregado registra la venta (stock, informes, cierre).
- [ ] Cancelar libera el stock.
- [ ] Campanita con contador, lista y marcar leídas, por local y por usuario.
- [ ] Web Push opcional por usuario.
- [ ] Botones de WhatsApp (comprador → local, local → comprador).
- [ ] Tests xUnit: transiciones válidas e inválidas, entregado = venta, notificación solo a usuarios de ese local, cancelar libera stock.

## Fuera de v1

- WhatsApp automático con la API de Meta.
