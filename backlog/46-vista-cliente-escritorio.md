# Vista de cliente del carrito (escritorio)

- **Roles:** quien opera la caja (la abre) · cliente (la mira)
- **Tipo:** escritorio (WinForms en `StockControl/`)
- **Igual en web:** 45

## Qué

La misma idea que la 45, en el programa de escritorio. Un botón **Vista de cliente** arriba del carrito en `StockMain` abre una ventana aparte. Siempre es una ventana normal, que se puede mover, achicar, maximizar y cerrar. Si la computadora tiene un segundo monitor, se abre ahí.

La ventana se actualiza **al instante** cuando cambia el carrito (`CarritoSesion`). Corre en el mismo programa, así que no usa red ni base de datos.

## Qué muestra

- Nombre del local.
- Las líneas del carrito activo: producto, cantidad, precio y subtotal, con el último agregado resaltado.
- Descuento del ticket y total grande. El escritorio no tiene ofertas ni combos, así que no muestra ofertas aplicadas.
- Carrito vacío: pantalla de espera con el nombre del local.
- Al cobrar: «Gracias» con el total unos segundos, después espera.
- No muestra el tilde «Cobrar al costo». Si está marcado, muestra el precio y el subtotal que se cobran.

## Criterios de aceptación

- [ ] Botón **Vista de cliente** arriba del carrito en `StockMain`.
- [ ] Con dos monitores, la ventana se abre en el segundo; recuerda en cuál la dejaste. Siempre se puede mover, achicar y cerrar.
- [ ] Se actualiza al instante al agregar, cambiar cantidades, sacar, aplicar descuento y cambiar de carrito.
- [ ] Carrito vacío: espera con el nombre del local. Al cobrar: «Gracias» y total, después espera.
- [ ] Con «Cobrar al costo» muestra el precio y el subtotal cobrados; no muestra el tilde ni otros datos internos.
- [ ] Cerrar la vista no afecta la caja; cerrar la caja cierra la vista.
- [ ] Letra grande, legible a un metro y medio.

## Fuera

- Usar la vista desde otra computadora de la red.
- Publicidad en la pantalla de espera.
