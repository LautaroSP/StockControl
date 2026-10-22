# Mantenimiento y 404

- **Roles:** visitante, dueño, empleado
- **Tipo:** deploy (Caddy) + UI

## Qué

Si la API no responde, el sitio muestra una página estática de mantenimiento. Una ruta de la app que no existe la muestra Angular. Caddy queda como proxy: HTTPS y la revisión de Kestrel.

La página de mantenimiento no depende de Angular ni de la API. En local no hace falta Caddy: `ng serve` y la API en el puerto 5263 siguen igual.

## Criterios de aceptación

- [x] Caddy revisa si Kestrel responde. Si no, cualquier visita recibe una página estática: «Estamos en mantenimiento, disculpe las molestias.»
- [x] Esa página no se arma con Angular ni consulta la API.
- [x] Una ruta de la app que no existe (`/caja`, `/contacto` y el resto siguen siendo rutas del navegador) la muestra Angular, con el mismo tono y un botón para volver al inicio.
- [x] El comodín `**` deja de mandar a `/login`.
- [x] Caddy responde 404 solo cuando falta un archivo de verdad (una imagen, por ejemplo). No devuelve 404 en las rutas de la app.
- [x] El certificado y la config de Caddy quedan fuera de git.
