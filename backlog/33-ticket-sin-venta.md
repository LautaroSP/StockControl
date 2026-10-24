# Guardar ticket sin vender

- **Roles:** empleado, dueño
- **Tipo:** mostrador

## Qué

En la caja se puede guardar el carrito como ticket sin cobrar. No descuenta stock y no entra en la caja. Si el cliente cancela y después vuelve a pedir lo mismo, se abre ese ticket y los productos vuelven al carrito.

Los carritos de la 08 viven en esa sesión. Este pedido queda guardado: sobrevive a cerrar el navegador. Pertenece al **puesto de caja** donde se guardó. Otra caja del mismo local no lo ve.

No es el ticket impreso de la 18. En pantalla se llama **pedido en espera**.

## Dónde está en la caja

Debajo de la fila COBRAR y Cancelar, dos botones: **Guardar en espera** y **Pedidos en espera** (con la cantidad de ese puesto). El segundo abre la lista de ese puesto y deja cargar uno al carrito.

## Criterios de aceptación

- [x] Desde la caja, guardar el carrito actual sin cobrar, con un nombre o nota ("María", "pedido de las 11"). Guarda productos, cantidades, precio de genérico y sector, descuento y cobrar al costo.
- [x] El pedido es del puesto que lo guardó. Otro puesto del mismo local no lo ve ni lo carga.
- [x] Guardar no descuenta stock, no reserva stock, no crea una venta y no suma al cierre.
- [x] El pedido no aparece en Informes ni en Estadísticas.
- [x] Listar los pedidos en espera de ese puesto y cargar uno en el carrito, sin volver a escanear.
- [x] Al cargarlo, un producto común toma el precio actual. Si cambió desde que se guardó, la caja lo avisa en esa línea.
- [x] Al cargarlo, el pedido sigue guardado. Se borra a mano, o cuando esa venta se cobra.
- [x] El cobro y el borrado del pedido van juntos. Si ya se cobró desde otra pestaña de ese mismo puesto, la segunda recibe un aviso y no lo cobra de nuevo.
- [x] Un producto que se borró del catálogo no se carga; la caja avisa cuál faltó.
- [x] Con stock rígido, si al cobrar falta mercadería, el cobro falla como cualquier venta.
- [x] Un carrito vacío no se guarda.

## Fuera

- Reservar stock.
- Vencimiento automático de pedidos viejos.
- Pasar un pedido de un puesto a otro.
