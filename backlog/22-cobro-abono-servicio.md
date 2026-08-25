# Cobro del abono (te pagan a vos)

- **Roles:** dueño (paga) · admin (cobrás / activás a mano)
- **Tipo:** negocio — Argentina only
- **Precio:** **$ 9.999 / mes por local**
- **No es** la tarjeta 21 (eso es el cliente del kiosco pagando en caja)

## Qué

Jorge te paga el servicio. Pim: paga → queda la suscripción de ese local al día.

Con pocos usuarios no hace falta una pasarela perfecta ni factura. Prioridad: **rápido**.

## Cómo paga (v1)

1. **QR Mercado Pago (preferido)**  
   Pantalla “Activar local”: importe $ 9.999, QR de **tu** cuenta MP. Paga → el local pasa a al día 30 días.  
   Cada mes, otro QR (o el mismo flujo “renovar”). Con 5–10 locales se bancan. El débito automático de MP (suscripciones) se mira después, si molesta cobrar a mano.

2. **¿No tenés Mercado Pago? Contactá a ventas**  
   Texto fijo + **teléfono / WhatsApp** (número tuyo). Vos cobrás como sea y en admin marcás “pago recibido” (tarjeta 03). El local se activa igual.

No hay otro país. No hay otro precio en v1.

## Trial (7 días)

Opcional al registrarse: **1 semana** sin pagar, **limitada**.

Límite propuesto (ajustable):

- 1 solo local
- Caja + productos + stock (para probar de verdad)
- Banner “Prueba — te quedan N días”
- Sin contratar un segundo local
- Sin estadísticas comparando sucursales (no hay)

Al día 8 sin pago: mismo corte que un vencido (no opera caja) + CTA a pagar $ 9.999 o contactar ventas.

Si paga durante el trial, el trial se corta y arrancan 30 días pagos.

## Flujo feliz

1. Alta de dueño (mail, usuario, contraseña). Solo Argentina.
2. Elige: **empezar prueba 7 días** o **pagar ahora**.
3. Paga con QR → se crea/activa el local y la suscripción.
4. Sin MP → “Contactá a ventas” y vos lo das de alta (tarjeta 03).
5. Otro local: otra vez $ 9.999 (tarjeta 04), mismo QR o ventas.

## Criterios de aceptación

- [ ] Precio único: $ 9.999 ARS / mes / local. Empleados no suman.
- [ ] QR de cobro usa **tu** MP (plata a vos). No el MP del kiosco.
- [ ] Pago acreditado → local `al día`, vencimiento = hoy + 30 días, dueño ya entra a caja.
- [ ] Falló / cerró el QR → no se crea suscripción; puede reintentar.
- [ ] Link/botón “¿No tenés Mercado Pago? Contactá a ventas” con teléfono configurable (admin).
- [ ] Admin puede marcar pago a mano (transferencia, efectivo, lo que sea) y queda igual de al día.
- [ ] Trial 7 días, un local, banner con días restantes; al vencer, bloqueo + pagar / ventas.
- [ ] Solo cuentas Argentina (dato de país fijo; no mostrar USD ni otros medios).
- [ ] No emitir factura fiscal en v1 (lo anotás vos afuera si hace falta).

## Qué no es esto

- QR en el **mostrador** para el cliente del almacén → tarjeta 21.
- Débito automático todos los meses sin que Jorge abra la app → más adelante, si el QR mensual cansa.

## Spike (cuando toque)

- [ ] Un QR de tu MP por $ 9.999 y un pago de prueba.
- [ ] Cómo te enterás (webhook vs mirar el panel de MP vs el dueño aprieta “ya pagué”). Con pocos usuarios, “ya pagué” + vos confirmás en admin también sirve.
