import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService, ProveedorFichaDto, RevisionCatalogoDto } from '../servicios/api.service';
import { descargar } from '../servicios/descargar';

const MAX_BYTES = 2 * 1024 * 1024;

@Component({
  selector: 'sc-catalogo-proveedor',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="topbar">
      <h1>Cargar productos · {{ proveedor?.nombre }}</h1>
      <a class="btn" [routerLink]="['/proveedores', id]">Volver al proveedor</a>
    </div>
    <div class="toolbar" style="margin-bottom:12px">
      <button class="btn" type="button" (click)="bajarPlantilla()">Descargar plantilla</button>
      <label class="btn">
        Subir Excel
        <input type="file" accept=".xlsx" hidden (change)="subir($event)" />
      </label>
      @if (revision) {
        <span class="meta">{{ revision.validas.length }} para importar · {{ revision.invalidas.length }} con error</span>
        <button class="btn btn-primary" type="button" [disabled]="!revision.validas.length || confirmando" (click)="confirmar()">
          {{ confirmando ? 'Importando...' : 'Confirmar' }}
        </button>
      }
    </div>
    <p class="meta">
      Columnas: Codigo, Nombre de producto, Costo, Cantidad. El precio sale de costo × ganancia × IVA del local.
      Si el código ya existe, se actualiza y la cantidad se suma al stock.
    </p>
    @if (error) {
      <p class="error">{{ error }}</p>
    }
    @if (mensaje) {
      <p class="meta">{{ mensaje }}</p>
    }
    @if (revision) {
      @if (revision.invalidas.length) {
        <section class="panel" style="margin-bottom:12px">
          <table class="data">
            <thead><tr><th>Fila</th><th>Código</th><th>Error</th></tr></thead>
            <tbody>
              @for (f of revision.invalidas; track f.fila) {
                <tr><td>{{ f.fila }}</td><td>{{ f.codigo }}</td><td class="error">{{ f.motivo }}</td></tr>
              }
            </tbody>
          </table>
        </section>
      }
      <section class="panel">
        <table class="data">
          <thead>
            <tr>
              <th>Fila</th>
              <th>Código</th>
              <th>Nombre</th>
              <th class="num">Costo</th>
              <th class="num">Precio</th>
              <th class="num">Cantidad</th>
              <th>Acción</th>
            </tr>
          </thead>
          <tbody>
            @for (f of revision.validas; track f.fila) {
              <tr>
                <td>{{ f.fila }}</td>
                <td>{{ f.codigo }}</td>
                <td>{{ f.nombre }}</td>
                <td class="num">{{ dinero(f.costo) }}</td>
                <td class="num">{{ dinero(f.precio) }}</td>
                <td class="num">{{ f.cantidad }}</td>
                <td>
                  {{ f.accion === 'crear' ? 'Nuevo' : 'Actualiza' }}
                  @if (f.aviso) { <span class="meta"> · {{ f.aviso }}</span> }
                </td>
              </tr>
            }
          </tbody>
        </table>
      </section>
    }
  `
})
export class CatalogoProveedorComponent implements OnInit {
  id = 0;
  proveedor: ProveedorFichaDto | null = null;
  revision: RevisionCatalogoDto | null = null;
  confirmando = false;
  error = '';
  mensaje = '';

  constructor(
    private readonly api: ApiService,
    private readonly ruta: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.id = Number(this.ruta.snapshot.paramMap.get('id'));
    this.api.proveedor(this.id).subscribe({
      next: (r) => (this.proveedor = r),
      error: () => (this.error = 'No se pudo cargar el proveedor.')
    });
  }

  bajarPlantilla(): void {
    this.api.plantillaProveedor().subscribe({
      next: (archivo) => descargar(archivo, 'plantilla-proveedor.xlsx'),
      error: () => (this.error = 'No se pudo bajar la plantilla.')
    });
  }

  subir(evento: Event): void {
    const input = evento.target as HTMLInputElement;
    const archivo = input.files?.[0];
    input.value = '';
    if (!archivo) return;
    if (archivo.size > MAX_BYTES) {
      this.error = 'El Excel no puede pesar más de 2 MB.';
      return;
    }
    this.error = '';
    this.mensaje = '';
    this.revision = null;
    const lector = new FileReader();
    lector.onload = () => {
      this.api.revisarCatalogo(this.id, String(lector.result)).subscribe({
        next: (r) => (this.revision = r),
        error: (e) => (this.error = typeof e.error === 'string' ? e.error : 'No se pudo leer el Excel.')
      });
    };
    lector.readAsDataURL(archivo);
  }

  confirmar(): void {
    if (!this.revision) return;
    this.confirmando = true;
    this.api.confirmarCatalogo(this.id, this.revision.filas).subscribe({
      next: (r) => {
        this.confirmando = false;
        this.revision = null;
        this.mensaje = `Listo: ${r.creados} nuevos, ${r.actualizados} actualizados.`;
        if (r.invalidas.length) this.mensaje += ` ${r.invalidas.length} filas no se importaron.`;
      },
      error: (e) => {
        this.confirmando = false;
        this.error = typeof e.error === 'string' ? e.error : 'No se pudo importar.';
      }
    });
  }

  dinero(n: number): string {
    return `$ ${Number(n).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }
}
