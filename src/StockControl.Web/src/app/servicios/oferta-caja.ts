import { ComboVigenteDto, OfertaVigenteDto } from './api.service';

const redondear = (n: number) => Math.round(n * 100) / 100;

export function precioConOferta(oferta: OfertaVigenteDto | undefined, precioLista: number): number {
  if (oferta?.tipo !== 'descuento') return precioLista;
  return redondear((precioLista * (100 - oferta.porcentaje)) / 100);
}

/** Precio de una unidad con la oferta. Las de precio por cantidad dependen de cuántas se llevan. */
export function precioUnitarioConOferta(oferta: OfertaVigenteDto | undefined, precioLista: number, cantidad: number): number {
  if (oferta?.tipo === 'precioDesde') return cantidad >= oferta.lleva ? oferta.precioUnidad : precioLista;
  if (oferta?.tipo === 'precioExacto') return cantidad === oferta.lleva ? oferta.precioUnidad : precioLista;
  return precioConOferta(oferta, precioLista);
}

export function cantidadCobrada(oferta: OfertaVigenteDto | undefined, cantidad: number): number {
  if (oferta?.tipo !== 'llevaPaga') return cantidad;
  const grupos = Math.floor(cantidad / oferta.lleva);
  return grupos * oferta.paga + (cantidad - grupos * oferta.lleva);
}

export interface LineaParaCombo {
  idProducto: number;
  cantidad: number;
  subtotal: number;
}

/** Lo que se descuenta por los combos completos, solo si le conviene al cliente. Igual que el servidor. */
export function descuentoCombos(combos: ComboVigenteDto[], lineas: LineaParaCombo[]): number {
  const trabajo = lineas.map((l) => ({ ...l }));
  return redondear(combos.reduce((suma, combo) => suma + descuentoDeUnCombo(combo, trabajo), 0));
}

function descuentoDeUnCombo(combo: ComboVigenteDto, lineas: LineaParaCombo[]): number {
  const completos = Math.min(
    ...combo.lineas.map((c) => Math.floor(lineas.filter((l) => l.idProducto === c.idProducto).reduce((n, l) => n + l.cantidad, 0) / c.cantidad))
  );
  if (!(completos > 0)) return 0;
  const tomadas = tomar(combo, lineas, completos);
  const valor = tomadas.reduce((n, t) => n + t.valor, 0);
  const descuento = redondear(valor - completos * combo.precioTotal);
  if (descuento <= 0) return 0;
  repartir(tomadas, descuento, valor);
  return descuento;
}

function tomar(combo: ComboVigenteDto, lineas: LineaParaCombo[], completos: number): { linea: LineaParaCombo; valor: number }[] {
  const tomadas: { linea: LineaParaCombo; valor: number }[] = [];
  for (const c of combo.lineas) {
    let faltan = c.cantidad * completos;
    for (const l of lineas.filter((x) => x.idProducto === c.idProducto && x.cantidad > 0)) {
      if (faltan <= 0) break;
      const unidades = Math.min(faltan, l.cantidad);
      tomadas.push({ linea: l, valor: (unidades * l.subtotal) / l.cantidad });
      faltan -= unidades;
    }
  }
  return tomadas;
}

function repartir(tomadas: { linea: LineaParaCombo; valor: number }[], descuento: number, valor: number): void {
  let restante = descuento;
  tomadas.forEach((t, i) => {
    const parte = i === tomadas.length - 1 ? restante : redondear((descuento * t.valor) / valor);
    t.linea.subtotal = Math.max(0, t.linea.subtotal - parte);
    restante -= parte;
  });
}
