# Tienda: pedido mínimo

- **Roles:** dueño y socio (configuran) · comprador (lo ve en el carrito)
- **Tipo:** configuración de la tienda + checkout
- **Depende de:** 37 Tienda: carrito y pedido, 50 Tienda: panel

## Qué

En Tienda › Configuración el dueño fija un importe mínimo para tomar un pedido online. Si es 0, no hay mínimo. El carrito avisa cuánto falta y no deja confirmar hasta llegar.

## Reglas

- Importe entero en pesos, de 0 a 1.000.000. Arranca en 0 (sin mínimo).
- Se compara con el subtotal de productos que cotiza el servidor. El envío (40) y la gestión (38) no suman.
- Vale igual para retiro y para envío, y para cualquier medio de pago.
- El servidor valida al cotizar y al crear el pedido. El importe que manda el navegador no se usa.
- Cambiar el mínimo no toca los pedidos ya creados.
- El catálogo muestra «Pedido mínimo $ X» cerca del carrito cuando es mayor a 0.
- Dueño y socio. El empleado no lo configura.

## Criterios de aceptación

- [ ] Tienda › Configuración guarda el mínimo; 0 lo deja sin mínimo.
- [ ] El catálogo y el carrito muestran el mínimo cuando es mayor a 0.
- [ ] El carrito dice cuánto falta y no deja confirmar por debajo del mínimo.
- [ ] Crear un pedido por debajo del mínimo se rechaza en el servidor.
- [ ] Envío y gestión no cuentan para llegar al mínimo.
- [ ] Test del dominio: mínimo 0, subtotal debajo, igual y arriba del mínimo, envío y gestión que no suman, importe fuera de rango.

## Fuera de v1

- Un mínimo distinto para envío y para retiro.
