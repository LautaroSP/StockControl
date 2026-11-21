# Plan 65 · Aumento: redondeo, vista previa y deshacer

Tarjeta: `backlog/65-aumento-redondeo-deshacer.md` (History #66, estado `no`). Depende de la 62 (ya `sí`).
Reglas: español, sin tocar WinForms, tests xUnit en el mismo cambio, sin secretos.

## 1. Resumen

Hoy `POST /productos/aumentar` (`Program.cs`) llama a `ServicioAumento.Aplicar` y guarda. El modal «Aumentar precio y costo» de `productos.component.ts` (`abrirAumento` / `aplicarAumento`) manda `AumentoPedido` y no muestra nada antes de aplicar.

Se agrega:
- **Redondeo** opcional del precio, siempre para arriba, a 10, 50 o 100.
- **Vista previa**: producto, precio y costo antes y después. Los grupos aparecen una vez.
- **Historial**: cada aumento guarda fecha, usuario y valores anteriores.
- **Deshacer** solo el último aumento. Vuelven los que nadie tocó después; los otros se listan y quedan como están.

## 2. Decisiones cerradas

- Redondeo **para arriba** (techo) al múltiplo de 10, 50 o 100. Un valor que ya es múltiplo no cambia. Ej.: 1.996,50 → 2.000.
- Redondeo **solo al precio** (producto, y `PrecioGrupo` en grupos). El costo nunca se redondea.
- Sin redondeo = valor `0` / ausente (comportamiento actual).
- Deshacer **solo el último aumento** del local. No hay deshacer de aumentos viejos.
- **No pisar lo tocado después**: un producto (o grupo) vuelve atrás solo si su precio y costo actuales son iguales a los que dejó el aumento. Si no coinciden, se omite y se avisa.
- Un grupo es una unidad: se compara y se revierte el grupo (`PrecioGrupo`, `Costo`) y luego `ServicioGrupo.AplicarAMiembros`.
- Quien aumenta y deshace: dueño (`Permisos.PuedeEditarProducto`, igual que hoy).
- El historial es por local (`IdLocal`, query filter como el resto).

## 3. Archivos a tocar / crear

Dominio (`src/StockControl.Dominio/`)
- Editar `ServicioAumento.cs`: redondeo, vista previa, devolver lo cambiado, deshacer. Si crece, partir en `ServicioDeshacerAumento.cs`.
- Crear `AumentoHistorial.cs` y `AumentoHistorialItem.cs` (entidades).

API (`src/StockControl.Api/`)
- Editar `Contratos/Pedidos.cs`: `AumentoPedido` suma `RedondeoPrecio` (`[Range(0,100)]`, valida 0/10/50/100 en dominio).
- Editar `Program.cs`: `POST /productos/aumentar` guarda historial; nuevos `POST /productos/aumentar/vista-previa`, `GET /productos/aumentar/ultimo`, `POST /productos/aumentar/deshacer`. Todos `RequireAuthorization().RequireRateLimiting("api")`.
- Editar `Datos/AppDbContext.cs`: `DbSet`s, `HasQueryFilter` por `IdLocal`, índice `(IdLocal, Fecha)`.
- Crear migración `AumentoHistorial` (requiere aplicar a la base; anotarlo en History).

Web (`src/StockControl.Web/src/app/`)
- Editar `servicios/api.service.ts`: `aumentarProductos` con `redondeoPrecio`; `vistaPreviaAumento`, `ultimoAumento`, `deshacerAumento`.
- Editar `paginas/productos.component.ts`: select de redondeo, paso de vista previa, botón «Deshacer último aumento» con aviso de omitidos.

Tests (`src/StockControl.Tests/`)
- Editar `ServicioAumentoTests.cs` (redondeo, preview, registro). Crear `ServicioDeshacerAumentoTests.cs` si se separó el servicio.

Docs: `backlog/65-…md`, `backlog/History.md`, `Manual-de-usuario.md` (sección de aumento).

## 4. Pasos en orden

1. **Redondeo (dominio).** `ServicioAumento.Redondear(decimal precio, int multiplo)`: `Math.Ceiling(precio / multiplo) * multiplo`; `multiplo == 0` devuelve el precio. `NormalizarRedondeo(int)` acepta solo 0/10/50/100, si no `ErrorNegocio("El redondeo es a 10, 50 o 100.")`. Aplicarlo en `Aplicar` solo donde se calcula precio: `p.Precio` (no sector) y `grupo.PrecioGrupo`. Nunca en costo. Si el redondeo está elegido pero no hay aumento de precio, no redondear (el aumento de costo sigue exigiendo `ExigirAlguno`); definir: redondeo sin aumento de precio se ignora.
2. **Resultado de `Aplicar`.** Que devuelva la lista de cambios (`IdProducto` o `IdGrupo`, precio/costo antes, precio/costo después) en vez de solo el `int`. Mantener `cantidad` (miembros afectados) para el mensaje actual. Ajustar los tests existentes a la nueva firma.
3. **Vista previa (dominio).** `ServicioAumento.Simular(...)`: misma lógica que `Aplicar` sobre **copias** (no muta entidades ni toca `FechaModificacion`). Reusar un método interno común para no duplicar. Devuelve filas con producto/grupo, nombre, precio y costo antes/después; un grupo una sola fila.
4. **Entidades de historial** (sección 5) y migración. Configurar en `AppDbContext`.
5. **Registrar el aumento (API).** En `/productos/aumentar`, dentro de la misma `SaveChangesAsync`, crear `AumentoHistorial` + ítems con valores anteriores y posteriores. Revisar si `Aplicar` y el guardado están en una sola transacción.
6. **Endpoint de vista previa.** Mismo pedido que aumentar; resuelve ids igual (categoría o `Ids`); no guarda. Devuelve filas con tope razonable (p. ej. primeras 200 + total) por la regla de listados con tope.
7. **Deshacer (dominio).** `ServicioDeshacerAumento.Deshacer(historial, productos, grupos, usuario)`: por cada ítem, si los valores actuales == `*Despues` → restaurar `*Antes` (grupo vía `AplicarAMiembros`); si no → a la lista de omitidos. Marcar `Deshecho = true`, `FechaDeshecho`, `UsuarioDeshizo`. Error si no hay aumento, o si el último ya está deshecho.
8. **Endpoints.** `GET /productos/aumentar/ultimo` (fecha, usuario, cantidad, redondeo, `Deshecho`; 404/204 si no hay). `POST /productos/aumentar/deshacer` devuelve `{ restaurados, omitidos: [{ nombre }] }`.
9. **Angular.** Modal: select «Redondear precio» (No redondear / 10 / 50 / 100). Botón «Ver vista previa» llama al endpoint y muestra tabla antes → después; desde ahí «Aplicar» o «Volver». Botón «Deshacer último aumento» (confirmación) que muestra fecha/usuario y, tras deshacer, un aviso con los omitidos. Estilos con variables CSS (modo oscuro).
10. **Tests** (sección 6), `dotnet test`, y `ng build` del web.
11. **Cierre** (sección 9).

## 5. Modelo de datos del historial

`AumentoHistorial` (una fila por aumento)
- `Id` (int), `IdLocal` (int, filtro), `Fecha` (DateTimeOffset UTC), `Usuario` (string, máx. 120)
- `TipoPrecio`, `ValorPrecio`, `TipoCosto`, `ValorCosto`, `RedondeoPrecio` (int: 0/10/50/100)
- `Deshecho` (bool), `FechaDeshecho` (DateTimeOffset?), `UsuarioDeshizo` (string?)

`AumentoHistorialItem` (una fila por producto suelto o por grupo)
- `Id`, `IdAumentoHistorial` (FK, cascade), `IdLocal`
- `IdProducto` (int?) **o** `IdGrupoProducto` (int?): exactamente uno
- `Nombre` (para listar omitidos aunque se renombre)
- `PrecioAntes`, `PrecioDespues`, `CostoAntes`, `CostoDespues` (decimal(18,2); en sector `Precio*` no cambia y queda igual)

"Último" = el de mayor `Fecha` del local. Si el último ya está `Deshecho`, no hay nada para deshacer (no se retrocede al anterior).

## 6. Tests xUnit (español)

Redondeo
- `Redondear_sube_al_multiplo_de_10`
- `Redondear_sube_al_multiplo_de_50`
- `Redondear_sube_al_multiplo_de_100`
- `Redondear_deja_igual_un_precio_que_ya_es_multiplo`
- `Redondear_1996_50_a_100_da_2000`
- `Redondear_cero_no_redondea`
- `Redondeo_distinto_de_10_50_100_falla`
- `Aplicar_redondea_el_precio_y_no_el_costo`
- `Aplicar_redondea_el_precio_del_grupo_y_lo_copia_a_los_miembros`
- `Aplicar_no_redondea_el_producto_sector`

Vista previa
- `Simular_no_modifica_productos_ni_grupos`
- `Simular_da_el_mismo_resultado_que_aplicar`
- `Simular_muestra_un_grupo_una_sola_vez`

Historial
- `Aplicar_devuelve_valores_antes_y_despues_por_producto`
- `Aplicar_guarda_un_solo_item_por_grupo`

Deshacer
- `Deshacer_vuelve_precio_y_costo_anteriores`
- `Deshacer_vuelve_el_grupo_y_sus_miembros`
- `Deshacer_no_pisa_un_producto_tocado_despues_y_lo_lista`
- `Deshacer_no_pisa_un_grupo_tocado_despues`
- `Deshacer_restaura_los_no_tocados_aunque_otros_hayan_cambiado`
- `Deshacer_marca_el_aumento_como_deshecho`
- `Deshacer_dos_veces_falla`
- `Deshacer_sin_aumentos_falla`
- `Deshacer_solo_actua_sobre_el_ultimo_aumento`
- `Empleado_no_puede_aumentar_ni_deshacer` (vía `Permisos.PuedeEditarProducto`)

Mantener verdes los tests actuales de `ServicioAumentoTests`.

## 7. Criterios de aceptación

- [ ] Redondeo opcional a 10, 50 o 100, siempre para arriba, solo en el precio (producto y grupo); el costo queda como sale del aumento.
- [ ] Vista previa antes de aplicar con precio y costo de antes y después; grupos una vez; no guarda nada.
- [ ] Cada aumento queda en el historial con fecha, usuario y valores anteriores.
- [ ] Deshacer el último aumento restaura lo que nadie tocó después; lo tocado no vuelve y se avisa por nombre.
- [ ] Solo dueño puede aumentar y deshacer; filtro por `IdLocal`.
- [ ] Tests xUnit pasan; `ng build` pasa; migración creada.
- [ ] Criterios tildados en la tarjeta 65.

## 8. Fuera de alcance

- Deshacer aumentos anteriores al último o historial navegable en pantalla.
- Redondeo del costo, a otros múltiplos o hacia abajo / al más cercano.
- Alcance por proveedor y lista en Excel (tarjeta 66).
- Deshacer cambios que no sean un aumento masivo (edición suelta, stock).
- Escritorio WinForms y tienda pública.
- Resolver conflictos (pisar a la fuerza lo tocado después).

## 9. Cierre History.md

1. Tildar los criterios en `backlog/65-aumento-redondeo-deshacer.md`.
2. En `backlog/History.md`, fila `#66` (tarjeta 65): estado `sí`; en notas, aclarar «Requiere migración `AumentoHistorial`».
3. Actualizar **Siguiente** (hoy apunta a #55 · 52 Actualizar Angular y pendientes de prueba; poner la próxima tarjeta por `#orden`, la 66 Aumento por proveedor y Excel, salvo que el dueño diga otra cosa).
4. Agregar al **Log** (arriba): `2026-11-20 · 65 Aumento: redondeo, vista previa y deshacer · sí · <resumen corto>` (usar la fecha real del cierre).
5. Actualizar `Manual-de-usuario.md` en la sección de aumento de precios.
