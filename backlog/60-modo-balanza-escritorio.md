# Modo balanza (escritorio)

- **Roles:** quien opera la caja (lo usa) · quien carga productos (precio por kilo)
- **Tipo:** escritorio (WinForms en `StockControl/`)
- **Depende de:** 59 Modo balanza (web)
- **Igual en web:** 59

## Qué

La misma pantalla de balanza que la 59, en el programa de escritorio. El botón **Modo balanza** en `StockMain` abre una ventana aparte (`frmBalanza`) con las tres columnas de la 59: buscador y listado a la izquierda, **Pesando…** y **Total:** al centro, carrito con **Aceptar** y **Salir** a la derecha.

## Reglas

- La pantalla, el cálculo y el ritmo son los de la 59: kilos por precio por kilo, redondeado a centavos; peso mayor a 0 y hasta 100 kg; clic o Enter agrega la pesada al carrito de la balanza; Aceptar la pasa al carrito abierto; Salir (o Esc) cierra sin pasar nada y pregunta si había pesadas.
- La ventana abre maximizada sobre `StockMain`, modal. Al aceptar, la venta sigue en el carrito abierto con el foco en el lector.
- El producto (común o sector) guarda el precio por kilo en la base del escritorio y se carga en `frmProducto`. En 0, o sin cargar, no aparece en el listado.
- La línea del carrito guarda los kilos. El común descuenta stock; el sector no. El precio por kilo no lo pisa el descuento ni cobrar al costo.
- La balanza digital se lee por el puerto serie, a 9600. La primera vez se elige el puerto y queda recordado en la configuración del escritorio. Sin balanza conectada, la ventana avisa y sigue con el lector.
- El lector se comporta como en la 59: un código del listado lo deja elegido; un número suelto es el peso; un producto común avisa y no se agrega.

## Criterios de aceptación

- [x] Botón **Modo balanza** en la venta que abre la ventana de tres columnas.
- [x] El producto (común o sector) tiene precio por kilo. En 0 no sale en el listado.
- [x] Total muestra el importe según el peso. Clic o Enter agrega la pesada al carrito de la balanza.
- [x] **Aceptar** pasa las pesadas al carrito abierto, con kilos e importe. **Salir** no pasa nada.
- [x] El puerto serie se elige una vez y queda recordado. Sin balanza, se usa el lector.
- [x] Sin test de dominio: es WinForms. El cálculo se prueba en la 59.

## Fuera de v1

- Lo mismo que quedó fuera en la 59.
