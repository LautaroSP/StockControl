# Paso a paso · Probar lo nuevo (65–70)

Antes de todo: aplicar a la base las migraciones nuevas (en este orden):

1. `AumentoHistorial`
2. `FacturaArca`
3. `AgregarVentaSinConexion`
4. `TiendaOfertasFicha`

Reiniciar API y front. Entrar como **dueño**.

---

## 0. Humo rápido

1. Login dueño → Productos, Caja, Informes, Tienda, Facturación cargan sin error 500.
2. Empleado: no ve Facturación ni puede aumentar / deshacer / facturar.

---

## 65 · Aumento: redondeo, vista previa y deshacer

1. **Productos** → marcar 3 productos (uno suelto, uno de grupo, uno sector si hay).
2. Abrir **Aumentar precio y costo**.
3. Precio +10 %, redondeo a **100** → **Ver vista previa**.
   - Ver precio/costo antes y después.
   - El grupo aparece **una sola vez**.
   - El sector no cambia precio (solo costo si lo pediste).
4. Confirmar que **no se guardó** nada hasta **Aplicar**.
5. **Aplicar** → precios redondeados para arriba (ej. 1996,50 → 2000). El costo no redondea.
6. Editar a mano el precio de **uno** de los tres.
7. **Deshacer último aumento**:
   - Los no tocados vuelven.
   - El editado **no** vuelve y aparece en el aviso de omitidos.
8. Deshacer de nuevo → debe fallar / no hacer nada (solo el último, y ya está deshecho).
9. Redondeo **sin** aumento de precio → se ignora (o no redondea).

---

## 66 · Aumento por proveedor y lista Excel

### Alcance proveedor

1. En el modal de aumento, alcance **Todo un proveedor**.
2. Elegir un proveedor con varios productos → +8 % precio → vista previa → aplicar.
3. Solo cambian los de ese proveedor (no genéricos).

### Lista Excel

1. Descargar la **plantilla**.
2. Completar código de barras + costo (incluir: uno OK, uno inexistente, uno de otro proveedor, uno sector, uno de grupo repetido).
3. **Revisar** → tres listas: encontrados / no encontrados / salteados.
4. **Aplicar**:
   - Costo = pack ÷ `UnidadesPack` (sin pack = unidad).
   - Precio = costo × factor (local o `ValorGanancia` si ganancia propia).
   - Grupo una sola vez.
5. Confirmar que **no** se crearon productos ni subió stock.

---

## 67 · Factura electrónica ARCA (parcial)

Sin certificado en entorno, facturar debe decir que ARCA no está configurado. Con `Arca__Certificado` / `Arca__ClavePrivada` (o rutas) y `Arca__Ambiente=homologacion`:

1. Menú **Facturación**: cargar CUIT + punto de venta; leer la guía de delegación.
2. Cerrar una caja con varias ventas (o usar una cerrada).
3. **Cajas cerradas** → sección Factura electrónica:
   - Facturar **global** (toda la caja).
   - Facturar **por venta** (elegir algunas).
4. Comprobante Aceptado → CAE visible; Rechazado no marca ventas; Pendiente se puede **Reintentar**.
5. Empleado no ve la sección.
6. Anular una venta **facturada** de caja cerrada (dueño/socio) → debe ir NC C (o Pendiente si ARCA cae).
7. Pendiente de homologación: tope consumidor final, CAE/QR en el ticket impreso, botón al cerrar la caja.

---

## 68 · Caja sin conexión

1. Abrir la **Caja** con internet (para llenar el cache IndexedDB).
2. DevTools → Network → **Offline**.
3. Cobrar **3 ventas** en efectivo (o medio manual).
   - Banner: `Sin conexión · N ventas por mandar`.
   - Mercado Pago, dejar a cuenta y pedidos en espera: deshabilitados.
4. Ir a Informes → **Cerrar caja** debe estar **bloqueado**.
5. Volver **Online** → la cola sincroniza sola.
   - 3 ventas en el servidor, **sin duplicados** (reintentar no duplica).
6. Forzar conflicto: producto con stock rígido bajo, vender más offline → al sync queda **Revisar stock** / `StockEnConflicto`.
7. Informes → filtro «Para revisar» → **Marcar como revisada** (dueño/socio).
8. Sin cola pendiente → el cierre de caja vuelve a habilitarse.
9. Nota: la app tiene que haberse abierto antes con red (no hay service worker completo).

---

## 70 · Tienda: ofertas, dos columnas y ficha

1. **Ofertas**: crear/editar una oferta con casilla **también en la tienda**. Otra sin marcar.
2. **Productos**: poner **Descripción** en un publicado.
3. Abrir `/tienda/{alias}` en celular (~360 px):
   - Grilla de **2 columnas**.
   - Solo la oferta marcada aparece (precio tachado / etiqueta).
4. Ordenar por **nombre** y por **precio** (precio con oferta).
5. Abrir la **ficha** `/tienda/{alias}/producto/{id}`: nombre, precio, descripción, imagen.
6. Carrito y pedido: muestran tachado y ahorro.
7. Compartir el link de la tienda (WhatsApp o debugger OG) → título + logo (proxy/`/compartir/tienda/{alias}`).
8. Con abono vencido / tienda cerrada: el dueño ve aviso en **Tienda online** con ir a pagar el abono.

---

## Checklist final

- [ ] Migraciones aplicadas sin error
- [ ] 65 vista previa + redondeo + deshacer
- [ ] 66 proveedor + Excel
- [ ] 67 config fiscal + facturar caja (homologación aparte)
- [ ] 68 offline 3 ventas + sync + cierre bloqueado
- [ ] 70 ofertas tienda + 2 cols + ficha + OG
- [ ] Empleado sin permisos de dueño/socio en 65–67–68 revisar

Tests automáticos actuales: **912** xUnit en verde (`dotnet test`). Falta la prueba en navegador de este documento.
