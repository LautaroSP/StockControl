# Tienda online: pago con Mercado Pago y gestión

- **Roles:** comprador (paga) · dueño (importe de gestión) · admin (ve la gestión cobrada)
- **Tipo:** cobro online con Checkout Pro
- **Depende de:** 37 Carrito y pedido, 21 Pasarela (cuenta MP del local), 20 Configuración

## Qué

El comprador paga el pedido con Mercado Pago. La plata va a la cuenta **del local** (la conectada en la 21). Es otro Mercado Pago que el de la 22 (abono).

## Pago

- Checkout Pro con el token del local. El cartel QR del mostrador no entra: es del puesto.
- Si el local no conectó Mercado Pago, la tienda no ofrece este medio.
- El pedido pasa a **Pagado** solo con el webhook firmado de MP. No sirve que vuelva el navegador.
- Cancelado antes de entregar: reembolso por MP desde el panel.

## Gestión (importe fijo)

- Campo en **Configuración**: importe en pesos por pedido online. Cada sucursal el suyo. El empleado no lo ve. 0 = no se cobra.
- El dueño elige: **incluida** (el comprador ve el precio de lista) o **a cargo del comprador** (línea aparte “Gestión”).
- No cambia con el tamaño del carrito. Si el subtotal es menor que el importe, la gestión es el subtotal.
- Con MP, va como `marketplace_fee`: MP te lo separa solo.
- Pedido cancelado antes de entregar: no se cobra gestión.
- El admin ve la gestión por local y por mes.

## Criterios de aceptación

- [ ] Checkout Pro con la cuenta MP del local; sin cuenta, el medio no se ofrece.
- [ ] Pedido Pagado solo por webhook firmado.
- [ ] Importe fijo de gestión en Configuración, incluida o a cargo del comprador; desglose visible antes de pagar.
- [ ] `marketplace_fee` es ese importe; tope en el subtotal.
- [ ] Cancelar un pedido pagado permite reembolso y no cobra gestión.
- [ ] El admin ve la gestión cobrada por local y mes.
- [ ] Tests xUnit: gestión incluida vs a cargo del comprador, tope en el subtotal, 0 no cobra, cancelado sin gestión, pedido no pagado sin webhook válido.
