# Cierre de caja

- **Roles:** empleado · dueño
- **Tipo:** caja del día

## Qué

Cerrar la caja del local. El resumen **no** es un solo número: se parte por medio de pago, más total, más **nro de caja**.

## Qué se guarda en cada cierre

Un cierre = un número de caja correlativo **por local** (Caja 1, Caja 2, …).

Filas:

| Nro caja | Medio de pago | Cant. ventas | Total |
|---|---|---|---|
| 14 | Efectivo | 22 | $ 92.100 |
| 14 | Mercado Pago | 18 | $ 94.300 |
| 14 | Transferencia | 7 | $ 12.000 |
| 14 | **Total** | 47 | **$ 198.400** |

También: fecha/hora, quién cerró, local.

## Criterios de aceptación

- [ ] Empleado y dueño pueden cerrar la caja del **local en el que están**.
- [ ] Confirmación antes de cerrar.
- [ ] Si no hay ventas abiertas para ese período, avisa y no genera cierre vacío.
- [ ] Desglose por cada medio de pago usado + fila Total.
- [ ] Nro de caja correlativo por local (no se reinicia por empleado; sí es de esa sucursal).
- [ ] Se puede cerrar el día de hoy o un día anterior (por si se olvidaron).
- [ ] Las ventas ya incluidas en un cierre no vuelven a entrar en el siguiente.
- [ ] Listado de cajas cerradas: nro, fecha, quién, total. Click ve el desglose.
