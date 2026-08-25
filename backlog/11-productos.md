# Productos

- **Roles:** dueño
- **Tipo:** catálogo

## Qué

Alta, edición y baja de productos del **local actual**. Código, nombre, costo, precio, stock, grupo, sector, ganancia propia.

## Criterios de aceptación

- [ ] Crear / editar / eliminar producto (código único por local).
- [ ] Precio se calcula con factor de ganancia × IVA del local, salvo ganancia propia o grupo.
- [ ] Ganancia propia por producto: override del factor global.
- [ ] Si pertenece a un grupo, precio y costo salen del grupo (no se editan a mano en la ficha).
- [ ] Sector: ver tarjeta 13. No se mezcla con grupo.
- [ ] Fecha de última modificación visible.
- [ ] Empleado no entra al ABM. Excepción: permiso de precios (tarjeta 06) solo toca **precio de venta**.
- [ ] Código ya existente: no duplicar; avisar.
