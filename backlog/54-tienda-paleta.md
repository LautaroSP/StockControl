# Tienda: paleta

- **Roles:** dueño y socio (eligen) · comprador (la ve)
- **Tipo:** configuración de la tienda + catálogo público
- **Depende de:** 29 Tienda: URL pública y catálogo, 50 Tienda: panel, 23 Modo oscuro

## Qué

En Tienda › Configuración el dueño elige el fondo de la página y el color de los botones. Vacío es la paleta de StockControl. La vista previa usa un producto publicado (foto, nombre, precio, stock y botón) y la ficha sigue clara. El catálogo, el carrito y el pedido siguen con el interruptor: en claro usan esos colores; en oscuro, la paleta oscura de StockControl.

No es un editor de toda la hoja de estilos. El fondo cambia la página. La ficha cambia la carta del producto y el texto de esa carta contrasta con ella. Los botones (también + y −), los chips activos, los enlaces y el precio usan el otro color.

## Reglas

- El color es `#` y seis hexadecimales. Vacío vuelve a la paleta de StockControl.
- El botón se rechaza si el texto blanco no se lee encima.
- El fondo se rechaza si no se lee ni con texto claro ni con texto oscuro. Si el fondo es oscuro, el título, la paginación y el pie van en blanco. Las fichas siguen claras.
- El interruptor de modo oscuro sigue en el catálogo, el carrito y el pedido. En claro se ven el fondo y los botones del local. En oscuro, la paleta oscura de StockControl.
- El interruptor guarda la preferencia, igual que en la caja.
- El QR de la 53 no usa este color: el afiche sigue verde, con StockControl.
- La caja, el panel, la landing y el admin no cambian.
- Dueño y socio. El empleado no la configura.

## Criterios de aceptación

- [ ] En Tienda › Configuración se eligen fondo y botones, con vista previa, o se dejan vacíos para la paleta de StockControl.
- [x] Un color inválido o ilegible sobre el botón se rechaza. Un fondo que no se lee tampoco.
- [ ] Catálogo, carrito y pedido usan el fondo y los botones. Las fichas de producto siguen claras.
- [ ] Con paleta propia, el modo claro usa esos colores y el modo oscuro usa la paleta oscura de StockControl.
- [ ] Sin paleta propia, el modo oscuro del comprador sigue.
- [x] El afiche del QR sigue verde.
- [x] Test del dominio: vacío, hex válido, hex inválido y color ilegible.
