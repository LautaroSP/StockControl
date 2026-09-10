import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService, CajaDetalleDto, CajaListaDto, GrupoUnificableDto, VentaDetalleDto, VentaListaDto } from '../servicios/api.service';
import { SesionService } from '../servicios/sesion.service';

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
        <input type="text" inputmode="numeric" placeholder="dd/mm/aaaa" [(ngModel)]="desde" />
        <input type="text" inputmode="numeric" placeholder="dd/mm/aaaa" [(ngModel)]="hasta" />
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
                <td>{{ fechaHora(c.fecha) }}</td>
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
             Caja {{ detalle.nroCaja }} · {{ detalle.nombreCierre }} · {{ fechaHora(detalle.fecha) }}
          </strong>
          <button class="btn" type="button" (click)="cerrarDetalle()">Cerrar detalle</button>
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
        <div class="toolbar" style="padding:12px">
          <strong style="font-size:14px">Ventas de esta caja</strong>
        </div>
        @if (ventas.length === 0) {
          <p class="meta" style="padding:0 12px 12px">No hay ventas en este cierre.</p>
        } @else {
          <table class="data">
            <thead>
              <tr>
                <th>Hora</th>
                <th class="num">Total</th>
                <th>Método</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              @for (v of ventas; track v.idInformeVenta) {
                <tr>
                  <td>{{ hora(v.fecha) }}</td>
                  <td class="num">{{ dinero(v.total) }}</td>
                  <td>{{ v.metodoPago }}</td>
                  <td><button class="btn" type="button" (click)="verVenta(v.idInformeVenta)">Ver detalles</button></td>
                </tr>
              }
            </tbody>
          </table>
        }
      </section>
    }

    @if (venta) {
      <div class="modal-back show">
        <div class="modal" style="max-width:520px">
          <h2 style="margin:0 0 8px;font-size:18px">Venta {{ hora(venta.fecha) }}</h2>
          <p class="meta">{{ venta.metodoPago }} · {{ dinero(venta.total) }}</p>
          <table class="data">
            <thead>
              <tr>
                <th>Producto</th>
                <th class="num">Cant</th>
                @if (sesion.esDueno()) {
                  <th class="num">Costo</th>
                }
                <th class="num">Subt</th>
              </tr>
            </thead>
            <tbody>
              @for (i of venta.items; track i.idInformeVentaDetalle) {
                <tr>
                  <td>{{ i.nombre }}</td>
                  <td class="num">{{ i.cantidad }}</td>
                  @if (sesion.esDueno()) {
                    <td class="num">{{ i.costo == null ? '—' : dinero(i.costo) }}</td>
                  }
                  <td class="num">{{ dinero(i.subTotal) }}</td>
                </tr>
              }
            </tbody>
          </table>
          <div class="row" style="margin-top:12px">
            <button class="btn grow" type="button" (click)="venta = null">Cerrar</button>
          </div>
        </div>
      </div>
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
  ventas: VentaListaDto[] = [];
  venta: VentaDetalleDto | null = null;
  error = '';
  cargando = false;

  constructor(private readonly api: ApiService, readonly sesion: SesionService) {}

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

  fechaHora(iso: string): string {
    const d = new Date(iso);
    return `${String(d.getDate()).padStart(2, '0')}/${String(d.getMonth() + 1).padStart(2, '0')}/${d.getFullYear()} ${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`;
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
    const desde = fechaIso(this.desde);
    const hasta = fechaIso(this.hasta);
    if (!desde || !hasta) {
      this.error = 'Las fechas deben tener formato dd/mm/aaaa.';
      this.cargando = false;
      return;
    }
    this.api
      .consultaCajas({
        desde,
        hasta,
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
            this.cerrarDetalle();
          }
        },
        error: () => {
          this.error = 'No se pudieron cargar las cajas.';
          this.items = [];
          this.total = 0;
          this.unificables = [];
          this.cerrarDetalle();
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
        this.cerrarDetalle();
        this.buscar();
        this.abrir(r.idCierre);
      },
      error: (err) => (this.error = typeof err.error === 'string' ? err.error : 'No se pudo unificar.')
    });
  }

  abrir(idCierre: number): void {
    this.api.cierre(idCierre).subscribe({
      next: (d) => {
        this.detalle = d;
        this.venta = null;
        this.cargarVentas(idCierre);
      },
      error: () => (this.error = 'No se pudo abrir el detalle.')
    });
  }

  cargarVentas(idCierre: number): void {
    this.api.ventas({ idCierre, tamano: 200 }).subscribe({
      next: (r) => (this.ventas = r.items),
      error: () => {
        this.ventas = [];
        this.error = 'No se pudieron cargar las ventas de la caja.';
      }
    });
  }

  verVenta(id: number): void {
    this.api.venta(id).subscribe({
      next: (d) => (this.venta = d),
      error: () => (this.error = 'No se pudo abrir la venta.')
    });
  }

  hora(iso?: string): string {
    if (!iso) return '';
    return new Date(iso).toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' });
  }

  cerrarDetalle(): void {
    this.detalle = null;
    this.ventas = [];
    this.venta = null;
  }
}

function hoyYmd(): string {
  const d = new Date();
  return dmy(d.getFullYear(), d.getMonth() + 1, d.getDate());
}

function primerDiaMesYmd(): string {
  const d = new Date();
  return dmy(d.getFullYear(), d.getMonth() + 1, 1);
}

function dmy(y: number, m: number, day: number): string {
  return `${String(day).padStart(2, '0')}/${String(m).padStart(2, '0')}/${y}`;
}

function fechaIso(valor: string): string | null {
  const partes = valor.trim().split('/');
  if (partes.length !== 3) return null;
  const [dia, mes, anio] = partes.map(Number);
  const fecha = new Date(anio, mes - 1, dia);
  if (!dia || !mes || !anio || fecha.getFullYear() !== anio || fecha.getMonth() !== mes - 1 || fecha.getDate() !== dia) return null;
  return `${anio}-${String(mes).padStart(2, '0')}-${String(dia).padStart(2, '0')}`;
}
