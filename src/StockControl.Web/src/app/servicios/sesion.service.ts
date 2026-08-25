import { Injectable, computed, signal } from '@angular/core';

const TOKEN = 'sc_token';
const ROL = 'sc_rol';
const NOMBRE = 'sc_nombre';
const LOCAL = 'sc_localId';
const LOCAL_NOMBRE = 'sc_localNombre';

@Injectable({ providedIn: 'root' })
export class SesionService {
  readonly token = signal(localStorage.getItem(TOKEN) ?? '');
  readonly rol = signal(localStorage.getItem(ROL) ?? '');
  readonly nombre = signal(localStorage.getItem(NOMBRE) ?? '');
  readonly idLocal = signal(Number(localStorage.getItem(LOCAL) || '0'));
  readonly nombreLocal = signal(localStorage.getItem(LOCAL_NOMBRE) ?? '');

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
}
