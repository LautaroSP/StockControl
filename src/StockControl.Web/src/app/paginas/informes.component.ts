import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import {
  ApiService,
  CajaDetalleDto,
  CajaListaDto,
  VentaDetalleDto,
  VentaListaDto
} from '../servicios/api.service';
import { SesionService } from '../servicios/sesion.service';

@Component({
  selector: 'sc-informes',
  standalone: true,
  imports: [FormsModule, RouterLink],
  template: `
    <div class="topbar">
      <h1>Informes</h1>
      <div class="row">
        <select [(ngModel)]="desglose">
          <option value="medio">Desglose: medio</option>
          <option value="usuario">Desglose: usuario</option>
        </select>
        <button class="btn btn-primary" type="button" (click)="cerrarHoy()">Cerrar caja de hoy</button>
        <input type="date" [(ngModel)]="fechaAnterior" />
        <button class="btn" type="button" (click)="cerrarAnterior()">Cerrar caja anterior</button>
      </div>
    </div>
    <div class="kpis">
      <div class="panel kpi">
        <div class="n">{{ dinero(pendientes.total) }}</div>
        <div class="l">Ventas de hoy (sin cerrar) · Caja {{ sesion.nroCaja() }}</div>
      </div>
      <div class="panel kpi">
        <div class="n">{{ pendientes.tickets }}</div>
        <div class="l">Tickets abiertos</div>
      </div>
      <div class="panel kpi">
        <div class="n">{{ sesion.nombre() }}</div>
        <div class="l">{{ sesion.esDueno() ? 'Dueño' : 'Puede cerrar (empleado)' }}</div>
      </div>
      @if (sesion.esDueno()) {
        <div class="panel kpi">
          <div class="row" style="align-items:center;gap:8px">
            <input type="number" min="1" max="50" [(ngModel)]="cantidadCajas" style="width:64px" />
            <button class="btn" type="button" (click)="guardarCantidad()">Guardar</button>
          </div>
          <div class="l">Cantidad de cajas</div>
        </div>
      }
    </div>
    @if (error) {
      <p class="error">{{ error }}</p>
    }
    @if (cierre) {
      <section class="panel" style="margin-bottom:16px">
        <div class="toolbar" style="padding:12px 12px 0">
          <strong style="font-size:14px">Caja {{ cierre.nroCaja }} · cierre {{ cierre.idCierre }} · cerró {{ cierre.nombreCierre }}</strong>
        </div>
        <table class="data">
          <thead>
            <tr>
              <th>Puesto</th>
              <th>{{ cierre.tipoDesglose === 'Usuario' ? 'Usuario' : 'Medio de pago' }}</th>
              <th class="num">Cant. ventas</th>
              <th class="num">Total</th>
            </tr>
          </thead>
          <tbody>
            @for (f of cierre.filas; track f.metodoPago) {
              <tr>
                <td>{{ cierre.nroCaja }}</td>
                <td>{{ f.metodoPago }}</td>
                <td class="num">{{ f.cantidadVentas }}</td>
                <td class="num">{{ dinero(f.total) }}</td>
              </tr>
            }
          </tbody>
        </table>
      </section>
    }
    <section class="panel" style="margin-bottom:16px">
      <div class="toolbar" style="padding:12px">
        <button type="button" class="colapsar" [attr.aria-expanded]="ventasAbiertas" (click)="ventasAbiertas = !ventasAbiertas">
          <span class="flecha" [class.abierta]="ventasAbiertas">▸</span>
          Ventas
        </button>
        <input type="date" [(ngModel)]="fecha" (ngModelChange)="cargarVentas()" />
        <select [(ngModel)]="medio" (ngModelChange)="cargarVentas()">
          <option value="">Todos los medios</option>
          <option>Efectivo</option>
          <option>Mercado Pago</option>
        </select>
        <span class="meta">{{ totalVentas }} tickets · {{ dinero(sumaVentas) }}</span>
      </div>
      @if (ventasAbiertas) {
        <table class="data">
          <thead>
            <tr>
              <th>Hora</th>
              <th class="num">Total</th>
              <th>Método</th>
              <th>Notas</th>
            </tr>
          </thead>
          <tbody>
            @for (v of ventas; track v.idInformeVenta) {
              <tr class="clickable" (click)="abrir(v.idInformeVenta)">
                <td>{{ hora(v.fecha) }}</td>
                <td class="num">{{ dinero(v.total) }}</td>
                <td>{{ v.metodoPago }}</td>
                <td>{{ notas(v) }}</td>
              </tr>
            }
          </tbody>
        </table>
      }
    </section>
    <section class="panel">
      <div class="toolbar" style="padding:12px">
        <strong style="font-size:14px">Cajas cerradas</strong>
        <a class="btn" routerLink="/cajas">Consultar cajas</a>
      </div>
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
          @for (c of cajas; track c.idCierre) {
            <tr class="clickable" (click)="verCaja(c.idCierre)">
              <td>{{ c.nroCaja }}</td>
              <td>{{ fechaCorta(c.fecha) }}</td>
              <td>{{ c.nombreCierre }}</td>
              <td class="num">{{ c.cantidadVentas }}</td>
              <td class="num">{{ dinero(c.total) }}</td>
            </tr>
          }
        </tbody>
      </table>
    </section>
    @if (detalle) {
      <div class="modal-back show">
        <div class="modal" style="max-width:520px">
          <h2 style="margin:0 0 8px;font-size:18px">Venta {{ hora(detalle.fecha) }}</h2>
          <p class="meta">{{ detalle.metodoPago }} · {{ dinero(detalle.total) }}</p>
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
              @for (i of detalle.items; track i.idInformeVentaDetalle) {
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
            <button class="btn" type="button" (click)="imprimir()">Reimprimir</button>
            <button class="btn" type="button" (click)="copiar()">Copiar al carrito</button>
            @if (sesion.esDueno()) {
              <button class="btn btn-danger" type="button" [disabled]="!!detalle.idCierre" (click)="anular()">
                Anular
              </button>
            }
            <button class="btn grow" type="button" (click)="detalle = null">Cerrar</button>
          </div>
        </div>
      </div>
    }

    @if (ticket) {
      <div class="ticket-print">
        <strong>{{ sesion.nombreLocal() }}</strong>
        <p>{{ fechaCorta(ticket.fecha) }} {{ hora(ticket.fecha) }}</p>
        <table>
          @for (i of ticket.items; track i.idInformeVentaDetalle) {
            <tr>
              <td>{{ i.nombre }}</td>
              <td>{{ i.cantidad }}</td>
              <td>{{ dinero(i.subTotal) }}</td>
            </tr>
          }
        </table>
        <p><strong>Total {{ dinero(ticket.total) }}</strong></p>
        <p>{{ ticket.metodoPago }}</p>
        @if (ticket.descuento > 0) {
          <p>Descuento {{ ticket.descuento }}%</p>
        }
        @if ((ticket.precioCosto || '').toUpperCase() === 'SI') {
          <p>Cobrado al costo</p>
        }
      </div>
    }
  `
})
export class InformesComponent implements OnInit {
  fecha = hoyYmd();
  fechaAnterior = hoyYmd();
  medio = '';
  desglose: 'medio' | 'usuario' = 'medio';
  cantidadCajas = 1;
  ventas: VentaListaDto[] = [];
  totalVentas = 0;
  sumaVentas = 0;
  cajas: CajaListaDto[] = [];
  pendientes = { tickets: 0, total: 0 };
  detalle: VentaDetalleDto | null = null;
  ticket: VentaDetalleDto | null = null;
  cierre: CajaDetalleDto | null = null;
  error = '';
  ventasAbiertas = true;

  constructor(
    private readonly api: ApiService,
    private readonly router: Router,
    readonly sesion: SesionService
  ) {}

  ngOnInit(): void {
    this.cargarVentas();
    this.cargarCajas();
    if (this.sesion.esDueno()) {
      this.api.cantidadCajas().subscribe({
        next: (r) => (this.cantidadCajas = r.cantidad)
      });
    }
  }

  dinero(n: number): string {
    return `$ ${Number(n).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }

  hora(iso?: string): string {
    if (!iso) return '';
    return new Date(iso).toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' });
  }

  fechaCorta(iso: string): string {
    return new Date(iso).toLocaleDateString('es-AR');
  }

  notas(v: VentaListaDto): string {
    const bits: string[] = [];
    if (v.descuento > 0) bits.push(`Desc. ${v.descuento}%`);
    if ((v.precioCosto || '').toUpperCase() === 'SI') bits.push('Al costo');
    if (v.nroCaja) bits.push(`Puesto ${v.nroCaja}`);
    if (v.idCierre) bits.push('Cerrada');
    return bits.join(' · ');
  }

  cargarVentas(): void {
    this.api.ventas({ desde: this.fecha, hasta: this.fecha, medio: this.medio || undefined }).subscribe({
      next: (r) => {
        this.ventas = r.items;
        this.totalVentas = r.total;
        this.sumaVentas = r.suma;
        this.error = '';
      },
      error: () => (this.error = 'No se pudieron cargar las ventas.')
    });
  }

  cargarCajas(): void {
    this.api.cajas().subscribe({
      next: (r) => {
        this.cajas = r.items;
        this.pendientes = r.pendientesHoy;
      },
      error: () => (this.error = 'No se pudieron cargar las cajas.')
    });
  }

  guardarCantidad(): void {
    const n = Math.max(1, Math.min(50, Number(this.cantidadCajas) || 1));
    this.cantidadCajas = n;
    this.api.setCantidadCajas(n).subscribe({
      next: () => (this.error = ''),
      error: (err) => (this.error = typeof err.error === 'string' ? err.error : 'No se pudo guardar.')
    });
  }

  abrir(id: number): void {
    this.api.venta(id).subscribe({
      next: (d) => (this.detalle = d),
      error: () => (this.error = 'No se pudo abrir la venta.')
    });
  }

  anular(): void {
    if (!this.detalle || !this.sesion.esDueno()) return;
    if (!confirm('¿Anular esta venta y devolver stock?')) return;
    this.api.anularVenta(this.detalle.idInformeVenta).subscribe({
      next: () => {
        this.detalle = null;
        this.cargarVentas();
        this.cargarCajas();
      },
      error: (err) => (this.error = typeof err.error === 'string' ? err.error : 'No se pudo anular.')
    });
  }

  copiar(): void {
    if (!this.detalle) return;
    this.sesion.dejarCarritoPendiente(
      this.detalle.items.map((i) => ({
        idProducto: i.idProducto ?? 0,
        codigo: i.codigo,
        nombre: i.nombre,
        cantidad: i.cantidad,
        precioUnitario: i.precio,
        costo: i.costo ?? 0,
        sector: false,
        generico: i.codigo.toUpperCase().startsWith('GENERIC-')
      }))
    );
    this.detalle = null;
    void this.router.navigateByUrl('/caja');
  }

  imprimir(): void {
    if (!this.detalle) return;
    this.ticket = this.detalle;
    setTimeout(() => {
      window.print();
      this.ticket = null;
    }, 50);
  }

  verCaja(idCierre: number): void {
    this.api.cierre(idCierre).subscribe({
      next: (c) => (this.cierre = c),
      error: () => (this.error = 'No se pudo abrir la caja.')
    });
  }

  cerrarHoy(): void {
    this.cerrar(undefined);
  }

  cerrarAnterior(): void {
    if (!this.fechaAnterior) return;
    this.cerrar(this.fechaAnterior);
  }

  private cerrar(fecha?: string): void {
    const msg = fecha ? `¿Cerrar la caja del ${fecha}?` : '¿Cerrar la caja de hoy?';
    if (!confirm(msg)) return;
    this.api.cerrarCaja(fecha, this.desglose).subscribe({
      next: (r) => {
        this.cierre = {
          idCierre: r.idCierre,
          nroCaja: r.nroCaja,
          fecha: new Date().toISOString(),
          nombreCierre: this.sesion.nombre(),
          tipoDesglose: this.desglose === 'usuario' ? 'Usuario' : 'Medio',
          filas: r.filas
        };
        this.error = '';
        this.cargarVentas();
        this.cargarCajas();
      },
      error: (err) => (this.error = typeof err.error === 'string' ? err.error : 'No se pudo cerrar la caja.')
    });
  }
}

function hoyYmd(): string {
  const d = new Date();
  const m = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  return `${d.getFullYear()}-${m}-${day}`;
}
