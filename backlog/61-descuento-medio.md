# Descuento por medio de pago

- **Roles:** dueño (carga el porcentaje). Empleado y dueño lo ven al cobrar.
- **Tipo:** mostrador
- **Depende de:** 16 Medios de pago, 07 Venta en caja, 18 Ticket

## Qué

El dueño pone un porcentaje de 0 a 99 en el medio de pago. Cero es sin descuento.

Al cobrar, el porcentaje corre sobre el total que ya quedó después de ofertas, combos y el descuento manual de la caja. Es un porcentaje del total: 10 % en efectivo son $ 100 menos en un ticket de $ 1.000.

Solo corre si hay un único medio y ese medio paga el ticket entero. Si se agrega otro medio, el descuento se va y el total vuelve al anterior.

La venta guarda el porcentaje y el importe. Un ticket viejo no cambia si después se edita el medio.

Mercado Pago cobra ese mismo total. El QR o el Point salen por el importe ya descontado.

Queda afuera el pago de una cuenta pendiente, la tienda y el escritorio.

## Criterios de aceptación

- [x] En Medios de pago, el dueño carga un porcentaje de 0 a 99. Cero no descuenta.
- [x] En la caja, con un solo medio y porcentaje, el total baja y se ve la línea del descuento. La vista del cliente también.
- [x] Con dos medios no hay descuento.
- [x] El porcentaje se calcula después de las ofertas y del descuento manual de la caja.
- [x] El pago tiene que igualar el total ya descontado. Mercado Pago pide ese importe.
- [x] El ticket guarda una línea propia, por ejemplo «Efectivo 10 %», con el importe cobrado.
- [x] Una venta `CUENTA-` no recibe el descuento.
- [x] Migración `DescuentoMedio`.
- [x] Tests xUnit (`ServicioDescuentoMedioTests`).
