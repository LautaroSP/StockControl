import { DatePipe } from '@angular/common';
import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { toDataURL } from 'qrcode';
import { AbonoDto, ApiService } from '../servicios/api.service';
import { DialogoService } from '../servicios/dialogo.service';
import { SesionService } from '../servicios/sesion.service';

const INTERVALO_MS = 3000;
const MAX_CONSULTAS = 40;

@Component({
  selector: 'sc-activar',
  standalone: true,
  imports: [DatePipe, FormsModule, RouterLink],
  template: `
    <div class="topbar">
      <div>
        <h1>Abono del local</h1>
        <p class="meta">$ {{ pesos(abono()?.precioMensual ?? 0) }} por mes · {{ sesion.nombreLocal() }}</p>
      </div>
    </div>

    @if (abono(); as a) {
      <section class="panel pad" style="margin-bottom:14px">
        <p style="margin:0">
          <strong>{{ etiqueta(a) }}</strong>
          @if (a.vence) {
            · vence el {{ a.vence | date: 'dd/MM/yyyy HH:mm' }}
          }
        </p>
        @if (a.enGracia && a.limite) {
          <p class="error">No pudimos cobrar el débito. La caja se bloquea el {{ a.limite | date: 'dd/MM HH:mm' }}.</p>
        }
        @if (aviso()) {
          <p class="ok">{{ aviso() }}</p>
        }
        @if (consultando()) {
          <p class="meta">Esperando la confirmación de Mercado Pago…</p>
        }
      </section>

      <div class="activar-grid">
        <section class="panel pad">
          <h2>Pagar este mes</h2>
          @if (a.debito === 'activa') {
            <p>El débito automático cobra este mes. Cancelalo para pagar a mano.</p>
          } @else {
            <p class="meta">Con dinero en cuenta, QR, tarjeta o efectivo. Suma un mes desde tu vencimiento.</p>
            <button class="btn btn-primary" type="button" (click)="pagar()" [disabled]="ocupado()">Pagar con Mercado Pago</button>
            @if (qr()) {
              <div class="activar-qr">
                <img [src]="qr()" alt="QR para pagar con Mercado Pago" width="220" height="220" />
                <p class="meta">Escaneá con el celular o pagá desde esta compu.</p>
                <a class="btn btn-primary" [href]="urlPago()" target="_blank" rel="noopener">Abrir Mercado Pago</a>
              </div>
            }
          }
        </section>

        <section class="panel pad">
          <h2>Débito automático</h2>
          @switch (a.debito) {
            @case ('activa') {
              <p>Débito automático activo: se cobra solo cada mes con tu tarjeta.</p>
              <button class="btn" type="button" (click)="cancelarDebito()" [disabled]="ocupado()">Cancelar débito</button>
            }
            @case ('pendiente') {
              <p>Esperando que autorices en Mercado Pago.</p>
              <button class="btn" type="button" (click)="activarDebito()" [disabled]="ocupado()">Volver a abrir la autorización</button>
            }
            @default {
              @if (a.debito === 'pausada') {
                <p class="error">Mercado Pago pausó el débito: revisá la tarjeta o pagá este mes.</p>
              }
              <p class="meta">Autorizás una vez con tarjeta y Mercado Pago cobra $ {{ pesos(a.precioMensual) }} cada mes.</p>
              <label class="field">
                Mail de tu cuenta de Mercado Pago
                <input class="wide" type="email" name="mailPagador" [(ngModel)]="mailPagador" maxlength="200" />
              </label>
              <button class="btn btn-primary" type="button" (click)="activarDebito()" [disabled]="ocupado()">Activar débito automático</button>
            }
          }
        </section>
      </div>

      @if (error()) {
        <p class="error">{{ error() }}</p>
      }
      <p class="meta" style="margin-top:14px">
        ¿No tenés Mercado Pago?
        <a routerLink="/contacto">Contactá a ventas</a>
      </p>
    }
  `
})
export class ActivarComponent implements OnInit, OnDestroy {
  readonly abono = signal<AbonoDto | null>(null);
  readonly qr = signal('');
  readonly urlPago = signal('');
  readonly aviso = signal('');
  readonly error = signal('');
  readonly ocupado = signal(false);
  readonly consultando = signal(false);
  mailPagador = '';
  private temporizador?: ReturnType<typeof setInterval>;

  constructor(
    private readonly api: ApiService,
    readonly sesion: SesionService,
    private readonly ruta: ActivatedRoute,
    private readonly dialogo: DialogoService
  ) {}

  ngOnInit(): void {
    const vuelta = this.ruta.snapshot.queryParamMap;
    const volvioDeMp = vuelta.get('status') === 'approved' || vuelta.has('preapproval_id');
    this.api.abonoActual().subscribe({
      next: (a) => {
        this.mostrar(a);
        this.mailPagador = a.email;
        if (volvioDeMp || a.pagoPendiente || a.debito === 'pendiente') this.esperar(a);
      },
      error: () => this.error.set('No se pudo cargar el abono.')
    });
  }

  ngOnDestroy(): void {
    this.dejarDeEsperar();
  }

  pagar(): void {
    if (this.abono()?.debito === 'activa') return;
    this.iniciar(this.api.pagarAbono(), async (url) => {
      this.urlPago.set(url);
      this.qr.set(await toDataURL(url, { width: 220, margin: 2 }));
    });
  }

  activarDebito(): void {
    if (!this.mailPagador.trim()) {
      this.error.set('Ingresá el mail de tu cuenta de Mercado Pago.');
      return;
    }
    this.iniciar(this.api.activarDebito(this.mailPagador.trim()), async (url) => {
      window.location.href = url;
    });
  }

  async cancelarDebito(): Promise<void> {
    if (!await this.dialogo.confirmar('¿Cancelar el débito automático? El mes ya pagado sigue vigente.')) return;
    this.ocupado.set(true);
    this.api.cancelarDebito().subscribe({
      next: () => {
        this.ocupado.set(false);
        this.aviso.set('Débito automático cancelado.');
        this.recargar();
      },
      error: (err) => this.fallo(err)
    });
  }

  etiqueta(a: AbonoDto): string {
    switch (a.estado) {
      case 'trial':
        return `Prueba · ${a.diasRestantes} días`;
      case 'por_vencer':
        return `Por vencer · ${a.diasRestantes} días`;
      case 'vencido':
        return 'Vencido: no se puede cobrar';
      default:
        return 'Al día';
    }
  }

  pesos(n: number): string {
    return Math.trunc(n).toLocaleString('es-AR');
  }

  private iniciar(pedido: ReturnType<ApiService['pagarAbono']>, abrir: (url: string) => Promise<void>): void {
    this.error.set('');
    this.ocupado.set(true);
    pedido.subscribe({
      next: async (r) => {
        this.ocupado.set(false);
        await abrir(r.urlPago);
        const a = this.abono();
        if (a) this.esperar(a);
      },
      error: (err) => this.fallo(err)
    });
  }

  private esperar(inicial: AbonoDto): void {
    this.dejarDeEsperar();
    this.consultando.set(true);
    let consultas = 0;
    this.temporizador = setInterval(() => {
      if (++consultas > MAX_CONSULTAS) {
        this.dejarDeEsperar();
        return;
      }
      this.api.abonoActual().subscribe((a) => {
        if (a.vence !== inicial.vence) {
          this.aviso.set(`Pago acreditado. Vence el ${new Date(a.vence!).toLocaleString('es-AR')}.`);
        } else if (a.debito === 'activa' && inicial.debito !== 'activa') {
          this.aviso.set('Débito automático activo.');
        } else {
          return;
        }
        this.mostrar(a);
        this.dejarDeEsperar();
      });
    }, INTERVALO_MS);
  }

  private dejarDeEsperar(): void {
    if (this.temporizador) clearInterval(this.temporizador);
    this.temporizador = undefined;
    this.consultando.set(false);
  }

  private recargar(): void {
    this.api.abonoActual().subscribe((a) => this.mostrar(a));
  }

  private mostrar(a: AbonoDto): void {
    this.abono.set(a);
    this.sesion.abono.set(a);
    if (a.debito === 'activa') {
      this.qr.set('');
      this.urlPago.set('');
    }
  }

  private fallo(err: { error?: unknown }): void {
    this.ocupado.set(false);
    this.error.set(typeof err.error === 'string' ? err.error : 'No se pudo conectar con Mercado Pago. Probá de nuevo.');
  }
}
