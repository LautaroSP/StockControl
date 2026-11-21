# Plan 67 · Factura electrónica (ARCA)

Tarjeta: `backlog/67-factura-arca.md` (#orden 68 en History). Depende de 09 Cierre de caja, 18 Ticket y 25 Consulta de cajas cerradas. Este plan no incluye código.

## 1. Resumen

Locales monotributistas emiten **Factura C** a consumidor final contra ARCA (WSAA para autenticar, WSFEv1 para autorizar). Se factura **por caja cerrada**: al cerrarla o después desde cajas anteriores. En cada caja el dueño elige **una factura por venta** o **una global por el total**. El ticket muestra CAE, vencimiento y QR. Anular una venta facturada emite **nota de crédito C**.

Lo que hay hoy en `src/` y condiciona el diseño:

- **Cierre:** `POST /cajas/cerrar` (`Program.cs`) usa `ServicioCierre.Cerrar`, que marca `IdCierre` en las ventas del puesto. Puede cerrar varios puestos, pero solo si el rol es dueño operativo (`Permisos.EsDuenoOperativo`).
- **Anulación:** `ServicioAnularVenta.Anular` **rechaza** ventas con `IdCierre != null` («No se puede anular una venta de una caja ya cerrada»). La factura exige caja cerrada, así que hay un conflicto con «NC al anular». Ver riesgo R1 y decisión D8.
- **Secretos de terceros:** `AddDataProtection` con `DataProtection:Ruta`. Los tokens MP se protegen con `CreateProtector(CobrosMercadoPagoApp.Proposito)`. El ticket de ARCA usa el mismo patrón, con otro propósito.

## 2. Decisiones

| # | Decisión |
|---|---|
| D1 | Solo **monotributo, Factura C**. Sin IVA discriminado, sin Factura A/B. |
| D2 | **Certificado delegado.** StockControl tiene un único certificado y clave privada. Van **solo en entorno**, nunca en git. El local delega el servicio en ARCA y carga CUIT y punto de venta. Sin certificado por local. |
| D3 | Se factura **caja cerrada**, al cerrar o después (Consulta de cajas cerradas). No se factura una caja abierta. |
| D4 | El dueño elige por caja: **por venta** o **global**. Además hay un default global del local (ajuste) que la pantalla propone pero no impone. |
| D5 | Permisos: **dueño y socio sí, empleado no**. El empleado no ve la opción. Una regla nueva en `Permisos`, no `if` sueltos. |
| D6 | **NC C al anular** una venta facturada. Total si la factura era de esa venta; parcial si era la global. |
| D7 | Tope de consumidor final sin identificar: **se verifica en homologación** antes de codear la fase 3. Hasta entonces va como valor configurable, sin número fijo en el código. |
| D8 | Anular una venta de caja cerrada hoy está prohibido. Hay que decidir (con el dueño) cómo se habilita la anulación posterior solo para ventas facturadas. Propuesta: permitir anular venta cerrada **solo dueño/socio y solo si está facturada**, con NC obligatoria. Pendiente de confirmar. |
| D9 | Las ventas ya facturadas no se refacturan. |
| D10 | Los montos en ARCA van en pesos con 2 decimales. El total facturado debe igualar la suma de las ventas, sin redondeos propios. |

## 3. Fases

1. **Configuración del local.** CUIT, punto de venta, condición «monotributo», instrucciones de delegación en pantalla. Validar CUIT (dígito verificador) y punto de venta. Sin llamar a ARCA todavía.
2. **WSAA.** Pedir y cachear el token y sign por servicio. Reusar mientras no venza, renovar antes. Un solo ticket de acceso a la vez (ARCA rechaza pedir otro con uno vigente).
3. **Homologación y tope.** Probar `FECompUltimoAutorizado` y `FECAESolicitar` en homologación. **Confirmar el tope de consumidor final.** Dejar el valor anotado en la tarjeta.
4. **Facturar por venta.** Una Factura C por venta de una caja cerrada. Guardar CAE y vencimiento. Idempotente.
5. **Facturar global.** Una Factura C por el total de la caja (solo ventas sin facturar). Bloquear si supera el tope y ofrecer por venta.
6. **Ticket.** CAE, vencimiento y QR de ARCA en el ticket (tarjeta 18).
7. **Nota de crédito.** Al anular una venta facturada (según D8). Parcial contra global.
8. **Front.** Pantalla de configuración. Botón «Facturar» al cerrar y en cajas anteriores, con elección por venta o global. Oculto al empleado.
9. **Producción.** Certificado de producción en entorno, probar con un local real.

## 4. Modelo

Nombres en español. Un archivo por clase.

- **`ConfiguracionFiscal`** (por local): `IdLocal`, `Cuit`, `PuntoVenta`, `Activa`, `FacturaGlobalPorDefecto`.
- **`ComprobanteFiscal`**: `Id`, `IdLocal`, `IdCierre`, `NroCaja`, `Tipo` (FacturaC / NotaCreditoC), `PuntoVenta`, `Numero`, `Importe`, `Cae`, `VencimientoCae`, `Fecha`, `Alcance` (Venta / Caja), `IdComprobanteOrigen` (la factura que anula la NC), `Estado` (Autorizado / Rechazado / Pendiente), `Observacion`.
- **`ComprobanteFiscalVenta`**: une comprobante con `IdInformeVenta` e `Importe`. Una factura por venta tiene una fila; la global tiene muchas. Una venta solo puede estar en una factura vigente (índice único sobre ventas con factura autorizada sin NC total).
- **`InformeVenta`**: sin columna nueva. «Facturada» se deduce de `ComprobanteFiscalVenta`.
- `IdLocal` y filtro EF por local, y política RLS en Postgres, como el resto. Migración EF nueva.
- Nada de claves en tablas. El certificado y su clave no se guardan en la base.

## 5. WSAA / WSFEv1 (alto nivel)

**WSAA**
- Se arma un pedido de acceso (TRA) para el servicio `wsfe`, se firma (CMS) con el certificado de StockControl y se envía.
- ARCA devuelve token y sign con vencimiento. Se cachean en memoria hasta poco antes de vencer. Un solo token por servicio, compartido por todos los locales: el CUIT del local va en cada pedido de WSFE.
- Homologación y producción tienen URLs distintas, por configuración.

**WSFEv1**
- `FECompUltimoAutorizado` para saber el próximo número del punto de venta del local.
- `FECAESolicitar` con Factura C: tipo comprobante C, concepto productos, documento consumidor final, importe total, fecha, moneda pesos. Responde CAE y vencimiento.
- La NC C se pide con `FECAESolicitar` e incluye el comprobante asociado.
- QR: se arma con los datos del comprobante según la especificación de ARCA y se muestra en el ticket.
- **Errores:** rechazo de ARCA, timeout, ARCA caída. Si se pierde la respuesta, antes de reintentar consultar `FECompConsultar`, para no duplicar número. Estado `Pendiente` hasta confirmar.
- Los locales deben haber delegado el servicio. Si no, el error se traduce a un mensaje claro («falta delegar en ARCA»).
- Un local a la vez por punto de venta: serializar la emisión para no pisar numeración.

**Secretos (solo entorno)**
- Certificado y clave privada de StockControl: variables de entorno (por ejemplo, el contenido o una ruta fuera del repo). Nada en `appsettings.json` versionado ni en git.
- Si hay que persistir algo de terceros, va con Data Protection (propósito propio, `DataProtection:Ruta`). El token WSAA es corto y se mantiene en memoria.
- Agregar al `.gitignore` los certificados y claves (`*.pfx`, `*.key`, `*.crt`) si no están.

## 6. Archivos

Dominio (`src/StockControl.Dominio/`):
- `ConfiguracionFiscal.cs`, `ComprobanteFiscal.cs`, `ComprobanteFiscalVenta.cs`: entidades.
- `ServicioFacturacion.cs`: elige por venta/global, filtra ya facturadas, valida el tope y que la caja esté cerrada.
- `ServicioNotaCredito.cs`: calcula total o parcial al anular.
- `Cuit.cs`: validación del dígito verificador.
- `Permisos.cs` (editar): regla `PuedeFacturar` para dueño y socio.
- `ServicioAnularVenta.cs` (editar): según D8, permitir caja cerrada si está facturada.

API (`src/StockControl.Api/`):
- `Fiscal/ClienteWsaa.cs`: token y sign, con caché.
- `Fiscal/ClienteWsfe.cs`: últimos números, solicitar CAE, consultar.
- `Fiscal/QrArca.cs`: armado del QR.
- `Fiscal/EndpointsFiscal.cs`: `/fiscal/configuracion`, `/cajas/{idCierre}/facturar`, `/fiscal/comprobantes`.
- `Program.cs` (editar): registrar endpoints y clientes, y llamar a NC desde la anulación.
- Migración EF y política RLS de las tablas nuevas.

Front (`src/StockControl.Web/`):
- Componente de configuración fiscal (CUIT, punto de venta, guía de delegación).
- Botón «Facturar» en cierre y en cajas anteriores. Elección por venta/global. Oculto al empleado.
- Ticket con CAE, vencimiento y QR.

Docs: `Manual-de-usuario.md` (sección facturación y delegación), tarjeta 67 y History.

## 7. Tests (xUnit, `src/StockControl.Tests/`)

El dominio se prueba con un cliente ARCA falso. Un test = un comportamiento.

- `Facturar_por_venta_genera_una_factura_por_cada_venta_de_la_caja`
- `Facturar_global_genera_una_sola_factura_por_el_total`
- `Facturar_no_repite_ventas_ya_facturadas`
- `Facturar_rechaza_caja_abierta`
- `Facturar_global_se_bloquea_si_supera_el_tope`
- `Facturar_global_ofrece_por_venta_cuando_supera_el_tope`
- `Facturar_guarda_cae_y_vencimiento`
- `Facturar_si_arca_rechaza_no_marca_las_ventas_como_facturadas`
- `Facturar_reintento_tras_timeout_no_duplica_numero`
- `Empleado_no_puede_facturar` / `Duenio_y_socio_pueden_facturar`
- `Anular_venta_facturada_por_venta_emite_nc_total`
- `Anular_venta_de_factura_global_emite_nc_parcial`
- `Anular_venta_no_facturada_no_emite_nc`
- `Cuit_valida_digito_verificador`
- `Configuracion_fiscal_exige_cuit_y_punto_de_venta`
- `Facturar_no_toca_otro_local`
- Test de que la clave/certificado no se serializa a la base ni al log.

Prueba manual: homologación completa (no es xUnit).

## 8. Criterios

Los de la tarjeta, más:
- [ ] Configurar CUIT y punto de venta; instrucciones de delegación.
- [ ] Facturar al cerrar y desde cajas anteriores, por venta o global.
- [ ] Solo dueño y socio; el empleado no ve la opción.
- [ ] Ventas ya facturadas no se repiten; CAE y vencimiento guardados.
- [ ] Global bloqueada sobre el tope; tope confirmado en homologación y anotado.
- [ ] Ticket con CAE y QR.
- [ ] NC C al anular una venta facturada (D8 resuelta).
- [ ] Probado contra homologación.
- [ ] Tests xUnit verdes.
- [ ] Sin secretos en git ni en logs.

## 9. Riesgos

| # | Riesgo | Mitigación |
|---|---|---|
| R1 | Hoy no se anulan ventas de caja cerrada, pero se factura con caja cerrada. | D8: habilitarlo solo para facturadas, con NC obligatoria. Confirmar con el dueño. |
| R2 | Pérdida de respuesta de ARCA: factura emitida sin guardar. | Estado `Pendiente`, consultar antes de reintentar. |
| R3 | Numeración pisada por concurrencia en un punto de venta. | Serializar emisión por local y punto de venta. |
| R4 | Tope de CF desconocido. | Verificar en homologación (D7). |
| R5 | Local sin delegar en ARCA. | Mensaje claro y guía en pantalla. |
| R6 | Filtración del certificado único (afecta a todos los locales). | Solo entorno, permisos mínimos, rotar si aparece en un diff. |
| R7 | Vencimiento del certificado de StockControl. | Aviso con anticipación, renovar antes. |
| R8 | ARCA caída: el cierre no debe bloquearse. | Facturar es un paso aparte del cierre. |
| R9 | Redondeo en la global con descuentos y varios medios de pago. | Facturar el total real de las ventas; test de suma. |
| R10 | La tarjeta 68 (Caja sin conexión) sincroniza ventas tarde. | El cierre espera a sincronizar, así que se factura después. |

## 10. History

Al cerrar la tarjeta:
1. Tildar criterios en `backlog/67-factura-arca.md` y anotar el tope confirmado.
2. `backlog/History.md`: fila 68 en **sí**, actualizar **Siguiente**.
3. Una línea en el **Log**: `fecha · 67 Factura electrónica (ARCA) · sí`.
4. Tests xUnit en el mismo cambio.
5. Actualizar `Manual-de-usuario.md`.
