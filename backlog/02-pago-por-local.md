# Pago por local

- **Roles:** admin (cobra) · dueño (paga)
- **Tipo:** negocio
- **Cómo se cobra:** tarjeta 22 ($ 9.999, QR MP o ventas)

## Qué

La unidad de cobro es el **local**, no el dueño ni la cantidad de empleados.

Jorge con 3 locales = 3 × $ 9.999. Puede tener 20 empleados en un solo local y paga uno.

Solo **Argentina**. Precio de lista: **$ 9.999 / mes** (no $ 10.000).

## Estados de un local

- **trial** — 7 días, limitado (ver 22)
- **al día** — pagó; vence en fecha
- **por vencer** — avisos
- **vencido** — no opera caja hasta pagar o hasta que admin marque el pago

## Criterios de aceptación

- [ ] Cada local tiene estado y fecha de vencimiento.
- [ ] Local vencido (y trial vencido): empleados y dueño no operan caja; mensaje + pagar / contactar ventas.
- [ ] El dueño ve cuántos locales tiene, estado y cuándo vence cada uno.
- [ ] Empleados ilimitados: no cambian el precio.
- [ ] Admin ve el mes ≈ locales pagos × $ 9.999 (los trial no cuentan como cobrado).
