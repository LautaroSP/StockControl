# Tienda online: gestión de pedidos fuera de Mercado Pago

- **Roles:** dueño (elige quién paga la gestión) · comprador (la ve en el ticket) · admin (ve y cobra lo pendiente)
- **Tipo:** cargo fijo por pedido + deuda del local sumada al abono
- **Depende de:** 37 Carrito y pedido, 38 Mercado Pago y gestión (importe configurable), 39 Transferencia, 22 Cobro del abono

## Qué

Cuando el pedido de la tienda **no** se paga por Mercado Pago (transferencia o efectivo), la plata no pasa por vos y no hay `marketplace_fee`. La gestión del pedido queda como **deuda del local** y se suma al total a pagar en el **próximo abono**.

## Reglas

- El importe es **un monto en pesos, nunca un porcentaje**. Es el mismo campo de **Configuración** de la 38 (por sucursal, el empleado no lo ve). Valor inicial: **$ 1.000**. 0 = no se cobra.
- Aplica a pedidos pagados por **transferencia** o **efectivo**. Con Mercado Pago (QR / Checkout Pro) sigue como en la 38.
- Casilla en Configuración de la tienda (dueño): **«Incluir gasto de gestión $ {importe} en ticket»** (con el importe vigente, p. ej. «$ 1.000 (mil pesos)») y un **(?)**.
  - El (?) es un tooltip al pasar el mouse: *«Es un gasto por la gestión del pedido. Si lo tildás, se le suma al comprador. Si no, lo abonás vos como dueño del local.»*
  - También se abre con foco o toque (celular), no solo con mouse.
  - **Tildada:** línea aparte «Gestión» en el pedido y el ticket del comprador; el total la incluye.
  - **Sin tildar:** el comprador paga solo el pedido; la gestión es del dueño.
- En ambos casos el local debe la gestión al próximo abono (si se tildó, el comprador se la reintegra al local con el pedido).
- La deuda nace cuando el pedido pasa a **Pagado / Entregado**. Pedido cancelado o vencido antes de confirmar: no genera deuda.
- El importe queda **congelado en el pedido** al crearlo: cambiarlo después no altera pedidos ya hechos.
- Si el subtotal es menor que el importe, la gestión es el subtotal (igual que la 38).
- El abono (22) muestra el desglose: abono del mes + gestión de N pedidos. Al acreditarse el pago, esas deudas quedan saldadas.
- El admin ve la gestión pendiente y saldada por local y por mes.

## Criterios de aceptación

- [ ] El importe es un monto en pesos (sin %), el de Configuración de la 38; inicial $ 1.000.
- [ ] Casilla «Incluir gasto de gestión $ {importe} en ticket» con tooltip (?) por mouse, foco y toque.
- [ ] Tildada: el comprador ve la línea «Gestión» y el total la incluye. Sin tildar: no se le suma.
- [ ] Transferencia y efectivo generan deuda de gestión al pasar a Pagado / Entregado, con el importe congelado en el pedido.
- [ ] Mercado Pago no genera deuda (va por `marketplace_fee`).
- [ ] El próximo abono suma la deuda pendiente con desglose y la salda al acreditarse.
- [ ] El admin ve lo pendiente por local y mes.
- [ ] Migración: bandera en `Local` y tabla de gestiones pendientes.
- [ ] Tests xUnit: gestión incluida vs a cargo del dueño, importe congelado, tope en el subtotal, 0 no cobra, MP sin deuda, cancelado sin deuda, abono suma y salda la deuda.
