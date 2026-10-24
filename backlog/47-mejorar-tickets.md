# Mejorar los tickets

- **Roles:** dueño · empleado (imprime) · cliente (lo recibe)
- **Tipo:** web (`ServicioTicket`, `ticket.component.ts`, detalle de venta en Informes)
- **Relacionada:** 18 (Ticket), 34 (Ofertas y combos)

## Qué

Hoy el ticket sale correcto en el total, pero no se lee bien cuando hay ofertas. El caso que lo mostró fue un combo: el descuento va repartido dentro del subtotal de cada línea, y entonces `2 × $ 4.100,00` aparece al lado de `$ 4.939,76`. El cliente no puede verificar la cuenta.

## Cómo queda

- Cada línea muestra cantidad × precio y su subtotal **a precio de lista**, sin decimales raros.
- Debajo de la línea (o al final del ticket) aparece una línea aparte por cada oferta aplicada, con el ahorro en negativo.

```
2 × $ 4.100,00                $ 8.200,00
1 × $ 25.000,00              $ 25.000,00
Combo 2 Coca + Fernet        - $ 13.200,00
Total                        $ 20.000,00
```

- Descuento de caja y «Al costo» se muestran igual: una línea de descuento, no un precio cambiado.
- «Llevás 3, pagás 2» muestra la línea completa y una línea «Promo 3x2 − $ …».
- El total no cambia respecto de hoy.
- Preview e impresión salen iguales en POS-58, POS-80 y A4.

## Qué hay que decidir al implementar

- Guardar el ahorro de cada oferta como línea propia de la venta, o calcularlo al armar el ticket. Lo primero es más claro para Informes y para anular una venta.
- Qué pasa con las ventas viejas, que tienen el descuento adentro del subtotal. Se dejan como están.

## Criterios de aceptación

- [ ] Un combo se ve como líneas a precio de lista y una línea de ahorro con el nombre del combo.
- [ ] Una oferta «llevás N, pagás M» y una de precio por cantidad se ven con su ahorro en línea aparte.
- [ ] El descuento de caja y el cobro al costo se ven como línea de descuento.
- [ ] Cantidad × precio coincide con el subtotal en todas las líneas.
- [ ] El total y los medios de pago no cambian.
- [ ] El detalle de la venta en Informes usa la misma presentación que el ticket.
- [ ] Las ventas anteriores se siguen viendo bien.
- [ ] Test xUnit de `ServicioTicket` con combo, oferta de unidades, descuento de caja y venta sin ofertas.

## Fuera

- Impresión directa a impresora térmica (ESC/POS).
- Logo o texto personalizado del local en el ticket.
