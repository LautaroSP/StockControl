# Tienda online: envío por km

- **Roles:** dueño (configura) · comprador (elige envío o retiro)
- **Tipo:** costo de envío en el checkout
- **Depende de:** 37 Carrito y pedido

## Qué

El local puede ofrecer envío con precio por distancia. Siempre queda la opción de **retiro** sin costo.

## Reglas

- La ubicación del local es la que el dueño ya marcó en el mapa de la 29. Sin ubicación, no se ofrece envío.
- El dueño carga el **precio por km**. Opcionales: envío mínimo, radio máximo y envío gratis desde cierto monto.
- El comprador marca su dirección. Fuera del radio, solo retira.
- Distancia en **línea recta** (sin APIs pagas). Distancia por calle: fuera de v1.
- El envío se calcula en el servidor y se muestra como línea aparte.
- La gestión (38) es la misma con envío o sin envío.
- Envío sin configurar: la tienda solo ofrece retiro.

## Criterios de aceptación

- [ ] Configuración guarda precio por km, mínimo, radio y gratis desde; usa la ubicación de la 29.
- [ ] El checkout calcula el envío en el servidor y lo muestra aparte.
- [ ] Fuera del radio solo se puede retirar.
- [ ] Se aplica el mínimo y el envío gratis.
- [ ] Tests xUnit: cálculo por km, mínimo, radio máximo, gratis desde un monto, sin configurar = solo retiro.

## Fuera de v1

- Distancia por calle con API paga.
- Integración con correos o mensajerías.
