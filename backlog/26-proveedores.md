# Proveedores y órdenes de compra

- **Roles:** dueño · socio (el empleado no ve la pestaña)
- **Tipo:** compras / catálogo
- **Pisa:** productos (11), stock (12), config ganancia/IVA (20)
- **#orden:** 22 (después de carteles, antes del cobro del abono)

## Qué

El local carga **proveedores**. Cada uno tiene un catálogo (Excel) y **órdenes de compra**. Un proveedor → muchas órdenes; una orden → un proveedor.

Nav: **Proveedores**. Solo dueño y socio.

## Datos del proveedor

- Nombre
- Tipo de productos (texto libre: “Bebidas”, “Limpieza”, etc.)
- Teléfono WhatsApp (número, código país 54)

Alta, editar, eliminar. Con órdenes históricas: **no borrar**; desactivar (igual que medios de pago).

## Catálogo del proveedor (Excel de entrada)

Plantilla descargable (`.xlsx`) con columnas fijas:

| Codigo | Nombre de producto | Costo | Cantidad |
|---|---|---|---|

Al subir:

1. Se muestran las filas en una tabla (revisar antes de confirmar).
2. Precio de venta default = `costo × factor de ganancia × IVA` de la config del local (tarjeta 20). Si el producto tiene ganancia propia o grupo, valen esas reglas (11 / 14).
3. Confirmar: crea o actualiza **productos del local** (código único por local) y los liga a ese proveedor.
4. Código ya existente: actualiza costo, cantidad (suma stock) y vínculo; no duplica. Avisar.
5. Filas inválidas (sin código, costo ≤ 0): se listan y no entran.

Sector y genéricos no se importan por este Excel.

## Órdenes

Flujo:

1. Listado de proveedores.
2. Click en uno: ficha + **última orden** (si hay) con **Repetir**.
3. **Nueva orden**: buscador de productos ligados a ese proveedor (los que cargaste). Click agrega una línea; abajo tabla con cantidad editable.
4. Listado de la orden: Producto · Cantidad.
5. **Generar orden** pregunta (se puede más de una):
   - Mensaje WhatsApp
   - Excel de salida
   - PDF para imprimir / pedir a mano

Excel de salida y PDF: nombre del local, proveedor, fecha, líneas (código, nombre, cantidad). WhatsApp: el mismo listado en texto.

**Repetir** copia las líneas de la última a una orden nueva (cantidades editables).

La orden queda guardada (fecha, quién, líneas). No se borra al generar.

## WhatsApp (v1)

No hay API de WhatsApp ni envío silencioso. El browser **no** puede mandar el mensaje aunque WhatsApp Web esté abierto.

Al elegir WhatsApp: se abre  
`https://wa.me/54XXXXXXXXXX?text=…`  
(número en formato internacional, sin 0 ni 15; texto URL-encoded).

Si hay sesión en la app, WhatsApp Desktop o WhatsApp Web, se abre el chat con el texto listo. El usuario **tiene que apretar Enviar**. Si no hay WhatsApp, se avisa y la orden **igual se guarda**. Business API / plantillas oficiales: fuera (más adelante, si hace falta).

Botón aparte en la ficha: **Contactar** (mismo `wa.me` sin el pedido, o con un “Hola, …” corto).

## Criterios de aceptación

- [ ] Pestaña Proveedores solo dueño y socio. Empleado no la ve.
- [ ] ABM proveedor: nombre, tipo de productos, teléfono WhatsApp.
- [ ] No borrar proveedor con órdenes; se desactiva.
- [ ] Descargar plantilla Excel (Codigo, Nombre de producto, Costo, Cantidad).
- [ ] Importar Excel → tabla de revisión; precio = costo × ganancia × IVA; confirmar crea/actualiza productos del local ligados a ese proveedor.
- [ ] Código duplicado en el local: actualiza, no crea otro.
- [ ] Listado → ficha con última orden y Repetir.
- [ ] Nueva orden: buscar productos de ese proveedor, setear cantidad, ver Producto · Cantidad.
- [ ] Generar orden: WhatsApp (`wa.me` + Enviar a mano) y/o Excel y/o PDF. La orden se guarda igual si WhatsApp falla.
- [ ] Excel/PDF de salida con local, proveedor, fecha y líneas.
- [ ] Un proveedor, muchas órdenes; una orden, un proveedor. Datos del **local actual**.

## Fuera (v1)

- WhatsApp Business API / envío automático.
- Varios proveedores por el mismo producto (un producto ↔ un proveedor de catálogo).
- Empleado carga órdenes.
- Factura del proveedor / cuentas a pagar.
- Recibir mercadería y sumar stock: tarjeta [27](27-recibir-orden.md).
