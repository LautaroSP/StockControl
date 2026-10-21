import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService, ProveedorFormDto, ProveedorListaDto } from '../servicios/api.service';
import { DialogoService } from '../servicios/dialogo.service';

@Component({
  selector: 'sc-proveedores',
  standalone: true,
  imports: [FormsModule, RouterLink],
  template: `
    <div class="topbar">
      <h1>Proveedores</h1>
      <button class="btn btn-primary" type="button" (click)="nuevo()">Nuevo proveedor</button>
    </div>
    @if (error) {
      <p class="error">{{ error }}</p>
    }
    <section class="panel">
      <table class="data">
        <thead>
          <tr>
            <th>Nombre</th>
            <th>Tipo de productos</th>
            <th>WhatsApp</th>
            <th class="num">Productos</th>
            <th>Última orden</th>
            <th>Estado</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          @for (p of proveedores; track p.id) {
            <tr>
              <td><a [routerLink]="['/proveedores', p.id]">{{ p.nombre }}</a></td>
              <td>{{ p.tipoProductos }}</td>
              <td>{{ p.telefono }}</td>
              <td class="num">{{ p.cantidadProductos }}</td>
              <td>{{ p.ultimaOrden ? fecha(p.ultimaOrden) : '—' }}</td>
              <td>{{ p.activo ? 'Activo' : 'Inactivo' }}</td>
              <td class="num">
                @if (p.activo) {
                  <button class="btn" type="button" (click)="editar(p)">Editar</button>
                  <button class="btn btn-danger" type="button" (click)="eliminar(p)">Eliminar</button>
                } @else {
                  <button class="btn" type="button" (click)="activar(p)">Activar</button>
                }
              </td>
            </tr>
          } @empty {
            <tr><td colspan="7" class="meta">Todavía no cargaste proveedores.</td></tr>
          }
        </tbody>
      </table>
    </section>

    <div class="modal-back" [class.show]="formAbierto">
      <div class="modal">
        <h2 style="margin:0 0 8px;font-size:18px">{{ editando ? 'Editar proveedor' : 'Nuevo proveedor' }}</h2>
        <label class="field">Nombre <input class="wide" type="text" maxlength="200" [(ngModel)]="form.nombre" /></label>
        <label class="field">Tipo de productos <input class="wide" type="text" maxlength="200" [(ngModel)]="form.tipoProductos" /></label>
        <label class="field">
          Teléfono WhatsApp
          <input class="wide" type="tel" maxlength="30" [(ngModel)]="form.telefono" placeholder="11 1234-5678" />
        </label>
        <p class="meta" style="margin:4px 0 0">Con código de área, sin 0 ni 15. Se agrega el 54 solo.</p>
        @if (errorForm) {
          <p class="error">{{ errorForm }}</p>
        }
        <div class="row" style="margin-top:12px">
          <button class="btn btn-primary grow" type="button" (click)="guardar()">Guardar</button>
          <button class="btn" type="button" (click)="formAbierto = false">Cancelar</button>
        </div>
      </div>
    </div>
  `
})
export class ProveedoresComponent implements OnInit {
  proveedores: ProveedorListaDto[] = [];
  form: ProveedorFormDto = { nombre: '', tipoProductos: '', telefono: '' };
  editando: ProveedorListaDto | null = null;
  formAbierto = false;
  error = '';
  errorForm = '';

  constructor(private readonly api: ApiService, private readonly dialogo: DialogoService) {}

  ngOnInit(): void {
    this.cargar();
  }

  cargar(): void {
    this.api.proveedores().subscribe({
      next: (r) => (this.proveedores = r),
      error: () => (this.error = 'No se pudieron cargar los proveedores.')
    });
  }

  nuevo(): void {
    this.editando = null;
    this.form = { nombre: '', tipoProductos: '', telefono: '' };
    this.errorForm = '';
    this.formAbierto = true;
  }

  editar(p: ProveedorListaDto): void {
    this.editando = p;
    this.form = { nombre: p.nombre, tipoProductos: p.tipoProductos, telefono: p.telefono };
    this.errorForm = '';
    this.formAbierto = true;
  }

  guardar(): void {
    if (!this.form.nombre.trim()) {
      this.errorForm = 'El nombre es obligatorio.';
      return;
    }
    const pedido = this.editando
      ? this.api.editarProveedor(this.editando.id, this.form)
      : this.api.crearProveedor(this.form);
    pedido.subscribe({
      next: () => {
        this.formAbierto = false;
        this.cargar();
      },
      error: (e) => (this.errorForm = this.mensaje(e))
    });
  }

  async eliminar(p: ProveedorListaDto): Promise<void> {
    if (!await this.dialogo.confirmar(`¿Eliminar ${p.nombre}? Si tiene órdenes, queda inactivo.`)) return;
    this.api.eliminarProveedor(p.id).subscribe({
      next: () => this.cargar(),
      error: (e) => (this.error = this.mensaje(e))
    });
  }

  activar(p: ProveedorListaDto): void {
    this.api.activarProveedor(p.id).subscribe({
      next: () => this.cargar(),
      error: (e) => (this.error = this.mensaje(e))
    });
  }

  fecha(iso: string): string {
    return new Date(iso).toLocaleDateString('es-AR');
  }

  private mensaje(e: { error?: unknown }): string {
    return typeof e.error === 'string' ? e.error : 'No se pudo guardar el proveedor.';
  }
}
