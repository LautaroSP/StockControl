# Tienda: ofertas, dos columnas y ficha de producto

- **Roles:** comprador, dueño
- **Tipo:** tienda
- **Depende de:** 29 Tienda, 34 Ofertas, 37 Carrito y pedido

## Qué

Las ofertas y los combos de la caja corren en la tienda **solo si el dueño marca «también en la tienda»** en esa oferta. La ficha muestra el precio tachado y el ahorro; el carrito y el pedido los guardan.

En el celular, dos columnas de productos en vez de una. Ficha del producto al tocarlo: fotos grandes, nombre, precio, stock, **descripción** (campo opcional nuevo en el producto) y agregar. Ordenar por nombre o precio.

Al compartir el link de la tienda por WhatsApp se ve el nombre y el logo del local (Open Graph servido por la API, porque WhatsApp no corre JavaScript).

Si la tienda se cierra por el abono vencido, el dueño ve el aviso en Tienda online.

## Criterios de aceptación

- [x] Opción «también en la tienda» en la oferta; la tienda aplica solo esas.
- [x] Dos columnas en el celular.
- [x] Descripción opcional en el producto y ficha de producto.
- [x] Ordenar por nombre o precio.
- [x] Vista previa del link con nombre y logo.
- [x] Aviso al dueño de tienda cerrada por abono.
- [x] Tests xUnit.
