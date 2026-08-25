# Roles y permisos

- **Roles:** admin · dueño · empleado
- **Tipo:** base (sin esto no se arma el resto)

## Qué

Tres roles. El dueño hereda todo lo del empleado.

## Matriz

| Acción | Empleado | Dueño | Admin |
|---|---|---|---|
| Vender / carritos / ticket | sí | sí | no |
| Cerrar caja | sí | sí | no |
| Sumar stock | sí | sí | no |
| Modificar precios | no, salvo config | sí | no |
| Alta / edición de productos | no | sí | no |
| Grupos, sector, medios de pago | no | sí | no |
| Ver estadísticas | no | sí | no |
| Alta de empleados | no | sí | no |
| Alta de locales | no | no | sí |
| Contratar otro local (pago) | no | sí (pide / paga) | confirma / cobra |
| Panel de cobros del servicio | no | no | sí |
| Ver locales de otros clientes | no | no | sí |

## Criterios de aceptación

- [ ] Login con usuario y contraseña. Elige **local** entre los que tiene asignados. Elegir **caja** (puesto) es la tarjeta 24.
- [ ] Un empleado no ve costo, ganancia, estadísticas ni config del local.
- [ ] Un dueño ve todos sus locales; no ve locales de otro dueño.
- [ ] Admin no entra a cobrar en la caja del cliente (v1). Opera el servicio.
- [ ] Si un usuario no tiene permiso, la UI no muestra la acción (no solo error al guardar).
