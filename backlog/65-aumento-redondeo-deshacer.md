# Aumento: redondeo, vista previa y deshacer

- **Roles:** dueño
- **Tipo:** catálogo
- **Depende de:** 62 Aumento de precio y costo

## Qué

Un 10 % deja precios como $ 1.996,50. Al aumentar, el dueño elige redondear el precio **siempre para arriba** al múltiplo de $ 10, $ 50 o $ 100 (o no redondear): $ 1.996,50 → $ 2.000. El costo no se redondea. El precio de un grupo se redondea igual.

Antes de aplicar, una vista previa: producto, precio y costo de antes y de después. Los grupos aparecen una vez.

Cada aumento queda guardado con fecha, usuario y los valores anteriores. Se puede deshacer **solo el último aumento**, y solo vuelven atrás los productos (o grupos) que nadie tocó después. Los que cambiaron se listan y quedan como están.

## Criterios de aceptación

- [x] Redondeo opcional a 10, 50 o 100, siempre para arriba, solo en el precio.
- [x] Vista previa antes de aplicar.
- [x] Historial de aumentos con los valores anteriores.
- [x] Deshacer el último aumento; los productos tocados después no vuelven y se avisan.
- [x] Tests xUnit.
