import { Injectable, computed, signal } from '@angular/core';

const TOKEN = 'sc_token';
const ROL = 'sc_rol';
const NOMBRE = 'sc_nombre';
const LOCAL = 'sc_localId';
const LOCAL_NOMBRE = 'sc_localNombre';

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

@Injectable({ providedIn: 'root' })
export class SesionService {
  readonly token = signal(localStorage.getItem(TOKEN) ?? '');
  readonly rol = signal(localStorage.getItem(ROL) ?? '');
  readonly nombre = signal(localStorage.getItem(NOMBRE) ?? '');
  readonly idLocal = signal(Number(localStorage.getItem(LOCAL) || '0'));
  readonly nombreLocal = signal(localStorage.getItem(LOCAL_NOMBRE) ?? '');
  private carritoPendiente: LineaCarritoPendiente[] | null = null;

  readonly hayToken = computed(() => this.token().length > 0);
  readonly esDueno = computed(() => this.rol() === 'dueno' || this.rol() === 'admin');

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
  }

  limpiarLocal(): void {
    this.idLocal.set(0);
    this.nombreLocal.set('');
    localStorage.removeItem(LOCAL);
    localStorage.removeItem(LOCAL_NOMBRE);
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
}
