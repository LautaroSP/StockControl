import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService, CajaDetalleDto, CajaListaDto, GrupoUnificableDto } from '../servicios/api.service';

@Component({
  selector: 'sc-cajas',
  standalone: true,
  imports: [FormsModule, RouterLink],
  template: `
    <div class="topbar">
      <h1>Cajas cerradas</h1>
      <a class="btn" routerLink="/informes">Ir a Informes</a>
    </div>
    <section class="panel" style="margin-bottom:16px">
      <div class="toolbar" style="padding:12px">
        <input type="date" [(ngModel)]="desde" />
        <input type="date" [(ngModel)]="hasta" />
        <select [(ngModel)]="quien">
          <option value="">Quién cerró: todos</option>
          @for (p of personas; track p) {
            <option [value]="p">{{ p }}</option>
          }
        </select>
        <select [(ngModel)]="medio">
          <option value="">Medio: todos</option>
          @for (m of medios; track m) {
            <option [value]="m">{{ m }}</option>
          }
        </select>
        <button class="btn btn-primary" type="button" (click)="buscar()">Buscar</button>
        <span class="meta">{{ total }} cierres</span>
      </div>
      @if (error) {
        <p class="error" style="padding:0 12px 12px">{{ error }}</p>
      }
      @if (unificables.length > 0) {
        <div style="padding:0 12px 12px">
          @for (g of unificables; track trackGrupo(g)) {
            <p class="meta" style="margin:0 0 8px">
              {{ g.idsCierre.length }} cierres · Caja {{ g.nroCaja }} · {{ g.nombreCierre }} · {{ g.dia }}
              <button class="btn btn-primary" type="button" style="margin-left:8px" (click)="unificar(g)">
                Unificar
              </button>
            </p>
          }
        </div>
      }
      @if (!cargando && total === 0 && !error) {
        <p class="meta" style="padding:0 12px 12px">No hay cajas con esos filtros.</p>
      }
      @if (total > 0) {
        <table class="data">
          <thead>
            <tr>
              <th>Puesto</th>
              <th>Fecha</th>
              <th>Quién</th>
              <th class="num">Tickets</th>
              <th class="num">Total</th>
            </tr>
          </thead>
          <tbody>
            @for (c of items; track c.idCierre) {
              <tr class="clickable" (click)="abrir(c.idCierre)">
                <td>{{ c.nroCaja }}</td>
                <td>{{ fechaCorta(c.fecha) }}</td>
                <td>{{ c.nombreCierre }}</td>
                <td class="num">{{ c.cantidadVentas }}</td>
                <td class="num">{{ dinero(c.total) }}</td>
              </tr>
            }
          </tbody>
        </table>
      }
    </section>

    @if (detalle) {
      <section class="panel">
        <div class="toolbar" style="padding:12px 12px 0">
          <strong style="font-size:14px">
            Caja {{ detalle.nroCaja }} · {{ detalle.nombreCierre }} · {{ fechaCorta(detalle.fecha) }}
          </strong>
          <button class="btn" type="button" (click)="detalle = null">Cerrar detalle</button>
        </div>
        <table class="data">
          <thead>
            <tr>
              <th>{{ detalle.tipoDesglose === 'Usuario' ? 'Usuario' : 'Medio de pago' }}</th>
              <th class="num">Cant. ventas</th>
              <th class="num">Total</th>
            </tr>
          </thead>
          <tbody>
            @for (f of filasDetalle(); track f.metodoPago) {
              <tr>
                <td>{{ f.metodoPago }}</td>
                <td class="num">{{ f.cantidadVentas }}</td>
                <td class="num">{{ dinero(f.total) }}</td>
              </tr>
            }
          </tbody>
        </table>
      </section>
    }
  `
})
export class CajasComponent implements OnInit {
  desde = '';
  hasta = '';
  quien = '';
  medio = '';
  personas: string[] = [];
  medios: string[] = [];
  items: CajaListaDto[] = [];
  unificables: GrupoUnificableDto[] = [];
  total = 0;
  detalle: CajaDetalleDto | null = null;
  error = '';
  cargando = false;

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    const hoy = hoyYmd();
    this.desde = primerDiaMesYmd();
    this.hasta = hoy;
    this.api.filtrosCajas().subscribe({
      next: (r) => {
        this.personas = r.personas;
        this.medios = r.medios;
      }
    });
    this.buscar();
  }

  trackGrupo(g: GrupoUnificableDto): string {
    return `${g.nroCaja}-${g.nombreCierre}-${g.dia}-${g.idsCierre.join(',')}`;
  }

  dinero(n: number): string {
    return `$ ${Number(n).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }

  fechaCorta(iso: string): string {
    return new Date(iso).toLocaleDateString('es-AR');
  }

  filasDetalle(): { metodoPago: string; cantidadVentas: number; total: number }[] {
    if (!this.detalle) return [];
    if (!this.medio) return this.detalle.filas;
    return this.detalle.filas.filter(
      (f) => f.metodoPago.localeCompare(this.medio, undefined, { sensitivity: 'accent' }) === 0
    );
  }

  buscar(): void {
    this.cargando = true;
    this.error = '';
    const idAbierto = this.detalle?.idCierre;
    this.api
      .consultaCajas({
        desde: this.desde || undefined,
        hasta: this.hasta || undefined,
        quien: this.quien || undefined,
        medio: this.medio || undefined
      })
      .subscribe({
        next: (r) => {
          this.items = r.items;
          this.total = r.total;
          this.unificables = r.unificables ?? [];
          this.cargando = false;
          if (idAbierto != null && !r.items.some((c) => c.idCierre === idAbierto)) {
            this.detalle = null;
          }
        },
        error: () => {
          this.error = 'No se pudieron cargar las cajas.';
          this.items = [];
          this.total = 0;
          this.unificables = [];
          this.detalle = null;
          this.cargando = false;
        }
      });
  }

  unificar(g: GrupoUnificableDto): void {
    const msg = `¿Unificar ${g.idsCierre.length} cierres de Caja ${g.nroCaja} (${g.nombreCierre}, ${g.dia})?`;
    if (!confirm(msg)) return;
    this.api.unificarCajas(g.idsCierre).subscribe({
      next: (r) => {
        this.error = '';
        this.detalle = null;
        this.buscar();
        this.abrir(r.idCierre);
      },
      error: (err) => (this.error = typeof err.error === 'string' ? err.error : 'No se pudo unificar.')
    });
  }

  abrir(idCierre: number): void {
    this.api.cierre(idCierre).subscribe({
      next: (d) => (this.detalle = d),
      error: () => (this.error = 'No se pudo abrir el detalle.')
    });
  }
}

function hoyYmd(): string {
  const d = new Date();
  return ymd(d.getFullYear(), d.getMonth() + 1, d.getDate());
}

function primerDiaMesYmd(): string {
  const d = new Date();
  return ymd(d.getFullYear(), d.getMonth() + 1, 1);
}

function ymd(y: number, m: number, day: number): string {
  return `${y}-${String(m).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
}
