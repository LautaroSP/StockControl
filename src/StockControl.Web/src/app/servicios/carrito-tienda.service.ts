import { Injectable } from '@angular/core';
import { ItemCarritoTienda } from './api.service';

export const MAXIMO_POR_PRODUCTO = 99;

/** Carrito del comprador en el navegador, uno por tienda. Solo guarda producto y cantidad. */
@Injectable({ providedIn: 'root' })
export class CarritoTiendaService {
  items(alias: string): ItemCarritoTienda[] {
    try {
      const guardado = JSON.parse(localStorage.getItem(this.clave(alias)) ?? '[]');
      return Array.isArray(guardado)
        ? guardado.filter((i) => Number.isInteger(i?.idProducto) && Number.isInteger(i?.cantidad) && i.cantidad > 0)
        : [];
    } catch {
      return [];
    }
  }

  poner(alias: string, idProducto: number, cantidad: number): void {
    const resto = this.items(alias).filter((i) => i.idProducto !== idProducto);
    const valor = Math.min(MAXIMO_POR_PRODUCTO, Math.floor(cantidad));
    this.guardar(alias, valor > 0 ? [...resto, { idProducto, cantidad: valor }] : resto);
  }

  sacar(alias: string, idProducto: number): void {
    this.poner(alias, idProducto, 0);
  }

  vaciar(alias: string): void {
    localStorage.removeItem(this.clave(alias));
  }

  private guardar(alias: string, items: ItemCarritoTienda[]): void {
    if (items.length === 0) this.vaciar(alias);
    else localStorage.setItem(this.clave(alias), JSON.stringify(items));
  }

  private clave(alias: string): string {
    return `carrito-tienda:${alias}`;
  }
}
