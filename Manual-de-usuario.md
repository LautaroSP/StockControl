# Manual de usuario — StockControl

Guía para usar el programa de stock y cobro en el local.

---

## 1. Para qué sirve

StockControl es el programa de mostrador: cargás productos, armás el carrito, cobrás, imprimís tickets y carteles, y cerrás la caja del día.

La pantalla se divide en dos partes:

- **Izquierda:** el listado de productos.
- **Derecha (Opciones):** el carrito de la venta actual, el total y el cobro.

---

## 2. Primer uso

La primera vez que abrís el programa, te pide el **nombre del local**. Es obligatorio: aparece en el ticket.

Si lo cerrás sin guardarlo, el programa te lo vuelve a pedir.

Después podés cambiarlo cuando quieras desde el engranaje de **Configuración** (arriba a la derecha).

---

## 3. Pantalla principal

### Listado de productos

Arriba ves:

- **Cantidad:** cuántos productos hay cargados.
- **Buscador:** filtrá por nombre o código. Se actualiza mientras escribís.

La grilla muestra código, nombre, cantidad en stock, costo, precio, si es de sector, grupo y fecha de última modificación.

Si un producto tiene **menos de 5 unidades**, la fila se pinta de naranja claro para que se note que está por agotarse.

### Botones de productos

| Botón | Qué hace |
| --- | --- |
| **NUEVO PRODUCTO** | Abre la ficha para cargar uno nuevo. |
| **EDITAR PRODUCTO** | Abre la ficha del producto seleccionado. |
| **ELIMINAR PRODUCTO** | Borra el producto (pide confirmación). |
| **GENERAR BARCODE** | Imprime el código de barras en la impresora predeterminada. |
| **GRUPO DE PRODUCTOS** | Abre la pantalla de grupos. |
| **Ver Informes** | Ventas, cierre de caja y resumen mensual. |
| **Engranaje** | Configuración del local. |

### Agregar por código

Arriba a la derecha está el campo **Agregar por código** (`Codigo + Enter`).

Es el lugar pensado para el **lector de código de barras**. El programa deja el cursor ahí después de cada venta o de editar el carrito.

Cómo usarlo:

1. Escaneá o escribí el código.
2. Presioná **Enter**.
3. El producto entra al carrito.

Si el código no existe, el programa pregunta si querés crear el producto con ese código. Si aceptás, se abre la ficha ya con el código cargado. Al grabarlo, se agrega solo al carrito.

Junto a ese campo hay un **botón con ícono** (sin texto): sirve para agregar un **producto genérico** (un ítem que no está en el stock, por ejemplo algo suelto o un precio especial). Ver [Productos genéricos](#62-productos-genéricos).

---

## 4. Cómo vender (el día a día)

### 4.1 Armar el carrito

Hay tres formas de agregar un producto:

1. **Doble clic** en el producto de la lista.
2. **Escanear** el código y Enter.
3. El **botón de producto genérico**.

Si el producto ya está en el carrito, se suma 1 a la cantidad (salvo que sea de **sector**: esos se cobran uno por uno y el precio lo escribís vos).

En el carrito ves: nombre, cantidad, precio y subtotal.

- **Cantidad** y **precio** se pueden editar en la grilla (según el tipo de producto).
- **Doble clic en una fila del carrito** la saca.
- El total y la cantidad de ítems se actualizan solos.

### 4.2 Varios carritos a la vez

Abajo del carrito hay pestañas: `1 F1`, `2 F2`, `3 F3`, `4 F4`.

Sirve cuando un cliente deja la compra a medias y otro se acerca a pagar.

| Control | Qué hace |
| --- | --- |
| **+** | Abre un carrito nuevo. |
| **X** | Cierra el carrito actual. Si tiene productos, pide confirmación. Siempre queda al menos uno abierto. |
| **F1 a F4** | Salta al carrito 1, 2, 3 o 4. |

Cada carrito guarda lo suyo: productos, descuento, cobrar al costo, método de pago, pago múltiple e imprimir ticket.

### 4.3 Método de pago

Elegí el medio en la lista desplegable (efectivo, transferencia, etc.).

Para crear uno nuevo, elegí **Agregar método de pago...** y escribí el nombre.

**Atajo desde el escáner:** con el cursor en el campo de código, presioná **+** (teclado numérico). Se abre la lista de métodos de pago. Con **+** otra vez, el foco pasa a **COBRAR**.

### 4.4 Cobrar

1. Revisá el carrito y el total.
2. Elegí el método de pago (o armá un pago múltiple).
3. Si corresponde, marcá **Imprimir Ticket**.
4. Tocá **COBRAR**.

Qué pasa al cobrar:

- Se guarda la venta.
- Se descuenta el stock (excepto productos de sector y genéricos).
- Si el stock queda en negativo, se deja en **0**.
- Si **Imprimir Ticket** está tildado, sale el ticket por la impresora predeterminada de Windows.
- El carrito se cierra y queda uno vacío listo para la siguiente venta.

Si algo falla, el carrito **no se borra** para que puedas reintentar.

**CANCELAR** vacía el carrito actual y saca descuento, costo y el texto de descuento. No borra la venta (todavía no se cobró).

### 4.5 Imprimir ticket

El tilde **Imprimir Ticket** viene activado en cada carrito nuevo.

El ticket muestra:

- Nombre del local
- Fecha y hora
- Productos, cantidades y subtotales
- Total
- La leyenda *Ticket no valido como factura*
- *Gracias por su compra*

Sale por la **impresora predeterminada** de Windows.

---

## 5. Descuento, costo y pago combinado

### 5.1 Descuento

1. Tildá **Descuento**.
2. Escribí un número del **0 al 100**.
3. Confirmá con **Enter** o saliendo del campo. Se muestra con `%`.

El descuento **no se aplica sobre el precio entero**. Se aplica sobre el **margen** (precio menos costo).

Ejemplo: costo $1.000, precio $1.500. El margen es $500. Un 50% de descuento deja el precio en **$1.250** (no en $750).

No se descuenta:

- Productos **genéricos**
- Productos de **sector** (el precio lo pone el cajero)

### 5.2 Cobrar al costo

Si tildás **Cobrar al costo**, cada producto del carrito pasa a su costo (sin descuento).

Al cobrar, el programa pide confirmación.

Los genéricos y los de sector no se tocan: siguen con el precio que cargaste a mano.

### 5.3 Varios métodos de pago

Tildá **Multiples Metodos de pago**. El carrito no puede estar vacío.

Se abre una ventana con:

- Casilla para elegir cada medio
- Monto de cada uno
- **Cobrar restante** (completa solo el saldo)
- **Resto** arriba, para ver cuánto falta

La suma tiene que coincidir con el total de la venta (con un centavo de tolerancia). Si no cierra, no deja aceptar.

---

## 6. Tipos de producto

### 6.1 Producto común

Tiene código, nombre, costo, precio y cantidad. Al vender se descuenta el stock. En el carrito podés cambiar la cantidad; el precio de lista no se edita ahí.

### 6.2 Productos genéricos

Sirven para cobrar algo que no está en el catálogo (un artículo suelto, un ajuste, etc.).

1. Tocá el botón con ícono al lado del campo de código.
2. Entra una línea que se llama **Producto**, con precio 0.
3. El cursor queda en el **nombre** para que lo cambies.
4. Después pasá a cantidad y precio.

No descuenta stock. El descuento automático y el “cobrar al costo” no le cambian el precio.

### 6.3 Productos de sector

Son productos a **peso o a precio variable** (por ejemplo fiambre, verdura, o un mostrador donde el precio sale de la balanza).

En la ficha, tildá **Sector**. Entonces:

- Cantidad queda en 1 y no se edita.
- Costo y precio quedan en 1; el precio real se carga al vender.
- No se puede meter en un grupo.

Al escanearlo o hacer doble clic:

- Entra al carrito.
- El cursor salta al **precio** para que lo escribas.

No se descuenta stock. En el carrito no se edita la cantidad; sí el precio.

---

## 7. Alta y edición de productos

**NUEVO PRODUCTO** o **EDITAR PRODUCTO** (hay que tener uno seleccionado).

Campos:

| Campo | Notas |
| --- | --- |
| **Código** | En edición no se cambia. No puede repetirse. |
| **Nombre** | Nombre que ves en el carrito y en el ticket. |
| **Costo** | Precio de compra. |
| **Precio** | Precio de venta. Si no usás ganancia propia, se calcula solo: costo × factor de ganancia × IVA (los de Configuración). |
| **Cantidad** | Stock. En productos de sector queda en 1. |
| **Ganancia por producto** | Si lo tildás, usás un factor de ganancia e IVA propios, no los generales. |
| **Sector** | Ver [Productos de sector](#63-productos-de-sector). En edición no se puede cambiar. |
| **Grupo** | **Buscar Grupo** o **Sacar Grupo**. Si tiene grupo, costo y precio los pone el grupo y no se editan en la ficha. |

Al grabar pide confirmación. Al cancelar también.

Si el producto pertenece a un grupo, el precio y el costo salen del grupo, no de lo que veas escrito en los campos.

---

## 8. Grupos de productos

**GRUPO DE PRODUCTOS** abre la pantalla para armar familias con el mismo precio (por ejemplo todas las gaseosas de 500 ml).

### Crear o editar un grupo

En el panel izquierdo:

1. Nombre, costo y precio (o ganancia propia, igual que en el producto).
2. **Nuevo Grupo** para crearlo, o seleccioná uno y **Guardar Cambios**.

Al guardar un grupo, **todos los productos de ese grupo** pasan al nuevo costo y precio.

**Eliminar** saca el grupo y deja los productos sin grupo (no borra los productos).

El buscador de grupos filtra por nombre.

### Asignar productos

1. Elegí el grupo en la lista de la izquierda.
2. A la derecha se habilita la lista de productos (no aparecen los de sector).
3. Tildá el producto para meterlo en el grupo. En ese momento toma el precio y el costo del grupo.
4. Destildalo para sacarlo.

Opciones útiles:

- **MOSTRAR SELECCIONADOS:** ves solo los que ya están en el grupo.
- **Buscar producto:** por código o nombre.
- **Cantidad de productos:** cuántos tiene el grupo actual.

Desde la ficha de un producto también podés **Buscar Grupo** (doble clic en el grupo) o crear uno nuevo con **Nuevo Grupo**.

---

## 9. Códigos de barras

1. Seleccioná el producto.
2. **GENERAR BARCODE**.
3. Confirmá.
4. Si querés más de uno, decí que sí e ingresá la cantidad. Si el número no es válido, imprime 1.

Sale un código **CODE 128** por la impresora predeterminada.

---

## 10. Carteles de precio

Atajo: **Ctrl + P** desde la pantalla principal.

1. Buscá por código o nombre.
2. Tildá los productos (o hacé clic en la fila, menos en nombre y precio).
3. Elegí **Cartel Chico** o **Cartel Grande**.
4. Abajo ves cuántos seleccionaste y cuántas hojas A4 van a salir.
5. **Imprimir**.

Tamaños:

- **Grande:** 6 carteles por hoja (3 × 2).
- **Chico:** 30 carteles por hoja (6 × 5).

Se genera un PDF y se abre solo. Si no se abre, queda en la carpeta temporal de Windows con un nombre como `carteles_20260821_141500.pdf`.

Nombre y precio se pueden retocar en la grilla solo para esa impresión; no cambia el producto en el stock.

---

## 11. Informes, caja y resumen

**Ver Informes** tiene tres pestañas.

### 11.1 Informes de venta

Lista las ventas (las más nuevas arriba). Columnas: fecha, total, método de pago, si fue pago múltiple, si tiene detalle, subtotal, descuento y si se cobró al costo.

- Clic en el encabezado de una columna para ordenar.
- **Doble clic** en una venta: ves el detalle (productos, cantidades, precios y subtotales).
- **Eliminar:** borra la venta y **devuelve el stock** (excepto sector). Pide confirmación.
- **Copiar ticket:** carga esos ítems en el carrito actual (útil para repetir o rehacer una venta). Si un código ya no existe, avisa y sigue con el resto.
- **Imprimir ticket:** reimprime esa venta.
- **Cerrar Caja:** cierra las ventas del **día de hoy** que todavía no están en una caja. Agrupa por método de pago y agrega una fila Total. Después pasa a la pestaña Cajas Cerradas.
- **Cerrar Caja Anterior:** elegís una fecha (hasta 7 días atrás; por defecto ayer). Cierra las ventas de ese día.

### 11.2 Cajas cerradas

Muestra los cierres. Por defecto, los de **hoy**. Tildá **Mostrar todo** para ver el historial.

También se puede ordenar haciendo clic en los encabezados.

### 11.3 Resumen mensual

1. Elegí el mes.
2. **Generar resumen**.

Suma las **cajas ya cerradas** de ese mes, por método de pago, y muestra el total. Si no hay cajas cerradas en ese mes, avisa.

El resumen no mira las ventas sueltas: si no cerraste caja, no entra.

---

## 12. Configuración

El botón con **engranaje**, arriba a la derecha.

| Campo | Para qué |
| --- | --- |
| **Nombre del Local** | Obligatorio. Sale en el ticket. |
| **Factor de ganancia** | Se usa para calcular el precio: costo × ganancia × IVA. |
| **Factor IVA** | Igual, en el cálculo de precio. |
| **Base de datos** | Dónde está el archivo `.db`. |

**Examinar...** elige otro archivo `.db` (o uno nuevo; pregunta si lo crea). **Usar raíz** vuelve al `stock.db` que está junto al programa.

Al guardar:

- Si cambiaste ganancia o IVA, pregunta si querés **recalcular los precios** de los productos que no tienen ganancia propia.
- Si cambiaste la base de datos, el programa **se reinicia**.

**Salir** cierra sin guardar.

---

## 13. Atajos de teclado

| Tecla | Dónde | Qué hace |
| --- | --- | --- |
| **Enter** | Campo de código | Agrega el producto al carrito. |
| **+** (numérico) | Campo de código | Abre los métodos de pago. |
| **+** (numérico) | Métodos de pago | Pasa el foco a **COBRAR**. |
| **Enter** | Campo de descuento | Confirma el % y vuelve al escáner. |
| **F1, F2, F3, F4** | Pantalla principal | Cambia al carrito 1, 2, 3 o 4. |
| **Ctrl + P** | Pantalla principal | Abre imprimir carteles. |
| **Doble clic** | Lista de productos | Agrega al carrito. |
| **Doble clic** | Carrito | Saca el ítem. |
| **Doble clic** | Informe de venta | Abre el detalle. |

---

## 14. Preguntas frecuentes

**¿Puedo vender si no hay stock?**  
Sí. El control de stock al cobrar está desactivado. Si la cantidad queda bajo cero, se guarda como 0.

**¿Por qué el descuento no baja tanto como esperaba?**  
Porque descuenta el margen (precio − costo), no el precio de góndola.

**¿El ticket sirve de factura?**  
No. El propio ticket dice que no es válido como factura.

**¿A qué impresora sale?**  
A la impresora predeterminada de Windows. Cambiala en Windows si el ticket o el código de barras salen por la impresora incorrecta.

**Perdí una venta en el carrito.**  
**CANCELAR** y **X** (cerrar carrito) no guardan la venta. Solo **COBRAR** la registra. Podés usar varios carritos para no mezclar clientes.

**Copié un ticket al carrito y no aparece nada.**  
Tiene que ser una venta con detalle. Las partes de un pago múltiple que no tienen el detalle adjunto no se pueden copiar.

**Cambié el precio de un grupo y un producto no se actualizó.**  
Confirmá que el producto esté tildado en ese grupo y tocá **Guardar Cambios**. Los de sector no entran en grupos.

**¿Dónde está la base de datos?**  
Por defecto, `stock.db` en la carpeta del programa. En Configuración ves la ruta y podés cambiarla.

---

## 15. Recorrido rápido de una venta

1. Escaneá los productos (o doble clic).
2. Si hace falta, ajustá cantidades en el carrito.
3. Si hay descuento, tildalo y poné el %.
4. Elegí el método de pago (o pago múltiple).
5. Dejá tildado **Imprimir Ticket** si el cliente lo quiere.
6. **COBRAR**.
7. El cursor vuelve al campo de código, listo para el siguiente cliente.
