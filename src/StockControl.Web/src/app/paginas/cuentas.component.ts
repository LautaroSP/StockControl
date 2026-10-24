import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, CuentaDto, CuentaListaDto, MetodoPagoDto } from '../servicios/api.service';
import { DialogoService } from '../servicios/dialogo.service';

@Component({
  selector: 'sc-cuentas',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="topbar">
      <h1>Cuentas pendientes</h1>
      <div class="row">
        <input type="search" [(ngModel)]="cliente" (ngModelChange)="cargar(1)" placeholder="Buscar cliente" />
        <select [(ngModel)]="estado" (ngModelChange)="cargar(1)">
          <option value="abierta">Abiertas</option>
          <option value="pagada">Pagadas</option>
          <option value="incobrable">Incobrables</option>
          <option value="todas">Todas</option>
        </select>
      </div>
    </div>
    @if (error) { <p class="error">{{ error }}</p> }
    @if (mensaje) { <p class="meta">{{ mensaje }}</p> }
    @if (detalle; as d) {
      <section class="panel pad" style="margin-bottom:16px">
        <div class="row">
          <h2 class="grow" style="margin:0;font-size:18px">{{ d.cliente }} @if (d.telefono) { <span class="meta">{{ d.telefono }}</span> }</h2>
          <button class="btn" type="button" (click)="detalle = null">Cerrar</button>
        </div>
        <p class="meta">
          Fiado el {{ fecha(d.fecha) }} por {{ d.nombreUsuario || 'sin usuario' }} en la caja {{ d.nroCaja }} · {{ textoEstado(d.estado) }}
          @if (d.estado === 'incobrable') { · anulada por {{ d.usuarioIncobrable }} el {{ fecha(d.fechaIncobrable!) }} }
        </p>
        <div class="panel pad" style="margin-bottom:12px">
          <strong>{{ d.cliente }} debe en total {{ dinero(deudaCliente) }}</strong>
          <span class="meta"> · {{ cuentasCliente.length }} {{ cuentasCliente.length === 1 ? 'cuenta abierta' : 'cuentas abiertas' }}</span>
          @for (o of cuentasCliente; track o.id) {
            <div class="row" style="margin-top:6px">
              <span class="grow">{{ fecha(o.fecha) }} · saldo {{ dinero(o.saldo) }}</span>
              @if (o.id !== d.id) { <button class="btn" type="button" (click)="abrir(o)">Ver</button> }
            </div>
          }
        </div>
        <table class="data">
          <thead><tr><th>Producto</th><th class="num">Cant</th><th class="num">Precio</th><th class="num">Subt</th></tr></thead>
          <tbody>
            @for (l of d.lineas; track l.id) {
              <tr>
                <td>{{ l.nombre }} @if (l.descripcionOferta) { <span class="badge badge-aviso">{{ l.descripcionOferta }}</span> }</td>
                <td class="num">{{ l.cantidad }}</td>
                <td class="num">{{ dinero(l.precio) }}</td>
                <td class="num">{{ dinero(l.subTotal) }}</td>
              </tr>
            }
          </tbody>
        </table>
        <p><strong>Importe {{ dinero(d.importe) }} · Saldo {{ dinero(d.saldo) }}</strong></p>
        @if (d.pagos.length) {
          <p class="meta">Pagos:</p>
          @for (p of d.pagos; track p.id) {
            <div class="meta">{{ fecha(p.fecha) }} · {{ dinero(p.importe) }}</div>
          }
        }
        @if (d.estado === 'abierta') {
          <div class="row" style="margin-top:12px">
            <label>Importe <input type="number" min="0.01" [max]="d.saldo" step="0.01" style="width:120px" [(ngModel)]="importe" /></label>
            <select [(ngModel)]="idMedio">
              @for (m of medios; track m.id) { <option [ngValue]="m.id">{{ m.descripcion }}</option> }
            </select>
            <button class="btn btn-primary" type="button" [disabled]="trabajando || !(importe > 0) || importe > d.saldo || !idMedio" (click)="pagar(d)">Cobrar</button>
            @if (puedeAnular) {
              <button class="btn btn-danger" type="button" [disabled]="trabajando" (click)="incobrable(d)">Incobrable</button>
            }
          </div>
        }
      </section>
    }
    <section class="panel">
      <table class="data">
        <thead><tr><th>Cliente</th><th>Fecha</th><th class="num">Importe</th><th class="num">Saldo</th><th>Estado</th><th></th></tr></thead>
        <tbody>
          @for (c of cuentas; track c.id) {
            <tr>
              <td>{{ c.cliente }}</td>
              <td>{{ fecha(c.fecha) }}</td>
              <td class="num">{{ dinero(c.importe) }}</td>
              <td class="num">{{ dinero(c.saldo) }}</td>
              <td>{{ textoEstado(c.estado) }}</td>
              <td class="num"><button class="btn" type="button" (click)="abrir(c)">Ver</button></td>
            </tr>
          } @empty {
            <tr><td colspan="6" class="meta">No hay cuentas.</td></tr>
          }
        </tbody>
      </table>
      <div class="row pad">
        <span class="meta grow">{{ total }} {{ total === 1 ? 'cuenta' : 'cuentas' }} · saldo {{ dinero(saldo) }}</span>
        @if (total > tamano) {
          <button class="btn" type="button" [disabled]="pagina === 1" (click)="cargar(pagina - 1)">Anterior</button>
          <span class="meta">Página {{ pagina }}</span>
          <button class="btn" type="button" [disabled]="pagina * tamano >= total" (click)="cargar(pagina + 1)">Siguiente</button>
        }
      </div>
    </section>
  `
})
export class CuentasComponent implements OnInit {
  cuentas: CuentaListaDto[] = [];
  total = 0;
  saldo = 0;
  pagina = 1;
  tamano = 50;
  cliente = '';
  estado = 'abierta';
  detalle: CuentaDto | null = null;
  medios: MetodoPagoDto[] = [];
  idMedio = 0;
  importe = 0;
  cuentasCliente: CuentaListaDto[] = [];
  deudaCliente = 0;
  puedeAnular = false;
  trabajando = false;
  error = '';
  mensaje = '';

  constructor(private readonly api: ApiService, private readonly dialogo: DialogoService) {}

  ngOnInit(): void {
    this.api.permisosCuentas().subscribe({ next: (p) => (this.puedeAnular = p.puedeAnular) });
    this.api.mediosPago().subscribe({
      next: (r) => {
        this.medios = r.filter((m) => m.activo && (!m.integracion || m.integracion === 'manual'));
        this.idMedio = this.medios[0]?.id ?? 0;
      }
    });
    this.cargar(1);
  }

  cargar(pagina: number): void {
    this.api.cuentas({ cliente: this.cliente.trim(), estado: this.estado, pagina }).subscribe({
      next: (r) => {
        this.cuentas = r.items;
        this.total = r.total;
        this.saldo = r.saldo;
        this.pagina = r.pagina;
        this.tamano = r.tamano;
      },
      error: (e) => (this.error = e.status === 403 ? 'No tenés permiso para ver cuentas.' : 'No se pudieron cargar las cuentas.')
    });
  }

  private cargarDeudaCliente(idCliente: number): void {
    this.api.cuentas({ idCliente, estado: 'abierta' }).subscribe({
      next: (r) => {
        this.cuentasCliente = r.items;
        this.deudaCliente = r.saldo;
      }
    });
  }

  abrir(c: CuentaListaDto): void {
    this.mensaje = '';
    this.error = '';
    this.api.cuenta(c.id).subscribe({
      next: (d) => {
        this.detalle = d;
        this.importe = d.saldo;
        this.cargarDeudaCliente(d.idCliente);
      },
      error: () => (this.error = 'No se pudo abrir la cuenta.')
    });
  }

  pagar(d: CuentaDto): void {
    this.trabajando = true;
    this.error = '';
    this.api.pagarCuenta(d.id, Number(this.importe), this.idMedio).subscribe({
      next: (r) => {
        this.trabajando = false;
        this.mensaje = r.estado === 'pagada' ? `${d.cliente} pagó todo.` : `Cobrado. Saldo ${this.dinero(r.saldo)}.`;
        this.abrir(d);
        this.cargar(this.pagina);
      },
      error: (e) => {
        this.trabajando = false;
        this.error = typeof e.error === 'string' ? e.error : 'No se pudo cobrar.';
      }
    });
  }

  async incobrable(d: CuentaDto): Promise<void> {
    if (!await this.dialogo.confirmar(`¿Anular la cuenta de ${d.cliente} como incobrable? El saldo de ${this.dinero(d.saldo)} queda como pérdida.`)) return;
    this.trabajando = true;
    this.api.cuentaIncobrable(d.id).subscribe({
      next: () => {
        this.trabajando = false;
        this.detalle = null;
        this.mensaje = 'Cuenta anulada como incobrable.';
        this.cargar(this.pagina);
      },
      error: (e) => {
        this.trabajando = false;
        this.error = typeof e.error === 'string' ? e.error : 'No se pudo anular la cuenta.';
      }
    });
  }

  textoEstado(estado: string): string {
    if (estado === 'pagada') return 'Pagada';
    if (estado === 'incobrable') return 'Incobrable';
    return 'Abierta';
  }

  fecha(f: string): string {
    return new Date(f).toLocaleDateString('es-AR');
  }

  dinero(n: number): string {
    return `$ ${Number(n).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }
}
