# Mejorar los tickets

- **Roles:** dueño · empleado (imprime) · cliente (lo recibe)
- **Tipo:** web (`ServicioTicket`, `ticket.component.ts`, detalle de venta en Informes)
- **Relacionada:** 18 (Ticket), 34 (Ofertas y combos)

## Qué

Hoy el ticket sale correcto en el total, pero no se lee bien cuando hay ofertas. El caso que lo mostró fue un combo: el descuento va repartido dentro del subtotal de cada línea, y entonces `2 × $ 4.100,00` aparece al lado de `$ 4.939,76`. El cliente no puede verificar la cuenta.

Además, el ticket impreso se ve pobre: sin separadores, importes sin alinear, total poco destacado y el código de barras ocupando lugar.

## Cómo queda

- Cada línea muestra cantidad × precio y su subtotal **a precio de lista**, sin decimales raros.
- Debajo de la línea (o al final del ticket) aparece una línea aparte por cada oferta aplicada, con el ahorro en negativo.

```
          Lo de Pepe
     26/10/2026 11:40 · Caja 1
--------------------------------
Coca 1.75 lt
  2 × $ 4.100,00        $ 8.200,00
Fernet 750
  1 × $ 25.000,00      $ 25.000,00
--------------------------------
Combo 2 Coca + Fernet −$ 13.200,00
--------------------------------
TOTAL                 $ 20.000,00
--------------------------------
Efectivo              $ 20.000,00
3 ítems
    ¡Gracias por su compra!
 Ticket no válido como factura
```

- El ahorro de una oferta por producto va debajo de su línea. Combos y descuento de caja van al final, antes del total.
- El descuento de caja es una línea de descuento, no un precio cambiado.
- «Al costo» muestra directo el precio cobrado, sin línea de ahorro, para no dejar ver el margen.
- «Llevás 3, pagás 2» muestra la línea completa y una línea «Promo 3x2 − $ …».
- El total no cambia respecto de hoy.

## Cómo se imprime

- **Encabezado:** nombre del local centrado y más grande; fecha, hora y puesto de caja; separador.
- **Líneas:** nombre del producto en un renglón (con «…» si no entra en POS-58), sin código de barras. Debajo, cantidad × precio a la izquierda y subtotal a la derecha, importes encolumnados.
- **Ahorros:** con sangría y signo «−», para que no se confundan con productos.
- **Total:** entre separadores, «TOTAL» a la izquierda e importe a la derecha, más grande y en negrita.
- **Medios de pago:** sección propia, cada medio con su importe alineado.
- **Pie:** cantidad de ítems, «¡Gracias por su compra!» y, abajo de todo, **«Ticket no válido como factura»**.
- **Por formato:** letra más chica en POS-58, normal en POS-80 y más grande en A4; márgenes para que no se corte en la térmica.
- Solo negro sobre blanco, sin grises (las térmicas los imprimen mal).
- Preview e impresión salen iguales en POS-58, POS-80 y A4.

## Cómo se guarda

- La línea de la venta (y la de cuenta pendiente) suma `PrecioBase`, `AhorroOferta`, `AhorroDescuento`, `AhorroCombo` y `NombreCombo`. `PrecioBase × Cantidad − ahorros = SubTotal`. `PrecioBase` es el de lista; con «Al costo», el costo; en sector y genérico, el de caja. `SubTotal` sigue siendo lo cobrado: Estadísticas, cierre y anular no cambian. Migración `AhorrosVenta`.
- El combo ya no pisa `DescripcionOferta` con «Combo»: la oferta de la línea se conserva.
- El ahorro no se guarda como línea de venta negativa (rompería margen y top de productos).
- `ServicioTicket.Armar` (dominio) arma las líneas y los ahorros; la API devuelve el ticket armado y Angular solo lo dibuja.
- Ventas viejas (sin `PrecioLista`): se ven como hoy.

## Criterios de aceptación

- [x] Un combo se ve como líneas a precio de lista y una línea de ahorro con el nombre del combo.
- [x] Una oferta «llevás N, pagás M» y una de precio por cantidad se ven con su ahorro en línea aparte.
- [x] El descuento de caja se ve como línea de descuento. El cobro al costo muestra el precio cobrado, sin línea de ahorro.
- [x] Cantidad × precio coincide con el subtotal en todas las líneas.
- [x] El total y los medios de pago no cambian.
- [x] El detalle de la venta en Informes usa la misma presentación que el ticket.
- [x] Las ventas anteriores se siguen viendo bien.
- [ ] El ticket impreso tiene encabezado, separadores, importes alineados, total destacado y medios de pago en sección propia, en POS-58, POS-80 y A4.
- [x] El código de barras no aparece en el ticket.
- [x] Abajo de todo dice «Ticket no válido como factura».
- [x] Test xUnit de `ServicioTicket` con combo, oferta de unidades, descuento de caja y venta sin ofertas.

## Fuera

- Impresión directa a impresora térmica (ESC/POS).
- Logo o texto personalizado del local en el ticket.
