import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, ConfiguracionTiendaDto, LugarEncontradoDto, VistaTienda } from '../servicios/api.service';
import { DialogoService } from '../servicios/dialogo.service';
import { IconoComponent } from '../icono.component';
import { MapaComponent, Ubicacion } from '../mapa.component';

const CENTRO_POR_DEFECTO: Ubicacion = { latitud: -34.603722, longitud: -58.381592 };

@Component({
  selector: 'sc-config-tienda',
  standalone: true,
  imports: [FormsModule, IconoComponent, MapaComponent],
  template: `
    <div class="topbar">
      <h1>Configuración de la tienda</h1>
    </div>
    <section class="panel config-form">
      <h2><sc-icono nombre="tienda" /> Tienda online</h2>
      <p class="hint">Catálogo público del local, sin login. Comprar llega en otra etapa.</p>

      <div class="row">
        <label class="field grow">Estado
          <select class="wide" [(ngModel)]="form.estado">
            <option value="inactiva">Apagada (el link no existe)</option>
            <option value="activa">Activa</option>
            <option value="pausada">Pausada (avisa que no está disponible)</option>
          </select>
        </label>
        <label class="field grow">Alias
          <input class="wide" type="text" maxlength="40" placeholder="almacen-don-pepe" [(ngModel)]="form.alias" />
        </label>
      </div>
      <p class="hint">Letras sin acento, números y guiones. Es único en todo el sistema.</p>
      @if (link()) {
        <div class="row">
          <a [href]="link()" target="_blank" rel="noopener">{{ link() }}</a>
          <button class="btn" type="button" (click)="copiar()">Copiar link</button>
        </div>
      }
      @if (vista === 'cerrada') {
        <p class="aviso-abono">Con el abono vencido o pendiente, la tienda muestra «Tienda temporalmente cerrada».</p>
      }

      <label class="field">Dirección <input class="wide" type="text" maxlength="200" [(ngModel)]="form.direccion" /></label>
      <label class="field">Horario <input class="wide" type="text" maxlength="200" placeholder="Lunes a sábado de 9 a 20" [(ngModel)]="form.horario" /></label>
      <label class="field">WhatsApp <input class="wide" type="tel" maxlength="30" placeholder="011 15 1234-5678" [(ngModel)]="form.whatsApp" /></label>
      <label class="check"><input type="checkbox" [(ngModel)]="form.haceEnvios" /> Hace envíos (si no, solo retiro en el local)</label>

      <h2>Ubicación</h2>
      <div class="row">
        <input class="grow" type="search" maxlength="200" placeholder="Calle, número y localidad" aria-label="Buscar dirección"
               [(ngModel)]="busqueda" (keydown.enter)="buscar()" />
        <button class="btn" type="button" [disabled]="buscando" (click)="buscar()">{{ buscando ? 'Buscando...' : 'Buscar' }}</button>
        @if (tieneUbicacion()) {
          <button class="btn" type="button" (click)="quitarUbicacion()">Quitar ubicación</button>
        } @else {
          <button class="btn" type="button" (click)="marcar(centroPorDefecto)">Marcar en el mapa</button>
        }
      </div>
      @if (lugares.length > 0) {
        <ul class="lista-lugares">
          @for (l of lugares; track $index) {
            <li><button class="btn" type="button" (click)="marcar(l)">{{ l.nombre }}</button></li>
          }
        </ul>
      }
      @if (tieneUbicacion()) {
        <sc-mapa [latitud]="form.latitud!" [longitud]="form.longitud!" [editable]="true" (movido)="marcar($event)" />
        <p class="hint">Mové el marcador o tocá el mapa para ajustar.</p>
      } @else {
        <p class="meta">Sin ubicación: la tienda muestra solo la dirección escrita.</p>
      }

      <p class="meta">Productos publicados: {{ publicados }}. Se eligen en Productos.</p>
      <div class="row" style="margin-top:16px">
        <button class="btn btn-primary" type="button" [disabled]="guardando" (click)="guardar()">Guardar tienda</button>
      </div>
      @if (error) { <p class="error">{{ error }}</p> }
      @if (mensaje) { <p class="meta">{{ mensaje }}</p> }
    </section>
  `,
  styles: `
    .lista-lugares { list-style: none; padding: 0; margin: 8px 0; display: grid; gap: 6px; }
    .lista-lugares .btn { text-align: left; width: 100%; }
  `
})
export class ConfigTiendaComponent implements OnInit {
  form: ConfiguracionTiendaDto = { alias: '', estado: 'inactiva', direccion: '', horario: '', whatsApp: '', haceEnvios: false, latitud: null, longitud: null };
  readonly centroPorDefecto = CENTRO_POR_DEFECTO;
  aliasGuardado = '';
  vista: VistaTienda | null = null;
  publicados = 0;
  busqueda = '';
  lugares: LugarEncontradoDto[] = [];
  buscando = false;
  guardando = false;
  error = '';
  mensaje = '';

  constructor(
    private readonly api: ApiService,
    private readonly dialogo: DialogoService
  ) {}

  ngOnInit(): void {
    this.api.configuracionTienda().subscribe({
      next: (r) => {
        const { vista, publicados, conImagen, ...form } = r;
        this.form = form;
        this.aliasGuardado = form.alias;
        this.vista = vista;
        this.publicados = publicados;
      },
      error: (e) => (this.error = this.mensajeError(e, 'No se pudo cargar la tienda.'))
    });
  }

  link(): string {
    return this.aliasGuardado ? `${location.origin}/tienda/${this.aliasGuardado}` : '';
  }

  tieneUbicacion(): boolean {
    return this.form.latitud !== null && this.form.longitud !== null;
  }

  copiar(): void {
    void navigator.clipboard?.writeText(this.link()).then(() => (this.mensaje = 'Link copiado.'));
  }

  buscar(): void {
    const q = this.busqueda.trim();
    if (q.length < 4) {
      this.error = 'Escribí la dirección con calle, número y localidad.';
      return;
    }
    this.buscando = true;
    this.api.buscarUbicacion(q).subscribe({
      next: (r) => {
        this.buscando = false;
        this.lugares = r;
        this.error = r.length === 0 ? 'No encontramos esa dirección. Probá con la localidad o marcá en el mapa.' : '';
      },
      error: (e) => {
        this.buscando = false;
        this.error = e.status === 429 ? 'Esperá un segundo entre búsquedas.' : this.mensajeError(e, 'No se pudo buscar la dirección.');
      }
    });
  }

  marcar(lugar: Ubicacion): void {
    this.form = { ...this.form, latitud: lugar.latitud, longitud: lugar.longitud };
    this.lugares = [];
  }

  quitarUbicacion(): void {
    this.form = { ...this.form, latitud: null, longitud: null };
  }

  async guardar(): Promise<void> {
    const alias = this.form.alias.trim().toLowerCase();
    if (this.aliasGuardado && alias !== this.aliasGuardado
      && !await this.dialogo.confirmar(`El link /tienda/${this.aliasGuardado} va a dejar de funcionar. ¿Cambiar el alias?`)) {
      return;
    }
    this.guardando = true;
    this.api.guardarConfiguracionTienda({ ...this.form, alias }).subscribe({
      next: (r) => {
        this.guardando = false;
        this.form = { ...this.form, alias: r.alias, estado: r.estado };
        this.aliasGuardado = r.alias;
        this.vista = r.vista;
        this.error = '';
        this.mensaje = 'Tienda guardada.';
      },
      error: (e) => {
        this.guardando = false;
        this.mensaje = '';
        this.error = this.mensajeError(e, 'No se pudo guardar la tienda.');
      }
    });
  }

  private mensajeError(error: { error?: unknown }, defecto: string): string {
    return typeof error.error === 'string' ? error.error : defecto;
  }
}
