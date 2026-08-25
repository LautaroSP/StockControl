# Consulta de cajas cerradas

- **Roles:** empleado · dueño
- **Tipo:** historial / consulta
- **Relacionada:** el cierre sigue en [09](09-cierre-de-caja.md); los puestos en [24](24-puestos-de-caja.md). Esta tarjeta es **ver y filtrar**, no cerrar.

## Qué

Pantalla (o sección propia) para **consultar** las cajas ya cerradas del local. No alcanza el listado corto al pie de Informes: hace falta buscar por persona y por medio de pago.

## Filtros

- **Quién cerró** (persona / usuario).
- **Medio de pago** (Efectivo, Mercado Pago, etc.): muestra cierres que tengan ese medio en el desglose.
- Rango de fechas (desde–hasta), default razonable (ej. mes actual).

## Listado y detalle

Cada fila: nro de caja, fecha, quién cerró, tickets, total.

Click → desglose (medios y/o usuarios según cómo se cerró; ver 09 / 24).

## Criterios de aceptación

- [ ] Empleado y dueño pueden consultar las cajas del **local en el que están**.
- [ ] Filtro por persona (quién cerró).
- [ ] Filtro por medio de pago.
- [ ] Filtro por rango de fechas.
- [ ] Listado: nro, fecha, quién, tickets, total.
- [ ] Click abre el desglose de esa caja.
- [ ] Sin resultados: mensaje claro, no tabla vacía muda.
- [ ] Entrada visible en el nav (o desde Informes) sin depender de scrollear el listado de ventas.

## Fuera

Cerrar caja (09). Elegir puesto al vender (24). Estadísticas del mes (17). Anular / reimprimir tickets (10).
