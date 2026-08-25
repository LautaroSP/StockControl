# Stock

- **Roles:** empleado (sumar / ajustar básico) · dueño (todo + config)
- **Tipo:** inventario

## Qué

El empleado puede **sumar stock** (llegó mercadería). El dueño además configura si el stock es rígido.

Hoy en escritorio se descuenta al vender pero **no** bloquea si hay 0.

## Criterios de aceptación

- [ ] Empleado: ingresar cantidad a un producto (entrada de mercadería). Queda quién y cuándo.
- [ ] Al vender un producto común se descuenta. Sector y genérico no.
- [ ] Config del local: **stock rígido** off/on. Off = se puede vender con 0 (stock queda en 0). On = no cobra si no alcanza.
- [ ] Config: umbral de stock bajo (default 5). Lista y filas en naranja.
- [ ] Anular venta (dueño) devuelve stock.
- [ ] El stock es **por local**. Misma Coca en Almagro y Caballito = dos cantidades.
