# Vista de cliente del carrito (escritorio)

- **Roles:** quien opera la caja (la abre) · cliente (la mira)
- **Tipo:** escritorio (WinForms en `StockControl/`)
- **Igual en web:** 45

## Qué

La misma idea que la 45, en el programa de escritorio. Un botón **Vista de cliente** arriba del carrito en `StockMain` abre una ventana aparte. Si la computadora tiene un segundo monitor, la ventana se abre ahí, maximizada y sin bordes. Si no, se abre como ventana normal que se puede mover.

La ventana se actualiza **al instante** cuando cambia el carrito (`CarritoSesion`). Corre en el mismo programa, así que no usa red ni base de datos.

## Qué muestra

- Nombre del local.
- Las líneas del carrito activo: producto, cantidad, precio y subtotal, con el último agregado resaltado.
- Descuento del ticket y total grande.
- Carrito vacío: pantalla de espera con el nombre del local.
- Al cobrar: «Gracias» con el total unos segundos, después espera.
- Nunca muestra costo ni «Cobrar al costo».

## Criterios de aceptación

- [ ] Botón **Vista de cliente** arriba del carrito en `StockMain`.
- [ ] Con dos monitores, la ventana se abre en el segundo, maximizada; recuerda en cuál la dejaste.
- [ ] Se actualiza al instante al agregar, cambiar cantidades, sacar, aplicar descuento y cambiar de carrito.
- [ ] Carrito vacío: espera con el nombre del local. Al cobrar: «Gracias» y total, después espera.
- [ ] No muestra costo ni otros datos internos.
- [ ] Cerrar la vista no afecta la caja; cerrar la caja cierra la vista.
- [ ] Letra grande, legible a un metro y medio.

## Fuera

- Usar la vista desde otra computadora de la red.
- Publicidad en la pantalla de espera.
