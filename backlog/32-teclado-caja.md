# Teclado de la caja

- **Roles:** empleado, dueño
- **Tipo:** UI

## Qué

En la caja, el lector de código es el lugar donde vuelve el foco. Si minimizás y volvés, o si pasan 10 segundos sin tocar nada, el cursor queda ahí. El cobro se recorre con el teclado, igual que en la caja de escritorio, y al terminar el foco vuelve al lector.

Mientras haya un diálogo abierto (precio de sector, confirmar, genérico, cobrar al costo), el foco no se lo roba el lector.

El `+` es el de la caja de escritorio (`Keys.Add`, el más del teclado numérico). También vale el `+` de la fila de números.

En el escritorio, Ctrl+P abre los carteles. En la web no: el navegador usa esa tecla para imprimir.

## Criterios de aceptación

- [x] Al volver a la ventana (minimizar y restaurar, o cambiar de aplicación) y estar en la caja, el foco queda en el lector de código.
- [x] Si pasan 10 segundos sin teclear ni hacer clic, y la pantalla es la caja, el foco vuelve al lector de código.
- [x] Enter en el lector agrega el producto y el foco queda en el lector.
- [x] Desde el lector, + pasa al medio de pago. Desde el medio, + deja el foco en Cobrar.
- [x] Enter en el descuento lo confirma y vuelve al lector. Al salir del descuento sin Enter, el foco pasa al medio de pago.
- [x] Al cobrar (y si el cobro espera un QR, cuando ese cobro termina), el foco vuelve al lector.
- [x] F1 a F4 siguen cambiando de carrito, y al cambiar el foco vuelve al lector.
