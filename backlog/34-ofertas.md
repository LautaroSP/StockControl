# Configurar ofertas

- **Roles:** dueño (arma la oferta). Empleado y dueño la ven aplicada en la caja.
- **Tipo:** mostrador

## Qué

El dueño configura ofertas por producto, con fecha desde y hasta. Hay dos tipos: un descuento, o una promo de llevás N y pagás M (2x1, 3x2).

En la caja se aplican solas. Si el producto está en oferta y el carrito llega a la cantidad, cobra menos sin que nadie toque un botón. El descuento manual del ticket (tarjeta 07, sobre el margen) sigue aparte.

## Criterios de aceptación

- [ ] Pantalla Ofertas, solo dueño. Alta, edición y baja de una oferta del local.
- [ ] La oferta es de un producto del local. No aplica a producto sector ni a genérico.
- [ ] Fecha desde y hasta, inclusive, en el día de Argentina. Fuera de ese rango no corre.
- [ ] Un producto no tiene dos ofertas que se pisen en las fechas. Si no se pisan, puede tener varias.
- [ ] Descuento: porcentaje sobre el precio de venta de ese producto. En el carrito, cada unidad de ese producto sale con el descuento mientras la oferta esté vigente.
- [ ] Promo llevás N y pagás M, con N mayor que M. 2x1 y 3x2 entran en esa forma.
- [ ] Cada grupo de N cobra M. El resto se cobra entero. En un 3x2, 3 unidades cobran 2; 4 cobran 3; 6 cobran 4.
- [ ] La promo descuenta stock de lo que se lleva. En un 3x2 salen 3 del stock, aunque se cobren 2.
- [ ] El carrito y el ticket muestran la cantidad que se lleva y el importe que se cobra, con la oferta a la vista.
- [ ] La venta guarda la cantidad llevada y el total cobrado. Informes y estadísticas usan ese total.
- [ ] Si además hay descuento manual del ticket, primero corre la oferta y después ese descuento, como hoy.

## Fuera

- Combos de productos distintos.
- Cupones, cliente o tope de usos.
- Ofertas en la tienda (29).
- Apagar o reemplazar el descuento manual de la caja.
