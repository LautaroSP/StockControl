# Tienda online: pago por transferencia

- **Roles:** comprador (transfiere) · dueño / socio (confirman) · admin (ve la gestión pendiente)
- **Tipo:** medio de pago manual
- **Depende de:** 37 Carrito y pedido, 38 Mercado Pago (reglas de gestión), 22 Cobro del abono

## Qué

El comprador transfiere al CBU o alias del local y el local confirma a mano.

## Reglas

- El dueño carga CBU/alias y titular en Configuración. Sin esos datos, el medio no se ofrece.
- El pedido queda **A confirmar transferencia**; el stock sigue reservado. No se prepara ni se entrega hasta confirmar.
- El comprador ve CBU o alias y titular solo en la página de su pedido, no en el catálogo.
- Confirman el dueño y el socio (el empleado no ve la plata de la cuenta).
- El **dueño elige el plazo** para confirmar (en horas) o “sin vencimiento”. Se congela en el pedido. Vencido: se cancela, se libera el stock y se avisa al local.
- La gestión no pasa por vos: queda como **deuda del local**. Sumarla al próximo abono (22) y saldarla es la 51.
- Cancelado antes de confirmar: no genera deuda. Cancelar una transferencia confirmada es «ya la devolví» (a mano): tampoco deja deuda.
- Entregar un pedido por transferencia arma la venta con el medio que elige quien entrega.

## Criterios de aceptación

- [x] Configuración guarda CBU/alias; sin ellos no se ofrece transferencia.
- [x] El pedido queda A confirmar y reserva stock.
- [x] Vence según el plazo del dueño (o nunca) y libera el stock.
- [x] Confirmar la transferencia pasa el pedido a Pagado.
- [x] La gestión confirmada queda como deuda del local; el admin ve lo pendiente. Sumarla al próximo abono: 51.
- [x] Migración `TransferenciaTienda`.
- [x] Tests xUnit (`ServicioTransferenciaTiendaTests`): vencimiento según plazo, sin vencimiento, liberación de stock, gestión pendiente, cancelado sin deuda.
- [ ] Probar en el navegador con la base.
