import { Component, OnInit } from '@angular/core';
import { ApiService, CategoriaDto } from '../servicios/api.service';
import { DialogoService } from '../servicios/dialogo.service';

@Component({
  selector: 'sc-categorias',
  standalone: true,
  template: `
    <div class="topbar">
      <h1>Categorías</h1>
      <button class="btn btn-primary" type="button" (click)="crear()">Nueva categoría</button>
    </div>
    @if (error) { <p class="error">{{ error }}</p> }
    <section class="panel">
      <table class="data">
        <thead><tr><th>Categoría</th><th class="num">Productos</th><th class="num">Grupos</th><th></th></tr></thead>
        <tbody>
          @for (categoria of categorias; track categoria.id) {
            <tr>
              <td>{{ categoria.nombre }}</td>
              <td class="num">{{ categoria.cantidadProductos }}</td>
              <td class="num">{{ categoria.cantidadGrupos }}</td>
              <td class="num">
                <button class="btn" type="button" (click)="editar(categoria)">Editar</button>
                <button class="btn btn-danger" type="button" (click)="eliminar(categoria)">Eliminar</button>
              </td>
            </tr>
          } @empty {
            <tr><td colspan="4" class="meta">Todavía no hay categorías.</td></tr>
          }
        </tbody>
      </table>
    </section>
    <p class="hint">
      La categoría ordena los productos en la tienda. Un grupo de precio puede tener categoría:
      sus productos la heredan, salvo los que tengan una propia.
    </p>
  `
})
export class CategoriasComponent implements OnInit {
  categorias: CategoriaDto[] = [];
  error = '';

  constructor(private readonly api: ApiService, private readonly dialogo: DialogoService) {}

  ngOnInit(): void {
    this.cargar();
  }

  cargar(): void {
    this.api.categorias().subscribe({
      next: (r) => {
        this.categorias = r;
        this.error = '';
      },
      error: () => (this.error = 'No se pudieron cargar las categorías.')
    });
  }

  async crear(): Promise<void> {
    const nombre = (await this.dialogo.pedir('Nombre de la categoría (ej.: Bebidas)'))?.trim();
    if (!nombre) return;
    this.api.crearCategoria(nombre).subscribe({ next: () => this.cargar(), error: (e) => (this.error = this.mensaje(e)) });
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
    if (categoria.cantidadProductos) partes.push(`${categoria.cantidadProductos} producto${categoria.cantidadProductos === 1 ? '' : 's'}`);
    if (categoria.cantidadGrupos) partes.push(`${categoria.cantidadGrupos} grupo${categoria.cantidadGrupos === 1 ? '' : 's'}`);
    if (!partes.length) return `¿Eliminar «${categoria.nombre}»?`;
    return `«${categoria.nombre}» tiene ${partes.join(' y ')}. Quedan sin categoría, no se borran. ¿Eliminar?`;
  }

  private mensaje(error: { error?: unknown }): string {
    return typeof error.error === 'string' ? error.error : 'No se pudo guardar la categoría.';
  }
}
