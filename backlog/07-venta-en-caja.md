# Venta en caja

- **Roles:** empleado · dueño
- **Tipo:** mostrador

## Qué

Cobrar: scanner, búsqueda, carrito, medio de pago, descuento, genérico, sector.

## Criterios de aceptación

- [ ] Scanner (el lector escribe + Enter) agrega al carrito del local actual.
- [ ] Doble clic / tap en el listado agrega; en el carrito saca.
- [ ] Código inexistente: ofrecer alta solo al **dueño** (el empleado no crea productos).
- [ ] Producto **genérico**: nombre y precio libres, no mueve stock.
- [ ] Producto **sector**: pide precio en caja; cantidad 1; no mueve stock.
- [ ] Descuento % sobre el **margen** (precio − costo), no sobre el precio entero. No aplica a sector ni genéricos.
- [ ] Cobrar al costo (con confirmación).
- [ ] Un medio de pago, o pago múltiple que sume el total.
- [ ] QR Mercado Pago (generar, esperar, confirmar) es la tarjeta 21; no bloquea esta.
- [ ] Checkbox imprimir ticket (respeta default del local).
- [ ] Al cobrar: guarda la venta, descuenta stock (si aplica), opcional imprime.
- [ ] Cancelar vacía el carrito actual.
