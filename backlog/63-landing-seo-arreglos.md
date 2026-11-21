# Landing y SEO: arreglos

- **Roles:** visitante
- **Tipo:** UI — sitio público
- **Depende de:** 28 Landing, 23 Modo oscuro

## Qué

Arreglos que salieron de la revisión del 20/11 en escritorio y celular.

La landing suma preguntas frecuentes con datos reales (precio, prueba, empleados, locales, tienda) y cuenta lo que hoy no dice: tienda online, ofertas y combos, cuentas pendientes y balanza. Tiene el botón de tema. La alternancia oscuro ↔ claro de la 28 sigue solo mientras la persona no eligió tema.

SEO: descripción y datos para compartir en `index.html`, ícono, títulos propios en contacto, login y registro. `sitemap.xml` y `robots.txt` los arma la API con el host del pedido: URLs absolutas sin escribir el dominio. `robots.txt` no deja indexar el panel ni los links de pedido.

Textos: «Contraseña» en el login, sin «la misma para todos los locales» en contacto, sin el aviso de «no hay teléfono» mientras carga, «¡…!» en el precio. Arrepentimiento en el pie. Link al inicio en login y registro. Links con contraste en modo oscuro y botones de 40 px en pantallas táctiles.

«Sobre nosotros» es una página propia (`/sobre-nosotros`) con el texto del dueño: una sola persona, nació para el local de sus padres, basado en un almacén y un local de refrigeración, todo centralizado por un precio bajo, atención personalizada sin bots. Link en la barra y en el pie de la landing, y en el sitemap.

Las claves reales de Turnstile van en el entorno de producción (`Turnstile__SiteKey`, `Turnstile__Secret`).

## Criterios de aceptación

- [x] Sección de preguntas frecuentes en la landing, con link en la barra.
- [x] Tarjetas de tienda online, ofertas y combos, cuentas pendientes y balanza.
- [x] Botón de tema en la landing. Si hay tema guardado, se respeta y no alterna.
- [x] `meta description`, Open Graph e ícono.
- [x] Título propio en contacto, login y registro.
- [x] `sitemap.xml` con URLs absolutas y `robots.txt` que bloquea el panel y los links de pedido.
- [x] Arrepentimiento en el pie de la landing.
- [x] Contraste de links en modo oscuro y botones de 40 px en pantallas táctiles.
- [x] En el celular, la barra de la landing entra en una línea.
- [x] «Sobre nosotros» con texto real.
- [x] Tests xUnit (`ServicioSitemapTests`).
