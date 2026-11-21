import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, CategoriaDto, PlantillaCategoriaDto } from '../servicios/api.service';
import { DialogoService } from '../servicios/dialogo.service';

@Component({
  selector: 'sc-categorias',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="topbar">
      <h1>Categorías</h1>
      <button class="btn" type="button" (click)="mostrarPlantillas = !mostrarPlantillas">Plantillas</button>
      <button class="btn btn-primary" type="button" (click)="crear()">Nueva categoría</button>
    </div>
    @if (error) { <p class="error">{{ error }}</p> }
    @if (aviso) { <p class="meta">{{ aviso }}</p> }

    @if (mostrarPlantillas || categorias.length === 0) {
      <section class="panel pad" style="margin-bottom:14px">
        <h2 style="margin:0 0 8px;font-size:1.1rem">¿Qué vendés en este local?</h2>
        <p class="hint" style="margin-top:0">Elegí una plantilla. Se cargan categorías y subcategorías; después podés agregar, renombrar o borrar.</p>
        <div class="plantillas">
          @for (p of plantillas; track p.clave) {
            <button class="btn plantilla" type="button" [disabled]="ocupado" (click)="aplicar(p)">
              <strong>{{ p.titulo }}</strong>
              <span class="meta">{{ p.descripcion }}</span>
            </button>
          }
        </div>
      </section>
    }

    <section class="panel">
      <table class="data">
        <thead><tr><th>Categoría</th><th class="num">Productos</th><th class="num">Grupos</th><th></th></tr></thead>
        <tbody>
          @for (categoria of raizYHijos; track categoria.id) {
            <tr [class.sub]="!!categoria.idPadre">
              <td>{{ categoria.idPadre ? '↳ ' : '' }}{{ categoria.nombre }}</td>
              <td class="num">{{ categoria.cantidadProductos }}</td>
              <td class="num">{{ categoria.cantidadGrupos }}</td>
              <td class="num">
                @if (!categoria.idPadre) {
                  <button class="btn" type="button" (click)="crear(categoria.id)">Subcategoría</button>
                }
                <button class="btn" type="button" (click)="editar(categoria)">Editar</button>
                <button class="btn btn-danger" type="button" (click)="eliminar(categoria)">Eliminar</button>
              </td>
            </tr>
          } @empty {
            <tr><td colspan="4" class="meta">Todavía no hay categorías. Elegí una plantilla o creá una.</td></tr>
          }
        </tbody>
      </table>
    </section>
    <p class="hint">
      La categoría ordena los productos en la tienda. Un grupo de precio puede tener categoría:
      sus productos la heredan, salvo los que tengan una propia. En Tienda › Configuración elegís chips o menú lateral.
    </p>
  `,
  styles: `
    .plantillas { display: grid; gap: 8px; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); }
    .plantilla { text-align: left; display: grid; gap: 4px; height: auto; white-space: normal; padding: 12px; }
    tr.sub td:first-child { padding-left: 1.25rem; color: var(--ink-soft); }
  `
})
export class CategoriasComponent implements OnInit {
  categorias: CategoriaDto[] = [];
  plantillas: PlantillaCategoriaDto[] = [];
  mostrarPlantillas = false;
  ocupado = false;
  error = '';
  aviso = '';

  constructor(private readonly api: ApiService, private readonly dialogo: DialogoService) {}

  get raizYHijos(): CategoriaDto[] {
    const raices = this.categorias.filter((c) => !c.idPadre).sort((a, b) => a.nombre.localeCompare(b.nombre, 'es'));
    const lista: CategoriaDto[] = [];
    for (const r of raices) {
      lista.push(r);
      lista.push(
        ...this.categorias
          .filter((c) => c.idPadre === r.id)
          .sort((a, b) => a.nombre.localeCompare(b.nombre, 'es'))
      );
    }
    return lista;
  }

  ngOnInit(): void {
    this.cargar();
    this.api.plantillasCategoria().subscribe({ next: (p) => (this.plantillas = p) });
  }

  cargar(): void {
    this.api.categorias().subscribe({
      next: (r) => {
        this.categorias = r;
        this.mostrarPlantillas = r.length === 0;
        this.error = '';
      },
      error: () => (this.error = 'No se pudieron cargar las categorías.')
    });
  }

  aplicar(p: PlantillaCategoriaDto): void {
    this.ocupado = true;
    this.api.aplicarPlantillaCategoria(p.clave).subscribe({
      next: (r) => {
        this.ocupado = false;
        this.aviso = r.creadas > 0
          ? `Se agregaron ${r.creadas} categorías de «${r.titulo}».`
          : `Ya tenías las categorías de «${r.titulo}».`;
        this.mostrarPlantillas = false;
        this.cargar();
      },
      error: (e) => {
        this.ocupado = false;
        this.error = this.mensaje(e);
      }
    });
  }

  async crear(idPadre?: number): Promise<void> {
    const nombre = (await this.dialogo.pedir(idPadre ? 'Nombre de la subcategoría' : 'Nombre de la categoría (ej.: Bebidas)'))?.trim();
    if (!nombre) return;
    this.api.crearCategoria(nombre, idPadre).subscribe({ next: () => this.cargar(), error: (e) => (this.error = this.mensaje(e)) });
  }

  async editar(categoria: CategoriaDto): Promise<void> {
    const nombre = (await this.dialogo.pedir('Nombre de la categoría', categoria.nombre))?.trim();
    if (!nombre || nombre === categoria.nombre) return;
    this.api.editarCategoria(categoria.id, nombre).subscribe({ next: () => this.cargar(), error: (e) => (this.error = this.mensaje(e)) });
  }

  async eliminar(categoria: CategoriaDto): Promise<void> {
    if (!await this.dialogo.confirmar(this.avisoBaja(categoria))) return;
    this.api.eliminarCategoria(categoria.id).subscribe({ next: () => this.cargar(), error: (e) => (this.error = this.mensaje(e)) });
  }

  private avisoBaja(categoria: CategoriaDto): string {
    const partes = [];
    if (categoria.cantidadHijos) partes.push(`${categoria.cantidadHijos} subcategoría${categoria.cantidadHijos === 1 ? '' : 's'}`);
    if (categoria.cantidadProductos) partes.push(`${categoria.cantidadProductos} producto${categoria.cantidadProductos === 1 ? '' : 's'}`);
    if (categoria.cantidadGrupos) partes.push(`${categoria.cantidadGrupos} grupo${categoria.cantidadGrupos === 1 ? '' : 's'}`);
    if (!partes.length) return `¿Eliminar «${categoria.nombre}»?`;
    return `«${categoria.nombre}» tiene ${partes.join(', ')}. Las subcategorías se borran; productos y grupos quedan sin categoría. ¿Eliminar?`;
  }

  private mensaje(error: { error?: unknown }): string {
    return typeof error.error === 'string' ? error.error : 'No se pudo guardar la categoría.';
  }
}
