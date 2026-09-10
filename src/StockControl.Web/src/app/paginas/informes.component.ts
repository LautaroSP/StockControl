import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import {
  ApiService,
  CajaDetalleDto,
  VentaDetalleDto,
  VentaListaDto
} from '../servicios/api.service';
import { SesionService } from '../servicios/sesion.service';
import { TicketComponent } from './ticket.component';

@Component({
  selector: 'sc-informes',
  standalone: true,
  imports: [FormsModule, TicketComponent],
  template: `
    <div class="topbar">
      <h1>Informes</h1>
      <div class="row">
        <select [(ngModel)]="desglose">
          <option value="medio">Desglose: medio</option>
          <option value="usuario">Desglose: usuario</option>
        </select>
        <button class="btn btn-primary" type="button" (click)="cerrarHoy()">Cerrar caja de hoy</button>
        <input type="text" inputmode="numeric" placeholder="dd/mm/aaaa" [(ngModel)]="fechaAnterior" />
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
        <input type="text" inputmode="numeric" placeholder="dd/mm/aaaa" [(ngModel)]="fecha" (ngModelChange)="cargarVentas()" />
        <select [(ngModel)]="puesto" (ngModelChange)="cargarVentas()">
          <option [ngValue]="0">Todos los puestos</option>
          @for (n of puestos; track n) {
            <option [ngValue]="n">Puesto {{ n }}</option>
          }
        </select>
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
              <th>Puesto</th>
              <th class="num">Total</th>
              <th>Método</th>
              <th>Notas</th>
            </tr>
          </thead>
          <tbody>
            @for (v of ventas; track v.idInformeVenta) {
              <tr class="clickable" (click)="abrir(v.idInformeVenta)">
                <td>{{ hora(v.fecha) }}</td>
                <td>{{ v.nroCaja ? 'Caja ' + v.nroCaja : '—' }}</td>
                <td class="num">{{ dinero(v.total) }}</td>
                <td>{{ v.metodoPago }}</td>
                <td>{{ notas(v) }}</td>
              </tr>
            }
          </tbody>
        </table>
      }
    </section>
    @if (detalle) {
      <div class="modal-back show">
        <div class="modal" style="max-width:520px">
          <h2 style="margin:0 0 8px;font-size:18px">Venta {{ hora(detalle.fecha) }}</h2>
           <p class="meta">{{ detalle.metodoPago }} · {{ dinero(detalle.total) }}</p>
           @if (detalle.pagos.length > 0) {
             <p class="meta">{{ pagosTexto(detalle) }}</p>
           }
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
      <sc-ticket [venta]="ticket" [nombreLocal]="nombreTicket" [formato]="formatoTicket" />
    }
  `
})
export class InformesComponent implements OnInit {
  fecha = hoyYmd();
  fechaAnterior = hoyYmd();
  medio = '';
  puesto = 0;
  puestos: number[] = [];
  desglose: 'medio' | 'usuario' = 'medio';
  ventas: VentaListaDto[] = [];
  totalVentas = 0;
  sumaVentas = 0;
  pendientes = { tickets: 0, total: 0 };
  detalle: VentaDetalleDto | null = null;
  ticket: VentaDetalleDto | null = null;
  cierre: CajaDetalleDto | null = null;
  error = '';
  ventasAbiertas = true;
  formatoTicket = 'POS-80';
  nombreTicket = '';

  constructor(
    private readonly api: ApiService,
    private readonly router: Router,
    readonly sesion: SesionService
  ) {}

  ngOnInit(): void {
    this.puesto = this.sesion.nroCaja() || 0;
    this.nombreTicket = this.sesion.nombreLocal();
    this.api.configuracionTicket().subscribe({
      next: (c) => {
        this.formatoTicket = c.formatoTicket || 'POS-80';
        this.nombreTicket = c.nombreLocal || this.nombreTicket;
      }
    });
    this.api.puestos().subscribe({
      next: (r) => {
        this.puestos = Array.from({ length: r.cantidad }, (_, i) => i + 1);
      }
    });
    this.cargarVentas();
    this.cargarPendientes();
  }

  dinero(n: number): string {
    return `$ ${Number(n).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }

  hora(iso?: string): string {
    if (!iso) return '';
    return new Date(iso).toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' });
  }

  notas(v: VentaListaDto): string {
    const bits: string[] = [];
    if (v.descuento > 0) bits.push(`Desc. ${v.descuento}%`);
    if ((v.precioCosto || '').toUpperCase() === 'SI') bits.push('Al costo');
    if (v.idCierre) bits.push('Cerrada');
    return bits.join(' · ');
  }

  pagosTexto(v: VentaDetalleDto): string {
    return v.pagos.map((p) => `${p.descripcionMetodoPago}: ${this.dinero(p.importe)}`).join(' · ');
  }

  cargarVentas(): void {
    const dia = fechaIso(this.fecha);
    if (!dia) return;
    this.api.ventas({
      desde: dia,
      hasta: dia,
      medio: this.medio || undefined,
      nroCaja: this.puesto || undefined
    }).subscribe({
      next: (r) => {
        this.ventas = r.items;
        this.totalVentas = r.total;
        this.sumaVentas = r.suma;
        this.error = '';
      },
      error: () => (this.error = 'No se pudieron cargar las ventas.')
    });
  }

  cargarPendientes(): void {
    this.api.cajas().subscribe({
      next: (r) => {
        this.pendientes = r.pendientesHoy;
      },
      error: () => (this.error = 'No se pudieron cargar las cajas.')
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
        this.cargarPendientes();
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
      try {
        window.print();
      } catch {
        this.error = 'La venta está guardada. No se pudo abrir el diálogo de impresión.';
      }
      this.ticket = null;
    }, 50);
  }

  cerrarHoy(): void {
    this.cerrar(undefined);
  }

  cerrarAnterior(): void {
    const dia = fechaIso(this.fechaAnterior);
    if (!dia) {
      this.error = 'La fecha debe tener formato dd/mm/aaaa.';
      return;
    }
    this.cerrar(dia);
  }

  private cerrar(fecha?: string): void {
    const cerrar = (todas: boolean, confirmado = false) => this.ejecutarCierre(fecha, todas, confirmado);
    if (!this.sesion.esDueno()) {
      cerrar(false);
      return;
    }
    this.api.cajasAbiertas(fecha).subscribe({
      next: (abiertas) => {
        const otras = abiertas.filter((c) => c.nroCaja !== this.sesion.nroCaja() && c.tickets > 0);
        if (otras.length > 0) {
          const puestos = otras.map((c) => `Caja ${c.nroCaja}`).join(', ');
          if (confirm(`Hay cajas sin cerrar además de su puesto (${puestos}). ¿Desea cerrarlas?`)) {
            cerrar(true, true);
          } else {
            cerrar(false);
          }
          return;
        }
        cerrar(false);
      },
      error: () => (this.error = 'No se pudieron consultar las cajas abiertas.')
    });
  }

  private ejecutarCierre(fecha: string | undefined, todas: boolean, confirmado: boolean): void {
    const msg = fecha ? `¿Cerrar la caja del ${fechaVisible(fecha)}?` : '¿Cerrar la caja de hoy?';
    if (!confirmado && !confirm(msg)) return;
    this.api.cerrarCaja(fecha, this.desglose, todas).subscribe({
      next: (r) => {
        this.cierre = {
          idCierre: r.idCierre,
          nroCaja: r.nroCaja,
          fecha: new Date().toISOString(),
          nombreCierre: this.sesion.nombre(),
          tipoDesglose: this.desglose === 'usuario' ? 'Usuario' : 'Medio',
          filas: r.filas
        };
        this.cargarVentas();
        this.cargarPendientes();
        this.error = '';
      },
      error: (err) => (this.error = typeof err.error === 'string' ? err.error : 'No se pudo cerrar la caja.')
    });
  }
}

function hoyYmd(): string {
  const d = new Date();
  return `${String(d.getDate()).padStart(2, '0')}/${String(d.getMonth() + 1).padStart(2, '0')}/${d.getFullYear()}`;
}

function fechaIso(valor: string): string | null {
  const partes = valor.trim().split('/');
  if (partes.length !== 3) return null;
  const [dia, mes, anio] = partes.map(Number);
  const fecha = new Date(anio, mes - 1, dia);
  if (!dia || !mes || !anio || fecha.getFullYear() !== anio || fecha.getMonth() !== mes - 1 || fecha.getDate() !== dia) return null;
  return `${anio}-${String(mes).padStart(2, '0')}-${String(dia).padStart(2, '0')}`;
}

function fechaVisible(iso: string): string {
  const [anio, mes, dia] = iso.split('-');
  return `${dia}/${mes}/${anio}`;
}
