import { Injectable, signal } from '@angular/core';

const CLAVE = 'sc_tema';

@Injectable({ providedIn: 'root' })
export class TemaService {
  readonly tema = signal<'claro' | 'oscuro'>('claro');

  aplicar(): void {
    const guardado = localStorage.getItem(CLAVE);
    if (guardado === 'claro' || guardado === 'oscuro') {
      this.tema.set(guardado);
    } else if (window.matchMedia('(prefers-color-scheme: dark)').matches) {
      this.tema.set('oscuro');
    } else {
      this.tema.set('claro');
    }
    document.documentElement.setAttribute('data-tema', this.tema());
  }

  elegido(): boolean {
    const guardado = localStorage.getItem(CLAVE);
    return guardado === 'claro' || guardado === 'oscuro';
  }

  /** Cambia lo que se ve sin guardarlo como elección de la persona. */
  mostrar(tema: 'claro' | 'oscuro'): void {
    this.tema.set(tema);
    document.documentElement.setAttribute('data-tema', tema);
  }

  alternar(): void {
    const siguiente = this.tema() === 'oscuro' ? 'claro' : 'oscuro';
    this.tema.set(siguiente);
    localStorage.setItem(CLAVE, siguiente);
    document.documentElement.setAttribute('data-tema', siguiente);
  }
}
