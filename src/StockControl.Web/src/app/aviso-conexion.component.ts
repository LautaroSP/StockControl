import { Component } from '@angular/core';
import { ColaVentasService } from './servicios/cola-ventas.service';
import { ConexionService } from './servicios/conexion.service';
import { DialogoService } from './servicios/dialogo.service';

/** Banner fijo: sin conexión, mandando ventas o ventas para revisar. Colores solo por variables CSS. */
@Component({
  selector: 'sc-aviso-conexion',
  standalone: true,
  template: `
    @if (!conexion.enLinea()) {
      <div class="aviso-abono aviso-conexion" role="status">
        Sin conexión · {{ texto(cola.porMandar()) }}
      </div>
    } @else if (cola.sincronizando()) {
      <div class="aviso-abono aviso-conexion" role="status">Mandando {{ texto(cola.porMandar()) }}…</div>
    } @else if (cola.porMandar() > 0) {
      <div class="aviso-abono aviso-conexion" role="status">{{ texto(cola.porMandar()) }}</div>
    } @else if (cola.recienSincronizada()) {
      <div class="aviso-abono aviso-conexion-ok" role="status">Ventas sincronizadas</div>
    }
    @if (cola.rechazadas().length > 0) {
      <div class="aviso-abono aviso-abono-vencido" role="alert">
        {{ cola.rechazadas().length }} {{ cola.rechazadas().length === 1 ? 'venta' : 'ventas' }} para revisar
        <button class="btn" type="button" (click)="verRechazadas = !verRechazadas">Revisar</button>
      </div>
      @if (verRechazadas) {
        <div class="panel pad">
          @for (v of cola.rechazadas(); track v.idVentaCliente) {
            <div class="row">
              <span class="grow">{{ hora(v.fechaCobro) }} · $ {{ v.total }} · {{ v.ultimoError }}</span>
              <button class="btn" type="button" (click)="cola.reintentar(v.idVentaCliente)">Reintentar</button>
              <button class="btn btn-danger" type="button" (click)="descartar(v.idVentaCliente)">Descartar</button>
            </div>
          }
        </div>
      }
    }
  `
})
export class AvisoConexionComponent {
  verRechazadas = false;

  constructor(
    readonly conexion: ConexionService,
    readonly cola: ColaVentasService,
    private readonly dialogo: DialogoService
  ) {}

  texto(n: number): string {
    return n === 1 ? '1 venta por mandar' : `${n} ventas por mandar`;
  }

  hora(iso: string): string {
    return new Date(iso).toLocaleString('es-AR', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' });
  }

  async descartar(id: string): Promise<void> {
    if (!await this.dialogo.confirmar('¿Descartar esta venta? No se va a mandar al servidor.')) return;
    await this.cola.descartar(id);
  }
}
