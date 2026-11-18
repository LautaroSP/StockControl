# Tiendas: buscar producto

- **Roles:** comprador
- **Tipo:** sitio público del comprador
- **Depende de:** 56 Tiendas: cerca tuyo, 37 Tienda: carrito y pedido

## Qué

En la home `/tiendas` hay un buscador. El comprador escribe, por ejemplo, «leche la serenisima» y ve un listado **tienda · producto · precio** de las tiendas cercanas que lo tienen publicado. Puede agregarlo al carrito de esa tienda o abrir su catálogo.

## Reglas

- Busca en los productos publicados de las tiendas que aparecen en la 56 (misma ubicación, mismo radio, mismas condiciones).
- Cada palabra tiene que estar en el nombre, en cualquier orden. Sin distinguir mayúsculas ni acentos: «serenisima» encuentra «Serenísima». Hasta 5 palabras de 2 letras o más. Se escapan los comodines (`Busqueda.Patron`, 43).
- Un código de barras completo también busca, igual en todas las tiendas.
- Orden por precio, del más barato al más caro. Se puede cambiar a distancia.
- Cada fila: nombre de la tienda, distancia, nombre del producto, foto si tiene (36), precio y «Envía» o «Solo retiro». Si la tienda tiene pedido mínimo (55), se ve.
- El precio es el mismo que muestra el catálogo de esa tienda. Sin stock va al final, marcado.
- «Agregar» suma una unidad al carrito de esa tienda (el mismo de la 37) y ofrece ir al carrito. Cada tienda tiene su carrito: no se mezclan tiendas en un pedido.
- Producto sector y genérico no se publican, así que no aparecen.
- Tope de página (20) y tope por IP. Nada de costo ni cantidad en stock.
- La consulta cruza locales por la misma vía de solo lectura de la 56, sin columnas privadas.

## Criterios de aceptación

- [ ] Buscar «leche la serenisima» lista los productos que tienen las tres palabras en tiendas cercanas.
- [ ] Mayúsculas y acentos no cambian el resultado; el código de barras también encuentra.
- [ ] El listado muestra tienda, distancia, producto y precio, ordenado por precio, con opción por distancia.
- [ ] Sin stock aparece al final y no se puede agregar.
- [ ] Agregar suma al carrito de esa tienda sin tocar el de otras.
- [ ] Solo aparecen productos publicados de tiendas visibles en la 56.
- [ ] Listado paginado, con tope por IP, sin datos privados.
- [ ] Test del dominio: palabras en cualquier orden, acentos, código de barras, orden por precio y por distancia, sin stock al final, tienda no visible y producto no publicado que no salen.

## Fuera de v1

- Sinónimos o errores de tipeo («serenicima»).
- Comparar el mismo producto entre tiendas en una sola fila.
- Un carrito con productos de varias tiendas.
