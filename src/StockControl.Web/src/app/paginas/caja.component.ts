import { Component, HostListener, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, ItemVenta, MetodoPagoDto, ProductoDto, VentaDetalleDto } from '../servicios/api.service';
import { CarritoCaja, LineaCarritoPendiente, SesionService } from '../servicios/sesion.service';
import { TicketComponent } from './ticket.component';

const FIJOS = 4;

@Component({
  selector: 'sc-caja',
  standalone: true,
  imports: [FormsModule, TicketComponent],
  template: `
    <div class="topbar">
      <h1>Caja</h1>
      <div class="row">
        <span class="meta">Stock bajo en naranja</span>
        <input
          style="width:200px"
          type="text"
          [(ngModel)]="scanner"
          (keydown.enter)="escanear()"
          placeholder="Código + Enter"
          autofocus
        />
        <button class="btn" type="button" (click)="agregarGenerico()">Genérico</button>
      </div>
    </div>
    <div class="caja-grid">
      <section class="panel list-wrap">
        <div class="toolbar" style="padding:12px">
          <input class="grow" type="search" [ngModel]="buscar" (ngModelChange)="aplicarBuscar($event)" placeholder="Buscar por nombre o código" />
          <select [ngModel]="filtroTipo" (ngModelChange)="aplicarTipo($event)">
            <option value="">Tipo: todos</option>
            <option value="comun">Común</option>
            <option value="sector">Sector</option>
          </select>
          <select [ngModel]="filtroGrupo" (ngModelChange)="aplicarGrupo($event)">
            <option value="">Grupo: todos</option>
            <option value="sin">Sin grupo</option>
            @for (g of gruposFiltro; track g.idGrupoProducto) {
              <option [value]="g.idGrupoProducto">{{ g.nombreGrupo }}</option>
            }
          </select>
          <label class="check"><input type="checkbox" [(ngModel)]="stockBajo" (ngModelChange)="cargar()" /> Stock bajo</label>
          <span class="meta">{{ totalLista }} en lista</span>
        </div>
        <div class="scroll">
          <table class="data">
            <thead>
              <tr>
                <th>Código</th>
                <th>Nombre</th>
                <th class="num">Stock</th>
                <th class="num">Precio</th>
              </tr>
            </thead>
            <tbody>
              @for (p of visibles; track p.id) {
                 <tr class="clickable" [class.low]="!p.productoSector && p.cantidad <= umbralStockBajo" (click)="agregarProducto(p)">
                  <td>{{ p.codigo }}</td>
                  <td>{{ p.nombre }}</td>
                  <td class="num">{{ p.productoSector ? '—' : p.cantidad }}</td>
                  <td class="num">{{ p.productoSector ? 'en caja' : dinero(p.precio) }}</td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </section>
      <aside class="panel cart">
        <h2>Carrito</h2>
        <div class="scroll" style="min-height:180px;border:1px solid var(--line);border-radius:8px">
          <table class="data">
            <thead>
              <tr>
                <th>Producto</th>
                <th class="num">Cant</th>
                <th class="num">Subt</th>
              </tr>
            </thead>
            <tbody>
              @for (l of activo.lineas; track $index) {
                <tr class="clickable" (dblclick)="sacar(l)">
                  <td>{{ l.nombre }}</td>
                  <td class="num">
                    <input style="width:64px;text-align:right" type="number" min="0.001" step="1" [(ngModel)]="l.cantidad" />
                  </td>
                  <td class="num">{{ dinero(precioLinea(l) * l.cantidad) }}</td>
                </tr>
              }
            </tbody>
          </table>
        </div>
        <div class="total-box">
          <div>
            <div class="lbl">Ítems</div>
            <div>{{ items }}</div>
          </div>
          <div>
            <div class="lbl">Total</div>
            <div class="val">{{ dinero(total) }}</div>
          </div>
        </div>
        <div class="checks">
          <label class="check">
            <input type="checkbox" [ngModel]="activo.usarDesc" (ngModelChange)="aplicarDesc($event, txtDesc)" />
            Descuento %
          </label>
          <input
            #txtDesc
            style="width:72px"
            type="number"
            min="0"
            max="100"
            [(ngModel)]="activo.descuento"
            [disabled]="!activo.usarDesc"
          />
          @if (sesion.esDueno()) {
            <label class="check">
              <input type="checkbox" [ngModel]="activo.alCosto" (ngModelChange)="aplicarAlCosto($event)" />
              Cobrar al costo
            </label>
          }
          <label class="check">
            <input type="checkbox" [(ngModel)]="activo.imprimirTicket" />
            Imprimir ticket
          </label>
        </div>
        <div class="payment-list">
          @for (p of activo.pagos; track $index) {
            <div class="row">
              <select [(ngModel)]="p.idMetodoPago">
                @for (m of mediosPago; track m.id) {
                  @if (m.activo) { <option [ngValue]="m.id">{{ m.descripcion }}</option> }
                }
              </select>
              <input class="grow" type="number" min="0.01" [ngModel]="importePago(p, $index)" (ngModelChange)="p.importe = $event" [disabled]="activo.pagos.length === 1 || p.resto" />
              @if (activo.pagos.length > 1) {
                <label class="check"><input type="checkbox" [ngModel]="p.resto" (ngModelChange)="marcarResto($index, $event)" /> Resto</label>
                <button class="btn" type="button" (click)="quitarPago($index)">Quitar</button>
              }
            </div>
          }
          <button class="btn" type="button" [disabled]="mediosPago.length < 2" (click)="agregarPago()">Agregar otro medio</button>
        </div>
        <div class="tabs">
          @for (c of carritos; track c.id; let i = $index) {
            <button class="tab" type="button" [class.on]="i === indiceActivo" [disabled]="cobrando" (click)="seleccionar(i)">
              {{ titulo(i) }}
            </button>
          }
          <button class="btn" type="button" [disabled]="cobrando" (click)="crearCarrito()">+</button>
          <button class="btn" type="button" [disabled]="cobrando || indiceActivo < fijos" (click)="cerrarCarrito()">×</button>
        </div>
        <div class="row">
          <button class="btn btn-charge grow" type="button" [disabled]="cobrando || activo.lineas.length === 0" (click)="cobrar()">
            COBRAR
          </button>
          <button class="btn" type="button" (click)="cancelar()">Cancelar</button>
        </div>
        @if (mensaje) {
          <p class="meta">{{ mensaje }}</p>
        }
        @if (error) {
          <p class="error">{{ error }}</p>
        }
      </aside>
    </div>
    <div class="toast" [class.show]="toast">{{ toast }}</div>
    @if (ticket) {
      <sc-ticket [venta]="ticket" [nombreLocal]="nombreTicket" [formato]="formatoTicket" />
    }
  `
})
export class CajaComponent implements OnInit, OnDestroy {
  readonly fijos = FIJOS;
  productos: ProductoDto[] = [];
  visibles: ProductoDto[] = [];
  totalLista = 0;
  mediosPago: MetodoPagoDto[] = [];
  private proximoId = 1;
  private imprimirDefault = true;
  carritos: CarritoCaja[] = [this.vacio(), this.vacio(), this.vacio(), this.vacio()];
  indiceActivo = 0;
  buscar = '';
  filtroTipo = '';
  filtroGrupo = '';
  stockBajo = false;
  umbralStockBajo = 5;
  gruposFiltro: { idGrupoProducto: number; nombreGrupo: string }[] = [];
  scanner = '';
  cobrando = false;
  mensaje = '';
  error = '';
  toast = '';
  ticket: VentaDetalleDto | null = null;
  formatoTicket = 'POS-80';
  nombreTicket = '';

  constructor(
    private readonly api: ApiService,
    readonly sesion: SesionService
  ) {}

  get activo(): CarritoCaja {
    return this.carritos[this.indiceActivo] ?? this.carritos[0];
  }

  get items(): number {
    return this.activo.lineas.reduce((n, l) => n + Number(l.cantidad), 0);
  }

  get total(): number {
    return this.activo.lineas.reduce((n, l) => n + this.precioLinea(l) * Number(l.cantidad), 0);
  }

  precioLinea(l: LineaCarritoPendiente): number {
    if (l.sector || l.generico) return Number(l.precioUnitario);
    if (this.activo.alCosto) return Number(l.costo);
    const desc = this.activo.usarDesc ? Math.min(100, Math.max(0, Number(this.activo.descuento) || 0)) : 0;
    const lista = Number(l.precioUnitario);
    const costo = Number(l.costo);
    const margen = Math.max(0, lista - costo);
    return Math.round((lista - (margen * desc) / 100) * 100) / 100;
  }

  aplicarDesc(v: boolean, el: HTMLInputElement): void {
    this.activo.usarDesc = v;
    if (!v) this.activo.descuento = 0;
    else queueMicrotask(() => el.focus());
  }

  aplicarAlCosto(v: boolean): void {
    if (v && !confirm('¿Cobrar esta venta al costo?')) {
      this.activo.alCosto = false;
      return;
    }
    this.activo.alCosto = v;
  }

  titulo(indice: number): string {
    const n = indice + 1;
    return indice < FIJOS ? `${n} F${n}` : String(n);
  }

  seleccionar(indice: number): void {
    if (this.cobrando || indice < 0 || indice >= this.carritos.length) return;
    this.indiceActivo = indice;
  }

  crearCarrito(): void {
    this.carritos = [...this.carritos, this.vacio()];
    this.indiceActivo = this.carritos.length - 1;
  }

  cerrarCarrito(): void {
    if (this.indiceActivo < FIJOS) return;
    if (this.activo.lineas.length > 0 && !confirm('Este carrito tiene productos. ¿Desea cerrarlo?')) return;
    const i = this.indiceActivo;
    this.carritos = this.carritos.filter((_, n) => n !== i);
    this.indiceActivo = Math.min(i, this.carritos.length - 1);
  }

  @HostListener('window:keydown', ['$event'])
  atajo(e: KeyboardEvent): void {
    if (e.key !== 'F1' && e.key !== 'F2' && e.key !== 'F3' && e.key !== 'F4') return;
    e.preventDefault();
    this.seleccionar(Number(e.key.slice(1)) - 1);
  }

  ngOnInit(): void {
    const guardados = this.sesion.tomarCarritos();
    if (guardados && guardados.carritos.length >= FIJOS) {
      this.carritos = guardados.carritos;
      this.indiceActivo = Math.min(guardados.indice, this.carritos.length - 1);
      this.proximoId = Math.max(...this.carritos.map((c) => c.id), FIJOS) + 1;
    }
    const pend = this.sesion.tomarCarritoPendiente();
    if (pend?.length) this.activo.lineas = pend.map((l) => ({ ...l }));
    this.api.mediosPago().subscribe({
      next: (r) => {
        this.mediosPago = r;
        const id = r.find((m) => m.activo)?.id ?? r[0]?.id;
        if (!id) return;
        for (const c of this.carritos) {
          if (c.pagos.length === 0) c.pagos = [{ idMetodoPago: id, importe: 0, resto: false }];
        }
      },
      error: () => (this.error = 'No se pudieron cargar los medios de pago.')
    });
    this.nombreTicket = this.sesion.nombreLocal();
    this.api.configuracionTicket().subscribe({
      next: (c) => {
        this.imprimirDefault = c.imprimirTicketAlCobrar;
        this.formatoTicket = c.formatoTicket || 'POS-80';
        this.nombreTicket = c.nombreLocal || this.nombreTicket;
        if (!guardados) {
          for (const cart of this.carritos) cart.imprimirTicket = this.imprimirDefault;
        }
      }
    });
    this.api.gruposFiltro().subscribe({ next: (r) => (this.gruposFiltro = r) });
    this.cargar();
  }

  ngOnDestroy(): void {
    this.sesion.guardarCarritos(this.carritos, this.indiceActivo);
  }

  dinero(n: number): string {
    return `$ ${Number(n).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }

  cargar(): void {
    const sector = this.filtroTipo === 'sector' ? true : this.filtroTipo === 'comun' ? false : undefined;
    this.api.productos(this.buscar, {
      sector,
      idGrupo: this.filtroGrupo && this.filtroGrupo !== 'sin' ? Number(this.filtroGrupo) : undefined,
      sinGrupo: this.filtroGrupo === 'sin',
      stockBajo: this.stockBajo
    }).subscribe({
      next: (r) => {
        this.productos = r.items;
        this.visibles = r.items;
        this.totalLista = r.total;
        this.umbralStockBajo = r.umbralStockBajo;
        this.error = '';
      },
      error: () => (this.error = 'No se pudieron cargar los productos.')
    });
  }

  aplicarTipo(tipo: string): void {
    this.filtroTipo = tipo;
    this.cargar();
  }

  aplicarBuscar(q: string): void {
    this.buscar = q;
    this.cargar();
  }

  filtrar(): void {
    this.cargar();
  }

  escanear(): void {
    const codigo = this.scanner.trim();
    if (!codigo) return;
    this.scanner = '';
    const local = this.productos.find((x) => x.codigo.toLowerCase() === codigo.toLowerCase());
    if (local) {
      this.agregarProducto(local);
      return;
    }
    this.api.productos('', { codigo }).subscribe({
      next: (r) => {
        const p = r.items[0];
        if (p) this.agregarProducto(p);
        else this.error = `No hay producto ${codigo}`;
      },
      error: () => (this.error = `No hay producto ${codigo}`)
    });
  }

  agregarProducto(p: ProductoDto): void {
    this.error = '';
    if (p.productoSector) {
      const raw = prompt(`Precio de ${p.nombre}`, '');
      const precio = Number((raw ?? '').replace(',', '.'));
      if (!raw || !(precio > 0)) return;
      this.activo.lineas.push({
        idProducto: p.id,
        codigo: p.codigo,
        nombre: p.nombre,
        cantidad: 1,
        precioUnitario: precio,
        costo: p.costo,
        sector: true,
        generico: false
      });
      return;
    }
    const ya = this.activo.lineas.find((l) => l.idProducto === p.id && !l.generico && !l.sector);
    if (ya) ya.cantidad = Number(ya.cantidad) + 1;
    else {
      this.activo.lineas.push({
        idProducto: p.id,
        codigo: p.codigo,
        nombre: p.nombre,
        cantidad: 1,
        precioUnitario: p.precio,
        costo: p.costo,
        sector: false,
        generico: false
      });
    }
  }

  agregarGenerico(): void {
    const nombre = prompt('Nombre del genérico', 'Producto suelto');
    if (!nombre) return;
    const raw = prompt('Precio', '');
    const precio = Number((raw ?? '').replace(',', '.'));
    if (!raw || !(precio > 0)) return;
    this.activo.lineas.push({
      idProducto: 0,
      codigo: `GENERIC-${crypto.randomUUID().slice(0, 8)}`,
      nombre,
      cantidad: 1,
      precioUnitario: precio,
      costo: 0,
      sector: false,
      generico: true
    });
  }

  sacar(l: LineaCarritoPendiente): void {
    this.activo.lineas = this.activo.lineas.filter((x) => x !== l);
  }

  cancelar(): void {
    this.vaciarActivo();
    this.error = '';
    this.mensaje = '';
  }

  cobrar(): void {
    if (this.activo.lineas.length === 0) return;
    this.cobrando = true;
    this.error = '';
    const items: ItemVenta[] = this.activo.lineas.map((l) => ({
      idProducto: l.idProducto,
      codigo: l.codigo,
      nombre: l.nombre,
      cantidad: Number(l.cantidad),
      precioUnitario: l.precioUnitario
    }));
    const desc = this.activo.usarDesc ? Number(this.activo.descuento) || 0 : 0;
    const pagos = this.activo.pagos.map((p, i) => ({ idMetodoPago: p.idMetodoPago, importe: this.importePago(p, i) }));
    const imprimir = this.activo.imprimirTicket;
    this.api.cobrar(items, pagos, desc, this.sesion.esDueno() && this.activo.alCosto).subscribe({
      next: (r) => {
        this.toast = `Cobrado ${this.dinero(r.total)}`;
        this.cerrarTrasCobro();
        this.cobrando = false;
        this.cargar();
        if (imprimir) this.imprimirVenta(r.idInformeVenta, r.total);
        else setTimeout(() => (this.toast = ''), 2500);
      },
      error: (err) => {
        this.error = typeof err.error === 'string' ? err.error : 'No se pudo cobrar.';
        this.cobrando = false;
      }
    });
  }

  aplicarGrupo(grupo: string): void {
    this.filtroGrupo = grupo;
    this.cargar();
  }

  agregarPago(): void {
    const disponible = this.mediosPago.find((m) => m.activo && !this.activo.pagos.some((p) => p.idMetodoPago === m.id));
    if (disponible) this.activo.pagos = [...this.activo.pagos, { idMetodoPago: disponible.id, importe: 0, resto: false }];
  }

  quitarPago(indice: number): void {
    this.activo.pagos = this.activo.pagos.filter((_, i) => i !== indice);
  }

  marcarResto(indice: number, resto: boolean): void {
    this.activo.pagos = this.activo.pagos.map((p, i) => ({ ...p, resto: resto && i === indice }));
  }

  importePago(pago: CarritoCaja['pagos'][number], indice: number): number {
    if (this.activo.pagos.length === 1) return this.total;
    if (!pago.resto) return Number(pago.importe);
    const usado = this.activo.pagos
      .filter((_, i) => i !== indice)
      .reduce((suma, p) => suma + Number(p.importe), 0);
    return Math.max(0, Math.round((this.total - usado) * 100) / 100);
  }

  private vacio(): CarritoCaja {
    const idMedio = this.mediosPago.find((m) => m.activo)?.id ?? this.mediosPago[0]?.id;
    return {
      id: this.proximoId++,
      lineas: [],
      pagos: idMedio ? [{ idMetodoPago: idMedio, importe: 0, resto: false }] : [],
      usarDesc: false,
      descuento: 0,
      alCosto: false,
      imprimirTicket: this.imprimirDefault
    };
  }

  private vaciarActivo(): void {
    const c = this.activo;
    c.lineas = [];
    c.usarDesc = false;
    c.descuento = 0;
    c.alCosto = false;
    if (c.pagos.length > 1) c.pagos = c.pagos.slice(0, 1);
  }

  private imprimirVenta(id: number, total: number): void {
    this.api.venta(id).subscribe({
      next: (venta) => {
        this.ticket = venta;
        setTimeout(() => {
          try {
            window.print();
          } catch {
            /* el diálogo no corrió */
          }
          this.ticket = null;
          this.toast = `Cobrado ${this.dinero(total)}. Si el ticket no salió, la venta igual está guardada.`;
          setTimeout(() => (this.toast = ''), 4000);
        }, 50);
      },
      error: () => {
        this.toast = `Cobrado ${this.dinero(total)}. No se pudo armar el ticket; la venta está guardada.`;
        setTimeout(() => (this.toast = ''), 4000);
      }
    });
  }

  private cerrarTrasCobro(): void {
    if (this.indiceActivo >= FIJOS) {
      const i = this.indiceActivo;
      this.carritos = this.carritos.filter((_, n) => n !== i);
      this.indiceActivo = Math.min(i, this.carritos.length - 1);
      return;
    }
    this.vaciarActivo();
  }
}
