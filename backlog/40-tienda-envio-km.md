# Tienda online: envío por km

- **Roles:** dueño (configura) · comprador (elige envío o retiro)
- **Tipo:** costo de envío en el checkout
- **Depende de:** 37 Carrito y pedido

## Qué

El local puede ofrecer envío con precio por distancia. Siempre queda la opción de **retiro** sin costo. También puede dejar el envío **gratis siempre**.

## Reglas

- La ubicación del local es la que el dueño ya marcó en el mapa de la 29. Sin ubicación, no se ofrece envío.
- La casilla de envíos más un **precio por km** mayor a 0, o la opción de envío gratis siempre. Opcionales: envío mínimo, radio máximo (hasta 500 km) y envío gratis desde cierto monto del subtotal.
- Envío gratis siempre: importe 0 dentro del radio, sin precio por km. El mínimo no aplica. Fuera del radio, solo retiro.
- El comprador marca su dirección. Fuera del radio, solo retira.
- Los kilómetros salen de OpenRouteService (recorrido en auto). Si no hay clave, no responde o se agota el cupo, se usa la **línea recta**. El checkout y el pedido dicen cuál se usó. La clave va en `OpenRouteService__Clave`.
- El envío se calcula en el servidor y se muestra como línea aparte. El importe del cliente no se usa.
- La gestión (38) es la misma con envío o sin envío: se calcula sobre el subtotal de productos.
- Envío sin configurar: la tienda solo ofrece retiro.
- Al entregar, la venta del local incluye el envío congelado. La gestión no entra en la venta.

## Criterios de aceptación

- [x] Configuración guarda precio por km, mínimo, radio y gratis desde; usa la ubicación de la 29.
- [x] El dueño puede dejar el envío gratis siempre, sin precio por km.
- [x] El checkout calcula el envío en el servidor y lo muestra aparte.
- [x] Fuera del radio solo se puede retirar.
- [x] Se aplica el mínimo y el envío gratis.
- [x] Si OpenRouteService no responde, el envío sale en línea recta y se avisa.
- [x] Tests xUnit: cálculo por km, mínimo, radio máximo, gratis desde un monto, gratis siempre, sin configurar = solo retiro, línea recta cuando no hay recorrido.

## Fuera de v1

- Integración con correos o mensajerías.
