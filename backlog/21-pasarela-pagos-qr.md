# Pasarela de pagos (QR Mercado Pago / Posnet)

- **Roles:** dueño (vincula) · empleado / dueño (cobra)
- **Tipo:** mostrador — **última feature**
- **Estado:** spike primero; no se estima hasta probar la API

## Qué

Al cobrar, el cliente paga **en el local** con QR de Mercado Pago (o, más adelante, con Posnet).

No es el cobro de tu abono (eso es la tarjeta 22: $ 9.999 a **tu** MP). Acá la plata va a la cuenta de **ese local**.

Va **al final** del backlog: hay que integrar un tercero y todavía no está cerrado cómo.

## Dos caminos (no son lo mismo)

| | Mercado Pago QR | Posnet |
|---|---|---|
| Qué es | El cliente escanea un QR con el **importe de esta venta**. | Terminal de tarjeta (chip/contactless). |
| Integración | API de MP (Orders, tipo `qr`). | En la práctica casi nunca hay API barata. El cajero tipea el monto en el aparato. |
| v1 | **Esto.** Generar QR, esperar pago, recién ahí confirmar la venta. | Seguir como medio de pago **manual** (“ya cobré en el Posnet”). Integrar aparato = otro proyecto. |

Mercado Pago también vende **Point** (lector de tarjetas). Se parece al Posnet, pero es otro hardware y otra API. No mezclarlo en v1.

## Flujo v1 (QR dinámico)

1. Carrito listo. Medio = Mercado Pago QR (todo o una parte, si es pago múltiple).
2. StockControl pide a MP una orden por ese monto.
3. Con el `qr_data` se dibuja el QR **en pantalla** (y opcional en el ticket).
4. El cliente paga con la app.
5. MP avisa (webhook) o la caja consulta el estado.
6. **Pagado** → se confirma la venta (stock, informe, ticket).
7. El cajero **cancela** o se vence el QR → no hay venta; el carrito sigue.

Hasta que MP no confirme, no se descuenta stock.

## Config (dueño)

Cada local vincula **su** cuenta de Mercado Pago (OAuth / credenciales). Jorge no usa tu MP: usa el de Almagro.

## Criterios de aceptación (cuando se implemente)

- [ ] Spike escrito: app en MP, un local de prueba, una orden QR, un pago real de $ 1.
- [ ] Dueño: “Conectar Mercado Pago” por local. Empleado no pisa las keys.
- [ ] Al cobrar con QR: pantalla con QR + total + Esperando / Pagado / Cancelado / Vencido.
- [ ] Empleado puede cancelar el QR y volver al carrito.
- [ ] Solo con estado pagado se genera la venta y el nro de ticket.
- [ ] Si el local no está vinculado, ese medio no se ofrece (o avisa).
- [ ] En el cierre de caja, QR cuenta como medio “Mercado Pago” (o el nombre que defina el dueño).
- [ ] Posnet v1: medio manual, sin QR. Sin esperar API del banco.

## Spike (hacer antes de codear producto)

- [ ] Crear aplicación en [Tu integraciones](https://www.mercadopago.com.ar/developers) (cuenta del local o una de prueba).
- [ ] Probar **Orders API**: `POST /v1/orders` con `type: "qr"` y `config.qr.mode: "dynamic"`.
- [ ] Armar imagen QR a partir de `qr_data`.
- [ ] Ver cómo llega el aviso de pago (webhook vs polling).
- [ ] Confirmar: ¿un `external_pos_id` por local? ¿por caja/PC?
- [ ] Anotar comisiones y qué pasa si el cliente paga de más / de menos (no debería: el QR lleva el monto).

Docs: [QR Code Mercado Pago](https://www.mercadopago.com.ar/developers/es/docs/qr-code/overview).

## Fuera de v1

- Devolución automática por API.
- Mercado Pago Point / Posnet integrado al aparato.
- QR estático pegado en el mostrador (el dinámico es el que cierra bien con el ticket).
- Usar esta integración para el abono del servicio: eso es la tarjeta 22 (tu MP, no el del local).
