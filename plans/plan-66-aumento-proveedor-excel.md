# Plan 66 · Aumento por proveedor y lista en Excel

Tarjeta: `backlog/66-aumento-proveedor-excel.md` (`#orden` 67 en History). Depende de 26 Proveedores y 62 Aumento.

## Resumen

Dos cosas dentro del aumento de precios:

1. **Alcance «proveedor»**: el dueño elige un proveedor y aplica el aumento de la 62 (precio y costo por separado, porcentaje o pesos) a todos sus productos.
2. **Lista de precios en Excel**: sube código de barras + costo. Cruza por código de barras, muestra una vista previa (encontrados, no encontrados, salteados) y al confirmar actualiza **costo y precio**.

Esta feature **no crea productos ni suma stock**. Eso es del catálogo del proveedor (26, `ServicioCatalogoProveedor`).

## Decisiones

- **Alcance proveedor:** ids de productos con `IdProveedor` = el elegido, excluyendo genéricos. Después se llama a `ServicioAumento.Aplicar`, que ya maneja grupos una vez y el sector (solo costo).
- **Cruce Excel:** por `Producto.Codigo` (código de barras), exacto, `StringComparer.Ordinal`, igual que el catálogo.
- **Solo del proveedor:** una fila cuyo código existe pero es de otro proveedor (o sin proveedor) va a **salteados**, con motivo. No se reasigna proveedor (a diferencia del catálogo 26).
- **Costo:** el Excel trae el costo del pack. Se busca el `ProductoProveedor` (`IdProducto` + `IdProveedor`). Costo unitario = costo pack ÷ `UnidadesPack`. Sin `ProductoProveedor` o `UnidadesPack` 1 = costo por unidad. Redondeo a 2 decimales (`AwayFromZero`, como `ServicioAumento`).
- **Precio:**
  - Normal: `PrecioVenta.CalcularLista(costo, factorGananciaLocal, iva)`.
  - Con `GananciaIndividual`: mismo cálculo con el factor del producto (`ValorGanancia`) en lugar del factor del local.
  - Sector: se saltea (sin precio de lista; ver Fuera de alcance si se quiere subir solo costo).
- **Grupos:** si el producto está en un grupo, se actualiza una sola vez el costo del grupo. El precio del grupo se calcula con la ganancia del grupo (`GananciaIndividual` del grupo) o la del local. Se copia a los miembros con `ServicioGrupo.AplicarAMiembros`. Si varias filas del Excel caen en el mismo grupo, se aplica la primera y las demás se saltean con aviso.
- **Vista previa = misma lógica que aplicar** (`Revisar` y `Aplicar`, patrón del catálogo): lo que se ve es lo que se escribe.
- **Códigos repetidos en el Excel:** se saltea el repetido.
- **Genéricos** (`GENERIC-…`): se saltean, como en el catálogo.
- **Permisos:** dueño y socio (roles de la tarjeta). El empleado no.
- **Multi-local:** todo filtrado por `IdLocal` como el resto.

## Reuso vs nuevo

| Reuso | Nuevo |
|---|---|
| `ServicioAumento.Aplicar`, `Normalizar`, `Nuevo` (alcance proveedor) | Selector de ids por proveedor (`IdsDeProveedor`) en `ServicioAumento` |
| `ProductoProveedor.UnidadesPack` y `ServicioPacksProveedor` (si ya resuelve packs) | Cálculo costo pack ÷ `UnidadesPack` |
| `PrecioVenta.CalcularLista` | Elección de factor: local o `ValorGanancia` |
| `ServicioGrupo.AplicarAMiembros` | `Revisar`/`Aplicar` de lista de costos |
| Lectura Excel en `Api/Importacion/ExcelProveedor.cs` | Variante de lectura con solo 2 columnas (código, costo) |
| Patrón `FilaInvalida` / revisión del catálogo | Modelos de resultado: encontrados, no encontrados, salteados |

## Archivos

Dominio (`src/StockControl.Dominio/`):
- `ServicioAumento.cs`: agregar `IdsDeProveedor(int idProveedor, IEnumerable<Producto>)`. Editar, no crear.
- `ServicioListaProveedor.cs` (nuevo): `Revisar` y `Aplicar` de la lista de costos. Un archivo = un trabajo (no mezclar con el catálogo).
- Modelos de fila/resultado: dentro de `ServicioListaProveedor.cs` si son chicos, como hace `ServicioCatalogoProveedor.cs`.

API (`src/StockControl.Api/`):
- `Importacion/ExcelProveedor.cs`: reutilizar el lector; agregar el método de 2 columnas solo si el actual no sirve.
- Endpoints de aumento (donde está hoy el aumento 62): aceptar alcance `proveedor`; sumar `POST /aumento/proveedor/lista/revisar` y `/aplicar` (rutas en español, ajustar a las existentes).
- Validar tamaño del archivo, extensión y cantidad de filas (tope).

Web (`src/StockControl.Web/`):
- Pantalla de aumento: opción «Proveedor» + selector de proveedor.
- Subir Excel + vista previa con tres listas + botón confirmar.

Tests (`src/StockControl.Tests/`):
- `ServicioAumentoTests.cs`: casos de alcance proveedor.
- `ServicioListaProveedorTests.cs` (nuevo).

## Pasos

1. Leer `ServicioAumentoTests.cs` y `ServicioPacksProveedor` para seguir su estilo y no duplicar lógica de packs.
2. `ServicioAumento.IdsDeProveedor` + tests.
3. En `ServicioListaProveedor`: costo unitario (pack ÷ unidades) + tests.
4. Cálculo de precio (factor local vs `ValorGanancia`) + tests.
5. `Revisar` con clasificación encontrado / no encontrado / salteado + tests.
6. `Aplicar` con grupos una vez + tests.
7. Endpoints (aumento por proveedor, revisar y aplicar la lista), validaciones, permisos dueño/socio.
8. Lector Excel (reuso de `ExcelProveedor`) + test del lector.
9. Angular: alcance proveedor, subida, vista previa, confirmar.
10. Cerrar: tildar criterios en la tarjeta y actualizar History.

## Formato Excel

- Una hoja, primera fila = encabezado.
- Columnas: **Código de barras** | **Costo** (del pack, o de la unidad si no tiene pack).
- Código leído como texto (no perder ceros a la izquierda ni notación científica).
- Costo numérico > 0; se aceptan coma y punto decimal según el lector actual.
- Sin columnas de nombre ni cantidad: si vienen, se ignoran.
- Tope de filas y de tamaño de archivo; `.xlsx` únicamente.

### Clasificación de filas

| Estado | Cuándo |
|---|---|
| **Encontrado** | Código existe en el local, es del proveedor, costo válido. Muestra costo actual → nuevo, precio actual → nuevo. |
| **No encontrado** | El código no existe en el local. No se crea. |
| **Salteado** | Sector; genérico; de otro proveedor; código repetido; costo inválido o ≤ 0; grupo ya procesado. Con motivo. |

## Tests (xUnit, nombres en español)

- `IdsDeProveedor_trae_solo_productos_del_proveedor_sin_genericos`
- `Aumento_por_proveedor_sube_precio_y_costo`
- `Costo_pack_se_divide_por_unidades_del_pack`
- `Costo_sin_pack_es_por_unidad`
- `Precio_usa_factor_del_local_por_iva`
- `Precio_usa_ganancia_propia_si_es_individual`
- `Producto_sector_se_saltea`
- `Codigo_no_encontrado_no_crea_producto`
- `Lista_no_suma_stock`
- `Codigo_de_otro_proveedor_se_saltea`
- `Codigo_repetido_en_excel_se_saltea`
- `Costo_invalido_se_saltea`
- `Grupo_se_actualiza_una_vez_y_copia_a_miembros`
- `Vista_previa_coincide_con_lo_aplicado`
- `Empleado_no_puede_aplicar_aumento_por_proveedor` (permisos en API/caso de uso)

## Criterios de aceptación

- [ ] Alcance «proveedor» en el aumento.
- [ ] Subir Excel con código de barras y costo; vista previa con encontrados, no encontrados y salteados.
- [ ] Costo por pack dividido por unidades.
- [ ] Precio con el factor del local o el propio del producto; grupos una vez.
- [ ] No crea productos ni suma stock.
- [ ] Tests xUnit.

## Fuera de alcance

- Crear productos o sumar stock (catálogo 26).
- Cambiar el proveedor de un producto.
- Crear o editar packs (`ProductoProveedor`).
- Subir solo el costo de productos sector.
- Redondeo y deshacer (65).
- Formatos que no sean `.xlsx` (CSV, PDF).
- WinForms y `prototipo-web/`.

## History

Al cerrar:
1. Tildar criterios en `backlog/66-aumento-proveedor-excel.md`.
2. En `backlog/History.md`: fila `#67` (tarjeta 66) a estado **sí**, quitar huecos, actualizar **Siguiente**.
3. Una línea en el **Log**: `2026-MM-DD · 66 Aumento por proveedor y lista en Excel · sí`.
4. Tests en el mismo cambio.
