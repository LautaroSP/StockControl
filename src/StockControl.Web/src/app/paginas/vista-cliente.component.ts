import { Component, OnDestroy, OnInit, computed, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { SesionService } from '../servicios/sesion.service';
import { TemaService } from '../servicios/tema.service';
import { VistaClienteService } from '../servicios/vista-cliente.service';

const SIN_LATIDO_MS = 8000;
const RESALTADO_MS = 3000;

@Component({
  selector: 'sc-vista-cliente',
  standalone: true,
  template: `
    <main class="vc">
      <header class="vc-top">
        <strong>{{ sesion.nombreLocal() }}</strong>
        <button class="btn" type="button" (click)="pantallaCompleta()">Pantalla completa</button>
      </header>
      @if (estado(); as e) {
        @if (e.tipo === 'gracias') {
          <section class="vc-centro">
            <h1 class="vc-gracias">¡Gracias!</h1>
            <p class="vc-total">{{ dinero(e.total) }}</p>
            <p class="vc-medio">{{ e.medio }}</p>
          </section>
        } @else if (e.tipo === 'cobrando') {
          <section class="vc-centro">
            <p class="vc-medio">Escaneá para pagar</p>
            <img class="vc-qr" [src]="e.qrDataUrl" alt="QR de Mercado Pago" />
            <p class="vc-total">{{ dinero(e.total) }}</p>
          </section>
        } @else if (e.lineas.length === 0) {
          <section class="vc-centro"><h1 class="vc-espera">{{ sesion.nombreLocal() }}</h1></section>
        } @else {
          <section class="vc-lineas">
            @for (l of e.lineas; track $index) {
              <div class="vc-linea" [class.resaltada]="resaltada($index)">
                <div>
                  <div class="vc-nombre">{{ l.nombre }}</div>
                  @if (l.oferta) { <div class="vc-oferta">{{ l.oferta }}</div> }
                </div>
                <div class="vc-cant">{{ l.cantidad }} × {{ dinero(l.precioUnitario) }}</div>
                <div class="vc-sub">{{ dinero(l.subtotal) }}</div>
              </div>
            }
          </section>
          <footer class="vc-pie">
            @if (e.descuentoPorcentaje > 0) { <p class="vc-oferta">Descuento {{ e.descuentoPorcentaje }} %</p> }
            @if (e.ahorroCombos > 0) { <p class="vc-oferta">Combo: ahorrás {{ dinero(e.ahorroCombos) }}</p> }
            <div class="vc-total-fila"><span>Total</span><strong class="vc-total">{{ dinero(e.total) }}</strong></div>
          </footer>
        }
      } @else {
        <section class="vc-centro"><h1 class="vc-espera">{{ sesion.nombreLocal() }}</h1></section>
      }
    </main>
  `,
  styles: [`
    .vc { min-height: 100vh; display: flex; flex-direction: column; background: var(--paper); color: var(--ink); padding: 24px 40px; font-size: 28px; }
    .vc-top { display: flex; justify-content: space-between; align-items: center; font-size: 32px; }
    .vc-top .btn { font-size: 14px; opacity: .5; }
    .vc-centro { flex: 1; display: flex; flex-direction: column; align-items: center; justify-content: center; text-align: center; gap: 16px; }
    .vc-espera { font-size: 64px; color: var(--ink-soft); }
    .vc-gracias { font-size: 96px; color: var(--ok); }
    .vc-qr { width: min(60vh, 80vw); height: min(60vh, 80vw); background: #fff; image-rendering: pixelated; }
    .vc-lineas { flex: 1; margin-top: 16px; overflow: auto; }
    .vc-linea { display: grid; grid-template-columns: 1fr auto 200px; gap: 24px; align-items: center; padding: 14px 12px; border-bottom: 1px solid var(--line); }
    .vc-linea.resaltada { background: var(--table-selected); }
    .vc-nombre { font-weight: 600; }
    .vc-oferta { font-size: 22px; color: var(--charge); }
    .vc-cant { color: var(--ink-soft); }
    .vc-sub { text-align: right; font-weight: 600; }
    .vc-pie { border-top: 3px solid var(--line); padding-top: 12px; }
    .vc-total-fila { display: flex; justify-content: space-between; align-items: baseline; font-size: 44px; }
    .vc-total { font-size: 88px; font-weight: 700; }
    .vc-medio { font-size: 36px; color: var(--ink-soft); }
  `]
})
export class VistaClienteComponent implements OnInit, OnDestroy {
  private readonly ahora = signal(Date.now());
  private reloj: ReturnType<typeof setInterval> | null = null;

  readonly estado = computed(() => {
    const actual = this.vista.estado();
    return actual && this.ahora() - this.vista.recibidoEn() < SIN_LATIDO_MS ? actual : null;
  });

  constructor(
    readonly sesion: SesionService,
    private readonly vista: VistaClienteService,
    private readonly ruta: ActivatedRoute,
    private readonly tema: TemaService
  ) {
    this.tema.aplicar();
  }

  ngOnInit(): void {
    const caja = Number(this.ruta.snapshot.queryParamMap.get('caja') || '0');
    this.vista.abrir(this.sesion.idLocal(), caja);
    this.vista.escuchar();
    this.reloj = setInterval(() => this.ahora.set(Date.now()), 500);
  }

  ngOnDestroy(): void {
    if (this.reloj) clearInterval(this.reloj);
    this.vista.cerrar();
  }

  resaltada(indice: number): boolean {
    const e = this.estado();
    return !!e && e.ultimaLinea === indice && this.ahora() - e.marcaUltima < RESALTADO_MS;
  }

  pantallaCompleta(): void {
    void document.documentElement.requestFullscreen();
  }

  dinero(n: number): string {
    return `$ ${Number(n).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }
}
