# Creá tu tienda online

- **Roles:** dueño (arma la tienda y gestiona pedidos) · socio / empleado (preparan pedidos) · comprador (sitio público, sin cuenta) · admin (cobra el 5 %)
- **Tipo:** sitio público por local + panel de pedidos + Mercado Pago del local
- **Depende de:** 11 Productos, 12 Stock, 16 Medios de pago, 22 Cobro del abono (para sumar el 5 % de las transferencias)

## Qué

Cada local puede abrir su tienda online con los productos que elija. El comprador busca productos, arma un carrito y paga online con Mercado Pago o por transferencia. El pedido le llega al local, que lo prepara y lo entrega o lo despacha.

La tienda viene **incluida** en los $ 14.999 por mes del local. No hay abono extra.

Por cada pedido gestionado online se cobra un **5 % de gestión** para el servicio (vos), calculado **solo sobre los productos** (el envío no paga gestión). El dueño elige si ese 5 % lo absorbe él (incluido en el precio) o si se le suma al comprador como “costo de gestión”.

La plata del pedido va a la cuenta de **ese local**, no a la tuya. Es otro Mercado Pago, distinto del de la tarjeta 22 (abono).

## Tienda (comprador)

- URL propia por local: `/tienda/{alias}`. El alias lo elige el dueño, es único y sin espacios.
- Datos visibles: nombre del local, dirección, horario, WhatsApp, si hace envíos o solo retiro.
- Buscador por nombre y código. Filtro por categoría si existe.
- Ficha del producto: hasta **2 imágenes**, nombre, precio y si hay stock.
- Carrito: sumar, restar, sacar. Se guarda en el navegador.
- Checkout: nombre, teléfono, mail, retiro o envío (con dirección), medio de pago.
- Muestra el desglose: subtotal, envío, gestión (si la paga el comprador) y total.
- Al terminar: número de pedido y estado. Link para consultarlo sin cuenta.

## Panel del local

- Activar o pausar la tienda (pausada: se ve, pero no recibe pedidos).
- Elegir qué productos se publican. Por defecto ninguno.
- Producto sector y genérico **no** se venden online: no tienen precio fijo.
- Subir imágenes: máximo 2 por producto, se guardan en **400 × 400 px** como máximo.
- Pedidos: Nuevo → Pagado / A confirmar transferencia → Preparando → Listo / Enviado → Entregado. También: Cancelado.
- Aviso de pedido nuevo: **notificación en el panel** (contador + lista) y **WhatsApp** al número del local (ver Avisos).
- Al marcar Entregado, el pedido entra como venta del local: descuenta stock, va a informes y al cierre con el medio de pago real.

## Pagos

| Medio | Cómo cobra el local | Cómo cobrás vos el 5 % |
|---|---|---|
| Mercado Pago | Checkout Pro con la cuenta MP **del local** (OAuth). | `marketplace_fee` en la misma preferencia: MP te lo separa solo. |
| Transferencia | El comprador transfiere al CBU/alias del local y el local confirma a mano. | No pasa por vos. Se acumula como deuda del local y se suma al próximo abono (22). |

- Conectar Mercado Pago lo hace el dueño desde el panel. El token se guarda cifrado (Data Protection). El empleado no lo ve ni lo pisa.
- El pedido no se da por pagado hasta que llega el webhook firmado de MP. No sirve que vuelva el navegador.
- La transferencia queda “A confirmar”. El **dueño elige el plazo** para confirmarla (en horas), o “sin vencimiento”. Pasado el plazo, el pedido se cancela y el stock se libera.

## Gestión (5 %)

- El dueño elige: **incluida** (el comprador ve el precio de lista; al local le llega 5 % menos) o **a cargo del comprador** (se suma una línea “Gestión” al total).
- Se calcula sobre el subtotal de productos. El envío no paga gestión.
- Si el pedido se cancela antes de entregarse, no se cobra gestión. Si MP ya lo cobró, se devuelve con el reembolso.
- El admin ve el total de gestión por local y por mes: lo cobrado por MP y lo pendiente por transferencias.

## Avisos

El aviso que **siempre** llega es el del sistema. WhatsApp es un extra que depende del comprador.

### Notificaciones del sistema (sin costo)

- Cada pedido nuevo, pago aprobado por MP o transferencia por vencer genera una **notificación** para los usuarios del local (dueño, socio y empleados con acceso a pedidos).
- **Campanita** en el menú con contador de no leídas y lista: qué pasó, pedido, monto, hace cuánto. Clic → abre el pedido y la marca como leída. “Marcar todas como leídas”.
- Se actualiza sola mientras el panel está abierto (consulta cada pocos segundos), con **sonido** opcional y aviso en pantalla.
- **Notificaciones del navegador (Web Push):** llegan aunque la pestaña esté cerrada, en la compu y en el celular Android. En iPhone, solo si el panel está agregado a la pantalla de inicio (iOS 16.4+). Cada usuario las activa desde su navegador; las claves VAPID van en entorno, nunca en git.
- Cada usuario elige qué recibe (pedidos nuevos, pagos, vencimientos) y si quiere sonido.
- Las notificaciones son por local: nadie ve las de otro local.

### WhatsApp (link `wa.me`, sin costo)

- Al terminar la compra, el comprador ve “Avisar al local por WhatsApp” con el pedido ya escrito. Lo envía él: si no toca Enviar, no llega. Por eso no reemplaza a la notificación del sistema.
- Desde el panel, cada pedido tiene “Escribir al comprador” por WhatsApp.
- Que el mensaje salga solo, sin que nadie toque nada, exige la API oficial de WhatsApp (Meta): número verificado y costo por conversación. Queda fuera de v1.

## Envío por km

- El dueño carga la ubicación del local y el **precio por km**. Opcionales: envío mínimo, radio máximo y envío gratis desde cierto monto.
- El comprador marca su dirección. Si queda fuera del radio, solo puede retirar.
- Distancia: v1 en línea recta (sin APIs pagas). Si no alcanza, pasar a la distancia por calle (Google Distance Matrix u OSRM).
- Siempre hay opción de **retiro en el local** sin costo.

## Lo que faltaba (analizado)

- **Stock reservado:** mientras el pedido está pendiente de pago, ese stock no se vende en caja. Si vence, se libera.
- **Stock rígido:** si el local no vende sin stock, la tienda no deja agregar más de lo que hay.
- **Precio congelado:** el pedido guarda el precio del momento de la compra, aunque después cambie el precio en la lista.
- **Cancelaciones y reembolsos:** quién cancela, hasta cuándo, y reembolso por MP desde el panel.
- **Botón de arrepentimiento:** en Argentina, la venta online tiene que ofrecerlo (Res. 424/2020). Va en el pie de la tienda.
- **Términos y privacidad** de la tienda. Los datos del comprador son del local y no se usan para otra cosa.
- **Seguridad:** endpoints públicos con rate limit y honeypot en el checkout. Nunca exponen costo, stock exacto (opcional) ni datos de otro local. RLS por `IdLocal`.
- **Imágenes:** validar tipo real (JPG, PNG, WebP), peso máximo y redimensionar en el servidor. Guardarlas en un volumen, fuera de git y fuera de la base.
- **Abono vencido:** si el local no pagó el mes, la tienda se pausa sola.
- **Factura:** tu 5 % es un servicio que facturás al local. La factura del pedido al comprador es del local. Facturación electrónica (ARCA) queda fuera.

## Decisiones

- Tienda incluida en los $ 14.999 por mes. Sin extra.
- Gestión: 5 % solo sobre productos.
- Avisos: notificaciones del sistema (campanita + navegador) siempre; WhatsApp por link `wa.me`, sin costo.
- Plazo para confirmar transferencia: lo elige el dueño (horas o sin vencimiento).

## Preguntas abiertas

- Si el comprador paga la gestión: ¿la ve como línea aparte o sumada al precio de cada producto? (v1 propuesto: línea aparte)
- ¿El comprador necesita cuenta, o alcanza con mail y número de pedido? (v1 propuesto: sin cuenta)
- ¿Dominio propio por local (`tienda.mikiosco.com`) o solo `/tienda/{alias}`? (v1 propuesto: solo alias)

## Criterios de aceptación

- [ ] Dueño activa la tienda, elige alias único y datos visibles (dirección, horario, WhatsApp, retiro / envío).
- [ ] Dueño publica u oculta productos. Sector y genérico no se pueden publicar.
- [ ] Hasta 2 imágenes por producto, guardadas en 400 × 400 px como máximo; se rechaza lo que no sea imagen.
- [ ] Tienda pública con buscador, ficha con imágenes y carrito guardado en el navegador.
- [ ] Checkout con retiro o envío; el envío se calcula por km con mínimo, radio máximo y envío gratis opcional.
- [ ] Pago con Mercado Pago del local (OAuth) con `marketplace_fee` del 5 %; el pedido pasa a pagado solo por webhook firmado.
- [ ] Pago por transferencia: muestra CBU/alias, el pedido queda “A confirmar” y el local lo confirma; vence en el plazo que eligió el dueño (o nunca).
- [ ] Gestión del 5 % solo sobre productos, incluida o a cargo del comprador según elija el dueño; el desglose se ve antes de pagar.
- [ ] Notificaciones: campanita con contador y lista, marcar leídas, sonido opcional, por local y por usuario.
- [ ] Notificaciones del navegador (Web Push) que llegan con la pestaña cerrada; cada usuario las activa o desactiva.
- [ ] Al terminar la compra, botón “Avisar al local por WhatsApp” (`wa.me`) con el pedido escrito; desde el panel, “Escribir al comprador”.
- [ ] El stock se reserva mientras el pedido está pendiente y se libera si vence o se cancela.
- [ ] Panel de pedidos con estados; al entregar, se registra como venta (stock, informes, cierre de caja).
- [ ] La gestión de las transferencias se suma al próximo abono del local; el admin ve gestión cobrada y pendiente por local.
- [ ] Tienda pausada si el abono está vencido.
- [ ] Botón de arrepentimiento, términos y privacidad en el pie de la tienda.
- [ ] Rate limit + honeypot en el checkout; ningún endpoint público expone costo ni datos de otro local.
- [ ] Tests xUnit: cálculo de envío por km (radio, mínimo, gratis), gestión incluida vs a cargo del comprador (sin contar el envío), vencimiento de transferencia según plazo del dueño, pedido nuevo genera notificación solo para usuarios de ese local, reserva y liberación de stock, pedido entregado = venta, productos sector/genérico no publicables, gestión de transferencias sumada al abono.

## Fuera de v1

- Cuenta del comprador e historial de pedidos.
- Dominio propio por local.
- Distancia por calle con API paga.
- WhatsApp automático con la API de Meta (paga).
- Integración con correos o mensajerías (Andreani, Rappi, etc.).
- Cupones y descuentos online.
- Facturación electrónica.
