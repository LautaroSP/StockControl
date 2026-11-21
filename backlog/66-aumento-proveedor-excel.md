# Aumento por proveedor y lista en Excel

- **Roles:** dueño, socio
- **Tipo:** catálogo
- **Depende de:** 26 Proveedores, 62 Aumento de precio y costo

## Qué

El proveedor avisa «subimos 8 %». El dueño aplica el aumento a todos los productos de ese proveedor, con las mismas opciones de la 62 (precio y costo por separado, porcentaje o pesos). Es un alcance más del aumento: marcados, categoría o proveedor.

También puede subir la lista de precios del proveedor en Excel: código de barras y costo. No suma stock ni crea productos (eso ya lo hace el catálogo de la 26). Se cruza por el **código de barras** del producto. Muestra qué cambia, qué no se encontró y qué se saltea; al confirmar actualiza.

**Costo:** viene por el pack que ya está cargado en el proveedor (por ejemplo, caja de 12) y se divide por las unidades del pack. Sin pack, es por unidad.

**Precio:** se recalcula como al cargar el producto: costo × factor de ganancia del local × IVA. Si el producto tiene **ganancia propia**, se usa su factor: costo × su factor × IVA. Si está en un grupo, se actualiza el costo del grupo una vez y su precio con la ganancia del grupo (o la del local), y se copia a los miembros. El sector se saltea.

## Criterios de aceptación

- [x] Alcance «proveedor» en el aumento.
- [x] Subir Excel con código de barras y costo; vista previa con encontrados, no encontrados y salteados.
- [x] Costo por pack dividido por unidades.
- [x] Precio con el factor del local o el propio del producto; grupos una vez.
- [x] Tests xUnit.
