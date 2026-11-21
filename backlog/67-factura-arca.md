# Factura electrónica (ARCA)

- **Roles:** dueño y socio (configuran y facturan). El empleado no factura.
- **Tipo:** mostrador — fiscal
- **Depende de:** 09 Cierre de caja, 18 Ticket, 25 Consulta de cajas cerradas

## Qué

Primera versión: locales **monotributistas**, **Factura C**, con los web services de ARCA (WSAA + WSFEv1).

**Certificado delegado:** StockControl tiene un certificado propio. El local, desde su clave fiscal, le delega el servicio de facturación electrónica a StockControl y carga su CUIT y punto de venta. El local no genera certificado. El certificado de StockControl va en el entorno, nunca en git.

**Se factura por caja cerrada.** En la caja, al cerrar, y también desde las cajas anteriores (Consulta de cajas cerradas). Dueño y socio pueden facturar cualquier caja del local; el empleado no. En cada caja el dueño elige:

- **Una factura por venta:** cada venta de esa caja recibe su Factura C a consumidor final, con su CAE.
- **Una factura por el total de la caja:** una sola Factura C a consumidor final por las ventas de esa caja.

Las ventas ya facturadas no se vuelven a facturar. Si el total supera el tope vigente de ARCA para consumidor final sin identificar, la factura global no se emite y se ofrece facturar por venta. El tope se confirma contra homologación antes de codear.

El ticket de una venta facturada muestra CAE, vencimiento y el QR de ARCA. Anular una venta facturada emite la nota de crédito C (total si la factura era de esa venta, parcial si era la global).

## Criterios de aceptación

- [x] Configurar CUIT y punto de venta del local; instrucciones de delegación a StockControl.
- [x] Facturar una caja cerrada desde Cajas cerradas, eligiendo por venta o global. (Falta el botón al cerrar la caja en el mostrador; se factura después, desde Cajas cerradas.)
- [x] Solo dueño y socio; el empleado no ve la opción.
- [x] Ventas ya facturadas no se repiten. CAE y vencimiento guardados.
- [x] Global bloqueada si supera el tope de consumidor final. Tope configurable (`Arca__TopeConsumidorFinal`, por defecto $ 417.288 de referencia). **Verificar el valor vigente en homologación.**
- [ ] Ticket con CAE y QR de ARCA. (`GET /ventas/{id}` ya devuelve `factura` con CAE, vencimiento y URL del QR; falta imprimirlo en el ticket, tarjeta 18.)
- [x] Nota de crédito C al anular una venta facturada. D8: se permite anular una venta facturada de caja cerrada solo dueño o socio; la NC queda Pendiente si ARCA no está disponible. (El total de la caja cerrada no se recalcula.)
- [ ] Probado contra homologación.
- [ ] Tests xUnit.

## Estado: parcial

**Real (probado sin red):** modelo y migración `FacturaArca` (con RLS), dominio de facturación y NC, endpoints, pantalla, firma CMS del pedido WSAA, armado del XML de WSFEv1 y del QR.

**Sin homologar:** el cliente `ClienteWsaa` / `ClienteWsfe` está escrito contra la especificación pero **nunca habló con ARCA**. Pendiente: certificado de StockControl en el entorno (`Arca__Certificado` y `Arca__ClavePrivada` en PEM, o `Arca__CertificadoRuta` y `Arca__ClavePrivadaRuta`; `Arca__Ambiente` = homologacion | produccion), verificar el orden de campos del SOAP (`CondicionIVAReceptorId`), el tope de consumidor final, la fecha del comprobante y la delegación con un local real. Sin certificado el API arranca igual: facturar responde «ARCA no está configurado» y las NC quedan Pendientes.

**Decisiones:** nada de esto guarda secretos en la base, así que no usa Data Protection (el token WSAA vive en memoria). La emisión se serializa por local dentro del proceso: con más de una instancia del API hace falta un candado en la base.