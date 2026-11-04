# Tienda online: gestión de pedidos fuera de Mercado Pago

- **Roles:** dueño (elige quién paga la gestión) · comprador (la ve en el ticket) · admin (ve y cobra lo pendiente)
- **Tipo:** cargo fijo por pedido + deuda del local, cobrada aparte del abono
- **Depende de:** 37 Carrito y pedido, 38 Mercado Pago y gestión (importe configurable), 39 Transferencia, 22 Cobro del abono

## Qué

Cuando el pedido de la tienda **no** se paga por Mercado Pago (transferencia o efectivo), la plata no pasa por vos y no hay `marketplace_fee`. La gestión del pedido queda como **deuda del local**. El abono del mes sigue en $ 14.999. La deuda se paga con un cobro aparte.

## Reglas

- El importe es **un monto en pesos, nunca un porcentaje**: los **$ 1.000 por pedido** de la plataforma (38), iguales en todas las tiendas. El local que el admin eximió no genera deuda.
- Aplica a pedidos pagados por **transferencia** o **efectivo**. Con Mercado Pago (QR / Checkout Pro) sigue como en la 38.
- Casilla en Configuración de la tienda (dueño): **«Incluir gasto de gestión $ {importe} en ticket»** (con el importe vigente, p. ej. «$ 1.000 (mil pesos)») y un **(?)**.
  - El (?) es un tooltip al pasar el mouse: *«Es un gasto por la gestión del pedido. Si lo tildás, se le suma al comprador. Si no, lo abonás vos como dueño del local.»*
  - También se abre con foco o toque (celular), no solo con mouse.
  - **Tildada:** línea aparte «Gestión» en el pedido y el ticket del comprador; el total la incluye.
  - **Sin tildar:** el comprador paga solo el pedido; la gestión es del dueño.
- En ambos casos el local debe la gestión (si se tildó, el comprador se la reintegra al local con el pedido). No entra en el abono del mes.
- La deuda nace cuando el pedido pasa a **Pagado / Entregado**. Pedido cancelado o vencido antes de confirmar: no genera deuda.
- El importe queda **congelado en el pedido** al crearlo: cambiarlo después no altera pedidos ya hechos.
- Si el subtotal es menor que el importe, la gestión es el subtotal (igual que la 38).
- La deuda de las transferencias ya la registra la 39 (`ServicioGestionTienda.Pendiente`, columna Pendiente en el admin). Falta efectivo y el cobro aparte que la salda.
- El abono del mes (a mano, débito o el admin) sigue en $ 14.999 y no salda la gestión. En Abono hay un pago propio por el total pendiente. Al acreditarse salda esas deudas y no mueve el vencimiento. El admin puede saldarla, y anular esa marca, aparte de marcar el mes.
- Cada deuda vence con el abono del período en el que nació. Pasado ese vencimiento hay **7 días** para pagarla. Si sigue pendiente, se cierra la tienda online. La caja sigue: el abono vencido es lo único que la bloquea.
- Mientras haya gestión pendiente, un aviso indica el importe, la fecha límite y que a los 7 días se cierra la tienda, no la caja. Si ya cerró, el aviso lo dice.
- El admin ve la gestión pendiente y saldada por local y por mes.

## Criterios de aceptación

- [x] El importe es un monto en pesos (sin %): los $ 1.000 de la 38. Local eximido por el admin: sin deuda.
- [x] Casilla «Incluir gasto de gestión $ {importe} en ticket» con tooltip (?) por mouse, foco y toque.
- [x] Tildada: el comprador ve la línea «Gestión» y el total la incluye. Sin tildar: no se le suma.
- [x] Transferencia y efectivo generan deuda de gestión al pasar a Pagado / Entregado, con el importe congelado en el pedido.
- [x] Mercado Pago no genera deuda (va por `marketplace_fee`).
- [x] El abono del mes sigue en $ 14.999 y no salda la gestión. Un pago aparte cobra el total pendiente, la salda y no mueve el vencimiento. Sirve con o sin débito.
- [x] A los 7 días del vencimiento de ese abono, si la gestión sigue impaga, la tienda pública se cierra y la caja sigue operando. Pagarla la reabre.
- [x] Hay un aviso con el importe, la fecha límite y que se cierra la tienda, no la caja.
- [x] El admin ve lo pendiente por local y mes.
- [x] Migración: tabla de gestiones pendientes (la bandera de quién paga y la exención ya están en `Local` desde la 38).
- [x] Tests xUnit: gestión incluida vs a cargo del dueño, importe congelado, tope en el subtotal, eximido no genera deuda, MP sin deuda, cancelado sin deuda, el mes y el débito no saldan, el pago aparte salda sin mover el vencimiento, a los 7 días se cierra la tienda y la caja sigue.
