# Tienda online: imágenes de producto

- **Roles:** dueño y socio (eligen) · comprador (ve)
- **Tipo:** sugerencia de imágenes + carga de archivos + vista pública
- **Depende de:** 29 Tienda: URL pública y catálogo, 11 Productos (código de barras)

## Qué

Cada producto publicado puede tener hasta **2 imágenes**. En el panel de la tienda hay un apartado de imagen por producto. El sistema **propone** imágenes a partir del código de barras; el dueño las acepta o sube las propias.

## Apartado de imagen (panel de la tienda)

Por cada producto publicado:

1. **Aviso fijo:** «Generamos imágenes a partir de Open Food Facts, una base abierta hecha por usuarios (licencia CC-BY-SA). Pueden no coincidir con tu producto.»
2. **Propuesta:** «Te ofrecemos estas imágenes para este producto», con la vista previa.
3. Dos botones: **Aceptar** | **Subir propia**.

- Sin código de barras, o sin resultado: no hay propuesta; solo «Subir propia».
- **Rechazar** una propuesta es subir una propia; no se vuelve a ofrecer la rechazada.
- Aceptar o subir una propia reemplaza la anterior.
- Nada se publica sin que el dueño acepte. La propuesta no es automática.

## Propuesta por código de barras

- Consulta de **un producto por vez**, cuando el dueño abre el apartado. No hay carga masiva.
- La consulta sale del **servidor** a Open Food Facts (`/api/v3/product/{ean}.json`), con `User-Agent` identificable y rate limit. Nunca desde el navegador del comprador.
- Se usa la foto frontal (`selected_images`).
- Si la fuente no responde, se muestra «Subir propia» y el apartado sigue andando.
- Al aceptar, el servidor **descarga y guarda** la imagen; no se enlaza la externa.
- Se guarda el origen y la atribución (fuente, enlace, licencia). Se muestra en el pie de la ficha o de la tienda.

## Reglas de las imágenes

- Máximo 2 por producto. Se guardan en **400 × 400 px** como máximo (se redimensionan en el servidor).
- Vale igual para la propuesta aceptada y para la propia: tipo **real** (JPG, PNG, WebP), peso máximo.
- Se guardan en un volumen, fuera de git y fuera de la base. La base guarda la ruta y la atribución.
- El nombre en disco lo genera el servidor; nunca el del archivo subido.
- Una imagen solo se sirve si su producto está publicado en una tienda activa.
- Sin imagen: marcador.

## Criterios de aceptación

- [ ] Cada producto publicado tiene un apartado de imagen con el aviso de origen.
- [ ] Con código de barras, se propone la imagen de Open Food Facts; con **Aceptar** el servidor la descarga y la guarda.
- [ ] **Subir propia** permite cargar un archivo; se rechaza lo que no sea JPG, PNG o WebP reales o pese de más.
- [ ] Sin código de barras o sin resultado, solo «Subir propia».
- [ ] Si Open Food Facts no responde, el apartado sigue funcionando.
- [ ] Hasta 2 imágenes por producto, en 400 × 400 px como máximo.
- [ ] La atribución de la imagen aceptada se muestra en la tienda.
- [ ] Una propuesta rechazada no se vuelve a ofrecer.
- [ ] Una imagen de producto no publicado o de otro local no se sirve.
- [ ] Tests xUnit: propuesta solo con código de barras, aceptar guarda con atribución, rechazar no la repite, tope de 2, tipo real inválido, peso máximo, imagen de producto no publicado no disponible, caída de la fuente no rompe.

## Fuera de v1

- Carga masiva de propuestas.
- Otras fuentes (UPCitemdb, GS1 Argentina).
- Generar imágenes con IA.
