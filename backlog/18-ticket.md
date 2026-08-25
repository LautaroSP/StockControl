# Ticket (impresión)

- **Roles:** dueño (config) · empleado / dueño (imprimir al cobrar o reimprimir)
- **Tipo:** config + mostrador

## Qué

Hoy el escritorio imprime solo térmica ESC/POS. En web: elegir **POS** o **A4**, y si imprime por defecto.

La nube no habla con la impresora: un agente chico en la PC del mostrador (o diálogo de impresión del navegador en A4).

## Criterios de aceptación

- [ ] Config del local: formato POS (58/80 mm) o A4.
- [ ] Config: “imprimir ticket al cobrar” default on/off (el checkbox de caja lo puede pisar en esa venta).
- [ ] El ticket lleva nombre del local, fecha/hora, ítems, cantidades, subtotales, total, medio(s) de pago.
- [ ] Si hubo descuento o cobro al costo, que se note.
- [ ] Reimpresión desde informes (tarjeta 10).
- [ ] Si no hay agente / impresora, la venta **igual se guarda** y se avisa.
