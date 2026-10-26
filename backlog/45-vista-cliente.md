# Vista de cliente del carrito (web)

- **Roles:** dueño, socio, empleado (la abren) · cliente (la mira)
- **Tipo:** mostrador
- **Igual en escritorio:** 46

## Qué

Una segunda pantalla para que el cliente vea lo que se le está cobrando. Arriba del carrito de la caja hay un botón **Vista de cliente** que abre una pestaña nueva. Esa pestaña se puede llevar a un segundo monitor, o a una tablet o pantalla mirando al cliente, y ponerse en pantalla completa.

La vista se actualiza **al instante** cuando cambia el carrito: escanear, sumar, sacar, aplicar descuento, cambiar de carrito.

## Qué muestra

- Nombre del local arriba.
- Las líneas del carrito activo: producto, cantidad, precio y subtotal. El último producto agregado queda resaltado un momento.
- Ofertas aplicadas en la línea («3x2», «20 % off»), descuento del ticket y total grande, legible a un metro y medio.
- **Carrito vacío:** pantalla de espera con el nombre del local.
- **Esperando un pago de Mercado Pago con QR en pantalla:** el QR grande, para que el cliente lo escanee desde ahí.
- **Al cobrar:** «Gracias» con el total y el medio durante unos segundos. Después vuelve a la espera.
- Lo último agregado queda arriba de la lista.
- No muestra el tilde «Cobrar al costo» ni nada del resto de la caja. Si está marcado, muestra el precio y el subtotal que se cobran.

## Cómo se comunica

La caja y la vista corren en el **mismo navegador**, así que se hablan directo (`BroadcastChannel`), sin pasar por el servidor. Hay un canal por local y por puesto de caja: la caja abre `/vista-cliente?caja=N`, así que dos puestos en el mismo navegador no se mezclan. La caja emite un latido cada 2 s; sin latido por unos 8 s, la vista vuelve a la espera. Por eso el cambio es instantáneo y anda aunque el API tarde.

La vista no calcula nada: muestra lo que manda la caja. Si cierran la caja o se pierde la comunicación por más de un rato, la vista muestra la pantalla de espera.

## Criterios de aceptación

- [x] Botón **Vista de cliente** arriba del carrito; abre una pestaña nueva con la vista.
- [x] La vista se actualiza al instante al escanear, cambiar cantidades, sacar, aplicar descuento u oferta y cambiar de carrito (F1–F4 y extras).
- [x] Muestra líneas (lo nuevo arriba), ofertas, descuento y total. Con «Cobrar al costo» muestra el precio cobrado; no muestra el tilde ni otros datos de la caja.
- [x] Carrito vacío: pantalla de espera con el nombre del local.
- [x] Cobro con QR en pantalla de Mercado Pago: el QR se ve en la vista.
- [x] Al cobrar: «Gracias» con total y medio unos segundos; después, espera.
- [x] Sin comunicación con la caja: pasa a espera, sin dejar un carrito viejo a la vista.
- [x] Letra grande, modo oscuro y pantalla completa.
- [x] Una caja abierta en otro puesto no mezcla su carrito con esta vista.
- [x] Sin test de dominio si no hay regla nueva: la vista solo muestra lo que calcula la caja.

## Fuera

- Ver la vista desde otro dispositivo de la red (necesitaría pasar por el servidor).
- Publicidad o promociones rotando en la pantalla de espera.
