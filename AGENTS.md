# StockControl

WinForms en `StockControl/`. Web en `src/` (API + Angular). Tarjetas en `backlog/`. Estado y orden: `backlog/History.md`. Prototipo en `prototipo-web/` (maqueta, no se compila). Manual: `Manual-de-usuario.md`.

No tocar WinForms salvo que el usuario lo pida.

## Español y atómico

El modelo web se nombra en **español**. Inglés solo en tipos del framework (`DbContext`, `HttpClient`, `OnInit`).

```
✅ Producto, InformeVenta, IdLocal, /productos, caja, empleados
❌ Product, SaleReport, storeId, /products, cash-register
```

- Tablas, propiedades, endpoints y componentes Angular en español.
- Un archivo / una clase = un trabajo. Métodos cortos. Nada “por las dudas”.
- Preferir editar un archivo existente a crear otro.

## Secretos

Nunca commitear API keys, connection strings, certificados ni claves privadas.

- `.env`, `appsettings.*.local.json` y `secrets.json` van en `.gitignore`.
- Usar User Secrets (dev) o variables de entorno (prod).
- No hay “clave pública” de Postgres. Host, usuario y password **solo en entorno**.
- La BD no se publica a internet abierto. Si el API habla con Postgres remoto: **SSL obligatorio**.
- Si aparece un secreto en un diff: no commitear; rotar la clave.

```
✅ ConnectionStrings__Default = env
❌ "Host=...;Password=abc" en appsettings.json versionado
```

## Tests (`src/`)

Cada caso de uso del dominio web tiene test **xUnit**. Sin test, la feature no está cerrada.

Cubrir al menos: cobrar, stock (rígido / no), cierre de caja, descuento sobre margen, permisos dueño vs empleado, producto sector / genérico.

- Tests en el mismo cambio que el código.
- Un test = un comportamiento. Nombres en español (`Cobrar_descuenta_stock_del_producto_comun`).
- Probar el modelo / casos de uso, no el framework.
- En `src/` sí se piden tests (pisa “no crear tests salvo que pidan”).

## Seguridad API (`src/StockControl.Api/`)

- Contraseñas: ASP.NET Identity (hash). Nunca MD5/SHA suelto ni texto plano.
- Validar todos los inputs (DataAnnotations o FluentValidation).
- Rate limit. Listados con tope de página (no devolver la tabla entera).
- Cabeceras: `X-Content-Type-Options`, `Referrer-Policy`, HSTS en producción.
- Producción: forzar HTTPS. En local, `http://localhost` está bien.
- SSL lo termina Caddy/Nginx + Let’s Encrypt. Certificados **fuera de git**.
- RLS: filtro `IdLocal` en EF **y** políticas en Postgres. Un JWT no lee otro local.
- Cifrar en la app solo secretos de terceros (tokens MP del local) con Data Protection / clave en entorno. Productos y ventas no se cifran campo a campo.
- Formulario de contacto: honeypot + rate limit; CAPTCHA si no alcanza.

## Front público (`src/StockControl.Web/`)

- `sitemap.xml` en el sitio (rutas públicas: inicio, sobre nosotros, contacto, FAQs).
- Carga: lazy routes, CSS del prototipo (`prototipo-web/css/estilos.css`), sin assets pesados de más.
- **Modo oscuro** (tarjeta 23): colores solo por variables CSS. Toggle + `localStorage`; si no hay dato, `prefers-color-scheme`. Incluirlo al armar Angular, no como parche después.
- **Sobre nosotros**: texto real del negocio, no lorem. Si no hay copy, pedirlo; no inventar una empresa.
- **Contacto**: formulario corto, datos de contacto visibles, botón volver.
- **FAQs**: sección con preguntas reales (precio $ 9.999, trial, empleados, locales).
- HTTPS no se implementa en Angular: lo fuerza el reverse proxy.
