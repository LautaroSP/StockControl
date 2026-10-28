# Categorías de productos

- **Roles:** dueño (alta, edición, baja y asignación) · empleado (no administra categorías) · comprador de la tienda (filtra)
- **Tipo:** catálogo + filtro de la tienda
- **Depende de:** 11 Productos, 29 Tienda: URL pública y catálogo

## Qué

Cada local tiene sus **categorías** (Bebidas, Almacén, Limpieza, etc.) para ordenar los productos. Sirven para que el comprador de la tienda filtre el catálogo.

No es lo mismo que un grupo de precio (14). El grupo junta productos que tienen el mismo precio; la categoría sirve para mostrarlos. Un producto puede tener grupo, categoría, las dos cosas o ninguna.

## Categoría del grupo y del producto

- Un **grupo de precio** también puede tener categoría (opcional). Se elige en la pantalla Grupos.
- Un producto puede tener su **categoría propia**, aunque esté en un grupo.
- **Categoría efectiva** del producto: la propia; si no tiene, la de su grupo; si tampoco, ninguna. **La del producto domina** a la del grupo.
- Cambiar la categoría del grupo cambia la efectiva de los productos que no tienen una propia. No toca las categorías propias.
- La tienda, la tabla de Productos y los filtros usan siempre la categoría efectiva.

## Pantalla Categorías (dueño)

- Ruta `/categorias`, lazy, solo para el dueño. Entrada en la barra, junto a Grupos.
- Listado con el nombre y cuántos productos tiene cada categoría.
- Alta y edición: solo el nombre. Obligatorio, hasta 40 caracteres, sin repetirse dentro del local (sin distinguir mayúsculas ni espacios de los costados).
- Baja: se avisa cuántos productos y grupos tiene. Al confirmar, quedan **sin categoría** (los productos con categoría propia y los grupos). Ni el producto ni el grupo se borran.
- Las categorías son del local (`IdLocal`). Un local no ve las de otro.

## Producto

- En la ficha del producto, un seleccionable «Categoría» con las del local. La primera opción es «Sin categoría propia» y, si el producto está en un grupo con categoría, dice «La del grupo ({nombre})». Con eso hereda.
- Productos: la columna muestra la categoría efectiva; si viene del grupo, se indica (tenue o «(grupo)»).
- **No es obligatoria.** El producto nuevo nace sin categoría.
- Un producto tiene una categoría o ninguna.
- Productos: columna Categoría en la tabla.

## Tienda (comprador)

- Arriba del listado, las categorías como **seleccionables** (botones tipo chip). «Todas» viene marcada.
- Se pueden marcar **varias categorías** a la vez: salen los productos de cualquiera de las marcadas. Tocar una marcada la desmarca.
- Marcar una categoría desmarca «Todas». Tocar «Todas» o desmarcar la última saca el filtro.
- Solo aparecen las categorías con al menos un producto publicado. Sin ninguna, no se muestran los seleccionables.
- El filtro se combina con el buscador y el paginado.
- Los productos sin categoría salen solo en «Todas».
- Reemplaza el filtro por grupo de precio de la 29: el grupo deja de mostrarse en la tienda.

## Seguridad

- El endpoint público devuelve solo el id y el nombre de las categorías con productos publicados de una tienda abierta.
- Asignar una categoría de otro local al producto se rechaza. RLS por `IdLocal`.

## Criterios de aceptación

- [ ] Pantalla Categorías del dueño: alta, edición y baja, con la cantidad de productos de cada una.
- [ ] Nombre obligatorio, hasta 40 caracteres, sin repetirse en el local.
- [ ] Borrar una categoría deja sus productos sin categoría, sin borrarlos.
- [ ] La ficha del producto permite elegir una categoría o «Sin categoría»; no es obligatoria.
- [ ] Productos muestra la categoría efectiva en la tabla.
- [ ] El grupo de precio puede tener categoría; el producto hereda la del grupo si no tiene propia, y la propia domina.
- [ ] La tienda muestra las categorías con productos publicados como seleccionables; se pueden marcar varias y filtra por cualquiera de las marcadas, junto con el buscador.
- [ ] «Todas» muestra también los productos sin categoría.
- [ ] El filtro por grupo de precio sale de la tienda.
- [ ] El empleado no administra categorías.
- [ ] Requiere migración `Categorias`.
- [ ] Tests xUnit: nombre repetido o vacío se rechaza; borrar deja los productos sin categoría; producto sin categoría es válido; no se asigna una categoría de otro local (producto ni grupo); la categoría efectiva es la del producto si la tiene, si no la del grupo, si no ninguna; borrar la categoría deja sin categoría a productos y grupos; la tienda solo lista categorías con productos publicados; filtrar por varias categorías devuelve los productos publicados de cualquiera de ellas.

## Fuera

- Subcategorías: no va a haber.
- Varias categorías por producto.
- Orden manual de las categorías (van por nombre).
- Filtro por categoría en la caja y en Productos.
