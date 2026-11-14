# Tienda: paleta

- **Roles:** dueño y socio (eligen) · comprador (la ve)
- **Tipo:** configuración de la tienda + catálogo público
- **Depende de:** 29 Tienda: URL pública y catálogo, 50 Tienda: panel, 23 Modo oscuro

## Qué

En Tienda › Configuración el dueño elige el color de marca de la tienda pública. Vacío es la paleta de StockControl. Con un color propio, el catálogo, el carrito y el pedido lo usan en claro y no ofrecen modo oscuro.

No es un editor de toda la hoja de estilos. Fondo, texto y avisos siguen el claro de StockControl. Cambia el acento: botones primarios, chips activos, enlaces y precio.

## Reglas

- El color es `#` y seis hexadecimales. Vacío vuelve a la paleta de StockControl.
- Si el texto blanco del botón no se lee sobre ese color, se rechaza.
- Con paleta propia, el catálogo, el carrito y el pedido salen siempre en claro y no muestran el interruptor. No se pisa lo que el visitante guardó para la caja.
- Sin paleta propia, el interruptor del comprador sigue como en la 23.
- El QR de la 53 no usa este color: el afiche sigue verde, con StockControl.
- La caja, el panel, la landing y el admin no cambian.
- Dueño y socio. El empleado no la configura.

## Criterios de aceptación

- [ ] En Tienda › Configuración se elige el color, o se deja vacío para la paleta de StockControl.
- [ ] Un color inválido o ilegible sobre el botón se rechaza.
- [ ] Catálogo, carrito y pedido usan ese acento. El fondo y el texto siguen el claro de StockControl.
- [ ] Con paleta propia no hay modo oscuro en esas pantallas, y no se borra la preferencia del navegador.
- [ ] Sin paleta propia, el modo oscuro del comprador sigue.
- [ ] El afiche del QR sigue verde.
- [ ] Test del dominio: vacío, hex válido, hex inválido y color ilegible.
