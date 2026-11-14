# Legales: términos, privacidad y consumidor

- **Roles:** dueño (acepta y contrata) · empleado (usa la app) · comprador de la tienda · cualquiera que escribe en contacto · admin (ve pedidos de arrepentimiento y baja)
- **Tipo:** textos legales + pantallas + registro de aceptación
- **Depende de:** 22 Cobro del abono, 28 Landing, 29 Tienda, 35 Cuentas pendientes
- **Bloquea:** abrir la tienda al público real, 37 Carrito y pedido, salir a producción

## Qué

Toda la web con sus textos legales, no solo la tienda. Hoy hay unos párrafos de «Términos y condiciones» en el pie de la landing (precio y abono), sin privacidad, sin aceptación en el registro y sin botones de arrepentimiento ni de baja.

Los textos **no se inventan**. Se arma un borrador a partir de cómo funciona el sistema, con los datos del titular. La revisión de un profesional queda para cuando haya clientes: no frena esta tarjeta.

## Datos del titular

Persona física, sin razón social. La revisión de un profesional no frena la tarjeta.

- Titular: Lautaro Parandelli.
- Identificación: 23-40133767-9. Prefijo de persona física. Si está inscripto en ARCA, es el CUIT; si no, es el CUIL. En los textos se publica el número. El rótulo CUIT o CUIL queda para cuando lo confirme.
- Mail legal: lautaroparandelli@gmail.com.
- Jurisdicción: Provincia de Buenos Aires.
- Domicilio legal: Avenida Rivadavia 15240, 4.º D, Haedo, Partido de Morón, Provincia de Buenos Aires. La localidad sale de la altura (código postal 1706). Si no es Haedo, se corrige antes de publicarlo.

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
- El sitio y el sistema no se copian ni se republican. Ver la página no autoriza a armar otra con las mismas pantallas o textos. Sirve para pedir la baja si alguien publica una copia. No es un bloqueo técnico.

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
- El catálogo público (nombres, precios y fotos) es del local. Los términos de la tienda prohíben copiarlo para armar otra tienda o un listado. Igual que arriba: no frena el script; sirve para pedir la baja.
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

- [x] Datos del titular cargados (nombre o razón social, CUIT, domicilio, mail legal, jurisdicción).
- [x] Borrador de términos del servicio, privacidad y términos de la tienda escrito a partir del sistema, sin datos inventados. Ver Borrador. El rótulo CUIT o CUIL y la localidad se confirman antes de publicarlo.
- [x] Términos del servicio: no copiar ni republicar el sitio ni el sistema.
- [x] Términos de la tienda: el catálogo es del local y no se copia para publicarlo en otro lado.
- [x] `/terminos` y `/privacidad` públicas, con fecha y versión, y enlazadas en los pies, incluida la tienda.
- [x] Registro y contratar local piden aceptar con un tilde sin marcar; se guardan la versión y la fecha.
- [x] Términos nuevos: el dueño vuelve a aceptar al entrar y no opera hasta hacerlo.
- [x] Botón de arrepentimiento del servicio: formulario, código de trámite y aviso al admin.
- [x] Botón de baja del abono para el dueño, en línea.
- [x] Tienda: pie con términos de la tienda, datos del vendedor y su botón de arrepentimiento.
- [x] Contacto: aviso de uso de datos.
- [x] Privacidad con la leyenda de la AAIP y los terceros que reciben datos.
- [x] `sitemap.xml` con las páginas legales.
- [x] Tests xUnit: sin aceptar no hay registro; versión vieja obliga a reaceptar; el pedido de arrepentimiento da código y queda pendiente para el admin; la baja deja el local sin renovar.

## Después, cuando haya clientes (no frena la tarjeta)

- [ ] Un profesional revisa los textos finales.
- [ ] Inscripción de las bases de datos ante la AAIP, si corresponde.

## Borrador

Versión 1. 11 de noviembre de 2026. Texto de `/terminos` y `/privacidad`. Los términos de la tienda siguen sin pantalla.

### Titular

StockControl lo presta Lautaro Parandelli, persona física, identificación 23-40133767-9, con domicilio en Avenida Rivadavia 15240, 4.º D, Haedo, Partido de Morón, Provincia de Buenos Aires. Mail para temas legales y de datos personales: lautaroparandelli@gmail.com.

### Términos del servicio

StockControl es el servicio de caja y stock. Al crear la cuenta hay una prueba de 7 días, en un local. En la prueba se pueden cargar productos y cobrar. Pasados los 7 días, el local sigue si se abonan $ 14.999 por mes.

El abono se cobra con Mercado Pago: un pago del mes o el débito automático. Ese cobro es el servicio, no las ventas del mostrador. El débito se cancela desde Mercado Pago. Con débito activo hay 3 días de gracia después del vencimiento. Cada local se abona por separado. Un local nuevo que contrata el dueño no tiene prueba: no opera hasta el pago. Los empleados no tienen un cargo aparte.

Si el abono está vencido o pendiente, ese local no puede cobrar. La tienda, si estaba publicada, muestra que está temporalmente cerrada.

El precio puede cambiar. Se avisa al dueño antes de aplicarlo.

El servicio puede tener mantenimiento. No hay garantía de que esté disponible todo el tiempo.

Los productos, las ventas y los clientes son del local. El dueño responde por sus empleados y por lo que cargan. Al darse de baja puede pedir una copia de esos datos al mail legal. Se borran cuando el local ya no está activo, salvo lo que haga falta guardar mientras haya un reclamo abierto.

Mercado Pago es un tercero. El dinero del mostrador y de la tienda entra en la cuenta de Mercado Pago del local, no en la de StockControl. El ticket no es factura. La factura la emite el local, si corresponde.

No está permitido copiar el sitio ni el sistema, ni republicarlos para armar otro. Ver la página no autoriza a hacer una copia. StockControl puede suspender la cuenta por un uso que perjudique el servicio o a otros locales.

Los reclamos del servicio se rigen por la ley argentina. La jurisdicción es la Provincia de Buenos Aires.

### Privacidad

StockControl guarda datos para prestar el servicio.

- Dueño y empleados: nombre, mail y clave (la clave se guarda con el hash de ASP.NET Identity, nunca en texto plano). Sirven para entrar y operar el local.
- Contacto: nombre, mail y mensaje. Sirven para contestar la consulta.
- Clientes de cuentas pendientes: nombre y teléfono. El responsable de esos datos es el local. StockControl los guarda por él.
- Compradores de la tienda: nombre, teléfono y mail. El responsable es el local. StockControl los guarda por él.

En el navegador se guarda la sesión, el local, la caja y el tema. No hay cookies de publicidad. El captcha es de Cloudflare Turnstile y el mapa, de OpenStreetMap. La búsqueda de dirección usa Nominatim. Los mails salen por Resend. Los cobros, por Mercado Pago.

Para acceder, corregir o borrar datos personales, escribir a lautaroparandelli@gmail.com.

La Agencia de Acceso a la Información Pública, órgano de control de la Ley 25.326, atiende las denuncias y reclamos de quienes vean afectados sus derechos por el incumplimiento de las normas de protección de datos personales.

### Términos de la tienda

Quien vende es el local, no StockControl. En el pie van el nombre, la dirección y el horario que el local cargó. Los precios son los publicados. El stock que se muestra es si hay o no hay, sin la cantidad. El pedido es retiro en el local, o envío si el local lo ofrece. El pago es el que el local tenga habilitado.

El catálogo (nombres, precios y fotos) es del local. No se copia para armar otra tienda ni un listado.

El comprador puede arrepentirse del pedido con el botón de la tienda. Si el pago fue a Mercado Pago, el reintegro lo hace el local desde esa cuenta. Si fue una transferencia, la devolución es a mano, la hace el local.

## Fuera

- Factura electrónica (ARCA/AFIP): la emite el local, no StockControl.
- Libro de quejas en papel o reclamos fuera de la web.
- Bloquear el clic derecho, ofuscar el front o impedir que se lea el HTML. La página pública se puede bajar. El tope de pedidos de la tienda ya frena las ráfagas.
