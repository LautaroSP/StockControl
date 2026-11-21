# Caja sin conexión

- **Roles:** empleado, dueño
- **Tipo:** mostrador
- **Depende de:** 07 Venta en caja, 08 Varios carritos

## Qué

Si se corta internet, la caja sigue cobrando con los productos y precios que ya tenía. Las ventas quedan en el navegador (IndexedDB) con un id propio y se mandan solas cuando vuelve la conexión. El servidor no duplica una venta que ya recibió.

Sin conexión se cobra en **efectivo y medios manuales** (un posnet aparte, una transferencia anotada). No hay Mercado Pago, ni cuentas pendientes, ni ofertas nuevas. Un aviso fijo dice «Sin conexión · N ventas por mandar». Si el stock rígido no alcanza al sincronizar, la venta entra igual y queda marcada para revisar.

**La caja no se cierra sin conexión:** el cierre espera a que se manden todas las ventas.

## Criterios de aceptación

- [x] Catálogo y precios guardados en el navegador.
- [x] Cobrar en efectivo y medios manuales sin conexión.
- [x] Sincronizar al volver, sin duplicar (id de la venta).
- [x] Aviso con las ventas pendientes de mandar.
- [x] Cierre de caja bloqueado hasta sincronizar.
- [x] Ventas con stock en conflicto marcadas para revisar.
- [x] Tests xUnit.
