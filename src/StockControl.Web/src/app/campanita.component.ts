import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AvisoDto, ApiService } from './servicios/api.service';
import { AvisosService } from './servicios/avisos.service';
import { IconoComponent } from './icono.component';

@Component({
  selector: 'sc-campanita',
  standalone: true,
  imports: [IconoComponent],
  template: `
    <button class="campanita" type="button" (click)="alternar()" [attr.aria-expanded]="abierta"
      [attr.aria-label]="avisos.noLeidos() ? avisos.noLeidos() + ' avisos sin leer' : 'Avisos'">
      <sc-icono nombre="campana" />
      @if (avisos.noLeidos()) { <span class="contador">{{ avisos.noLeidos() > 99 ? '99+' : avisos.noLeidos() }}</span> }
    </button>
    @if (abierta) {
      <div class="campanita-fondo" (click)="abierta = false"></div>
      <section class="campanita-lista panel" role="dialog" aria-label="Avisos">
        <div class="row campanita-cabecera">
          <strong class="grow">Avisos</strong>
          <button class="btn" type="button" [disabled]="!avisos.noLeidos()" (click)="marcarTodas()">Marcar todas</button>
        </div>
        @for (a of lista; track a.id) {
          <button class="campanita-item" type="button" [class.no-leido]="!a.leido" (click)="abrir(a)">
            <span>{{ a.texto }}</span>
            <span class="meta">{{ hora(a.creado) }}</span>
          </button>
        } @empty {
          <p class="meta pad">{{ cargando ? 'Cargando…' : 'No hay avisos.' }}</p>
        }
      </section>
    }
  `,
  styles: `
    :host { display: inline-flex; position: relative; }
    .campanita {
      position: relative;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 36px;
      height: 32px;
      border: 1px solid rgba(255,255,255,.2);
      border-radius: 8px;
      background: transparent;
      color: var(--sidebar-text);
      cursor: pointer;
    }
    .contador {
      position: absolute;
      top: -6px;
      right: -8px;
      min-width: 18px;
      padding: 1px 5px;
      border-radius: 99px;
      background: var(--charge);
      color: #fff;
      font-size: 11px;
      font-weight: 700;
      line-height: 16px;
    }
    .campanita-fondo { position: fixed; inset: 0; z-index: 49; }
    .campanita-lista {
      position: fixed;
      top: 64px;
      left: 16px;
      z-index: 50;
      width: min(360px, calc(100vw - 32px));
      max-height: min(480px, calc(100vh - 96px));
      overflow-y: auto;
      color: var(--ink);
    }
    .campanita-cabecera { align-items: center; padding: 10px 12px; border-bottom: 1px solid var(--line); }
    .campanita-item {
      display: flex;
      flex-direction: column;
      gap: 2px;
      width: 100%;
      padding: 10px 12px;
      border: 0;
      border-bottom: 1px solid var(--table-row);
      background: transparent;
      color: inherit;
      font: inherit;
      font-size: 13px;
      text-align: left;
      cursor: pointer;
    }
    .campanita-item:hover { background: var(--table-hover); }
    .campanita-item.no-leido { background: var(--table-selected); font-weight: 600; }
    @media (max-width: 980px) {
      .campanita-lista { left: auto; right: 8px; top: 56px; }
    }
  `
})
export class CampanitaComponent {
  abierta = false;
  cargando = false;
  lista: AvisoDto[] = [];

  constructor(
    readonly avisos: AvisosService,
    private readonly api: ApiService,
    private readonly router: Router
  ) {}

  alternar(): void {
    this.abierta = !this.abierta;
    if (this.abierta) this.cargar();
  }

  abrir(a: AvisoDto): void {
    this.abierta = false;
    if (!a.leido) this.api.marcarAvisoLeido(a.id).subscribe({ next: () => this.avisos.consultar() });
    void this.router.navigate(['/mi-tienda/pedidos'], { queryParams: { id: a.idPedidoTienda } });
  }

  marcarTodas(): void {
    this.api.marcarAvisosLeidos().subscribe({
      next: () => {
        this.lista = this.lista.map((a) => ({ ...a, leido: true }));
        this.avisos.consultar();
      }
    });
  }

  hora(fecha: string): string {
    return new Date(fecha).toLocaleString('es-AR', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' });
  }

  private cargar(): void {
    this.cargando = true;
    this.api.avisos().subscribe({
      next: (r) => {
        this.lista = r;
        this.cargando = false;
      },
      error: () => (this.cargando = false)
    });
  }
}
