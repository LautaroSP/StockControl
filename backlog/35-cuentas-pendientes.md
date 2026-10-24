# Cuentas pendientes

- **Roles:** dueño (fía, cobra y anula). Empleado fía y cobra solo si el dueño lo habilita. El total en Estadísticas lo ve el dueño.
- **Tipo:** mostrador

## Qué

En la caja se puede cerrar el carrito como cuenta pendiente. María se lleva 3 leches y paga más tarde: queda la clienta María, debiendo el importe de ese ticket.

El stock baja en ese momento. La plata no entra: ni ganancia, ni total vendido, ni cierre, hasta que se cobra. María debe el importe de ese día, aunque después suban los precios.

No es el ticket sin vender de la 33. Ese no descuenta stock y no es una deuda. Este sí saca la mercadería y deja un saldo.

## Cómo se guarda

La cuenta pendiente es una tabla aparte, con sus líneas. **No** es una venta (`InformeVenta`) hasta que se cobra. Así el cierre, Informes y Estadísticas no cambian sus consultas y un fiado nunca aparece como plata cobrada.

Cada pago, total o parcial, crea una venta con la fecha, la caja y el medio de pago de ese momento. Esa venta no vuelve a bajar stock.

## Criterios de aceptación

- [x] Configuración: "El empleado puede fiar". Apagado por defecto. El dueño y el socio siempre pueden.
- [x] Desde la caja, con un cliente y un carrito con productos, dejar el ticket a cuenta en vez de cobrarlo.
- [x] El cliente es del local: nombre y teléfono opcional, y se puede volver a elegir. María con dos tickets tiene dos cuentas.
- [x] Cada cuenta guarda cliente, fecha, usuario que fió, líneas, importe y saldo. Se puede ver el detalle.
- [x] Al fiar baja el stock como en una venta cobrada. Sector y genérico siguen sin mover stock. Con stock rígido, si falta mercadería no se puede fiar.
- [x] Con el local vencido no se puede fiar, igual que no se puede cobrar.
- [x] Mientras no se paga, no suma ganancia, ni total vendido, ni ticket promedio, ni el cierre de caja.
- [x] Listar las cuentas pendientes del local, filtrar por cliente y cobrar.
- [x] Se puede pagar una parte. El saldo baja; la cuenta se cierra cuando llega a 0. No se paga más que el saldo.
- [x] Cada pago entra en ese momento, en la caja de ese día, con el medio elegido. La ganancia de ese pago es proporcional a lo que se pagó del ticket.
- [x] El dueño puede anular una cuenta incobrable. El saldo queda como pérdida, no devuelve stock y no entra como venta.
- [x] El ticket impreso al fiar dice "Pendiente de pago" y el saldo.
- [x] En Estadísticas, el dueño ve el total que se debe ahora, no solo lo del mes elegido.
- [x] Ese total se abre por cliente: María y cuánto debe, sumando sus cuentas abiertas.
- [x] Las unidades fiadas cuentan en el top de productos y en ventas del día, porque salieron del stock. La plata cuenta recién al cobrar.

## Fuera

- Intereses o límite de crédito.
- Cobrar una cuenta con el QR o el Point de Mercado Pago que esperan la confirmación (21). En v1 se cobra con un medio que no espera.
- Cuenta corriente entre locales.
- La deuda en la tienda (29).
