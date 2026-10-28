# Tienda: panel

- **Roles:** dueño y socio (todo) · empleado con acceso a pedidos (solo Pedidos, cuando exista la 41)
- **Tipo:** navegación + pantallas del dueño
- **Depende de:** 29 Tienda: URL pública y catálogo, 36 Tienda: imágenes
- **Sigue en:** 41 suma Pedidos; 38, 39 y 40 suman su configuración

## Qué

Todo lo de la tienda online junto, en un grupo **Tienda** de la barra lateral, igual que Reportes, Administración y Configuración. Hoy está repartido: los datos de la tienda están en Configuración del local y las imágenes debajo de ellos.

## Orden del grupo

1. **Tienda online:** resumen. Estado (apagada, activa, pausada o cerrada por el abono), link público con **Copiar** y **Ver mi tienda** (pestaña nueva), productos publicados y cuántos tienen imagen. Sin alias: aviso y botón a Configuración.
2. **Pedidos:** la pantalla de la 41. Hasta que exista, no aparece.
3. **Configuración:** alias, estado, dirección, horario, WhatsApp, envíos y ubicación. Sale de Configuración del local.
4. **Imágenes de productos:** el apartado de la 36.
5. Lo que venga después (gestión de la 38, transferencia de la 39, envío por km de la 40): debajo, sin orden fijo.

## Rutas

- `/mi-tienda` (resumen), `/mi-tienda/pedidos`, `/mi-tienda/configuracion`, `/mi-tienda/imagenes`. Carga bajo demanda.
- No se usa `/tienda`: es la página pública (`/tienda/{alias}`).

## Reglas

- Dueño y socio ven todo el grupo. El empleado solo ve Pedidos si la 41 le da acceso; si no, el grupo no aparece.
- La ruta también valida el rol (no alcanza con esconder el link). El API ya valida cada endpoint.
- Configuración del local deja de mostrar la sección Tienda y queda un enlace: «La tienda online se configura en Tienda».
- Publicar productos sigue en Productos (columna Tienda). El resumen tiene un enlace a Productos.

## Criterios de aceptación

- [x] Grupo **Tienda** en la barra con este orden: Tienda online, Pedidos (cuando exista), Configuración, Imágenes de productos.
- [x] Tienda online muestra estado, link con Copiar y Ver mi tienda, publicados y con imagen. Sin alias, lleva a Configuración.
- [x] La configuración de la tienda sale de Configuración del local sin perder ningún dato ni acción.
- [x] Imágenes de productos en su propia pantalla.
- [x] Rutas `/mi-tienda/...` bajo demanda, sin chocar con `/tienda/{alias}`.
- [x] Un empleado sin acceso no ve el grupo ni entra escribiendo la ruta.
- [x] Modo oscuro y celular.
- [x] Sin test de dominio si no hay regla nueva: es navegación y reubicación de pantallas.

## Fuera de v1

- Métricas de ventas de la tienda (van con la 41 o con Estadísticas).
