# Plan de Desarrollo - Requisitos Matias Sabio

**Fecha:** 25/06/2026
**Estado:** Pendiente de implementacion

---

## Resumen de requisitos

| # | Requisito | Estado | Prioridad |
|---|-----------|--------|-----------|
| 1 | Resumen mensual de caja | NO HECHO | Media |
| 2 | Imprimir ticket desde informes | NO HECHO | Alta |
| 3 | Fix bug grupos (precio con decimales) | BUG CONFIRMADO | Alta |
| 4 | Precio amigo | DESCARTADO (ya funciona con costo + descuento) | N/A |

---

## Tarea 1 — Fix bug grupos (precio con decimales)

**Prioridad:** ALTA
**Estimacion:** 30 min

### Problema

En `frmProducto.cs:428-429`, al asignar un grupo desde el producto, se formatea el precio con `"#0.00"` en el TextBox. Al guardar (`AsignarValoresAProd`), se lee ese valor y se guarda con `Math.Round()`, lo que puede generar decimales distintos al precio manual del grupo.

### Archivos a modificar

- `StockControl/frmProducto.cs` — Corregir `AsignarValoresAProd()` y/o `btnBuscarGrupo_Click()`

### Cambios

1. Cuando `_prod.IdGrupoProducto > 0`, al guardar, traer el precio/costo directamente del `GrupoProductos` (via `GrupoRepository.BuscarPorId`) en vez de leer de los TextBox
2. O alternativamente, usar `ActualizarGrupo()` del repository (que solo actualiza `IdGrupoProducto`) en vez de `Actualizar()` (que pisa precio/costo)

---

## Tarea 2 — Imprimir ticket desde informes

**Prioridad:** ALTA
**Estimacion:** 1 hora

### Problema

Para reimprimir un ticket hay que copiar la venta al carrito, volver a cobrar, y borrar la duplicada.

### Archivos a modificar

- `StockControl/frmInformeVentas.cs` / `StockControl/frmInformeVentas.Designer.cs` — Agregar boton `btnImprimirTicket`
- Posiblemente `StockControl/TicketPrinter.cs` — Adaptar constructor para recibir `InformeVentaDetalle`

### Cambios

1. Agregar boton "Imprimir Ticket" en el tab "Informes de Venta" de `frmInformeVentas`
2. Handler: obtener `InformeVenta` seleccionado -> cargar `InformeVentaDetalle` -> convertir a `List<ItemSeleccionado>` -> crear `TicketPrinter` -> `PrintTicketFinal()` a impresora por defecto
3. Reutilizar la logica de conversion que ya existe en `btnCopiarTicket_Click` (lineas 294-348)

---

## Tarea 3 — Resumen mensual de caja

**Prioridad:** MEDIA
**Estimacion:** 2-3 horas

### Necesidad

Saber cuanto se hizo en el mes por metodo de pago (efectivo, Mercado Pago, tarjeta, etc.) para poder facturar.

### Archivos a modificar

- `StockControl/frmInformeVentas.cs` / `StockControl/frmInformeVentas.Designer.cs` — Nueva tab "Resumen Mensual"
- `StockControl/Repository/InformeVentaRepository.cs` — Nuevo metodo `ListarResumenMensual(int mes, int anio)`
- `StockControl/Repository/InformeVentaRepository.cs` — Nuevo metodo `ListarVentasPorRango(DateTime desde, DateTime hasta)` (para obtener ventas sin cerrar caja)

### Cambios

1. Agregar tab "Resumen Mensual" en `frmInformeVentas` con:
   - `DateTimePicker` o ComboBox para seleccionar mes/anio
   - `DataGridView` con resumen agrupado por `MetodoPago` y total
   - Label con total general del mes
2. Query: consultar tabla `Cajas` filtrando por mes seleccionado, agrupar por `MetodoPago`, sumar `Total`
3. Agregar fila "Total" con la suma de todos los metodos
4. Considerar: incluir tambien ventas del mes que **no tienen caja cerrada** (consultando `InformeVentas` directamente) para que no falte nada

---

## Orden de ejecucion

| # | Tarea | Tiempo est. |
|---|-------|-------------|
| 1 | Fix bug grupos | 30 min |
| 2 | Imprimir ticket desde informes | 1 hora |
| 3 | Resumen mensual de caja | 2-3 horas |
| | **Total** | **~4-5 horas** |
