# Recibir orden de proveedor

- **Roles:** dueño · socio
- **Tipo:** compras / stock
- **Pisa:** [26](26-proveedores.md) (la orden) · [12](12-stock.md) (sumar cantidad)
- **#orden:** 23 (justo después de generar la orden)

## Qué

La mercadería llegó. Se marca la orden como **recibida** y se **suma stock** de cada producto común de las líneas.

Generar la orden (26) **no** mueve stock. Recibir sí.

## Flujo

En la ficha del proveedor (o en la orden):

1. Órdenes **pedidas** (aún no recibidas): botón **Marcar recibida**.
2. Tabla: producto, cantidad pedida, cantidad recibida (editable; default = pedida).
3. Confirmación.
4. Suma stock de cada línea (producto común). Queda quién y cuándo.
5. La orden pasa a **Recibida**. No se vuelve a recibir.

Si vino de menos o de más: se ajusta “cantidad recibida” antes de confirmar. El pedido original no se pisa (queda el histórico de lo pedido vs lo que entró).

## Criterios de aceptación

- [ ] Solo dueño y socio (misma pestaña que 26). El empleado sigue sumando stock a mano (12).
- [ ] Generar orden no mueve stock. Recibir sí.
- [ ] Confirmación antes de sumar.
- [ ] Cantidad recibida editable; default = pedida.
- [ ] Suma stock solo de productos comunes. Sector y genérico no (no deberían estar en la orden).
- [ ] Una orden recibida no se puede recibir de nuevo.
- [ ] Queda registro: usuario, fecha, cantidades pedidas y recibidas.
- [ ] Stock es del **local actual**.

## Fuera

- Deshacer recepción (restar stock).
- Recibir en varios tramos (parcial ahora = una sola vez, con las cantidades que indiques).
- Empleado recibe órdenes.
- Cambiar costos al recibir (el costo sigue siendo el del producto / Excel de la 26).
