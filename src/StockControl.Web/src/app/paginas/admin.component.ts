import { DatePipe, NgTemplateOutlet } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ApiService, LocalAdminDto, LocalPlataformaDto, MensajeContactoDto, UsuarioAdminDto, UsuarioCreadoDto } from '../servicios/api.service';
import { DialogoService } from '../servicios/dialogo.service';
import { SesionService } from '../servicios/sesion.service';
import { TemaComponent } from '../tema.component';

@Component({
  selector: 'sc-admin',
  standalone: true,
  imports: [DatePipe, FormsModule, NgTemplateOutlet, RouterLink, TemaComponent],
  template: `
    <sc-tema />
    <div class="auth" style="align-items:start;padding-top:72px">
      <div style="width:min(880px,100%)">
        <div class="topbar">
          <div>
            <h1>Panel de admin</h1>
            <p class="meta">Dueños registrados. Un clic muestra sus locales.</p>
          </div>
          <div class="row">
            <a class="btn" routerLink="/locales">Mis locales</a>
            <button class="btn" type="button" (click)="salir()">Salir</button>
          </div>
        </div>

        <section class="panel pad" style="margin-bottom:16px">
          <h2 style="margin:0 0 8px;font-size:16px">Alta de local</h2>
          <p class="meta">El primer local del dueño tiene 7 días de prueba. Si ya tiene uno, el siguiente espera el pago.</p>
          <div class="row">
            <label class="check">
              <input type="checkbox" [(ngModel)]="crearDueno" />
              Crear dueño en este paso
            </label>
            @if (!crearDueno) {
              <label class="field" style="margin:0;min-width:220px">
                Dueño
                <select [(ngModel)]="idDueno" (ngModelChange)="idLocalOrigen = 0">
                  <option [ngValue]="0">Elegí un dueño</option>
                  @for (u of usuarios; track u.id) {
                    @if (u.rol === 'dueno') {
                      <option [ngValue]="u.id">{{ u.nombre }} ({{ u.nombreUsuario }})</option>
                    }
                  }
                </select>
              </label>
            }
            <label class="field grow" style="margin:0">
              Nombre del local
              <input class="wide" type="text" [(ngModel)]="nombreLocal" />
            </label>
            @if (!crearDueno && localesDelDueno.length) {
              <label class="field" style="margin:0;min-width:220px">
                Copiar productos de
                <select [(ngModel)]="idLocalOrigen">
                  <option [ngValue]="0">No copiar</option>
                  @for (l of localesDelDueno; track l.idLocal) {
                    <option [ngValue]="l.idLocal">{{ l.nombre }}</option>
                  }
                </select>
              </label>
            }
            <button class="btn btn-primary" type="button" (click)="crearLocal()" [disabled]="ocupado">Dar de alta</button>
          </div>
          @if (crearDueno) {
            <div class="row" style="margin-top:8px">
              <label class="field grow" style="margin:0">Nombre <input class="wide" type="text" [(ngModel)]="nombreDueno" /></label>
              <label class="field grow" style="margin:0">Mail <input class="wide" type="email" [(ngModel)]="emailDueno" /></label>
              <label class="field grow" style="margin:0">Usuario <input class="wide" type="text" [(ngModel)]="usuarioDueno" autocomplete="off" /></label>
              <label class="field grow" style="margin:0">Clave <input class="wide" type="password" [(ngModel)]="claveDueno" autocomplete="new-password" /></label>
            </div>
          }
        </section>

        @if (error) {
          <p class="error">{{ error }}</p>
        }
        @if (aviso) {
          <p class="meta">{{ aviso }}</p>
        }

        <section class="panel pad" style="margin-bottom:16px">
          <h2 style="margin:0 0 8px;font-size:16px">Este mes</h2>
          <p class="meta">{{ localesPagos }} {{ localesPagos === 1 ? 'local pago' : 'locales pagos' }} × $ {{ pesos(precioMensual) }} = $ {{ pesos(totalMes) }}. La prueba no cuenta.</p>
          <div class="row" style="margin-top:12px">
            <label class="field grow" style="margin:0">
              Teléfono de ventas
              <input class="wide" type="tel" [(ngModel)]="telefono" />
            </label>
            <label class="field grow" style="margin:0">
              Mail de contacto
              <input class="wide" type="email" [(ngModel)]="emailContacto" />
            </label>
            <label class="field" style="margin:0;min-width:180px">
              Clave maestra
              <input class="wide" type="password" [(ngModel)]="claveTelefono" autocomplete="off" />
            </label>
            <button class="btn" type="button" (click)="guardarTelefono()" [disabled]="ocupado">Guardar contacto</button>
          </div>
        </section>

        <div class="row" style="margin:0 0 8px">
          <button class="btn" type="button" [class.btn-primary]="vista === 'usuarios'" (click)="vista = 'usuarios'">Usuarios</button>
          <button class="btn" type="button" [class.btn-primary]="vista === 'locales'" (click)="vista = 'locales'">Locales</button>
          <button class="btn" type="button" [class.btn-primary]="vista === 'contacto'" (click)="verContacto()">Contacto</button>
        </div>

        @if (vista === 'contacto') {
          <section class="panel pad" style="margin-bottom:16px">
            <div class="row">
              <strong>Formularios de contacto</strong>
              <button class="btn" type="button" (click)="alternarFormulario()" [disabled]="ocupado">
                {{ formularioActivo ? 'Desactivar formulario' : 'Activar formulario' }}
              </button>
            </div>
            <p class="meta">
              {{ formularioActivo ? 'Quien escribe en Contacto queda acá.' : 'El formulario está apagado. El teléfono y el mail siguen visibles.' }}
              Los leídos se borran a los 3 días.
            </p>
            @if (mensajes.length === 0) {
              <p class="meta">No hay mensajes.</p>
            }
            @for (m of mensajes; track m.id) {
              <article class="panel" style="margin-top:10px">
                <div class="pad">
                  <div class="row">
                    <strong>{{ m.nombre }}</strong>
                    <span class="meta">{{ m.correo }} · {{ m.creado | date: 'dd/MM/yyyy HH:mm' }}</span>
                    @if (!m.leido) {
                      <button class="btn" type="button" (click)="marcarLeido(m)" [disabled]="ocupado">Leído</button>
                    } @else {
                      <span class="meta">Leído · se borra {{ m.vence | date: 'dd/MM/yyyy HH:mm' }}</span>
                    }
                  </div>
                  <p style="white-space:pre-wrap;margin:8px 0 0">{{ m.mensaje }}</p>
                </div>
              </article>
            }
          </section>
        }

        @if (vista === 'locales') {
        @for (l of locales; track l.idLocal) {
          <article class="panel" style="margin-bottom:10px">
            <div class="pad">
              <div class="row">
                <span>
                  {{ l.cliente || 'Sin dueño' }} · {{ l.nombre }}
                  · {{ l.empleados }} {{ l.empleados === 1 ? 'empleado' : 'empleados' }}
                  · {{ l.estado }}
                  · vence {{ l.vence ? (l.vence | date: 'dd/MM/yyyy HH:mm') : '—' }}
                </span>
                @if (!l.activo) {
                  <span class="badge badge-vencido">Desactivado</span>
                }
              </div>
              <div class="row" style="margin-top:8px">
                <button class="btn" type="button" (click)="abrirAccion('pago', l.idLocal)">Marcar pago</button>
                <button class="btn" type="button" (click)="abrirAccion('anular', l.idLocal)">Anular pago</button>
                <button class="btn" type="button" (click)="abrirVence(l)">Vencimiento</button>
                @if (l.activo) {
                  <button class="btn btn-danger" type="button" (click)="abrirAccion('baja', l.idLocal)">Desactivar</button>
                } @else {
                  <button class="btn" type="button" (click)="abrirAccion('activar', l.idLocal)">Activar</button>
                }
              </div>
              <ng-container *ngTemplateOutlet="maestra; context: { tipo: 'pago', id: l.idLocal }" />
              <ng-container *ngTemplateOutlet="maestra; context: { tipo: 'anular', id: l.idLocal }" />
              <ng-container *ngTemplateOutlet="maestra; context: { tipo: 'vence', id: l.idLocal }" />
              <ng-container *ngTemplateOutlet="maestra; context: { tipo: 'baja', id: l.idLocal }" />
              <ng-container *ngTemplateOutlet="maestra; context: { tipo: 'activar', id: l.idLocal }" />
            </div>
          </article>
        }
        }

        @if (vista === 'usuarios') {
        @for (u of usuarios; track u.id) {
          <article class="panel" style="margin-bottom:10px">
            <button class="btn" type="button" style="width:100%;text-align:left" (click)="alternar(u.id)">
              {{ u.nombre }}
              <span class="meta">{{ u.nombreUsuario }} · {{ u.rol }}</span>
              @if (!u.activo) {
                <span class="badge badge-vencido">Bloqueado</span>
              }
            </button>
            @if (abierto === u.id) {
              <div class="pad">
                <h2 style="margin:0 0 8px;font-size:15px">Locales</h2>
                @if (!u.locales.length) {
                  <p class="meta">Sin locales.</p>
                } @else {
                  <div style="margin-bottom:12px">
                    @for (l of u.locales; track l.idLocal) {
                      <div class="row" style="margin-bottom:8px">
                        <span>{{ l.nombre }} · {{ l.estado }} · vence {{ l.vence ? (l.vence | date: 'dd/MM/yyyy HH:mm') : '—' }}</span>
                        <button class="btn" type="button" (click)="abrirAccion('pago', l.idLocal)">Marcar pago</button>
                        <button class="btn" type="button" (click)="abrirAccion('anular', l.idLocal)">Anular pago</button>
                        <ng-container *ngTemplateOutlet="maestra; context: { tipo: 'pago', id: l.idLocal }" />
                        <ng-container *ngTemplateOutlet="maestra; context: { tipo: 'anular', id: l.idLocal }" />
                      </div>
                    }
                  </div>
                }
                <div class="row">
                  <button class="btn" type="button" (click)="verCreados(u)">Ver usuarios</button>
                  @if (u.activo) {
                    <button class="btn" type="button" (click)="abrirAccion('bloquear', u.id)">Bloquear</button>
                  }
                  @if (u.rol !== 'admin') {
                    <button class="btn" type="button" (click)="pideClave = pideClave === u.id ? 0 : u.id">Hacer admin</button>
                  }
                  <button class="btn" type="button" (click)="abrirClave(u.id)">Cambiar clave</button>
                  <button class="btn btn-danger" type="button" (click)="abrirAccion('eliminar', u.id)">Eliminar</button>
                </div>
                <ng-container *ngTemplateOutlet="maestra; context: { tipo: 'bloquear', id: u.id }" />
                <ng-container *ngTemplateOutlet="maestra; context: { tipo: 'eliminar', id: u.id }" />
                @if (cambiaClave === u.id) {
                  <label class="field">
                    Nueva clave
                    <input class="wide" type="password" [(ngModel)]="nuevaClave" autocomplete="new-password" />
                  </label>
                  <label class="field">
                    Clave maestra
                    <input class="wide" type="password" [(ngModel)]="claveMaestra" autocomplete="off" />
                  </label>
                  <button class="btn btn-primary" type="button" (click)="guardarClave(u.id)" [disabled]="ocupado">Guardar clave</button>
                }
                @if (pideClave === u.id) {
                  <label class="field">
                    Clave maestra
                    <input class="wide" type="password" [(ngModel)]="clave" autocomplete="off" />
                  </label>
                  <button class="btn btn-primary" type="button" (click)="hacerAdmin(u)" [disabled]="ocupado">Confirmar admin</button>
                }
                @if (creadosDe === u.id) {
                  <h2 style="margin:14px 0 8px;font-size:15px">Usuarios de este dueño</h2>
                  @if (!creados.length) {
                    <p class="meta">No creó empleados ni socios.</p>
                  }
                  @for (c of creados; track c.id) {
                    <div class="row" style="margin-bottom:8px">
                      <span>{{ c.nombre }} · {{ c.nombreUsuario }} · {{ c.rol }}</span>
                      @if (!c.activo) {
                        <span class="badge badge-vencido">Bloqueado</span>
                      } @else {
                        <button class="btn" type="button" (click)="abrirAccion('bloquear', c.id)">Bloquear</button>
                      }
                      <button class="btn" type="button" (click)="abrirClave(c.id)">Cambiar clave</button>
                      <button class="btn btn-danger" type="button" (click)="abrirAccion('eliminar', c.id)">Eliminar</button>
                    </div>
                    <ng-container *ngTemplateOutlet="maestra; context: { tipo: 'bloquear', id: c.id }" />
                    <ng-container *ngTemplateOutlet="maestra; context: { tipo: 'eliminar', id: c.id }" />
                    @if (cambiaClave === c.id) {
                      <label class="field">
                        Nueva clave
                        <input class="wide" type="password" [(ngModel)]="nuevaClave" autocomplete="new-password" />
                      </label>
                      <label class="field">
                        Clave maestra
                        <input class="wide" type="password" [(ngModel)]="claveMaestra" autocomplete="off" />
                      </label>
                      <button class="btn btn-primary" type="button" (click)="guardarClave(c.id)" [disabled]="ocupado">Guardar clave</button>
                    }
                  }
                }
              </div>
            }
          </article>
        }
        }
      </div>
    </div>
    <ng-template #maestra let-tipo="tipo" let-id="id">
      @if (accion === tipo && accionId === id) {
        @if (tipo === 'pago') {
          <label class="field">
            Cómo pagó
            <select [(ngModel)]="canal">
              <option value="transferencia">Transferencia</option>
              <option value="efectivo">Efectivo</option>
              <option value="ventas">Hablé con ventas</option>
            </select>
          </label>
        }
        @if (tipo === 'vence') {
          <label class="field">
            Vencimiento
            <input class="wide" type="datetime-local" [(ngModel)]="vence" />
          </label>
        }
        <label class="field">
          Clave maestra
          <input class="wide" type="password" [(ngModel)]="claveAccion" autocomplete="off" />
        </label>
        <button class="btn btn-primary" type="button" (click)="confirmarAccion()" [disabled]="ocupado">Confirmar</button>
      }
    </ng-template>
  `
})
export class AdminComponent implements OnInit {
  usuarios: UsuarioAdminDto[] = [];
  locales: LocalPlataformaDto[] = [];
  vista: 'usuarios' | 'locales' | 'contacto' = 'usuarios';
  mensajes: MensajeContactoDto[] = [];
  formularioActivo = true;
  totalMes = 0;
  precioMensual = 14999;
  localesPagos = 0;
  telefono = '';
  emailContacto = '';
  claveTelefono = '';
  crearDueno = false;
  nombreDueno = '';
  emailDueno = '';
  usuarioDueno = '';
  claveDueno = '';
  canal = 'transferencia';
  vence = '';
  creados: UsuarioCreadoDto[] = [];
  abierto = 0;
  creadosDe = 0;
  pideClave = 0;
  cambiaClave = 0;
  accion = '';
  accionId = 0;
  idDueno = 0;
  idLocalOrigen = 0;
  nombreLocal = '';
  clave = '';
  nuevaClave = '';
  claveMaestra = '';
  claveAccion = '';
  error = '';
  aviso = '';
  ocupado = false;

  constructor(
    private readonly api: ApiService,
    private readonly sesion: SesionService,
    private readonly router: Router,
    private readonly dialogo: DialogoService
  ) {}

  ngOnInit(): void {
    this.cargar();
  }

  alternar(id: number): void {
    this.abierto = this.abierto === id ? 0 : id;
    this.creadosDe = 0;
    this.pideClave = 0;
    this.cambiaClave = 0;
    this.accion = '';
    this.clave = '';
    this.nuevaClave = '';
    this.claveMaestra = '';
    this.claveAccion = '';
    this.error = '';
    this.aviso = '';
  }

  abrirAccion(tipo: string, id: number): void {
    if (this.accion === tipo && this.accionId === id) {
      this.accion = '';
      this.claveAccion = '';
      return;
    }
    this.accion = tipo;
    this.accionId = id;
    this.claveAccion = '';
    this.cambiaClave = 0;
    this.pideClave = 0;
    this.error = '';
    this.aviso = '';
  }

  confirmarAccion(): void {
    const tipo = this.accion;
    const id = this.accionId;
    const clave = this.claveAccion;
    this.ocupado = true;
    const pedido = tipo === 'pago' ? this.api.marcarPagoAdmin(id, clave, this.canal)
      : tipo === 'anular' ? this.api.anularPagoAdmin(id, clave)
      : tipo === 'vence' ? this.api.cambiarVencimientoAdmin(id, this.vence, clave)
      : tipo === 'baja' ? this.api.desactivarLocal(id, clave)
      : tipo === 'activar' ? this.api.activarLocal(id, clave)
      : tipo === 'bloquear' ? this.api.bloquearUsuario(id, clave)
      : this.api.eliminarUsuario(id, clave);
    pedido.subscribe({
      next: () => {
        this.ocupado = false;
        this.accion = '';
        this.claveAccion = '';
        this.error = '';
        if (tipo === 'eliminar' && id === this.abierto) {
          this.abierto = 0;
          this.creadosDe = 0;
        }
        this.cargar();
        if (this.creadosDe) this.recargarCreados();
      },
      error: (err) => this.fallo(err)
    });
  }

  abrirClave(id: number): void {
    this.cambiaClave = this.cambiaClave === id ? 0 : id;
    this.accion = '';
    this.pideClave = 0;
    this.nuevaClave = '';
    this.claveMaestra = '';
    this.clave = '';
    this.error = '';
    this.aviso = '';
  }

  guardarClave(id: number): void {
    this.ocupado = true;
    this.api.cambiarClaveAdmin(id, this.nuevaClave, this.claveMaestra).subscribe({
      next: () => {
        this.nuevaClave = '';
        this.claveMaestra = '';
        this.cambiaClave = 0;
        this.ocupado = false;
        this.error = '';
        this.aviso = 'Clave actualizada.';
      },
      error: (err) => this.fallo(err)
    });
  }

  get localesDelDueno(): LocalAdminDto[] {
    return this.usuarios.find((u) => u.id === this.idDueno)?.locales ?? [];
  }

  async crearLocal(): Promise<void> {
    if (!this.nombreLocal.trim() || (!this.crearDueno && !this.idDueno)) {
      this.error = this.crearDueno ? 'Completá el nombre del local y los datos del dueño.' : 'Elegí un dueño y el nombre del local.';
      return;
    }
    const origen = this.crearDueno ? 0 : this.idLocalOrigen;
    let copiarStock = false;
    if (origen > 0) {
      const nombre = this.localesDelDueno.find((l) => l.idLocal === origen)?.nombre ?? 'el otro';
      copiarStock = await this.dialogo.confirmar(
        `¿Desea poner el stock igual que en el local ${nombre}? Si no, se pondrá en 0.`,
        'Sí, igual',
        'No, en 0'
      );
    }
    this.ocupado = true;
    this.api.altaLocalConDueno({
      nombre: this.nombreLocal.trim(),
      idDueno: this.crearDueno ? 0 : this.idDueno,
      nombreDueno: this.nombreDueno.trim(),
      email: this.emailDueno.trim(),
      nombreUsuario: this.usuarioDueno.trim(),
      claveDueno: this.claveDueno,
      idLocalOrigen: origen,
      copiarStock
    }).subscribe({
      next: (r) => {
        this.nombreLocal = '';
        this.nombreDueno = '';
        this.emailDueno = '';
        this.usuarioDueno = '';
        this.claveDueno = '';
        this.idLocalOrigen = 0;
        this.ocupado = false;
        this.error = '';
        this.aviso = r.cantidadProductos > 0
          ? `Local dado de alta. Se copiaron ${r.cantidadProductos} productos.`
          : 'Local dado de alta.';
        this.cargar();
      },
      error: (err) => this.fallo(err)
    });
  }

  abrirVence(local: LocalPlataformaDto): void {
    this.abrirAccion('vence', local.idLocal);
    this.vence = this.fechaLocal(local.vence);
  }

  guardarTelefono(): void {
    this.ocupado = true;
    this.api.guardarTelefonoVentas(this.telefono, this.emailContacto, this.claveTelefono).subscribe({
      next: (r) => {
        this.telefono = r.telefono;
        this.emailContacto = r.email;
        this.claveTelefono = '';
        this.ocupado = false;
        this.error = '';
        this.aviso = 'Contacto de ventas actualizado.';
      },
      error: (err) => this.fallo(err)
    });
  }

  verContacto(): void {
    this.vista = 'contacto';
    this.cargarMensajes();
  }

  marcarLeido(mensaje: MensajeContactoDto): void {
    this.ocupado = true;
    this.api.marcarContactoLeido(mensaje.id).subscribe({
      next: (r) => {
        mensaje.leido = r.leido;
        mensaje.vence = r.vence;
        this.ocupado = false;
        this.error = '';
      },
      error: (err) => this.fallo(err)
    });
  }

  async alternarFormulario(): Promise<void> {
    const clave = await this.dialogo.pedir('Clave maestra para cambiar el formulario', '', true);
    if (!clave) return;
    this.ocupado = true;
    this.api.guardarFormularioContacto(!this.formularioActivo, clave).subscribe({
      next: (r) => {
        this.formularioActivo = r.activo;
        this.ocupado = false;
        this.error = '';
        this.aviso = r.activo ? 'Formulario de contacto activado.' : 'Formulario de contacto desactivado.';
      },
      error: (err) => this.fallo(err)
    });
  }

  pesos(n: number): string {
    return Math.trunc(n).toLocaleString('es-AR');
  }

  verCreados(usuario: UsuarioAdminDto): void {
    if (this.creadosDe === usuario.id) {
      this.creadosDe = 0;
      return;
    }
    this.ocupado = true;
    this.api.usuariosDelDueno(usuario.id).subscribe({
      next: (lista) => {
        this.creados = lista;
        this.creadosDe = usuario.id;
        this.ocupado = false;
      },
      error: (err) => this.fallo(err)
    });
  }

  hacerAdmin(usuario: UsuarioAdminDto): void {
    this.ocupado = true;
    this.api.hacerAdmin(usuario.id, this.clave).subscribe({
      next: () => {
        this.clave = '';
        this.pideClave = 0;
        this.ocupado = false;
        this.cargar();
      },
      error: (err) => this.fallo(err)
    });
  }

  async salir(): Promise<void> {
    if (!await this.dialogo.confirmar('¿Cerrar la sesión?')) return;
    this.sesion.salir();
    void this.router.navigateByUrl('/login');
  }

  private cargarMensajes(): void {
    this.api.mensajesContacto().subscribe({
      next: (r) => {
        this.mensajes = r.items;
        this.formularioActivo = r.activo;
        this.error = '';
      },
      error: () => (this.error = 'No se pudieron cargar los mensajes.')
    });
  }

  private cargar(): void {
    this.api.usuariosAdmin().subscribe({
      next: (lista) => {
        this.usuarios = lista;
        this.error = '';
      },
      error: () => (this.error = 'No se pudo cargar el panel.')
    });
    this.api.localesAdmin().subscribe({
      next: (r) => {
        this.locales = r.items;
        this.totalMes = r.totalMes;
        this.precioMensual = r.precioMensual;
        this.localesPagos = r.localesPagos;
        this.telefono = r.telefono;
        this.emailContacto = r.email;
      },
      error: () => (this.error = 'No se pudo cargar el listado de locales.')
    });
  }

  private fechaLocal(iso: string | null): string {
    if (!iso) return '';
    const d = new Date(iso);
    if (Number.isNaN(d.getTime())) return '';
    const p = (n: number) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}T${p(d.getHours())}:${p(d.getMinutes())}`;
  }

  private recargarCreados(): void {
    this.api.usuariosDelDueno(this.creadosDe).subscribe({
      next: (lista) => (this.creados = lista),
      error: (err) => this.fallo(err)
    });
  }

  private fallo(err: HttpErrorResponse): void {
    this.ocupado = false;
    this.error = typeof err.error === 'string' && err.error ? err.error : 'No se pudo completar.';
  }
}
