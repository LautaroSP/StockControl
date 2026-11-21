import { Injectable, signal } from '@angular/core';

/** Dice si hay internet. No alcanza con navigator.onLine: un fallo real de red (status 0) también la marca caída. */
@Injectable({ providedIn: 'root' })
export class ConexionService {
  readonly enLinea = signal(navigator.onLine);

  constructor() {
    window.addEventListener('online', () => this.enLinea.set(true));
    window.addEventListener('offline', () => this.enLinea.set(false));
  }

  marcarCaida(): void {
    this.enLinea.set(false);
  }

  marcarVuelta(): void {
    this.enLinea.set(true);
  }

  /** true si el error es de red (sin respuesta del servidor). */
  esFalloDeRed(err: { status?: number }): boolean {
    return err.status === 0;
  }
}
