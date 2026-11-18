# Modo balanza (web)

- **Roles:** empleado y dueño (lo usan en la caja) · dueño (carga el precio por kilo)
- **Tipo:** mostrador
- **Depende de:** 07 Venta en caja, 08 Varios carritos, 13 Producto sector
- **Igual en escritorio:** 60

## Qué

En la caja, el botón **Modo balanza** abre una pantalla aparte para pesar. Se elige el producto, se pesa y la pesada va a un carrito propio de la balanza. **Aceptar** pasa todas las pesadas al carrito abierto de la caja; **Salir** vuelve sin pasar nada.

El precio por kilo es una característica de cualquier producto (común o sector). En 0 no entra al modo balanza. El sector sin precio por kilo sigue pidiendo el importe a mano en la caja (13).

## Pantalla

```
┌───────────────┬────────────────────────┬──────────────┐
│ Buscador      │                        │ Carrito      │
│ ┌───────────┐ │                        │  Pan  0,350  │
│ │ Listado   │ │   Pesando… 0,350 kg    │  Queso 0,210 │
│ │ de        │ │                        │              │
│ │ productos │ │   Total: $ 1.225,00    │              │
│ │           │ │                        ├──────────────┤
│ │           │ │                        │ [ Aceptar ]  │
│ └───────────┘ │                        │ [ Salir   ]  │
└───────────────┴────────────────────────┴──────────────┘
```

- **Izquierda:** buscador y, abajo, el listado de productos sector con precio por kilo (nombre y precio por kilo). El buscador filtra por nombre o código.
- **Centro:** **Pesando…** con el peso que llega, en grande, en kilos con 3 decimales. Debajo, **Total:** el importe del producto elegido con ese peso. Sin producto elegido o sin peso válido, el total queda vacío.
- **Derecha:** el carrito de la balanza (producto, kilos e importe; lo último arriba) con su total. Debajo, **Aceptar** y **Salir**.

## Reglas

- El botón está en la caja, junto al lector. Abre la pantalla encima de la caja, que queda como estaba.
- El dueño carga el **precio por kilo** en la ficha del producto (común o sector). En 0, o sin cargar, no aparece en el listado.
- El importe es el peso en kilos por el precio por kilo, redondeado a centavos (2 decimales, como el resto de la caja).
- Peso mayor a 0 y hasta 100 kg. En 0, negativo o por encima de 100 no arma la pesada.
- Con peso válido, clic en un producto, o Enter sobre el elegido, agrega la pesada al carrito de la balanza. La pantalla sigue leyendo para la siguiente. Las flechas mueven la selección del listado.
- Una pesada se saca del carrito de la balanza con su ×. Para corregirla se saca y se pesa de nuevo.
- **Aceptar** pasa todas las pesadas al carrito abierto de la caja (08), cierra la pantalla y el foco vuelve al lector (32). Sin pesadas, Aceptar está apagado.
- **Salir** (o Esc) cierra sin pasar nada. Si hay pesadas, pregunta antes de descartarlas.
- La línea guarda los kilos y el precio por kilo. En el carrito de la caja se ven el peso y el importe.
- La pesada cobra kilos × precio por kilo (sin oferta ni descuento % ni al costo). Si el producto es común, descuenta esos kilos del stock; si es sector, no.
- La balanza digital manda el peso en una línea de texto. Se toma el primer número, con punto o coma decimal. El navegador pide el puerto la primera vez (Web Serial) y lo recuerda en ese navegador. La velocidad arranca en 9600. Sin puerto serie, la pantalla avisa y sigue con el lector.
- El lector escribe en el buscador. Si el código es de un producto del listado, lo deja elegido. Si es un número que no es código del listado, es el peso (la balanza que escribe como teclado). Un código de producto común no se agrega: avisa «No es un producto por kilo».
- Colores solo con las variables del modo oscuro.

## Criterios de aceptación

- [x] Botón **Modo balanza** en la caja que abre la pantalla de tres columnas: buscador y listado, Pesando… y Total, carrito con Aceptar y Salir.
- [x] El listado muestra solo productos con precio por kilo (común o sector), y el buscador lo filtra por nombre o código.
- [x] Con un producto elegido, Total muestra el importe según el peso que llega.
- [x] Clic o Enter agrega la pesada al carrito de la balanza; se puede sacar con ×.
- [x] **Aceptar** pasa todas las pesadas al carrito abierto de la caja y cierra. **Salir** cierra sin pasar nada y pregunta si había pesadas.
- [x] El dueño carga el precio por kilo en común o sector. En 0, el producto no sale en el listado.
- [x] La línea guarda kilos y precio por kilo; el común descuenta stock y el sector no.
- [x] Peso en 0, negativo o mayor a 100 kg no arma la pesada.
- [x] El lector elige un producto del listado o, si manda un número suelto, lo toma como peso. Un producto común avisa y no se agrega.
- [x] Sin puerto serie se puede usar igual con el lector.
- [x] Test del dominio: importe redondeado, peso en 0, negativo y mayor a 100, precio por kilo en 0 fuera del listado, total del carrito de la balanza, texto de la balanza con punto, con coma y sin número, y un código de producto que no se lee como peso.

## Fuera de v1

- Etiqueta de balanza con el producto y el peso adentro del código.
- Protocolos binarios de una marca, y elegir la velocidad del puerto.
- Varias balanzas en el mismo puesto.
- Tara cargada en la pantalla (la hace la balanza).
