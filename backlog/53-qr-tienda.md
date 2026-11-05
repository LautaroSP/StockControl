# Tienda: QR para el comprador

- **Roles:** dueño y socio
- **Tipo:** pantalla del dueño
- **Depende de:** 29 Tienda: URL pública y catálogo, 50 Tienda: panel

## Qué

En Tienda, el dueño genera un QR que al escanearlo abre la tienda del comprador (`/tienda/{alias}`). Sirve para imprimir o descargar y armar una tarjeta.

## Tamaños

El impreso es un afiche: fondo verde, carro, «TIENDA ONLINE» y el QR en un recuadro blanco. Abajo dice StockControl, con la misma letra que el título. Chico (80 mm), mediano (120 mm) y grande (160 mm) de ancho. La descarga es el PNG de ese afiche.

## POS

También se imprime en la impresora del ticket. Ahí el afiche mide siempre 50 mm de ancho: no sigue al tamaño de la hoja. El papel es el del ticket (58 o 80 mm). Si el ticket está en A4, la POS sale en 80 mm.

## Reglas

- Sin alias no hay QR: la pantalla lleva a Configuración.
- Dueño y socio. El empleado no entra.
- El QR es negro sobre blanco, también en modo oscuro, para que se pueda escanear.

## Criterios de aceptación

- [x] En Tienda hay QR de la tienda. Sin alias, avisa y lleva a Configuración.
- [x] El QR abre `/tienda/{alias}`.
- [x] Tamaños chico, mediano y grande, para imprimir o descargar.
- [x] El afiche tiene el recuadro verde y, abajo, StockControl con la misma letra que «TIENDA ONLINE».
- [x] Imprimir en la POS usa un solo tamaño (50 mm de ancho), en el papel del ticket.
- [x] Modo oscuro: el afiche se ve como se imprime, en verde.
- [x] Test de `ServicioQrTienda`: tamaños, POS fija, ruta y alias vacío.
