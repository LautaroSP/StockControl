import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, ProductoDto } from '../servicios/api.service';
import { SesionService } from '../servicios/sesion.service';

@Component({
  selector: 'sc-productos',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="topbar">
      <h1>Productos</h1>
      <div class="row">
        @if (sesion.esDueno()) {
          <button class="btn btn-primary" type="button" (click)="nuevo()">Nuevo producto</button>
        }
        <button class="btn" type="button" [disabled]="!elegido" (click)="abrirStock()">Sumar stock</button>
      </div>
    </div>
    <div class="toolbar" style="margin-bottom:12px">
      <input class="grow" type="search" [ngModel]="q" (ngModelChange)="aplicarBuscar($event)" placeholder="Buscar nombre o código" />
      <select [ngModel]="tipo" (ngModelChange)="aplicarTipo($event)">
        <option value="">Tipo: todos</option>
        <option value="comun">Común</option>
        <option value="sector">Sector</option>
      </select>
      <span class="meta">{{ total }} en lista</span>
    </div>
    <section class="panel">
      <table class="data">
        <thead>
          <tr>
            <th>Código</th>
            <th>Nombre</th>
            <th class="num">Stock</th>
            @if (sesion.esDueno()) {
              <th class="num">Costo</th>
            }
            <th class="num">Precio</th>
            <th>Tipo</th>
          </tr>
        </thead>
        <tbody>
          @for (p of visibles; track p.id) {
            <tr
              class="clickable"
              [class.low]="!p.productoSector && p.cantidad <= 3"
              [class.selected]="elegido?.id === p.id"
              (click)="elegido = p"
              (dblclick)="editar(p)"
            >
              <td>{{ p.codigo }}</td>
              <td>{{ p.nombre }}</td>
              <td class="num">{{ p.productoSector ? '—' : p.cantidad }}</td>
              @if (sesion.esDueno()) {
                <td class="num">{{ dinero(p.costo) }}</td>
              }
              <td class="num">{{ p.productoSector ? 'en caja' : dinero(p.precio) }}</td>
              <td>{{ p.productoSector ? 'Sector' : 'Común' }}</td>
            </tr>
          }
        </tbody>
      </table>
    </section>
    @if (sesion.esDueno()) {
      <p class="meta" style="margin-top:12px">Dueño: alta y edición (doble clic). Empleado: lista y sumar stock.</p>
    } @else {
      <p class="meta" style="margin-top:12px">Podés sumar stock. No cambiás precios ni creás productos.</p>
    }
    @if (error) {
      <p class="error">{{ error }}</p>
    }

    <div class="modal-back" [class.show]="formAbierto">
      <div class="modal">
        <h2 style="margin:0 0 8px;font-size:18px">{{ editando ? 'Editar producto' : 'Nuevo producto' }}</h2>
        <label class="field">Código <input class="wide" [(ngModel)]="form.codigo" [disabled]="!!editando" /></label>
        <label class="field">Nombre <input class="wide" [(ngModel)]="form.nombre" /></label>
        <label class="field">Stock <input class="wide" type="number" [(ngModel)]="form.cantidad" /></label>
        <label class="field">Costo <input class="wide" type="number" [(ngModel)]="form.costo" /></label>
        <label class="field">Precio <input class="wide" type="number" [(ngModel)]="form.precio" /></label>
        <label class="check"><input type="checkbox" [(ngModel)]="form.productoSector" /> Producto sector</label>
        @if (error) {
          <p class="error">{{ error }}</p>
        }
        <div class="row" style="margin-top:12px">
          <button class="btn btn-primary grow" type="button" (click)="guardar()">Guardar</button>
          <button class="btn" type="button" (click)="formAbierto = false">Cancelar</button>
        </div>
      </div>
    </div>

    <div class="modal-back" [class.show]="stockAbierto">
      <div class="modal">
        <h2 style="margin:0 0 8px;font-size:18px">Sumar stock</h2>
        <p class="meta">{{ elegido?.nombre }}</p>
        <label class="field">Cantidad <input class="wide" type="number" min="0.001" [(ngModel)]="cantidadStock" /></label>
        <div class="row" style="margin-top:12px">
          <button class="btn btn-primary grow" type="button" (click)="sumar()">Sumar</button>
          <button class="btn" type="button" (click)="stockAbierto = false">Cancelar</button>
        </div>
      </div>
    </div>
    <div class="toast" [class.show]="toast">{{ toast }}</div>
  `
})
export class ProductosComponent implements OnInit {
  productos: ProductoDto[] = [];
  visibles: ProductoDto[] = [];
  total = 0;
  elegido: ProductoDto | null = null;
  q = '';
  tipo = '';
  error = '';
  toast = '';
  formAbierto = false;
  stockAbierto = false;
  editando: ProductoDto | null = null;
  cantidadStock = 1;
  form: Partial<ProductoDto> = { codigo: '', nombre: '', cantidad: 0, costo: 0, precio: 0, productoSector: false };

  constructor(
    private readonly api: ApiService,
    readonly sesion: SesionService
  ) {}

  ngOnInit(): void {
    this.cargar();
  }

  dinero(n: number): string {
    return `$ ${Number(n).toLocaleString('es-AR', { minimumFractionDigits: 0, maximumFractionDigits: 0 })}`;
  }

  cargar(): void {
    const sector = this.tipo === 'sector' ? true : this.tipo === 'comun' ? false : undefined;
    this.api.productos(this.q, { sector }).subscribe({
      next: (r) => {
        this.productos = r.items;
        this.visibles = r.items;
        this.total = r.total;
      },
      error: () => (this.error = 'No se pudieron cargar los productos.')
    });
  }

  aplicarTipo(tipo: string): void {
    this.tipo = tipo;
    this.cargar();
  }

  aplicarBuscar(q: string): void {
    this.q = q;
    this.cargar();
  }

  filtrar(): void {
    this.cargar();
  }

  nuevo(): void {
    this.editando = null;
    this.form = { codigo: '', nombre: '', cantidad: 0, costo: 0, precio: 0, productoSector: false };
    this.formAbierto = true;
  }

  editar(p: ProductoDto): void {
    if (!this.sesion.esDueno()) return;
    this.editando = p;
    this.form = { ...p };
    this.formAbierto = true;
  }

  guardar(): void {
    const pedido = this.form;
    const obs = this.editando
      ? this.api.editarProducto(this.editando.id, pedido)
      : this.api.crearProducto(pedido);
    obs.subscribe({
      next: () => {
        this.formAbierto = false;
        this.error = '';
        if (pedido.productoSector) this.tipo = 'sector';
        this.mostrar('Guardado');
        this.cargar();
      },
      error: (err) => (this.error = typeof err.error === 'string' ? err.error : 'No se pudo guardar.')
    });
  }

  abrirStock(): void {
    if (!this.elegido) return;
    this.cantidadStock = 1;
    this.stockAbierto = true;
  }

  sumar(): void {
    if (!this.elegido) return;
    this.api.sumarStock(this.elegido.id, Number(this.cantidadStock)).subscribe({
      next: () => {
        this.stockAbierto = false;
        this.mostrar('Stock actualizado');
        this.cargar();
      },
      error: (err) => (this.error = typeof err.error === 'string' ? err.error : 'No se pudo sumar stock.')
    });
  }

  private mostrar(t: string): void {
    this.toast = t;
    setTimeout(() => (this.toast = ''), 2000);
  }
}
