# Informe — auditoría de seguridad

Tarjeta 44. Fecha: 2026-11-01. La 42 y la 43 siguen en **sí** y no se reabren: este informe las confirma.

Gravedad: crítica, alta, media, baja. Estado: corregido, revisado o tarjeta nueva.

## Ya estaba y se confirmó

| Dónde | Gravedad | Cómo se reproduce | Estado |
|---|---|---|---|
| JWT con emisor, audiencia y `VersionSesion` (`Program.cs`, `MiddlewareSesion`) | alta | Un token viejo, de otro emisor o sin el claim `sesion` responde 401 | revisado (42) |
| Claves con `PasswordHasher` de Identity. El registro ya pedía 8 caracteres | alta | No hay MD5 ni clave en texto plano | revisado |
| Públicos: login, registro, confirmar correo, captcha, contacto, webhook de Mercado Pago, salud, tienda y la vuelta de OAuth. El resto pide autorización. El rol de dueño se mira en el API (`Permisos`) | alta | Un empleado que llama un endpoint de dueño recibe 403 | revisado |
| `IgnoreQueryFilters`: login, admin, webhook, copiar catálogo, tienda y jobs. Después filtran por local, por usuario o por los locales de la cuenta | crítica | Cambiar un id de la URL no devuelve otro local | revisado |
| Webhook: `FirmaWebhook.EsValida`. Secreto vacío no pasa. Un pago repetido no se vuelve a acreditar | alta | Un POST sin firma válida no mueve el abono | revisado |
| Fuera de Development el error es «No se pudo completar la operación.» Swagger solo en Development. HTTPS redirige fuera de Development | media | — | revisado |
| Angular no usa `innerHTML` ni `bypassSecurityTrust` | media | — | revisado |
| `appsettings.json` no tiene claves reales. Los tokens de Mercado Pago pasan por `IDataProtector` | crítica | — | revisado |
| Logs: no se escribe clave, token ni datos de tarjeta. `EnviadorPush` nombra las claves de configuración que faltan, no su valor | media | — | revisado |

## Decisiones

**Token en `localStorage`.** Sigue en `sesion.service.ts`, header `Authorization`. Pasarlo a cookie `httpOnly` obliga a CSRF y a reescribir el cliente. No hay un sumidero de HTML, la 42 corta el token y la CSP baja el riesgo de que un script lo lea.

**RLS para `stockcontrol_app`.** El dueño de las tablas y el superusuario de desarrollo la saltean: no hay `FORCE ROW LEVEL SECURITY`. En producción el API usa `stockcontrol_app` (`NOBYPASSRLS`, no es dueño). Sin `app.id_local` ni `app.alcance = todo`, cero filas. `alcance = todo` solo después de permiso o firma: admin de plataforma, registro, contratar un local, webhook ya firmado y los jobs. La tienda pública resuelve el alias con `tienda_por_alias` (`SECURITY DEFINER`) y el resto de la consulta queda con ese local. Cómo probarlo: `deploy/base.md`.

## Correcciones

### Login y claves — alta — corregido

El login compartía el tope general de 120 por minuto. Ahora la política `login` es 10 por minuto por IP y no consume ese tope. En `Usuario`, `IntentosFallidos` y `BloqueadoHasta`. Al quinto fallo seguido, `ServicioLogin` frena 15 minutos; un acierto reinicia. Si la clave está mal o la cuenta está frenada, la respuesta es 401. Una clave ya guardada de menos de 8 caracteres sigue entrando: el pedido de login no exige ese largo.

Alta, reseteo y cambio del admin usan `ServicioRegistro.LargoMinimoClave` (8). Los formularios dicen «mínimo 8».

Reproducción: cinco claves mal contra el mismo usuario y la sexta, aunque sea la buena, responde 401 hasta que pasan 15 minutos. Una clave nueva de 7 caracteres responde 400.

El tope por IP vive en memoria del proceso. Con dos réplicas del API, cada una cuenta aparte.

### RLS — crítica — corregido

Migración `20261101223000_AuditoriaSeguridad`. Políticas por `IdLocal` en las tablas que ya filtraba EF, más `UsuarioLocal` y `Local`. `Usuario` no lleva RLS (el login busca por nombre). `MensajeContacto` se puede insertar y solo listar con `alcance = todo`. `Ajuste` se lee en público solo por `ajuste_publico` (teléfono, mail y si el formulario está activo).

Prueba en la base de desarrollo, con el rol `stockcontrol_app` (no es dueño, `NOBYPASSRLS`), el 2026-11-01:

- sin setting: 0 productos (el superusuario veía 1818)
- `app.id_local = 2`: 1815
- `app.id_local = 4`: 3
- `app.alcance = todo`: 1818

### Costo — alta — corregido

`GET /productos` mandaba `Costo` a cualquier JWT. El listado y el detalle de ventas mandaban `PrecioCosto` (el texto SI/NO de venta al costo). Si el rol no puede ver el costo (`Permisos.PuedeVerCostoEnInforme`), el API manda null. El dueño lo sigue recibiendo. El listado de miembros de un grupo sigue con costo y solo lo pide quien administra grupos.

Reproducción: entrar como empleado y mirar el JSON de `/productos` y de `/ventas`. `costo` y `precioCosto` van en null.

### Listados — media — corregido

Tope `maxPagina` (100) en categorías, grupos, miembros, usuarios, proveedores, productos de un grupo, ofertas y combos vigentes (el GET), filtros de cajas y los chips de categoría de la tienda. El cobro sigue usando la lista completa del día para no cambiar un precio si hay más de 100 ofertas. Estadísticas y la consulta de cajas son totales de un período: paginarlos cambiaría el número. No son un listado suelto.

### Validación — alta — corregido

En .NET 8 las DataAnnotations no corrían solas. `FiltroValidacion` corre después del ruteo, valida el cuerpo JSON (también un nivel adentro) y responde el primer mensaje en texto. No toca los `multipart` de imágenes. Si el mensaje del atributo es el inglés de fábrica, responde «Revisá los datos enviados.»

Reproducción: `POST /auth/login` con usuario vacío responde 400.

### Cabeceras, CORS y `/api` — alta — corregido

El API manda `Content-Security-Policy: frame-ancestors 'none'; default-src 'none'` y `X-Frame-Options: DENY`, junto a `nosniff`, `Referrer-Policy` y HSTS fuera de localhost. nginx manda la CSP del HTML (Turnstile, teselas de OSM, `frame-ancestors 'none'`). CORS sale de `Cors:Origenes` (separados por coma). Si no hay valor, sigue `localhost:4200` y `127.0.0.1:4200`.

`environment.production.ts` usa `apiUrl: '/api'`. nginx y `deploy/Caddyfile.example` reenvían `/api/` al API y sacan el prefijo. `ng serve` sigue en `http://127.0.0.1:5263`.

### Data Protection — alta — corregido

Si no es Development y falta `DataProtection:Ruta`, el proceso no arranca. Sin el volumen `claves_api` un contenedor nuevo no descifra los tokens de Mercado Pago.

### Contacto — alta — corregido

`POST /ventas/contacto` usa el mismo Turnstile del registro. El cebo y el tope por IP ya estaban. El formulario de contacto y el de la landing muestran el widget. Sin claves de Turnstile en producción el captcha no se exige (igual que el registro): hay que poner `Turnstile__SiteKey` y `Turnstile__Secret`. En Development, sin claves, se usa la clave de prueba de Cloudflare.

### Secretos — crítica — revisado

`gitleaks` sobre 57 commits. Un hallazgo en `ServicioPushTests.cs`: claves de ejemplo de Web Push, no de un servicio. Quedó en `.gitleaks.toml` y el segundo escaneo no encontró nada. No se rota. La clave `stockcontrol` de compose y `.env.example` es de desarrollo: se anota y no se rota.

### Dependencias — alta — corregido en parte, el resto es la 52

`dotnet list package --vulnerable`: ninguno, después de fijar `SQLitePCLRaw.lib.e_sqlite3` 2.1.14 (el import del `.db` arrastraba una versión con CVE-2025-6965). SQLite no es la base en uso.

`npm audit fix` (sin `--force`) bajó el árbol de 72 a 60. En producción quedan 7: 4 altas y 3 medias, todas de `@angular/*` <= 19.2.25. El arreglo publicado es Angular 22.2.4. No se forzó: es un cambio de mayor. Queda en la [52](52-actualizar-angular.md). De esas altas, a esta app le alcanzan las de XSS en plantillas y SVG. No hay SSR, i18n ni `HttpTransferCache`.

Los críticos que quedan (`piscina`, `tar`) están en la herramienta de build. No van en la imagen de nginx.

### Backup — alta — corregido

`deploy/backup.md`, `backup.ps1` y `backup.sh`: `pg_dump` en formato custom, se borran los dumps de más de 14 días, ejemplo de cron. Postgres del compose escucha solo en `127.0.0.1`. Remoto: `SSL Mode=Require`, como `deploy/base.md`.

En esta máquina no hay `pg_dump`. La prueba usó el cliente de la imagen `postgres:16`: se insertó una fila, se hizo el dump, se restauró en `stockcontrol_restaurada` y el `SELECT` devolvió `ok`. Después se borraron la tabla, la base descartable y el dump. El cron hay que instalarlo en el servidor: el script no se programa solo.

## Fuera

Pentest contratado, PCI e ISO. El JWT no se movió a cookie. Los textos de la 48 no se tocaron.
