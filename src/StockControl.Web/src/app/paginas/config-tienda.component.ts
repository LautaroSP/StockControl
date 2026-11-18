import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, ConfiguracionTiendaDto, ImagenTiendaDto, LugarEncontradoDto, ProductoTiendaDto, VistaTienda } from '../servicios/api.service';
import { DialogoService } from '../servicios/dialogo.service';
import { IconoComponent } from '../icono.component';
import { MapaComponent, Ubicacion } from '../mapa.component';
import { BOTON_STOCK, FICHA_STOCK, FONDO_STOCK, hexDe, textoClaro } from '../servicios/paleta';

const CENTRO_POR_DEFECTO: Ubicacion = { latitud: -34.603722, longitud: -58.381592 };
const LADO_LOGO = 200;
const MAXIMO_BYTES_LOGO = 20 * 1024 * 1024;

async function achicarLogo(archivo: File): Promise<string> {
  const imagen = await createImageBitmap(archivo);
  const escala = Math.min(1, LADO_LOGO / imagen.width, LADO_LOGO / imagen.height);
  const lienzo = document.createElement('canvas');
  lienzo.width = Math.max(1, Math.round(imagen.width * escala));
  lienzo.height = Math.max(1, Math.round(imagen.height * escala));
  const contexto = lienzo.getContext('2d')!;
  contexto.drawImage(imagen, 0, 0, lienzo.width, lienzo.height);
  imagen.close();
  return lienzo.toDataURL('image/png');
}

const PLAZOS_TRANSFERENCIA: { horas: number | null; texto: string }[] = [
  { horas: 2, texto: '2 horas' },
  { horas: 6, texto: '6 horas' },
  { horas: 12, texto: '12 horas' },
  { horas: 24, texto: '24 horas' },
  { horas: 48, texto: '48 horas' },
  { horas: 72, texto: '72 horas' },
  { horas: null, texto: 'Sin vencimiento' }
];

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
      <p class="hint">Catálogo público del local, sin login.</p>

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
      @if (form.haceEnvios) {
        <label class="check"><input type="checkbox" [(ngModel)]="form.envioGratisSiempre" /> Envío gratis siempre</label>
        <p class="hint">Dentro del radio el comprador no paga envío. Fuera del radio, solo puede retirar. Sin pin en el mapa la tienda solo ofrece retiro.</p>
        @if (!form.envioGratisSiempre) {
          <div class="row">
            <label class="field grow">Precio por km
              <input class="wide" type="number" min="0" step="0.01" [(ngModel)]="form.precioEnvioKm" />
            </label>
            <label class="field grow">Envío mínimo
              <input class="wide" type="number" min="0" step="0.01" placeholder="Opcional" [(ngModel)]="form.envioMinimo" />
            </label>
          </div>
          <label class="field">Envío gratis desde
            <input class="wide" type="number" min="0" step="0.01" placeholder="Opcional, subtotal de productos" [(ngModel)]="form.envioGratisDesde" />
          </label>
        }
        <label class="field">Radio máximo (km)
          <input class="wide" type="number" min="0" max="500" step="0.01" placeholder="Opcional, hasta 500" [(ngModel)]="form.radioEnvioKm" />
        </label>
      }

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

      <h2>Logo</h2>
      <p class="hint">Se achica para entrar en 200 × 200, sin deformarlo. Queda centrado en la ficha de dirección, arriba del texto y del botón.</p>
      @if (logo && aliasGuardado) {
        <img class="logo-previa" [src]="urlLogo()" alt="Logo de la tienda" />
      }
      <div class="row">
        <label class="btn">
          {{ subiendoLogo ? 'Subiendo…' : 'Subir logo' }}
          <input type="file" accept="image/jpeg,image/png,image/webp" hidden (change)="subirLogo($event)" [disabled]="subiendoLogo" />
        </label>
        @if (logo) {
          <button class="btn" type="button" (click)="quitarLogo()" [disabled]="subiendoLogo">Quitar logo</button>
        }
      </div>

      <h2>Personalizar Tienda Online</h2>
      <p class="hint paleta-ayuda">Vacío es la paleta de StockControl. El fondo es la página, la ficha es la carta del producto y el texto de esa carta se lee sobre ese color.</p>
      <div class="paleta">
        <div class="paleta-controles">
          <div class="paleta-fila">
            <label class="paleta-texto">Fondo
              <input type="text" maxlength="7" placeholder="#f4efe4" [(ngModel)]="form.colorFondo" />
            </label>
            <input class="muestra-color" type="color" aria-label="Elegir fondo" [value]="hex(form.colorFondo) || fondoStock" (input)="elegirFondo($event)" />
          </div>
          <div class="paleta-fila">
            <label class="paleta-texto">Ficha
              <input type="text" maxlength="7" placeholder="#fffdf8" [(ngModel)]="form.colorFicha" />
            </label>
            <input class="muestra-color" type="color" aria-label="Elegir ficha" [value]="hex(form.colorFicha) || fichaStock" (input)="elegirFicha($event)" />
          </div>
          <div class="paleta-fila">
            <label class="paleta-texto">Botones
              <input type="text" maxlength="7" placeholder="#2a6b52" [(ngModel)]="form.colorMarca" />
            </label>
            <input class="muestra-color" type="color" aria-label="Elegir botones" [value]="hex(form.colorMarca) || botonStock" (input)="elegirBoton($event)" />
          </div>
        </div>
        <div class="vista-paleta" [style.background]="hex(form.colorFondo) || fondoStock" [style.color]="textoDeFondo()">
          <strong>Tu tienda</strong>
          <div class="vista-ficha" [style.background]="hex(form.colorFicha) || fichaStock" [style.color]="textoDeFicha()">
            @if (fotoMuestra(); as foto) {
              <img class="vista-foto" [src]="urlFoto(foto.id)" [alt]="muestra!.nombre" />
              @if (foto.origen === 'openfoodfacts') {
                <span class="vista-atribucion">Foto: {{ foto.fuente }} · {{ foto.licencia }}</span>
              }
            } @else {
              <span class="vista-foto vista-foto-vacia">Sin imagen</span>
            }
            <span class="vista-nombre">{{ muestra?.nombre ?? 'Producto' }}</span>
            <span class="vista-precio" [style.color]="hex(form.colorMarca) || botonStock">{{ precioTienda(muestra?.precio ?? 1000) }}</span>
            <span [class]="muestra && !muestra.hayStock ? 'vista-stock vista-stock-no' : 'vista-stock'">{{ muestra && !muestra.hayStock ? 'Sin stock' : 'Hay stock' }}</span>
            <span class="row tienda-cantidad">
              <button class="btn vista-cantidad" type="button" [style.background]="hex(form.colorMarca) || botonStock" [style.border-color]="hex(form.colorMarca) || botonStock">−</button>
              <span>1</span>
              <button class="btn vista-cantidad" type="button" [style.background]="hex(form.colorMarca) || botonStock" [style.border-color]="hex(form.colorMarca) || botonStock">+</button>
            </span>
          </div>
          <p class="vista-pie">Dirección · horario</p>
        </div>
      </div>
      <button class="btn" type="button" (click)="limpiarColores()">Paleta de StockControl</button>

      <h2>Pago con Mercado Pago</h2>
      @if (mercadoPago) {
        <p class="hint">El comprador puede pagar con la cuenta de Mercado Pago del local. La plata entra en esa cuenta.</p>
      } @else {
        <p class="meta">Conectá Mercado Pago en Configuración del local para cobrar los pedidos online.</p>
      }
      <h2>Pago por transferencia</h2>
      <p class="hint">El comprador ve estos datos en su pedido, transfiere y vos confirmás en Pedidos cuando la plata entra. Sin CBU o alias y titular, la tienda no ofrece transferencia.</p>
      <div class="row">
        <label class="field grow">CBU o CVU
          <input class="wide" type="text" inputmode="numeric" maxlength="30" placeholder="22 números" [(ngModel)]="form.cbu" />
        </label>
        <label class="field grow">Alias
          <input class="wide" type="text" maxlength="20" placeholder="CASA.PERRO.MESA" [(ngModel)]="form.aliasCbu" />
        </label>
      </div>
      <div class="row">
        <label class="field grow">Titular de la cuenta
          <input class="wide" type="text" maxlength="80" [(ngModel)]="form.titular" />
        </label>
        <label class="field grow">Plazo para confirmar
          <select class="wide" [(ngModel)]="form.plazoTransferenciaHoras">
            @for (p of plazos; track p.horas) { <option [ngValue]="p.horas">{{ p.texto }}</option> }
          </select>
        </label>
      </div>
      <p class="hint">Si no confirmás a tiempo, el pedido se cancela solo y se libera el stock.</p>

      @if (gestion > 0) {
        <label class="check">
          <input type="checkbox" [(ngModel)]="form.gestionACargoComprador" />
          Incluir gasto de gestión {{ precio(gestion) }} en ticket
          <span class="ayuda-wrap">
            <button type="button" class="ayuda" [attr.aria-expanded]="ayudaGestion" aria-describedby="ayuda-gestion" (click)="ayudaGestion = !ayudaGestion">?</button>
            <span id="ayuda-gestion" class="ayuda-texto" [class.visible]="ayudaGestion" role="tooltip">Es un gasto por la gestión del pedido. Si lo tildás, se le suma al comprador. Si no, lo abonás vos como dueño del local.</span>
          </span>
        </label>
        <p class="hint">Si el pedido es menor que {{ precio(gestion) }}, la gestión es el importe del pedido. Un pedido cancelado no paga gestión.
          Con Mercado Pago la separa Mercado Pago. Con transferencia o efectivo queda como deuda del local: se paga aparte del abono.</p>
      } @else {
        <p class="meta">Este local no paga gestión por los pedidos online.</p>
      }
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
    .ayuda-wrap { position: relative; display: inline-flex; }
    .ayuda {
      width: 22px; height: 22px; border-radius: 50%; border: 1px solid var(--line);
      background: var(--panel); color: var(--ink); font-weight: 700; line-height: 1; padding: 0; cursor: pointer;
    }
    .ayuda-texto {
      display: none; position: absolute; z-index: 2; left: 0; top: calc(100% + 6px); width: min(280px, 70vw);
      padding: 8px 10px; background: var(--panel); color: var(--ink); border: 1px solid var(--line);
      border-radius: 8px; font-size: 13px; font-weight: 400; line-height: 1.35;
    }
    .ayuda-wrap:hover .ayuda-texto,
    .ayuda-wrap:focus-within .ayuda-texto,
    .ayuda-texto.visible { display: block; }
    .paleta-ayuda { font-size: 12px; margin-top: 0; }
    .paleta { display: flex; align-items: flex-start; gap: 20px; flex-wrap: wrap; margin: 8px 0 12px; }
    .paleta-controles { display: grid; gap: 10px; }
    .paleta-fila { display: grid; grid-template-columns: auto 32px; align-items: center; column-gap: 12px; }
    .paleta-texto {
      display: flex; align-items: center; gap: 8px; margin: 0;
      font-size: 12px; color: var(--ink-soft);
    }
    .paleta-texto input { width: 108px; font-size: 13px; color: var(--ink); }
    .muestra-color {
      width: 32px; height: 32px; padding: 0; border: 1px solid var(--line); border-radius: 8px;
      background: transparent; cursor: pointer;
    }
    .vista-paleta { padding: 12px; border-radius: 10px; display: grid; gap: 8px; width: 220px; }
    .vista-ficha {
      background: #fffdf8; color: #1b241f; border: 1px solid #d9d0c0; border-radius: 10px;
      padding: 14px; display: flex; flex-direction: column; align-items: flex-start; gap: 6px;
    }
    .vista-foto {
      width: 100%; height: 120px; object-fit: contain; align-self: center;
      border-radius: 8px; background: #f4efe4;
    }
    .vista-foto-vacia { display: flex; align-items: center; justify-content: center; color: #4a574f; font-size: 12px; }
    .vista-atribucion { font-size: 11px; color: #4a574f; }
    .vista-nombre { font-weight: 600; }
    .vista-precio { font-size: 18px; font-variant-numeric: tabular-nums; }
    .vista-stock {
      display: inline-block; font-size: 11px; padding: 2px 7px; border-radius: 99px;
      background: #e5f3ea; color: #2f7d57;
    }
    .vista-stock-no { background: #f7e4c4; color: #b33a2b; }
    .vista-agregar, .vista-cantidad { color: #fff; }
    .vista-ficha .vista-foto-vacia,
    .vista-ficha .vista-atribucion { color: color-mix(in srgb, currentColor 72%, transparent); }
    .vista-pie { margin: 0; padding-top: 8px; border-top: 1px solid color-mix(in srgb, currentColor 40%, transparent); font-size: 12px; }
    .logo-previa { max-width: 200px; max-height: 200px; object-fit: contain; display: block; margin: 0 0 12px; }
  `
})
export class ConfigTiendaComponent implements OnInit {
  form: ConfiguracionTiendaDto = {
    alias: '', estado: 'inactiva', direccion: '', horario: '', whatsApp: '', haceEnvios: false,
    envioGratisSiempre: false, precioEnvioKm: null, envioMinimo: null, radioEnvioKm: null, envioGratisDesde: null,
    latitud: null, longitud: null,
    gestionACargoComprador: false, cbu: '', aliasCbu: '', titular: '', plazoTransferenciaHoras: 24,
    colorMarca: '', colorFondo: '', colorFicha: ''
  };
  readonly centroPorDefecto = CENTRO_POR_DEFECTO;
  readonly plazos = PLAZOS_TRANSFERENCIA;
  readonly fondoStock = FONDO_STOCK;
  readonly fichaStock = FICHA_STOCK;
  readonly botonStock = BOTON_STOCK;
  readonly hex = hexDe;
  aliasGuardado = '';
  logo: string | null = null;
  subiendoLogo = false;
  vista: VistaTienda | null = null;
  publicados = 0;
  muestra: ProductoTiendaDto | null = null;
  mercadoPago = false;
  gestion = 0;
  busqueda = '';
  lugares: LugarEncontradoDto[] = [];
  buscando = false;
  guardando = false;
  error = '';
  mensaje = '';
  ayudaGestion = false;

  constructor(
    private readonly api: ApiService,
    private readonly dialogo: DialogoService
  ) {}

  ngOnInit(): void {
    this.api.configuracionTienda().subscribe({
      next: (r) => {
        const { vista, publicados, conImagen, mercadoPago, gestion, logo, ...form } = r;
        this.logo = logo;
        this.form = { ...form, colorMarca: form.colorMarca ?? '', colorFondo: form.colorFondo ?? '', colorFicha: form.colorFicha ?? '' };
        this.aliasGuardado = form.alias;
        this.vista = vista;
        this.publicados = publicados;
        this.mercadoPago = mercadoPago;
        this.gestion = gestion;
        this.cargarMuestra(form.alias);
      },
      error: (e) => (this.error = this.mensajeError(e, 'No se pudo cargar la tienda.'))
    });
  }

  urlLogo(): string {
    return this.logo && this.aliasGuardado ? this.api.urlLogoTienda(this.aliasGuardado, this.logo) : '';
  }

  async subirLogo(evento: Event): Promise<void> {
    const input = evento.target as HTMLInputElement;
    const archivo = input.files?.[0];
    input.value = '';
    if (!archivo) return;
    if (archivo.size > MAXIMO_BYTES_LOGO) {
      this.error = 'La imagen no puede superar 20 MB.';
      return;
    }
    this.subiendoLogo = true;
    this.error = '';
    let imagen: string;
    try {
      imagen = await achicarLogo(archivo);
    } catch {
      this.subiendoLogo = false;
      this.error = 'No se pudo leer la imagen. Probá con JPG o PNG.';
      return;
    }
    this.api.subirLogoTienda(imagen).subscribe({
      next: (r) => {
        this.logo = r.logo;
        this.subiendoLogo = false;
        this.mensaje = 'Logo guardado.';
      },
      error: (e) => {
        this.subiendoLogo = false;
        this.error = this.mensajeError(e, 'No se pudo subir el logo.');
      }
    });
  }

  quitarLogo(): void {
    this.subiendoLogo = true;
    this.api.quitarLogoTienda().subscribe({
      next: () => {
        this.logo = null;
        this.subiendoLogo = false;
        this.mensaje = 'Logo quitado.';
      },
      error: (e) => {
        this.subiendoLogo = false;
        this.error = this.mensajeError(e, 'No se pudo quitar el logo.');
      }
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

  elegirBoton(evento: Event): void {
    this.form = { ...this.form, colorMarca: (evento.target as HTMLInputElement).value };
  }

  elegirFondo(evento: Event): void {
    this.form = { ...this.form, colorFondo: (evento.target as HTMLInputElement).value };
  }

  elegirFicha(evento: Event): void {
    this.form = { ...this.form, colorFicha: (evento.target as HTMLInputElement).value };
  }

  limpiarColores(): void {
    this.form = { ...this.form, colorMarca: '', colorFondo: '', colorFicha: '' };
  }

  textoDeFondo(): string {
    const fondo = this.hex(this.form.colorFondo);
    return fondo && textoClaro(fondo) ? '#ffffff' : '#1b241f';
  }

  textoDeFicha(): string {
    const ficha = this.hex(this.form.colorFicha);
    return ficha && textoClaro(ficha) ? '#ffffff' : '#1b241f';
  }

  fotoMuestra(): ImagenTiendaDto | null {
    return this.muestra?.imagenes[0] ?? null;
  }

  urlFoto(id: number): string {
    return this.api.urlImagenTienda(this.aliasGuardado, id);
  }

  precioTienda(n: number): string {
    return `$ ${Number(n).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }

  private cargarMuestra(alias: string, pagina = 1): void {
    if (!alias || this.vista !== 'catalogo' || pagina > 5) return;
    this.api.productosTienda(alias, { pagina }).subscribe({
      next: (r) => {
        const conFoto = r.items.find((p) => p.imagenes.length > 0);
        if (conFoto) {
          this.muestra = conFoto;
          return;
        }
        this.muestra ??= r.items[0] ?? null;
        if (pagina * r.tamano < r.total) this.cargarMuestra(alias, pagina + 1);
      },
      error: () => { this.muestra ??= null; }
    });
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

  precio(n: number): string {
    return `$ ${Number(n).toLocaleString('es-AR', { maximumFractionDigits: 2 })}`;
  }

  private mensajeError(error: { error?: unknown }, defecto: string): string {
    return typeof error.error === 'string' ? error.error : defecto;
  }
}
