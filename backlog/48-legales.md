# Legales: términos, privacidad y consumidor

- **Roles:** dueño (acepta y contrata) · empleado (usa la app) · comprador de la tienda · cualquiera que escribe en contacto · admin (ve pedidos de arrepentimiento y baja)
- **Tipo:** textos legales + pantallas + registro de aceptación
- **Depende de:** 22 Cobro del abono, 28 Landing, 29 Tienda, 35 Cuentas pendientes
- **Bloquea:** abrir la tienda al público real, 37 Carrito y pedido, salir a producción

## Qué

Toda la web con sus textos legales, no solo la tienda. Hoy hay unos párrafos de «Términos y condiciones» en el pie de la landing (precio y abono), sin privacidad, sin aceptación en el registro y sin botones de arrepentimiento ni de baja.

Los textos **no se inventan**. Se arma un borrador a partir de cómo funciona el sistema y lo revisa un profesional antes de producción. Faltan datos del titular del servicio (ver Datos que faltan).

## Datos que faltan (los da el usuario)

- Titular del servicio: nombre o razón social, CUIT, domicilio legal.
- Mail para temas legales y de datos personales.
- Jurisdicción (provincia) para consumidor y reclamos.

## Casos que cubre

### 1. Términos del servicio (StockControl ↔ dueño)

- Prueba de 7 días, un local. Abono de $ 14.999 por mes por local, empleados sin cargo.
- Pago del mes o débito automático con Mercado Pago; cómo se cancela el débito.
- Vencido: el local no cobra hasta renovar; la tienda muestra «temporalmente cerrada».
- Cambio de precio con aviso previo al dueño.
- Disponibilidad y mantenimiento (30): sin garantía de servicio ininterrumpido.
- Los datos del local (productos, ventas, clientes) son del local. Al darse de baja puede pedirlos y después se borran en un plazo fijo.
- El dueño responde por sus empleados y por lo que cargan.
- Mercado Pago es un tercero: el dinero del mostrador y de la tienda va a la cuenta del local.
- El ticket no es factura (47). La facturación fiscal es del local.
- Uso no permitido y causas de suspensión.

### 2. Privacidad (Ley 25.326)

- Qué datos se guardan y para qué: dueño y empleados (cuenta), contacto (nombre, mail, mensaje), clientes de cuentas pendientes (35: nombre, teléfono), compradores de la tienda (37: nombre, teléfono, mail).
- Roles: StockControl es responsable de los datos de dueños, empleados y contacto. De los clientes y compradores de cada local, el responsable es el local y StockControl solo los guarda por él.
- Terceros que reciben datos: Mercado Pago (cobros), Cloudflare Turnstile (captcha), Resend (mails), OpenStreetMap (mapa) y Nominatim (búsqueda de dirección).
- Plazos de guardado y borrado.
- Derechos de acceso, rectificación y supresión, y cómo pedirlos (mail legal).
- Leyenda obligatoria de la AAIP como órgano de control.

### 3. Almacenamiento en el navegador

- Qué guarda la app en `localStorage` (sesión, local, caja, tema) y que no hay cookies de publicidad.
- El captcha y el mapa son de terceros.

### 4. Consumidor (Ley 24.240)

- **Botón de arrepentimiento** (Res. SCI 424/2020) en la web del servicio, porque el abono se contrata en línea. Formulario corto; el sistema da un código de trámite y avisa al admin. El reintegro lo hace el admin.
- **Botón de baja** del abono, en línea y tan simple como el alta. El dueño da de baja su local sin hablar con nadie.
- En la tienda del local, el vendedor es el local, no StockControl. Términos de la tienda con datos del vendedor, precios finales, stock, retiro y su propio botón de arrepentimiento.
- Enlace a Defensa del Consumidor, si corresponde a la jurisdicción (lo confirma el profesional).

### 5. Aceptación y versiones

- Registro y contratar otro local: tilde **sin marcar** «Acepto los términos y la política de privacidad», con enlaces. Sin tilde no se crea la cuenta.
- Se guarda qué versión aceptó y cuándo.
- Si cambian los términos, el dueño los vuelve a aceptar al entrar. Hasta aceptar, no opera.
- Formulario de contacto: aviso corto de para qué se usan los datos.

## Pantallas

- `/terminos`, `/privacidad`, `/arrepentimiento` y la baja del abono. Rutas lazy y públicas, menos la baja.
- La tienda tiene su pie con los términos de la tienda y su botón de arrepentimiento (cierra el pie diferido de la 29).
- Pie con los enlaces en landing, login, registro, contacto y tienda. Enlace en Configuración.
- `sitemap.xml` suma `/terminos` y `/privacidad`.
- Los términos de la landing pasan a `/terminos`. El pie deja un resumen y el enlace.

## Criterios de aceptación

- [ ] Datos del titular cargados (nombre o razón social, CUIT, domicilio, mail legal, jurisdicción).
- [ ] Borrador de términos del servicio, privacidad y términos de la tienda escrito a partir del sistema, sin datos inventados.
- [ ] `/terminos` y `/privacidad` públicas, con fecha y versión, y enlazadas en todos los pies.
- [ ] Registro y contratar local piden aceptar con un tilde sin marcar; se guardan la versión y la fecha.
- [ ] Términos nuevos: el dueño vuelve a aceptar al entrar y no opera hasta hacerlo.
- [ ] Botón de arrepentimiento del servicio: formulario, código de trámite y aviso al admin.
- [ ] Botón de baja del abono para el dueño, en línea.
- [ ] Tienda: pie con términos de la tienda, datos del vendedor y su botón de arrepentimiento.
- [ ] Contacto: aviso de uso de datos.
- [ ] Privacidad con la leyenda de la AAIP y los terceros que reciben datos.
- [ ] `sitemap.xml` con las páginas legales.
- [ ] Tests xUnit: sin aceptar no hay registro; versión vieja obliga a reaceptar; el pedido de arrepentimiento da código y queda pendiente para el admin; la baja deja el local sin renovar.

## Antes de producción (no es código)

- [ ] Un profesional revisa los textos finales.
- [ ] Inscripción de las bases de datos ante la AAIP, si corresponde.

## Fuera

- Factura electrónica (ARCA/AFIP): la emite el local, no StockControl.
- Libro de quejas en papel o reclamos fuera de la web.
