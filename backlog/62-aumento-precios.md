# Aumento de precio y costo

- **Roles:** dueño
- **Tipo:** catálogo
- **Depende de:** 11 Productos, 14 Grupos de precio, 49 Categorías

## Qué

En Productos, el dueño aumenta el precio y el costo por separado. Cada uno puede ser un porcentaje o un importe en pesos. Cero no cambia ese lado.

El alcance es uno: los productos marcados, o toda una categoría. La categoría es la efectiva: la propia del producto, o la del grupo si el producto no tiene.

Si el producto está en un grupo de precio, el aumento va al grupo una sola vez y alcanza a todos sus miembros, aunque no estén marcados ni en esa categoría.

El sector no tiene precio de lista: solo sube el costo. El precio por kilo no cambia. El genérico queda afuera.

## Criterios de aceptación

- [x] El dueño elige productos marcados o una categoría.
- [x] Precio y costo se aumentan por separado, cada uno en porcentaje o en pesos.
- [x] Un miembro de un grupo mueve el grupo una sola vez y a todos sus miembros.
- [x] El sector solo sube el costo. El precio de lista y el precio por kilo no cambian.
- [x] El empleado no puede aumentar.
- [x] Tests xUnit (`ServicioAumentoTests`).
