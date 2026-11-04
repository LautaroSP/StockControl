# Tienda online: pago con Mercado Pago y gestión

- **Roles:** comprador (paga) · dueño (quién paga la gestión, reembolsa) · admin (exime locales, ve la gestión cobrada)
- **Tipo:** cobro online con Checkout Pro
- **Depende de:** 37 Carrito y pedido, 21 Pasarela (cuenta MP del local), 20 Configuración

## Qué

El comprador paga el pedido con Mercado Pago. La plata va a la cuenta **del local** (la conectada en la 21). Es otro Mercado Pago que el de la 22 (abono).

## Pago

- Checkout Pro con el token del local. El cartel QR del mostrador no entra: es del puesto.
- Si el local no conectó Mercado Pago, la tienda no ofrece este medio.
- El pedido pasa a **Pagado** solo con el webhook firmado de MP. No sirve que vuelva el navegador.
- El pago se pide a MP con el token del local y se valida: aprobado, ARS, importe igual al total, referencia del pedido y cobrador la cuenta del local.
- Mientras espera el pago, el pedido reserva stock y el panel lo muestra «Esperando pago» (no se prepara ni se entrega). El aviso de pedido nuevo sale al acreditarse.
- Sin pago en **30 minutos**, el pedido se cancela solo y libera el stock.
- Pagado: el comprador ya no lo cancela; contacta al local. El dueño lo cancela desde el panel con reembolso por MP.
- Entregar un pedido pagado arma la venta por el subtotal con el medio Mercado Pago. La gestión no es venta del local.

## Gestión (importe fijo de la plataforma)

- **$ 1.000 por pedido**, igual en todas las tiendas. Es de la plataforma (como el abono): el dueño no lo cambia.
- El admin puede **eximir** a un local del cargo (casilla por local en el panel de admin, con la clave maestra). Eximido: gestión 0.
- El dueño elige: **incluida** (el comprador ve el precio de lista y la gestión sale de lo que cobra el local) o **a cargo del comprador** (línea aparte «Gestión»). El empleado no lo ve.
- Se congela en el pedido al crearlo: eximir o cambiar la opción después no altera pedidos hechos.
- No cambia con el tamaño del carrito. Si el subtotal es menor que el importe, la gestión es el subtotal.
- Con MP, va como `marketplace_fee`: MP te lo separa solo.
- Pedido cancelado antes de entregar: no se cobra gestión.
- El admin ve la gestión por local y por mes.

## Criterios de aceptación

- [x] Checkout Pro con la cuenta MP del local; sin cuenta, el medio no se ofrece.
- [x] Pedido Pagado solo por webhook firmado y pago validado contra el pedido.
- [x] Pago pendiente vence a los 30 minutos y libera el stock.
- [x] Gestión de $ 1.000 por pedido en todas las tiendas; el admin exime locales.
- [x] El dueño elige incluida o a cargo del comprador; desglose visible antes de pagar.
- [x] `marketplace_fee` es la gestión congelada del pedido; tope en el subtotal.
- [x] Cancelar un pedido pagado (solo el local) reembolsa y no cobra gestión.
- [x] La venta del pedido pagado es el subtotal, sin la gestión.
- [x] El admin ve la gestión cobrada por local y mes.
- [x] Migración `PagosTienda`.
- [x] Tests xUnit (`ServicioPagoPedidoTiendaTests`): gestión incluida vs a cargo del comprador, tope en el subtotal, local eximido no cobra, importe congelado, cancelado sin gestión, pago inválido o sin webhook no marca Pagado, aviso repetido no duplica, vencido libera stock, el comprador no cancela pagado, entrega vende el subtotal.
