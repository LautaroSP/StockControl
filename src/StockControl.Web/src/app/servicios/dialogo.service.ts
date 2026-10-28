import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class DialogoService {
  readonly visible = signal(false);
  mensaje = '';
  valor = '';
  conCampo = false;
  oculto = false;
  textoAceptar = 'Aceptar';
  textoCancelar = 'Cancelar';
  textoOtro = '';
  private pendiente: ((valor: string | null) => void) | null = null;

  confirmar(mensaje: string, aceptar = 'Aceptar', cancelar = 'Cancelar'): Promise<boolean> {
    this.textoAceptar = aceptar;
    this.textoCancelar = cancelar;
    return this.abrir(mensaje, false, '', false).then((valor) => valor !== null);
  }

  /** Dos opciones más Cancelar (o Escape, que devuelve null). */
  elegir(mensaje: string, aceptar: string, otro: string, cancelar = 'Cancelar'): Promise<'aceptar' | 'otro' | null> {
    this.textoAceptar = aceptar;
    this.textoCancelar = cancelar;
    const respuesta = this.abrir(mensaje, false, '', false);
    this.textoOtro = otro;
    return respuesta.then((valor) => (valor === null ? null : valor === 'otro' ? 'otro' : 'aceptar'));
  }

  otro(): void {
    if (!this.pendiente) return;
    this.cerrar('otro');
  }

  pedir(mensaje: string, inicial = '', oculto = false): Promise<string | null> {
    return this.abrir(mensaje, true, inicial, oculto);
  }

  escribir(evento: Event): void {
    this.valor = (evento.target as HTMLInputElement).value;
  }

  aceptar(): void {
    if (!this.pendiente) return;
    this.cerrar(this.conCampo ? this.valor : '');
  }

  cancelar(): void {
    if (!this.pendiente) return;
    this.cerrar(null);
  }

  private abrir(mensaje: string, conCampo: boolean, inicial: string, oculto: boolean): Promise<string | null> {
    this.mensaje = mensaje;
    this.textoOtro = '';
    if (conCampo) {
      this.textoAceptar = 'Aceptar';
      this.textoCancelar = 'Cancelar';
    }
    this.conCampo = conCampo;
    this.oculto = oculto;
    this.valor = inicial;
    this.visible.set(true);
    return new Promise((resolve) => (this.pendiente = resolve));
  }

  private cerrar(valor: string | null): void {
    const resolver = this.pendiente;
    this.pendiente = null;
    this.visible.set(false);
    resolver?.(valor);
  }
}
