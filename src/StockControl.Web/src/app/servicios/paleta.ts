const TINTA_OSCURA = '#1b241f';
const CONTRASTE = 4.5;

export const FONDO_STOCK = '#f4efe4';
export const FICHA_STOCK = '#fffdf8';
export const BOTON_STOCK = '#2a6b52';

export function hexDe(color: string | null | undefined): string {
  return /^#[0-9a-fA-F]{6}$/.test(color ?? '') ? (color ?? '').toLowerCase() : '';
}

/** Igual que ServicioPaletaTienda.TextoClaro: el fondo no alcanza contraste con la tinta oscura. */
export function textoClaro(fondo: string): boolean {
  return contraste(fondo, TINTA_OSCURA) < CONTRASTE;
}

export function tintaDe(color: { colorFondo: string | null; textoClaro: boolean }): string | null {
  if (!color.colorFondo) return null;
  return color.textoClaro ? '#ffffff' : '#1b241f';
}

export function tintaDeFicha(color: { colorFicha: string | null; textoClaroFicha: boolean }): string | null {
  if (!color.colorFicha) return null;
  return color.textoClaroFicha ? '#ffffff' : '#1b241f';
}

/** En oscuro la tienda usa la paleta de StockControl. */
export function colorSiClaro(tema: 'claro' | 'oscuro', color: string | null): string | null {
  return tema === 'oscuro' ? null : color;
}

function contraste(a: string, b: string): number {
  const clara = Math.max(luminancia(a), luminancia(b));
  const oscura = Math.min(luminancia(a), luminancia(b));
  return (clara + 0.05) / (oscura + 0.05);
}

function luminancia(hex: string): number {
  const canal = (desde: number) => {
    const c = parseInt(hex.slice(desde, desde + 2), 16) / 255;
    return c <= 0.04045 ? c / 12.92 : ((c + 0.055) / 1.055) ** 2.4;
  };
  return 0.2126 * canal(1) + 0.7152 * canal(3) + 0.0722 * canal(5);
}
