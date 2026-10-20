# Cobro del abono (te pagan a vos)

- **Roles:** dueño (paga) · admin (cobrás / activás a mano)
- **Tipo:** negocio — Argentina only
- **Precio:** **$ 14.999 / mes por local**
- **No es** la tarjeta 21 (eso es el cliente del kiosco pagando en caja)

## Qué

Jorge te paga el servicio. Pim: paga → queda la suscripción de ese local al día.

Con pocos usuarios no hace falta una pasarela perfecta ni factura. Prioridad: **rápido**.

## Cómo paga (v1)

1. **Pagar este mes (Checkout Pro)**  
   Pantalla “Abono” (`/activar`): importe $ 14.999, botón + QR del link de pago de **tu** cuenta MP (cualquier medio, 1 cuota). Paga → el webhook verifica el pago contra la API de MP y el local pasa a al día. El mes nuevo arranca desde el vencimiento vigente; si ya venció o está en prueba, desde el pago (`ServicioAbono.VencePorRenovacion`).

2. **Débito automático (Suscripciones MP, opcional)**  
   El dueño autoriza una vez con tarjeta (mail de su cuenta MP) y MP cobra $ 14.999 cada mes. Si el local está al día, el primer cobro es al vencer; si no, al autorizar. Con débito activo hay **3 días de gracia** después del vencimiento. Se cancela desde la misma pantalla o desde MP.

3. **¿No tenés Mercado Pago? Contactá a ventas**  
   Texto fijo + **teléfono / WhatsApp** (número tuyo, `Ventas:WhatsApp`). Vos cobrás como sea y en admin marcás “pago recibido” (tarjeta 03). El local se activa igual.

No hay otro país. No hay otro precio en v1.

## Trial (7 días)

Opcional al registrarse: **1 semana** sin pagar, **limitada**.

Límite propuesto (ajustable):

- 1 solo local
- Caja + productos + stock (para probar de verdad)
- Banner “Prueba — te quedan N días”
- Sin contratar un segundo local
- Sin estadísticas comparando sucursales (no hay)

Al día 8 sin pago: mismo corte que un vencido (no opera caja) + CTA a pagar $ 14.999 o contactar ventas.

Si paga durante el trial, el trial se corta y arranca el mes pago (vence el mismo día del mes siguiente, a la misma hora).

## Flujo feliz

1. Alta de dueño (mail, usuario, contraseña). Solo Argentina.
2. Elige: **empezar prueba 7 días** o **pagar ahora**.
3. Paga con QR → se crea/activa el local y la suscripción.
4. Sin MP → “Contactá a ventas” y vos lo das de alta (tarjeta 03).
5. Otro local: otra vez $ 14.999 (tarjeta 04), mismo QR o ventas.

## Criterios de aceptación

- [x] Precio único: $ 14.999 ARS / mes / local. Empleados no suman.
- [x] QR de cobro usa **tu** MP (plata a vos). No el MP del kiosco.
- [x] Pago acreditado → local `al día`, vencimiento = mismo día del mes siguiente, misma hora (desde el vencimiento vigente si todavía no venció), dueño ya entra a caja.
- [x] Falló / cerró el QR → no se acredita nada; puede reintentar (el link pendiente se reutiliza 30 min).
- [x] Un mismo pago no se acredita dos veces (índice único `Abonos.IdPagoMp`).
- [x] Débito automático opcional (suscripción MP) con 3 días de gracia; se puede cancelar.
- [x] Link/botón “¿No tenés Mercado Pago? Contactá a ventas” con teléfono configurable (`Ventas:WhatsApp`; editarlo desde admin → 03).
- [x] Admin puede marcar pago a mano → tarjeta 03 (criterio “marcar pago recibido”).
- [x] Trial 7 días, un local, banner con días restantes; al vencer, bloqueo + pagar / ventas.
- [x] Registro público: dueño + primer local, eligiendo prueba o pagar (honeypot + rate limit).
- [x] Solo cuentas Argentina (ARS fijo; no se muestra USD ni otros medios).
- [x] No emitir factura fiscal en v1 (lo anotás vos afuera si hace falta).

## Qué no es esto

- QR en el **mostrador** para el cliente del almacén → tarjeta 21.
- Cambiar el precio de suscripciones ya activas → más adelante.

## Spike (prueba manual con credenciales de prueba)

- [ ] Un pago de prueba de $ 14.999 con Checkout Pro (comprador de prueba).
- [ ] Un débito automático autorizado con el comprador de prueba.
- [x] Cómo te enterás: webhook firmado (`x-signature`) + consulta a la API de MP.
