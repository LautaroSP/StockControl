# Configurar ofertas

- **Roles:** dueño (arma la oferta). Empleado y dueño la ven aplicada en la caja.
- **Tipo:** mostrador

## Qué

El dueño configura ofertas por producto, con fecha desde y hasta. Hay dos tipos: un descuento, o una promo de llevás N y pagás M (2x1, 3x2).

En la caja se aplican solas. Si el producto está en oferta y el carrito llega a la cantidad, cobra menos sin que nadie toque un botón. El descuento manual del ticket (tarjeta 07, sobre el margen) sigue aparte.

## Criterios de aceptación

- [x] Pantalla Ofertas, solo dueño. Alta, edición y baja de una oferta del local.
- [x] La oferta es de un producto del local. No aplica a producto sector ni a genérico.
- [x] Fecha desde y hasta, inclusive, en el día de Argentina. Fuera de ese rango no corre.
- [x] Un producto no tiene dos ofertas que se pisen en las fechas. Si no se pisan, puede tener varias.
- [x] Descuento: porcentaje sobre el precio de venta de ese producto. En el carrito, cada unidad de ese producto sale con el descuento mientras la oferta esté vigente.
- [x] Promo llevás N y pagás M, con N mayor que M. 2x1 y 3x2 entran en esa forma.
- [x] Cada grupo de N cobra M. El resto se cobra entero. En un 3x2, 3 unidades cobran 2; 4 cobran 3; 6 cobran 4.
- [x] La promo descuenta stock de lo que se lleva. En un 3x2 salen 3 del stock, aunque se cobren 2.
- [x] El carrito y el ticket muestran la cantidad que se lleva y el importe que se cobra, con la oferta a la vista.
- [x] La venta guarda la cantidad llevada y el total cobrado. Informes y estadísticas usan ese total.
- [x] Si además hay descuento manual del ticket, primero corre la oferta y después ese descuento, como hoy.
- [x] Cada oferta elige: «Aplicar descuento agregado desde la caja (deshabilita la oferta)». Si está marcada y la caja pone descuento, la oferta no corre.
- [x] Cada oferta elige: «Aplicar cobro al costo desde la caja (deshabilita la oferta)». Viene marcada. Si se desmarca, al costo mantiene la promo y el porcentaje va sobre el costo.
- [x] Cada oferta elige: «No aplicar ofertas en la caja». Si está marcada y entra en el carrito, la caja muestra el tilde «No aplicar ofertas»; dueño y empleado lo pueden usar en esa venta.

## Fuera

- Combos de productos distintos.
- Cupones, cliente o tope de usos.
- Ofertas en la tienda (29).
- Apagar o reemplazar el descuento manual de la caja.
