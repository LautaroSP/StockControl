import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, ComboDto, ComboPedidoDto, ProductoDto } from '../servicios/api.service';
import { DialogoService } from '../servicios/dialogo.service';

interface LineaForm {
  idProducto: number;
  codigo: string;
  nombre: string;
  cantidad: number;
}

@Component({
  selector: 'sc-combos',
  standalone: true,
  imports: [FormsModule],
  styles: `
    .combo-form { margin-bottom: 16px; }
    .combo-form h3 { margin: 22px 0 10px; font-size: 14px; color: var(--ink-soft); font-weight: 600; }
    .combo-campos { display: grid; grid-template-columns: repeat(auto-fit, minmax(170px, 1fr)); gap: 4px 16px; }
    .combo-campos .field { margin: 8px 0; }
    .combo-linea { display: flex; align-items: center; gap: 10px; padding: 8px 10px; border: 1px solid var(--line); border-radius: 6px; margin-bottom: 6px; }
    .combo-acciones { display: flex; gap: 10px; margin-top: 22px; }
    .combo-candidatos { max-height: 260px; overflow: auto; border: 1px solid var(--line); border-radius: 8px; margin-top: 8px; }
  `,
  template: `
    <div class="topbar">
      <h1>Combos</h1>
      <button class="btn btn-primary" type="button" (click)="nuevo()">Nuevo combo</button>
    </div>
    @if (error) { <p class="error">{{ error }}</p> }
    @if (form) {
      <section class="panel config-form combo-form">
        <h2>{{ editando ? 'Editar combo' : 'Nuevo combo' }}</h2>
        <div class="combo-campos">
          <label class="field">Nombre <input class="wide" type="text" maxlength="60" [(ngModel)]="form.nombre" placeholder="2 Coca + Fernet" /></label>
          <label class="field">Precio de todo el combo <input class="wide" type="number" min="0.01" step="0.01" [(ngModel)]="form.precioTotal" /></label>
          <label class="field">Desde <input class="wide" type="date" [(ngModel)]="form.fechaDesde" /></label>
          @if (!form.sinFin) {
            <label class="field">Hasta <input class="wide" type="date" [(ngModel)]="form.fechaHasta" /></label>
          }
        </div>
        <label class="check" style="margin-top:8px">
          <input type="checkbox" [(ngModel)]="form.sinFin" />
          Sin fecha de fin (queda vigente hasta que lo borres)
        </label>
        <p class="meta">No aplica si en la caja hay descuento, cobro al costo o "No aplicar ofertas". Solo se aplica si le conviene al cliente.</p>

        <h3>Productos del combo</h3>
        @for (l of lineas; track l.idProducto) {
          <div class="combo-linea">
            <span class="grow"><strong>{{ l.nombre }}</strong> <span class="meta">{{ l.codigo }}</span></span>
            <label>Cantidad <input type="number" min="1" max="100" style="width:80px" [(ngModel)]="l.cantidad" /></label>
            <button class="btn" type="button" (click)="quitar(l)">Quitar</button>
          </div>
        }
        <input class="wide" type="search" [(ngModel)]="buscar" (ngModelChange)="buscarProductos()" placeholder="Buscar producto para sumar" />
        @if (candidatos.length) {
          <div class="combo-candidatos">
            <table class="data">
              <tbody>
                @for (p of candidatos; track p.id) {
                  <tr class="clickable" (click)="sumar(p)"><td>{{ p.codigo }}</td><td>{{ p.nombre }}</td><td class="num">{{ dinero(p.precio) }}</td></tr>
                }
              </tbody>
            </table>
          </div>
        }

        <div class="combo-acciones">
          <button class="btn btn-primary" type="button" [disabled]="guardando" (click)="guardar()">Guardar</button>
          <button class="btn" type="button" (click)="cerrar()">Cancelar</button>
        </div>
      </section>
    }
    <section class="panel">
      <table class="data">
        <thead><tr><th>Combo</th><th>Productos</th><th class="num">Precio</th><th>Desde</th><th>Hasta</th><th>Estado</th><th></th></tr></thead>
        <tbody>
          @for (c of combos; track c.id) {
            <tr>
              <td>{{ c.nombre }}</td>
              <td>@for (l of c.lineas; track l.idProducto) { <div>{{ l.cantidad }} × {{ l.nombre }}</div> }</td>
              <td class="num">{{ dinero(c.precioTotal) }}</td>
              <td>{{ fecha(c.fechaDesde) }}</td>
              <td>{{ c.sinFin ? 'Sin fin' : fecha(c.fechaHasta) }}</td>
              <td>@if (c.vigente) { <span class="badge badge-aviso">Vigente</span> } @else { <span class="meta">{{ c.fechaHasta < hoy ? 'Terminó' : 'Programado' }}</span> }</td>
              <td class="num">
                <button class="btn" type="button" (click)="editar(c)">Editar</button>
                <button class="btn btn-danger" type="button" (click)="borrar(c)">Borrar</button>
              </td>
            </tr>
          } @empty {
            <tr><td colspan="7" class="meta">No hay combos cargados.</td></tr>
          }
        </tbody>
      </table>
    </section>
  `
})
export class CombosComponent implements OnInit {
  combos: ComboDto[] = [];
  error = '';
  form: ComboPedidoDto | null = null;
  lineas: LineaForm[] = [];
  editando: number | null = null;
  buscar = '';
  candidatos: ProductoDto[] = [];
  guardando = false;
  readonly hoy = new Date().toLocaleDateString('sv-SE', { timeZone: 'America/Argentina/Buenos_Aires' });

  constructor(private readonly api: ApiService, private readonly dialogo: DialogoService) {}

  ngOnInit(): void {
    this.cargar();
  }

  cargar(): void {
    this.api.combos().subscribe({
      next: (r) => (this.combos = r),
      error: () => (this.error = 'No se pudieron cargar los combos.')
    });
  }

  nuevo(): void {
    this.editando = null;
    this.lineas = [];
    this.form = { nombre: '', precioTotal: 0, fechaDesde: this.hoy, fechaHasta: this.hoy, sinFin: false, lineas: [] };
    this.reiniciarBusqueda();
  }

  editar(c: ComboDto): void {
    this.editando = c.id;
    this.lineas = c.lineas.map((l) => ({ idProducto: l.idProducto, codigo: '', nombre: l.nombre, cantidad: l.cantidad }));
    this.form = { nombre: c.nombre, precioTotal: c.precioTotal, fechaDesde: c.fechaDesde, fechaHasta: c.sinFin ? this.hoy : c.fechaHasta, sinFin: c.sinFin, lineas: [] };
    this.reiniciarBusqueda();
  }

  cerrar(): void {
    this.form = null;
    this.editando = null;
  }

  buscarProductos(): void {
    const q = this.buscar.trim();
    if (q.length < 2) {
      this.candidatos = [];
      return;
    }
    this.api.productos(q, { sector: false }).subscribe({ next: (r) => (this.candidatos = r.items.slice(0, 20)) });
  }

  sumar(p: ProductoDto): void {
    if (!this.lineas.some((l) => l.idProducto === p.id)) {
      this.lineas.push({ idProducto: p.id, codigo: p.codigo, nombre: p.nombre, cantidad: 1 });
    }
    this.reiniciarBusqueda();
  }

  quitar(l: LineaForm): void {
    this.lineas = this.lineas.filter((x) => x !== l);
  }

  guardar(): void {
    if (!this.form) return;
    const datos: ComboPedidoDto = {
      ...this.form,
      nombre: this.form.nombre.trim(),
      precioTotal: Number(this.form.precioTotal),
      lineas: this.lineas.map((l) => ({ idProducto: l.idProducto, cantidad: Number(l.cantidad) }))
    };
    this.guardando = true;
    const pedido = this.editando ? this.api.editarCombo(this.editando, datos) : this.api.crearCombo(datos);
    pedido.subscribe({
      next: () => {
        this.guardando = false;
        this.cerrar();
        this.cargar();
      },
      error: (e) => {
        this.guardando = false;
        this.error = typeof e.error === 'string' ? e.error : 'No se pudo guardar el combo.';
      }
    });
  }

  async borrar(c: ComboDto): Promise<void> {
    if (!await this.dialogo.confirmar(`¿Borrar el combo ${c.nombre}?`)) return;
    this.api.borrarCombo(c.id).subscribe({
      next: () => this.cargar(),
      error: () => (this.error = 'No se pudo borrar el combo.')
    });
  }

  fecha(f: string): string {
    const [a, m, d] = f.split('-');
    return `${d}/${m}/${a}`;
  }

  dinero(n: number): string {
    return `$ ${Number(n).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }

  private reiniciarBusqueda(): void {
    this.buscar = '';
    this.candidatos = [];
    this.error = '';
  }
}
