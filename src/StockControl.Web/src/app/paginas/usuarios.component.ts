import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, LocalDto, UsuarioDto } from '../servicios/api.service';
import { SesionService } from '../servicios/sesion.service';

@Component({
  selector: 'sc-usuarios',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="topbar">
      <h1>Usuarios</h1>
      <button class="btn btn-primary" type="button" (click)="nuevo()">Nuevo usuario</button>
    </div>
    @if (error) { <p class="error">{{ error }}</p> }
    <section class="panel">
      @if (!cargando && usuarios.length === 0) {
        <p class="meta">No hay usuarios asignados a este local.</p>
      }
      <table class="data">
        <thead><tr><th>Nombre</th><th>Usuario</th><th>Rol</th><th>Locales</th><th>Estado</th><th></th></tr></thead>
        <tbody>
          @for (usuario of usuarios; track usuario.id) {
            <tr>
              <td>{{ usuario.nombre }}</td>
              <td>{{ usuario.nombreUsuario }}</td>
              <td>{{ etiquetaRol(usuario.rol) }}</td>
              <td>{{ nombresLocales(usuario) }}</td>
              <td>{{ usuario.activo ? 'Activo' : 'Inactivo' }}</td>
              <td class="num">
                @if (puedeEditar(usuario)) {
                  <button class="btn" type="button" (click)="editar(usuario)">Editar</button>
                  @if (usuario.activo) {
                    <button class="btn btn-danger" type="button" (click)="cambiarEstado(usuario, false)">Desactivar</button>
                  } @else {
                    <button class="btn" type="button" (click)="cambiarEstado(usuario, true)">Activar</button>
                  }
                  <button class="btn" type="button" (click)="resetearClave(usuario)">Reset clave</button>
                }
              </td>
            </tr>
          }
        </tbody>
      </table>
    </section>

    <div class="modal-back" [class.show]="formAbierto">
      <div class="modal">
        <h2 style="margin:0 0 8px;font-size:18px">{{ editando ? 'Editar usuario' : 'Nuevo usuario' }}</h2>
        <label class="field">Nombre <input class="wide" [(ngModel)]="form.nombre" /></label>
        <label class="field">Usuario <input class="wide" [(ngModel)]="form.nombreUsuario" [disabled]="!!editando" /></label>
        @if (!editando) {
          <label class="field">Clave inicial <input class="wide" type="password" [(ngModel)]="form.clave" /></label>
        }
        <label class="field">Rol
          <select class="wide" [(ngModel)]="form.rol">
            <option value="empleado">Empleado</option>
            @if (sesion.esDuenoTitular()) { <option value="socio">Socio</option> }
          </select>
        </label>
        <p class="meta">Locales asignados</p>
        @for (local of locales; track local.idLocal) {
          <label class="check">
            <input type="checkbox" [checked]="form.idsLocal.includes(local.idLocal)" (change)="alternarLocal(local.idLocal, $event)" />
            {{ local.nombre }}
          </label>
        }
        <div class="row" style="margin-top:12px">
          <button class="btn btn-primary grow" type="button" (click)="guardar()">Guardar</button>
          <button class="btn" type="button" (click)="formAbierto = false">Cancelar</button>
        </div>
      </div>
    </div>
  `
})
export class UsuariosComponent implements OnInit {
  usuarios: UsuarioDto[] = [];
  locales: LocalDto[] = [];
  formAbierto = false;
  cargando = false;
  error = '';
  editando: UsuarioDto | null = null;
  form = this.formulario();

  constructor(readonly api: ApiService, readonly sesion: SesionService) {}

  ngOnInit(): void {
    this.cargar();
    this.api.locales().subscribe({ next: (r) => (this.locales = r), error: () => (this.error = 'No se pudieron cargar los locales.') });
  }

  cargar(): void {
    this.cargando = true;
    this.api.usuarios().subscribe({
      next: (r) => { this.usuarios = r; this.cargando = false; },
      error: (e) => { this.error = this.mensaje(e, 'No se pudieron cargar los usuarios.'); this.cargando = false; }
    });
  }

  nuevo(): void {
    this.editando = null;
    this.form = this.formulario();
    this.form.idsLocal = this.sesion.idLocal() ? [this.sesion.idLocal()] : [];
    this.formAbierto = true;
  }

  editar(usuario: UsuarioDto): void {
    this.editando = usuario;
    this.form = {
      nombre: usuario.nombre,
      nombreUsuario: usuario.nombreUsuario,
      rol: usuario.rol === 'socio' && !this.sesion.esDuenoTitular() ? 'empleado' : usuario.rol,
      idsLocal: usuario.locales.map((l) => l.idLocal),
      clave: ''
    };
    this.formAbierto = true;
  }

  alternarLocal(idLocal: number, event: Event): void {
    const marcado = (event.target as HTMLInputElement).checked;
    this.form.idsLocal = marcado
      ? [...this.form.idsLocal, idLocal]
      : this.form.idsLocal.filter((id) => id !== idLocal);
  }

  guardar(): void {
    if (!this.form.nombre.trim() || !this.form.nombreUsuario.trim() || this.form.idsLocal.length === 0) {
      this.error = 'Nombre, usuario y al menos un local son obligatorios.';
      return;
    }
    const observable = this.editando
      ? this.api.editarUsuario(this.editando.id, { ...this.form, nombre: this.form.nombre.trim(), nombreUsuario: this.form.nombreUsuario.trim() })
      : this.api.crearUsuario({ ...this.form, nombre: this.form.nombre.trim(), nombreUsuario: this.form.nombreUsuario.trim(), clave: this.form.clave });
    observable.subscribe({
      next: () => { this.formAbierto = false; this.error = ''; this.cargar(); },
      error: (e) => (this.error = this.mensaje(e, 'No se pudo guardar el usuario.'))
    });
  }

  cambiarEstado(usuario: UsuarioDto, activo: boolean): void {
    if (!activo && !confirm(`¿Desactivar a ${usuario.nombre}?`)) return;
    this.api.cambiarEstadoUsuario(usuario.id, activo).subscribe({
      next: () => this.cargar(),
      error: (e) => (this.error = this.mensaje(e, 'No se pudo cambiar el estado.'))
    });
  }

  resetearClave(usuario: UsuarioDto): void {
    const clave = prompt(`Nueva clave para ${usuario.nombre}`);
    if (!clave) return;
    this.api.resetearClaveUsuario(usuario.id, clave).subscribe({
      next: () => (this.error = ''),
      error: (e) => (this.error = this.mensaje(e, 'No se pudo resetear la clave.'))
    });
  }

  nombresLocales(usuario: UsuarioDto): string {
    return usuario.locales.map((l) => l.nombre).join(', ');
  }

  etiquetaRol(rol: string): string {
    return rol === 'socio' ? 'Socio' : rol === 'dueno' ? 'Dueño' : 'Empleado';
  }

  puedeEditar(usuario: UsuarioDto): boolean {
    return this.sesion.esDuenoTitular() || usuario.rol === 'empleado';
  }

  private formulario() {
    return { nombre: '', nombreUsuario: '', rol: 'empleado', idsLocal: [] as number[], clave: '' };
  }

  private mensaje(error: { error?: unknown }, defecto: string): string {
    return typeof error.error === 'string' ? error.error : defecto;
  }
}
