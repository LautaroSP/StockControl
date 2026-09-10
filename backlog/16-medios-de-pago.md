# Medios de pago

- **Roles:** dueño (ABM) · empleado (elige al cobrar)
- **Tipo:** caja

## Qué

Cada local tiene sus medios (efectivo, MP, transferencia, etc.). El empleado los usa; el dueño los carga.

## Criterios de aceptación

- [x] Dueño: alta de medio de pago para ese local.
- [x] Empleado no crea medios (evita basura en el cierre). Si hace falta, lo pide al dueño.
- [x] Pago múltiple: varios medios cuyos montos suman el total del ticket.
- [x] El cierre de caja parte por estos medios (tarjeta 09).
- [x] No se puede borrar un medio con ventas históricas; se desactiva para no usarlo más.
- [x] “Mercado Pago QR” y “Posnet” pueden existir como medios. El QR integrado es la tarjeta 21 (última). Hasta entonces, Posnet y MP se marcan a mano (el cajero cobra afuera y confirma).
