# Tienda online: URL pública y catálogo

- **Roles:** dueño (activa, elige alias y qué publica) · comprador (cualquiera, sin cuenta ni login)
- **Tipo:** sitio público por local
- **Depende de:** 11 Productos, 12 Stock, 20 Configuración, 02 Pago por local
- **Sigue en:** 36 Imágenes, 37 Carrito y pedido, 38 Mercado Pago, 39 Transferencia, 40 Envío por km, 41 Panel de pedidos

## Qué

Cada local tiene **una URL única y pública**: `/tienda/{alias}`. Cualquiera entra, sin registrarse ni iniciar sesión. Esta tarjeta solo muestra el catálogo; comprar es la 37.

La tienda viene **incluida** en los $ 14.999 por mes del local. No hay abono extra.

## URL

- Una URL por local. Un dueño con dos locales tiene dos URL, cada una con su alias.
- El alias lo elige el dueño en Configuración. Es **único en todo el sistema**, en minúsculas, sin espacios (letras, números y guion).
- No se puede usar un alias reservado (`admin`, `api`, `login`, `registro`, `tienda`, etc.) ni uno ya tomado.
- Cambiar el alias deja de servir el anterior. Se avisa antes de confirmar.
- La URL no tiene ids numéricos: no se puede recorrer de local en local.
- Alias inexistente o tienda sin activar: la misma página 404, sin revelar si el local existe.

## Tienda (comprador)

- Sin autenticación. La ruta Angular es lazy y pública.
- Datos visibles: nombre del local, dirección, horario, WhatsApp, si hace envíos o solo retiro.
- **Mapa** con la ubicación del local (ver Mapa).
- Buscador por nombre y código. Filtro por categoría si existe.
- Listado paginado con tope. Cada producto: nombre, precio y si hay stock (sin cantidad exacta).
- Pie: términos y privacidad de la tienda y **botón de arrepentimiento** (Res. 424/2020).
- Modo oscuro y estilos del resto del sitio.

## Mapa

- Mapa de **OpenStreetMap** (gratis) con Leaflet, con un marcador en el local. Sin API key.
- El dueño marca la ubicación en Configuración: busca la dirección o mueve el marcador. Se guardan latitud y longitud. La 40 (envío por km) usa esta misma ubicación.
- Botón «Cómo llegar» que abre la ubicación en una app de mapas.
- Atribución visible «© OpenStreetMap contributors» (obligatoria).
- Sin ubicación cargada: no se muestra el mapa, solo la dirección escrita.
- Los mosaicos del servidor público de OSM tienen **política de uso razonable** (sin uso masivo, con atribución). Alcanza para v1. Si el tráfico crece, pasar a un proveedor de mosaicos o alojarlos.
- Al buscar la dirección, la consulta sale del servidor (Nominatim: máximo 1 por segundo, `User-Agent` identificable), solo cuando el dueño busca. El comprador no consulta nada de eso.
- La ubicación es la del negocio, no un dato personal del comprador.

## Panel del local (dueño)

- Activar o pausar la tienda. Pausada: se ve el catálogo, no recibe pedidos.
- Elegir alias y datos visibles.
- Elegir qué productos se publican. Por defecto ninguno.
- Producto sector y genérico **no** se publican: no tienen precio fijo.
- Abono vencido: la tienda se pausa sola.

## Seguridad

- Endpoints públicos con rate limit y tope de página.
- Nunca exponen costo, stock exacto ni datos de otro local. RLS por `IdLocal`.
- Solo salen productos publicados de una tienda activa.

## Decisiones

- Solo `/tienda/{alias}`. Dominio propio por local queda fuera de v1.
- Sin cuenta del comprador.

## Criterios de aceptación

- [ ] Dueño elige un alias único; el sistema rechaza repetidos, reservados y con caracteres inválidos.
- [ ] `/tienda/{alias}` abre el catálogo del local a cualquier persona, sin login.
- [ ] Un dueño con varios locales tiene una URL distinta por local.
- [ ] Alias inexistente o tienda sin activar muestran el mismo 404.
- [ ] Dueño activa o pausa la tienda y publica u oculta productos; sector y genérico no se pueden publicar.
- [ ] La tienda pública muestra solo productos publicados, con buscador, filtro y paginado.
- [ ] El endpoint público no devuelve costo, stock exacto ni datos de otro local.
- [ ] Dueño marca la ubicación del local (búsqueda de dirección o marcador) y se guardan latitud y longitud.
- [ ] La tienda muestra el mapa de OpenStreetMap con el marcador, «Cómo llegar» y la atribución.
- [ ] Sin ubicación, la tienda no muestra el mapa y sigue andando.
- [ ] Tienda pausada si el abono está vencido.
- [ ] Pie con términos, privacidad y botón de arrepentimiento.
- [ ] Rate limit en los endpoints públicos.
- [ ] Tests xUnit: alias único, reservado e inválido; latitud y longitud dentro de rango; sin ubicación no hay mapa; tienda inactiva no resuelve; solo productos publicados; sector/genérico no publicables; abono vencido pausa la tienda; un local no ve productos de otro.

## Fuera de v1

- Dominio propio por local.
- Cuenta del comprador.
