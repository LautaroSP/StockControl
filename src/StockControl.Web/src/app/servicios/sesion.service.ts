import { Injectable, computed, signal } from '@angular/core';
import type { PagoVenta } from './api.service';

const TOKEN = 'sc_token';
const ROL = 'sc_rol';
const NOMBRE = 'sc_nombre';
const LOCAL = 'sc_localId';
const LOCAL_NOMBRE = 'sc_localNombre';
const CAJA = 'sc_nroCaja';

export interface LineaCarritoPendiente {
  idProducto: number;
  codigo: string;
  nombre: string;
  cantidad: number;
  precioUnitario: number;
  costo: number;
  sector: boolean;
  generico: boolean;
}

export interface CarritoCaja {
  id: number;
  lineas: LineaCarritoPendiente[];
  pagos: (PagoVenta & { resto: boolean })[];
  usarDesc: boolean;
  descuento: number;
  alCosto: boolean;
  imprimirTicket: boolean;
}

@Injectable({ providedIn: 'root' })
export class SesionService {
  readonly token = signal(localStorage.getItem(TOKEN) ?? '');
  readonly rol = signal(localStorage.getItem(ROL) ?? '');
  readonly nombre = signal(localStorage.getItem(NOMBRE) ?? '');
  readonly idLocal = signal(Number(localStorage.getItem(LOCAL) || '0'));
  readonly nombreLocal = signal(localStorage.getItem(LOCAL_NOMBRE) ?? '');
  readonly nroCaja = signal(Number(localStorage.getItem(CAJA) || '0'));
  private carritoPendiente: LineaCarritoPendiente[] | null = null;
  private carritos: CarritoCaja[] | null = null;
  private indiceCarrito = 0;

  readonly hayToken = computed(() => this.token().length > 0);
  readonly esDueno = computed(() => this.rol() === 'dueno' || this.rol() === 'socio' || this.rol() === 'admin');
  readonly esDuenoTitular = computed(() => this.rol() === 'dueno' || this.rol() === 'admin');
  readonly puedeGestionarUsuarios = computed(() => this.rol() === 'dueno' || this.rol() === 'socio' || this.rol() === 'admin');
  readonly puedeConfigurarLocal = computed(() => this.rol() === 'dueno' || this.rol() === 'socio');
  readonly hayCaja = computed(() => this.nroCaja() > 0);

  guardarLogin(token: string, rol: string, nombre: string): void {
    this.token.set(token);
    this.rol.set(rol);
    this.nombre.set(nombre);
    localStorage.setItem(TOKEN, token);
    localStorage.setItem(ROL, rol);
    localStorage.setItem(NOMBRE, nombre);
    this.limpiarLocal();
  }

  guardarLocal(token: string, idLocal: number, nombreLocal: string): void {
    this.token.set(token);
    this.idLocal.set(idLocal);
    this.nombreLocal.set(nombreLocal);
    localStorage.setItem(TOKEN, token);
    localStorage.setItem(LOCAL, String(idLocal));
    localStorage.setItem(LOCAL_NOMBRE, nombreLocal);
    this.limpiarCaja();
  }

  guardarCaja(token: string, nroCaja: number): void {
    this.token.set(token);
    this.nroCaja.set(nroCaja);
    localStorage.setItem(TOKEN, token);
    localStorage.setItem(CAJA, String(nroCaja));
  }

  limpiarCaja(): void {
    this.nroCaja.set(0);
    localStorage.removeItem(CAJA);
    this.carritos = null;
    this.indiceCarrito = 0;
  }

  limpiarLocal(): void {
    this.idLocal.set(0);
    this.nombreLocal.set('');
    localStorage.removeItem(LOCAL);
    localStorage.removeItem(LOCAL_NOMBRE);
    this.limpiarCaja();
  }

  salir(): void {
    this.token.set('');
    this.rol.set('');
    this.nombre.set('');
    this.limpiarLocal();
    localStorage.removeItem(TOKEN);
    localStorage.removeItem(ROL);
    localStorage.removeItem(NOMBRE);
  }

  dejarCarritoPendiente(lineas: LineaCarritoPendiente[]): void {
    this.carritoPendiente = lineas;
  }

  tomarCarritoPendiente(): LineaCarritoPendiente[] | null {
    const p = this.carritoPendiente;
    this.carritoPendiente = null;
    return p;
  }

  tomarCarritos(): { carritos: CarritoCaja[]; indice: number } | null {
    if (!this.carritos) return null;
    return { carritos: this.carritos, indice: this.indiceCarrito };
  }

  guardarCarritos(carritos: CarritoCaja[], indice: number): void {
    this.carritos = carritos;
    this.indiceCarrito = indice;
  }
}
