# Tienda online: carrito y pedido

- **Roles:** comprador (sin cuenta) · dueño / socio / empleado (reciben el pedido)
- **Tipo:** carrito público + creación de pedido
- **Depende de:** 29 Tienda, 12 Stock

## Qué

El comprador arma un carrito, completa sus datos y genera un **pedido con retiro en el local**. El pago online es la 38, la transferencia la 39 y el envío la 40. Sin esas tarjetas, el pedido queda **A coordinar** y se paga al retirar.

## Carrito

- Sumar, restar y sacar. Se guarda en el navegador.
- **Stock rígido:** si el local no vende sin stock, no deja agregar más de lo que hay.
- Los precios se recalculan en el servidor; el navegador no manda importes.

## Checkout

- Datos: nombre, teléfono, mail. Retiro en el local.
- Muestra el desglose: subtotal y total.
- Honeypot y rate limit.
- Al terminar: **número de pedido**, estado y un link para consultarlo sin cuenta (código largo no adivinable).

## Reglas del pedido

- **Precio congelado:** el pedido guarda el precio del momento, aunque después cambie la lista.
- **Stock reservado** mientras el pedido está pendiente. Si se cancela o vence, se libera. Esa reserva no se puede vender en caja.
- El pedido pertenece a un local; nadie ve pedidos de otro.
- Estados de base: Nuevo → Cancelado. El resto lo define la 41.

## Criterios de aceptación

- [ ] Carrito en el navegador; el servidor recalcula precios y valida stock.
- [ ] Stock rígido: no se pide más de lo disponible.
- [ ] Checkout con nombre, teléfono, mail y retiro; honeypot y rate limit.
- [ ] El pedido guarda el precio congelado y reserva stock.
- [ ] Cancelar libera la reserva.
- [ ] El comprador consulta su pedido con un código no adivinable.
- [ ] Tienda pausada no recibe pedidos.
- [ ] Tests xUnit: precio congelado, reserva y liberación, stock rígido, pedido de tienda pausada rechazado, el servidor ignora precios del cliente, un local no ve pedidos de otro.

## Fuera de v1

- Cuenta del comprador e historial.
- Cupones y descuentos online.
