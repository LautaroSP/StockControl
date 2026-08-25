# Configuración del local

- **Roles:** dueño
- **Tipo:** config (una pantalla)

## Qué

Todo lo configurable **por sucursal**. El empleado no entra.

## Campos

- Nombre del local (sale en el ticket).
- Factor de ganancia e IVA. Al cambiar: ofrecer actualizar precios de productos sin ganancia propia.
- Stock rígido sí/no (tarjeta 12).
- Umbral de stock bajo.
- Empleado puede modificar precios sí/no (tarjeta 06).
- Ticket: POS o A4; imprimir al cobrar default (tarjeta 18).
- Plantillas de cartel (tarjeta 19).
- Vincular Mercado Pago para QR (tarjeta 21, última).

## Criterios de aceptación

- [ ] Cada local tiene su config. Cambiar Almagro no toca Caballito.
- [ ] Recalcular precios es opt-in (Sí/No), no silencioso.
- [ ] Admin no pisa esta config; solo el estado del abono del local.
