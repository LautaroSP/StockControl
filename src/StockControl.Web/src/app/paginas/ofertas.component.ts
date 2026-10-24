import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, OfertaDto, OfertaPedidoDto, ProductoDto } from '../servicios/api.service';
import { DialogoService } from '../servicios/dialogo.service';

@Component({
  selector: 'sc-ofertas',
  standalone: true,
  imports: [FormsModule],
  styles: `
    .oferta-form { margin-bottom: 16px; }
    .oferta-form h3 { margin: 22px 0 10px; font-size: 14px; color: var(--ink-soft); font-weight: 600; }
    .oferta-campos {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(170px, 1fr));
      gap: 4px 16px;
    }
    .oferta-campos .field { margin: 8px 0; }
    .producto-elegido {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 8px 10px;
      border: 1px solid var(--line);
      border-radius: 6px;
    }
    .oferta-opciones {
      display: flex;
      flex-direction: column;
      gap: 12px;
      padding: 14px 16px;
      border: 1px solid var(--line);
      border-radius: 8px;
    }
    .oferta-acciones { display: flex; gap: 10px; margin-top: 22px; }
    .oferta-titulo-lista { margin: 22px 0 8px; }
    .oferta-candidatos { max-height: 260px; overflow: auto; border: 1px solid var(--line); border-radius: 8px; }
  `,
  template: `
    <div class="topbar">
      <h1>Ofertas</h1>
      <button class="btn btn-primary" type="button" (click)="nueva()">Nueva oferta</button>
    </div>
    @if (error) { <p class="error">{{ error }}</p> }
    @if (form) {
      <section class="panel config-form oferta-form">
        <h2>{{ editando ? 'Editar oferta' : 'Nueva oferta' }}</h2>

        <div class="field">
          <label>Producto</label>
          @if (producto) {
            <div class="producto-elegido">
              <span class="grow"><strong>{{ producto.nombre }}</strong> <span class="meta">{{ producto.codigo }}</span></span>
              <button class="btn" type="button" (click)="producto = null">Cambiar</button>
            </div>
          } @else {
            <input class="wide" type="search" [(ngModel)]="buscar" (ngModelChange)="buscarProductos()" placeholder="Buscar por nombre o código" />
          }
        </div>

        <div class="oferta-campos">
          <label class="field">Tipo
            <select class="wide" [(ngModel)]="form.tipo">
              <option value="descuento">Descuento %</option>
              <option value="llevaPaga">Llevás N, pagás M</option>
              <option value="precioDesde">Desde N unidades, cada una a $</option>
              <option value="precioExacto">Exactamente N unidades, cada una a $</option>
            </select>
          </label>
          @if (form.tipo === 'descuento') {
            <label class="field">Descuento %
              <input class="wide" type="number" min="1" max="99" [(ngModel)]="form.porcentaje" />
            </label>
          } @else if (form.tipo === 'llevaPaga') {
            <label class="field">Llevás
              <input class="wide" type="number" min="2" [(ngModel)]="form.lleva" />
            </label>
            <label class="field">Pagás
              <input class="wide" type="number" min="1" [(ngModel)]="form.paga" />
            </label>
          } @else {
            <label class="field">{{ form.tipo === 'precioDesde' ? 'Desde (unidades)' : 'Unidades' }}
              <input class="wide" type="number" min="2" [(ngModel)]="form.lleva" />
            </label>
            <label class="field">Precio por unidad
              <input class="wide" type="number" min="0.01" step="0.01" [(ngModel)]="form.precioUnidad" />
            </label>
          }
          <label class="field">Desde <input class="wide" type="date" [(ngModel)]="form.fechaDesde" /></label>
          @if (!form.sinFin) {
            <label class="field">Hasta <input class="wide" type="date" [(ngModel)]="form.fechaHasta" /></label>
          }
        </div>
        <label class="check" style="margin-top:8px">
          <input type="checkbox" [(ngModel)]="form.sinFin" />
          Sin fecha de fin (queda vigente hasta que la borres)
        </label>

        <h3>En la caja</h3>
        <div class="oferta-opciones">
          <label class="check">
            <input type="checkbox" [(ngModel)]="form.anulaConDescuentoCaja" />
            Aplicar descuento agregado desde la caja (deshabilita la oferta)
          </label>
          <label class="check">
            <input type="checkbox" [(ngModel)]="form.anulaConAlCosto" />
            Aplicar cobro al costo desde la caja (deshabilita la oferta)
          </label>
          <label class="check">
            <input type="checkbox" [(ngModel)]="form.permiteNoAplicarEnCaja" />
            No aplicar ofertas en la caja (muestra el tilde en la caja cuando entra esta oferta)
          </label>
        </div>

        <div class="oferta-acciones">
          <button class="btn btn-primary" type="button" [disabled]="!producto || guardando" (click)="guardar()">Guardar</button>
          <button class="btn" type="button" (click)="cerrar()">Cancelar</button>
        </div>

        @if (!producto && candidatos.length) {
          <p class="meta oferta-titulo-lista">Elegí el producto</p>
          <div class="oferta-candidatos">
            <table class="data">
              <tbody>
                @for (p of candidatos; track p.id) {
                  <tr class="clickable" (click)="elegir(p)"><td>{{ p.codigo }}</td><td>{{ p.nombre }}</td><td class="num">{{ dinero(p.precio) }}</td></tr>
                }
              </tbody>
            </table>
          </div>
        }
      </section>
    }
    <section class="panel">
      <table class="data">
        <thead><tr><th>Producto</th><th>Oferta</th><th>Desde</th><th>Hasta</th><th>Estado</th><th></th></tr></thead>
        <tbody>
          @for (o of ofertas; track o.id) {
            <tr>
              <td>{{ o.codigo }} · {{ o.nombre }}</td>
              <td>
                {{ o.descripcion }}
                @if (opciones(o); as texto) { <div class="meta">{{ texto }}</div> }
              </td>
              <td>{{ fecha(o.fechaDesde) }}</td>
              <td>{{ o.sinFin ? 'Sin fin' : fecha(o.fechaHasta) }}</td>
              <td>
                @if (o.vigente) { <span class="badge badge-aviso">Vigente</span> } @else { <span class="meta">{{ o.fechaHasta < hoy ? 'Terminó' : 'Programada' }}</span> }
              </td>
              <td class="num">
                <button class="btn" type="button" (click)="editar(o)">Editar</button>
                <button class="btn btn-danger" type="button" (click)="borrar(o)">Borrar</button>
              </td>
            </tr>
          } @empty {
            <tr><td colspan="6" class="meta">No hay ofertas cargadas.</td></tr>
          }
        </tbody>
      </table>
      @if (total > ofertas.length || pagina > 1) {
        <div class="row pad">
          <button class="btn" type="button" [disabled]="pagina === 1" (click)="cargar(pagina - 1)">Anterior</button>
          <span class="meta">Página {{ pagina }}</span>
          <button class="btn" type="button" [disabled]="pagina * tamano >= total" (click)="cargar(pagina + 1)">Siguiente</button>
        </div>
      }
    </section>
  `
})
export class OfertasComponent implements OnInit {
  ofertas: OfertaDto[] = [];
  total = 0;
  pagina = 1;
  tamano = 50;
  error = '';
  form: OfertaPedidoDto | null = null;
  editando: number | null = null;
  producto: Pick<ProductoDto, 'id' | 'codigo' | 'nombre'> | null = null;
  buscar = '';
  candidatos: ProductoDto[] = [];
  guardando = false;
  readonly hoy = new Date().toLocaleDateString('sv-SE', { timeZone: 'America/Argentina/Buenos_Aires' });

  constructor(private readonly api: ApiService, private readonly dialogo: DialogoService) {}

  ngOnInit(): void {
    this.cargar(1);
  }

  cargar(pagina: number): void {
    this.api.ofertas(pagina).subscribe({
      next: (r) => {
        this.ofertas = r.items;
        this.total = r.total;
        this.pagina = r.pagina;
        this.tamano = r.tamano;
      },
      error: () => (this.error = 'No se pudieron cargar las ofertas.')
    });
  }

  nueva(): void {
    this.editando = null;
    this.producto = null;
    this.buscar = '';
    this.candidatos = [];
    this.form = {
      idProducto: 0,
      tipo: 'descuento',
      porcentaje: 10,
      lleva: 3,
      paga: 2,
      precioUnidad: 0,
      fechaDesde: this.hoy,
      fechaHasta: this.hoy,
      sinFin: false,
      anulaConDescuentoCaja: false,
      anulaConAlCosto: true,
      permiteNoAplicarEnCaja: false
    };
    this.error = '';
  }

  editar(o: OfertaDto): void {
    this.editando = o.id;
    this.producto = { id: o.idProducto, codigo: o.codigo, nombre: o.nombre };
    this.form = {
      idProducto: o.idProducto,
      tipo: o.tipo,
      porcentaje: o.porcentaje || 10,
      lleva: o.lleva || 3,
      paga: o.paga || 2,
      precioUnidad: o.precioUnidad,
      fechaDesde: o.fechaDesde,
      fechaHasta: o.sinFin ? this.hoy : o.fechaHasta,
      sinFin: o.sinFin,
      anulaConDescuentoCaja: o.anulaConDescuentoCaja,
      anulaConAlCosto: o.anulaConAlCosto,
      permiteNoAplicarEnCaja: o.permiteNoAplicarEnCaja
    };
    this.error = '';
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

  elegir(p: ProductoDto): void {
    this.producto = p;
    this.candidatos = [];
  }

  guardar(): void {
    if (!this.form || !this.producto) return;
    const datos = { ...this.form, idProducto: this.producto.id, porcentaje: Number(this.form.porcentaje), lleva: Number(this.form.lleva), paga: Number(this.form.paga), precioUnidad: Number(this.form.precioUnidad) || 0 };
    this.guardando = true;
    const pedido = this.editando ? this.api.editarOferta(this.editando, datos) : this.api.crearOferta(datos);
    pedido.subscribe({
      next: () => {
        this.guardando = false;
        this.cerrar();
        this.cargar(this.pagina);
      },
      error: (e) => {
        this.guardando = false;
        this.error = typeof e.error === 'string' ? e.error : 'No se pudo guardar la oferta.';
      }
    });
  }

  async borrar(o: OfertaDto): Promise<void> {
    if (!await this.dialogo.confirmar(`¿Borrar la oferta ${o.descripcion} de ${o.nombre}?`)) return;
    this.api.borrarOferta(o.id).subscribe({
      next: () => this.cargar(this.pagina),
      error: () => (this.error = 'No se pudo borrar la oferta.')
    });
  }

  opciones(o: OfertaDto): string {
    const textos = [
      o.anulaConDescuentoCaja ? 'el descuento de caja la saca' : '',
      o.anulaConAlCosto ? 'al costo la saca' : '',
      o.permiteNoAplicarEnCaja ? 'se puede no aplicar en caja' : ''
    ].filter(Boolean);
    return textos.join(' · ');
  }

  fecha(f: string): string {
    const [a, m, d] = f.split('-');
    return `${d}/${m}/${a}`;
  }

  dinero(n: number): string {
    return `$ ${Number(n).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }
}
