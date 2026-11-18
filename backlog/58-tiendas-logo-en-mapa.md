# Tiendas: logo en el mapa

- **Roles:** comprador (lo ve) · dueño y socio (suben el logo)
- **Tipo:** mapa público
- **Depende de:** 29 Tienda: URL pública y catálogo, 56 Tiendas: cerca tuyo

## Qué

En el mapa de OpenStreetMap, el local se marca con su logo dentro de un círculo. Sin logo, la gota de siempre. Vale para el mapa del catálogo (`/tienda/{alias}`) y para el de `/tiendas` (56).

El logo ya se sube en Tienda › Configuración y se sirve en `/tienda/{alias}/logo`. Esta tarjeta solo lo lleva al marcador.

## Reglas

- Círculo de 44 px con el borde del color de la tienda (54) o el de StockControl. El logo se recorta al círculo sin deformarse.
- La API pública dice si la tienda tiene logo, así el navegador no pide una imagen que no existe. Si igual falla la carga, vuelve a la gota.
- El marcador se arma con un elemento del DOM, no concatenando HTML. El alias va escapado en la URL.
- En `/tiendas`, con muchas tiendas en pantalla, los logos no pueden pasar el tope `tienda-imagenes` por IP. Se ajusta el tope o se agrupan los marcadores cercanos.
- El mapa del dueño (Tienda › Configuración, para mover el punto) también lo muestra, sin perder el arrastre.
- Modo oscuro: el fondo del círculo sigue blanco para que el logo se lea.

## Criterios de aceptación

- [ ] El mapa del catálogo muestra el logo en un círculo; sin logo, la gota.
- [ ] El mapa de `/tiendas` muestra el logo de cada tienda que lo tenga.
- [ ] Un logo que no carga vuelve a la gota.
- [ ] Con muchas tiendas en el mapa no se corta por el tope de imágenes.
- [ ] El dueño puede seguir moviendo el punto en Configuración.
- [ ] Test del dominio: la tienda pública informa si tiene logo. El dibujo del marcador no lleva test: es visual.

## Fuera de v1

- Un ícono distinto según el rubro del local.
