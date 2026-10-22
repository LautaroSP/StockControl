# Pasarela de pagos (QR Mercado Pago / Point)

- **Roles:** dueño (vincula) · empleado / dueño (cobra)
- **Tipo:** mostrador
- **Estado:** sí. Probada en el mostrador con QR en pantalla. Point y el QR impreso quedan en el producto; no se escanearon con un lector ni con el papel.

## Qué

Al cobrar, el cliente paga **en el local**. El cajero elige uno de tres caminos, y la plata entra en la cuenta de Mercado Pago **de ese local**.

No es el cobro de tu abono (eso es la tarjeta 22: $ 14.999 a **tu** MP).

Va **antes** de la tienda online. El dueño conecta Mercado Pago **una vez** por local. El mostrador y la tienda (29) usan esa misma cuenta. El spike sigue primero.

## Tres caminos en el mostrador

| | QR impreso (estático) | QR en pantalla (dinámico) | Point |
|---|---|---|---|
| Qué es | El cartel ya está en el mostrador. La caja le carga el importe de esta venta y el cliente escanea el mismo papel. | La caja arma un QR nuevo, solo para esta venta, y lo muestra en pantalla. | El lector del local. El monto lo manda la caja. |
| Integración | Una caja de Mercado Pago por puesto, con QR fijo. Cada cobro es una orden asignada a esa caja. | Orders API, QR dinámico. | Orders API de Point. |
| v1 | Sí. Un QR impreso por puesto. Una sola orden abierta a la vez. | Sí. | Sí. |

Un Posnet de otro banco, sin API, sigue como medio **manual**: el cajero cobra en el aparato y marca el medio en la caja. Eso no espera a Mercado Pago.

## Flujo en la caja

1. Carrito listo. El cajero elige QR impreso, QR en pantalla o Point (todo el total, o una parte si el pago es mixto).
2. StockControl pide a la cuenta de Mercado Pago **de ese local** una orden por ese monto.
3. QR impreso: la orden queda asignada al cartel de ese puesto. QR en pantalla: se dibuja un código nuevo. Point: la orden llega al lector.
4. MP avisa (webhook) o la caja consulta el estado. En el modelo atendido solo cierra la orden con estado cerrado, no un aviso a medias.
5. **Pagado** → se confirma la venta (stock, informe, ticket).
6. El cajero **cancela** o se vence la orden → se saca la orden de esa caja y no hay venta; el carrito sigue. Si no se saca, el próximo escaneo sigue mostrando el importe viejo.

Hasta que MP no confirme, no se descuenta stock.

## Config (dueño)

Cada local vincula **su** cuenta de Mercado Pago (OAuth / credenciales). Jorge no usa tu MP: usa el de Almagro.

Al vincular, por cada puesto se crea (o se elige) una caja de Mercado Pago. Esa caja trae un QR fijo: imagen y PDF para imprimir y pegar. El modo es el atendido (`pdv`), no el suelto: en el suelto el cliente tipea el monto y la caja no se entera.

Esa vinculación la reutiliza la tienda online: Checkout Pro sale con el mismo token. La gestión de la tienda es un importe fijo (29). En el mostrador no se descuenta. El cartel impreso queda en el puesto (un cobro a la vez) y no recibe pedidos de la web.

## Criterios de aceptación (cuando se implemente)

- [x] Spike escrito: app en MP, un local de prueba, una orden QR, un pago real de $ 1.
- [x] Dueño: “Conectar Mercado Pago” por local. Empleado no pisa las keys.
- [x] Al cobrar con el QR impreso: la orden de ese importe queda en el cartel de ese puesto. La pantalla muestra Esperando / Pagado / Cancelado / Vencido, sin dibujar un QR nuevo.
- [x] Un puesto tiene un solo cobro abierto en su QR. Dos puestos son dos carteles.
- [x] Al cobrar con QR en pantalla: QR + total + Esperando / Pagado / Cancelado / Vencido.
- [x] Al cobrar con Point: la caja manda el monto al lector y muestra los mismos estados.
- [x] Empleado puede cancelar la orden y volver al carrito.
- [x] Solo con estado pagado se genera la venta y el nro de ticket.
- [x] Si el local no está vinculado, QR y Point no se ofrecen (o avisan).
- [x] En el cierre de caja, esos cobros cuentan como medio “Mercado Pago” (o el nombre que defina el dueño).
- [x] Un Posnet de otro banco sigue manual: se marca el medio y no se espera una API.

## Spike (hacer antes de codear producto)

- [ ] Crear aplicación en [Tu integraciones](https://www.mercadopago.com.ar/developers) (cuenta del local o una de prueba).
- [ ] Probar **Orders API**: `POST /v1/orders` con `type: "qr"` y `config.qr.mode: "dynamic"`.
- [ ] Armar imagen QR a partir de `qr_data`.
- [ ] Probar una orden al Point de prueba y ver que el lector muestra el mismo monto.
- [ ] Ver cómo llega el aviso de pago (webhook vs polling).
- [ ] Confirmar: una caja de Mercado Pago por puesto de StockControl, y si un QR que el local ya imprimió desde la app se puede reutilizar o hay que pegar el que devuelve la API.
- [ ] Probar el QR fijo: crear la orden, escanear el mismo papel, ver el importe, cancelar y comprobar que el papel vuelve a quedar vacío.
- [ ] Anotar comisiones y qué pasa si el cliente paga de más / de menos (no debería: el QR lleva el monto).

Docs: [QR Code](https://www.mercadopago.com.ar/developers/es/docs/qr-code/overview) y [Point](https://www.mercadopago.com.ar/developers/es/docs/mp-point/create-application).

## Fuera de v1

- Devolución automática por API.
- Posnet de otro banco integrado al aparato. Point de Mercado Pago sí entra en esta tarjeta.
- QR suelto, sin integración: el cliente elige el monto en la app y la caja no registra la venta.
- Usar esta integración para el abono del servicio: eso es la tarjeta 22 (tu MP, no el del local).
