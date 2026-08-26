import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService, GrupoDetalleDto, GrupoListaDto, ProductoDto } from '../servicios/api.service';
import { SesionService } from '../servicios/sesion.service';

@Component({
  selector: 'sc-grupos',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="topbar">
      <h1>Grupos de precio</h1>
      <button class="btn btn-primary" type="button" (click)="nuevo()">Nuevo grupo</button>
    </div>
    @if (error) {
      <p class="error">{{ error }}</p>
    }
    <div class="split">
      <section class="panel">
        <div class="toolbar" style="padding:12px">
          <input
            class="grow"
            type="search"
            [(ngModel)]="qGrupo"
            placeholder="Buscar grupo"
          />
          <span class="meta">{{ gruposVisibles.length }} / {{ grupos.length }}</span>
        </div>
        <div class="scroll" style="max-height:calc(100vh - 200px);overflow:auto">
          <table class="data">
            <thead>
              <tr>
                <th>Grupo</th>
                <th class="num">Costo</th>
                <th class="num">Precio</th>
                <th class="num">SKU</th>
              </tr>
            </thead>
            <tbody>
              @for (g of gruposVisibles; track g.idGrupoProducto) {
                <tr
                  class="clickable"
                  [class.selected]="elegido?.idGrupoProducto === g.idGrupoProducto"
                  (click)="abrir(g.idGrupoProducto)"
                >
                  <td>{{ g.nombreGrupo }}</td>
                  <td class="num">{{ dinero(g.costo) }}</td>
                  <td class="num">{{ dinero(g.precioGrupo) }}</td>
                  <td class="num">{{ g.cantidad }}</td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </section>
      <section class="panel pad" #detalle>
        @if (!formAbierto && !elegido) {
          <p class="meta" style="margin-top:0">Elegí un grupo o creá uno nuevo.</p>
        }
        @if (formAbierto) {
          <label class="field">Nombre <input class="wide" [(ngModel)]="form.nombreGrupo" /></label>
          <div class="row" style="margin-top:8px">
            <label class="field grow">Costo <input class="wide" type="number" [(ngModel)]="form.costo" /></label>
            <label class="field grow">Precio <input class="wide" type="number" [(ngModel)]="form.precioGrupo" /></label>
          </div>
          @if (elegido) {
            <p class="meta">{{ elegido.nombreGrupo }} — al guardar, se actualiza el precio de todos.</p>
            <div class="row" style="margin:8px 0;gap:12px;align-items:center">
              <input
                class="grow"
                type="search"
                [(ngModel)]="qProd"
                (ngModelChange)="buscarProductos()"
                placeholder="Buscar producto común"
              />
              <label class="check">
                <input type="checkbox" [(ngModel)]="soloSeleccionados" />
                Solo seleccionados
              </label>
            </div>
            <div class="scroll" style="max-height:280px;overflow:auto;border:1px solid var(--line);border-radius:8px">
              <table class="data">
                <thead>
                  <tr>
                    <th style="width:36px"></th>
                    <th>Producto</th>
                    <th class="num">Precio</th>
                  </tr>
                </thead>
                <tbody>
                  @for (p of productosVisibles; track p.id) {
                    <tr class="clickable" (click)="toggleFila(p.id)">
                      <td>
                        <input
                          type="checkbox"
                          [checked]="idsMiembros.has(p.id)"
                          (click)="$event.stopPropagation()"
                          (change)="toggle(p.id, $event)"
                        />
                      </td>
                      <td>
                        {{ p.nombre }}
                        @if (p.idGrupoProducto && p.idGrupoProducto !== elegido.idGrupoProducto) {
                          <span class="meta"> · {{ p.nombreGrupo }}</span>
                        }
                      </td>
                      <td class="num">{{ dinero(p.precio) }}</td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
            <p class="hint">Los productos sector no entran a un grupo. El empleado no ve esta pantalla.</p>
          }
          <div class="row" style="margin-top:12px">
            <button class="btn btn-primary" type="button" (click)="guardar()">Guardar</button>
            @if (elegido) {
              <button class="btn btn-danger" type="button" (click)="eliminar()">Eliminar</button>
            }
            <button class="btn" type="button" (click)="cancelar()">Cancelar</button>
          </div>
        }
      </section>
    </div>
    <div class="toast" [class.show]="toast">{{ toast }}</div>
  `
})
export class GruposComponent implements OnInit {
  @ViewChild('detalle') detalle?: ElementRef<HTMLElement>;
  grupos: GrupoListaDto[] = [];
  elegido: GrupoDetalleDto | null = null;
  productos: ProductoDto[] = [];
  idsMiembros = new Set<number>();
  soloSeleccionados = false;
  formAbierto = false;
  form = { nombreGrupo: '', costo: 0, precioGrupo: 0 };
  qGrupo = '';
  qProd = '';
  error = '';
  toast = '';

  constructor(
    private readonly api: ApiService,
    private readonly router: Router,
    readonly sesion: SesionService
  ) {}

  ngOnInit(): void {
    if (!this.sesion.esDueno()) {
      void this.router.navigateByUrl('/caja');
      return;
    }
    this.cargar();
  }

  dinero(n: number): string {
    return `$ ${Number(n).toLocaleString('es-AR', { minimumFractionDigits: 0, maximumFractionDigits: 0 })}`;
  }

  get gruposVisibles(): GrupoListaDto[] {
    const q = this.qGrupo.trim().toLowerCase();
    if (!q) return this.grupos;
    return this.grupos.filter((g) => g.nombreGrupo.toLowerCase().includes(q));
  }

  get productosVisibles(): ProductoDto[] {
    if (!this.soloSeleccionados) return this.productos;
    return this.productos.filter((p) => this.idsMiembros.has(p.id));
  }

  cargar(): void {
    this.api.grupos().subscribe({
      next: (r) => {
        this.grupos = r;
        this.error = '';
      },
      error: () => (this.error = 'No se pudieron cargar los grupos.')
    });
  }

  nuevo(): void {
    this.elegido = null;
    this.idsMiembros = new Set();
    this.productos = [];
    this.form = { nombreGrupo: '', costo: 0, precioGrupo: 0 };
    this.formAbierto = true;
    this.irArriba();
  }

  abrir(id: number): void {
    this.api.grupo(id).subscribe({
      next: (d) => {
        this.elegido = d;
        this.form = { nombreGrupo: d.nombreGrupo, costo: d.costo, precioGrupo: d.precioGrupo };
        this.idsMiembros = new Set(d.miembros.map((m) => m.id));
        this.formAbierto = true;
        this.qProd = '';
        this.soloSeleccionados = false;
        this.buscarProductos();
        this.irArriba();
      },
      error: () => (this.error = 'No se pudo abrir el grupo.')
    });
  }

  private irArriba(): void {
    queueMicrotask(() => {
      const main = document.querySelector('main.main') as HTMLElement | null;
      if (main) main.scrollTo({ top: 0, behavior: 'smooth' });
      this.detalle?.nativeElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
    });
  }

  buscarProductos(): void {
    this.api.productos(this.qProd, { sector: false }).subscribe({
      next: (r) => {
        const miembros = this.elegido?.miembros ?? [];
        const porId = new Map(r.items.map((p) => [p.id, p]));
        for (const m of miembros) {
          if (!porId.has(m.id)) {
            porId.set(m.id, {
              id: m.id,
              idLocal: 0,
              codigo: m.codigo,
              nombre: m.nombre,
              cantidad: 0,
              costo: m.costo,
              precio: m.precio,
              productoSector: false,
              idGrupoProducto: this.elegido?.idGrupoProducto ?? 0,
              nombreGrupo: this.elegido?.nombreGrupo
            });
          }
        }
        this.productos = [...porId.values()].sort((a, b) => a.nombre.localeCompare(b.nombre, 'es'));
      },
      error: () => (this.error = 'No se pudieron cargar productos.')
    });
  }

  toggle(id: number, ev: Event): void {
    const on = (ev.target as HTMLInputElement).checked;
    if (on) this.idsMiembros.add(id);
    else this.idsMiembros.delete(id);
    this.idsMiembros = new Set(this.idsMiembros);
  }

  toggleFila(id: number): void {
    if (this.idsMiembros.has(id)) this.idsMiembros.delete(id);
    else this.idsMiembros.add(id);
    this.idsMiembros = new Set(this.idsMiembros);
  }

  guardar(): void {
    if (!this.form.nombreGrupo.trim()) {
      this.error = 'El nombre del grupo es obligatorio.';
      return;
    }
    if (this.elegido && this.idsMiembros.size > 0) {
      if (!confirm('¿Guardar y actualizar el precio/costo de todos los miembros?')) return;
    }
    const body = {
      nombreGrupo: this.form.nombreGrupo.trim(),
      costo: Number(this.form.costo) || 0,
      precioGrupo: Number(this.form.precioGrupo) || 0
    };
    if (!this.elegido) {
      this.api.crearGrupo(body).subscribe({
        next: (g) => {
          this.mostrar('Grupo creado');
          this.cargar();
          this.abrir(g.idGrupoProducto);
        },
        error: (err) => (this.error = typeof err.error === 'string' ? err.error : 'No se pudo crear.')
      });
      return;
    }
    const id = this.elegido.idGrupoProducto;
    this.api.editarGrupo(id, body).subscribe({
      next: () => {
        this.api.miembrosGrupo(id, [...this.idsMiembros]).subscribe({
          next: () => {
            this.mostrar('Grupo guardado');
            this.cargar();
            this.abrir(id);
          },
          error: (err) => (this.error = typeof err.error === 'string' ? err.error : 'No se pudieron guardar los miembros.')
        });
      },
      error: (err) => (this.error = typeof err.error === 'string' ? err.error : 'No se pudo guardar.')
    });
  }

  eliminar(): void {
    if (!this.elegido) return;
    if (!confirm('¿Eliminar el grupo? Los productos quedan sin grupo.')) return;
    this.api.eliminarGrupo(this.elegido.idGrupoProducto).subscribe({
      next: () => {
        this.mostrar('Grupo eliminado');
        this.cancelar();
        this.cargar();
      },
      error: (err) => (this.error = typeof err.error === 'string' ? err.error : 'No se pudo eliminar.')
    });
  }

  cancelar(): void {
    this.formAbierto = false;
    this.elegido = null;
    this.productos = [];
    this.idsMiembros = new Set();
  }

  private mostrar(t: string): void {
    this.toast = t;
    this.error = '';
    setTimeout(() => (this.toast = ''), 2000);
  }
}
