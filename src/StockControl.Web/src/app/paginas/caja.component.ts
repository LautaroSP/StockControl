import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, ItemVenta, ProductoDto } from '../servicios/api.service';
import { SesionService } from '../servicios/sesion.service';

interface LineaCarrito {
  idProducto: number;
  codigo: string;
  nombre: string;
  cantidad: number;
  precioUnitario: number;
  costo: number;
  sector: boolean;
  generico: boolean;
}

@Component({
  selector: 'sc-caja',
  standalone: true,
  imports: [FormsModule],
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
                <tr class="clickable" [class.low]="!p.productoSector && p.cantidad <= 3" (click)="agregarProducto(p)">
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
              @for (l of carrito; track $index) {
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
            <input type="checkbox" [ngModel]="usarDesc" (ngModelChange)="aplicarDesc($event, txtDesc)" />
            Descuento %
          </label>
          <input
            #txtDesc
            style="width:72px"
            type="number"
            min="0"
            max="100"
            [(ngModel)]="descuento"
            [disabled]="!usarDesc"
          />
          @if (sesion.esDueno()) {
            <label class="check">
              <input type="checkbox" [ngModel]="alCosto" (ngModelChange)="aplicarAlCosto($event)" />
              Cobrar al costo
            </label>
          }
        </div>
        <select [(ngModel)]="medio">
          <option>Efectivo</option>
          <option>Mercado Pago</option>
        </select>
        <div class="row">
          <button class="btn btn-charge grow" type="button" [disabled]="cobrando || carrito.length === 0" (click)="cobrar()">
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
  `
})
export class CajaComponent implements OnInit {
  productos: ProductoDto[] = [];
  visibles: ProductoDto[] = [];
  totalLista = 0;
  carrito: LineaCarrito[] = [];
  buscar = '';
  filtroTipo = '';
  scanner = '';
  usarDesc = false;
  descuento = 0;
  alCosto = false;
  medio = 'Efectivo';
  cobrando = false;
  mensaje = '';
  error = '';
  toast = '';

  constructor(
    private readonly api: ApiService,
    readonly sesion: SesionService
  ) {}

  get items(): number {
    return this.carrito.reduce((n, l) => n + Number(l.cantidad), 0);
  }

  get total(): number {
    return this.carrito.reduce((n, l) => n + this.precioLinea(l) * Number(l.cantidad), 0);
  }

  precioLinea(l: LineaCarrito): number {
    if (l.sector || l.generico) return Number(l.precioUnitario);
    if (this.alCosto) return Number(l.costo);
    const desc = this.usarDesc ? Math.min(100, Math.max(0, Number(this.descuento) || 0)) : 0;
    const lista = Number(l.precioUnitario);
    const costo = Number(l.costo);
    const margen = Math.max(0, lista - costo);
    return Math.round((lista - (margen * desc) / 100) * 100) / 100;
  }

  aplicarDesc(v: boolean, el: HTMLInputElement): void {
    this.usarDesc = v;
    if (!v) this.descuento = 0;
    else queueMicrotask(() => el.focus());
  }

  aplicarAlCosto(v: boolean): void {
    if (v && !confirm('¿Cobrar esta venta al costo?')) {
      this.alCosto = false;
      return;
    }
    this.alCosto = v;
  }

  ngOnInit(): void {
    this.cargar();
  }

  dinero(n: number): string {
    return `$ ${Number(n).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }

  cargar(): void {
    const sector = this.filtroTipo === 'sector' ? true : this.filtroTipo === 'comun' ? false : undefined;
    this.api.productos(this.buscar, { sector }).subscribe({
      next: (r) => {
        this.productos = r.items;
        this.visibles = r.items;
        this.totalLista = r.total;
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
      this.carrito.push({
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
    const ya = this.carrito.find((l) => l.idProducto === p.id && !l.generico && !l.sector);
    if (ya) ya.cantidad = Number(ya.cantidad) + 1;
    else {
      this.carrito.push({
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
    this.carrito.push({
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

  sacar(l: LineaCarrito): void {
    this.carrito = this.carrito.filter((x) => x !== l);
  }

  cancelar(): void {
    this.carrito = [];
    this.error = '';
    this.mensaje = '';
    this.usarDesc = false;
    this.descuento = 0;
    this.alCosto = false;
  }

  cobrar(): void {
    if (this.carrito.length === 0) return;
    this.cobrando = true;
    this.error = '';
    const items: ItemVenta[] = this.carrito.map((l) => ({
      idProducto: l.idProducto,
      codigo: l.codigo,
      nombre: l.nombre,
      cantidad: Number(l.cantidad),
      precioUnitario: l.precioUnitario
    }));
    const desc = this.usarDesc ? Number(this.descuento) || 0 : 0;
    this.api.cobrar(items, this.medio, desc, this.sesion.esDueno() && this.alCosto).subscribe({
      next: (r) => {
        this.toast = `Cobrado ${this.dinero(r.total)}`;
        setTimeout(() => (this.toast = ''), 2500);
        this.cancelar();
        this.cobrando = false;
        this.cargar();
      },
      error: (err) => {
        this.error = typeof err.error === 'string' ? err.error : 'No se pudo cobrar.';
        this.cobrando = false;
      }
    });
  }
}
