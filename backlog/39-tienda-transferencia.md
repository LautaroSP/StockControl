# Tienda online: pago por transferencia

- **Roles:** comprador (transfiere) · dueño / socio (confirman) · admin (ve la gestión pendiente)
- **Tipo:** medio de pago manual
- **Depende de:** 37 Carrito y pedido, 38 Mercado Pago (reglas de gestión), 22 Cobro del abono

## Qué

El comprador transfiere al CBU o alias del local y el local confirma a mano.

## Reglas

- El dueño carga CBU/alias y titular en Configuración. Sin esos datos, el medio no se ofrece.
- El pedido queda **A confirmar transferencia**; el stock sigue reservado.
- El **dueño elige el plazo** para confirmar (en horas) o “sin vencimiento”. Vencido: se cancela y se libera el stock.
- La gestión no pasa por vos: queda como **deuda del local** y se suma al próximo abono (22).
- Cancelado antes de confirmar: no genera deuda.

## Criterios de aceptación

- [ ] Configuración guarda CBU/alias; sin ellos no se ofrece transferencia.
- [ ] El pedido queda A confirmar y reserva stock.
- [ ] Vence según el plazo del dueño (o nunca) y libera el stock.
- [ ] Confirmar la transferencia pasa el pedido a Pagado.
- [ ] La gestión confirmada se suma al próximo abono del local; el admin ve lo pendiente.
- [ ] Tests xUnit: vencimiento según plazo, sin vencimiento, liberación de stock, gestión sumada al abono, cancelado sin deuda.
