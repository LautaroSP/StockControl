# Tiendas: cerca tuyo

- **Roles:** comprador (busca) · dueño y socio (eligen aparecer)
- **Tipo:** sitio público del comprador
- **Depende de:** 29 Tienda: URL pública y catálogo, 40 Tienda: envío por km

## Qué

Una home pública para el comprador, sin login, en `/tiendas`. Muestra en un mapa y en una lista las tiendas abiertas cerca de su ubicación. Cada tienda abre su `/tienda/{alias}`.

No reemplaza la landing (28): la landing le vende el sistema al dueño; esta home es para el que compra. La landing y el pie de cada tienda enlazan a `/tiendas`.

## Reglas

- La ubicación del comprador sale del navegador, con su permiso. Si no la da, escribe una dirección o barrio y se busca en Nominatim (como en la 29). No se guarda en el servidor; el navegador la recuerda en `localStorage`.
- Aparece la tienda con alias, ubicación marcada, abierta, con el abono al día y sin la gestión vencida (51). Pausada, vencida o cerrada no aparece.
- En Tienda › Configuración, casilla «Aparecer en Tiendas cerca». Arranca tildada; sin tildar, la tienda sigue en su URL pero no sale en el listado.
- Radio a elegir: 2, 5, 10 o 25 km (arranca en 5). Orden por distancia en línea recta.
- Cada tienda muestra nombre, dirección, distancia, horario, si envía a tu dirección (según el radio de la 40) o solo retiro, y el pedido mínimo (55) si tiene.
- Mapa de OpenStreetMap con Leaflet en carga diferida, como la 29. El punto del comprador y un marcador por tienda.
- Listado con tope de página. El servidor nunca devuelve todas las tiendas.
- Solo datos públicos de la tienda: nada de costos, stock, dueño ni ventas.
- La consulta cruza locales. No usa el filtro por `IdLocal` del usuario: lee solo columnas públicas de tiendas publicadas (vista o función de solo lectura en Postgres). Tope por IP como el resto de lo público.
- Ruta lazy. Suma `/tiendas` al `sitemap.xml`. Modo oscuro con las variables de siempre.

## Criterios de aceptación

- [ ] `/tiendas` pide la ubicación o deja escribir una dirección.
- [ ] Mapa y lista con las tiendas dentro del radio, de la más cercana a la más lejana.
- [ ] Pausada, con abono vencido, cerrada por la gestión o sin ubicación no aparece.
- [ ] El dueño puede sacar su tienda del listado sin cerrarla.
- [ ] Cada tienda dice si envía a tu dirección o solo retiro, y su pedido mínimo.
- [ ] Tocar una tienda abre su catálogo.
- [ ] La consulta no expone datos privados de ningún local y tiene tope por IP y por página.
- [ ] Test del dominio: tienda visible o no según estado, abono, gestión, ubicación y casilla; distancia y radio; envía o solo retiro.

## Fuera de v1

- Filtrar por categoría o por «abierta ahora» según el horario.
- Reseñas o puntaje de tiendas.
