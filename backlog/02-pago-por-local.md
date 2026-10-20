# Pago por local

- **Roles:** admin (cobra) · dueño (paga)
- **Tipo:** negocio
- **Cómo se cobra:** tarjeta 22 ($ 14.999, QR MP o ventas)

## Qué

La unidad de cobro es el **local**, no el dueño ni la cantidad de empleados.

Jorge con 3 locales = 3 × $ 14.999. Puede tener 20 empleados en un solo local y paga uno.

Solo **Argentina**. Precio de lista: **$ 14.999 / mes**.

## Estados de un local

- **trial** — 7 días, limitado (ver 22)
- **al día** — pagó; vence el mismo día del mes siguiente, a la misma hora del pago (hora argentina). Si ese día no existe, el último del mes.
- **por vencer** — faltan 5 días o menos; aviso arriba de la app
- **vencido** — desde la hora exacta del vencimiento. No cobra hasta pagar o hasta que admin marque el pago. Puede cerrar caja y consultar informes y productos.

## Criterios de aceptación

- [x] Cada local tiene estado y fecha de vencimiento.
- [x] Local vencido (y trial vencido): empleados y dueño no operan caja; mensaje + pagar / contactar ventas.
- [x] El dueño ve cuántos locales tiene, estado y cuándo vence cada uno.
- [x] Empleados ilimitados: no cambian el precio.

El resumen del admin (locales pagos × $ 14.999) pasó a la tarjeta [03](03-admin-alta-locales.md).
