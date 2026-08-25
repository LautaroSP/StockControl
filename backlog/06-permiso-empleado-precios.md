# Permiso: empleado puede modificar precios

- **Roles:** dueño (configura) · empleado (si está prendido)
- **Tipo:** config por local
- **Default:** apagado

## Qué

Por defecto el empleado **no** cambia precios de lista. Solo vende.

Si el dueño lo habilita en ese local, el empleado puede cambiar el precio del producto (ficha / lista).

No es lo mismo que el precio libre del **producto sector** o del **genérico** en el ticket: eso es de la venta, no de la lista.

## Criterios de aceptación

- [ ] Config del local: checkbox “El empleado puede modificar precios de lista”. Off de fábrica.
- [ ] Off: el empleado no ve campos de precio/costo editables en productos.
- [ ] On: puede editar **precio de venta** del producto. Costo y factor de ganancia siguen siendo del dueño.
- [ ] El permiso es por local (en Caballito sí, en Almagro no).
- [ ] Queda registro de quién cambió el precio (usuario + fecha).
