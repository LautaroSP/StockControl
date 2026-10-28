import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, ImagenProductoDto, ProductoDto, PropuestaImagenDto } from '../servicios/api.service';

const MAXIMO_BYTES_ORIGINAL = 20 * 1024 * 1024;
const LADO_MAXIMO = 400;
const AVISO_ORIGEN =
  'Generamos imágenes a partir de Open Food Facts, una base abierta hecha por usuarios (licencia CC-BY-SA). Pueden no coincidir con tu producto.';

/** Achica en el navegador para que una foto de celular llegue con pocos KB. El servidor igual valida y vuelve a achicar. */
async function achicar(archivo: File): Promise<string> {
  const imagen = await createImageBitmap(archivo);
  const escala = Math.min(1, LADO_MAXIMO / imagen.width, LADO_MAXIMO / imagen.height);
  const lienzo = document.createElement('canvas');
  lienzo.width = Math.max(1, Math.round(imagen.width * escala));
  lienzo.height = Math.max(1, Math.round(imagen.height * escala));
  const contexto = lienzo.getContext('2d')!;
  contexto.fillStyle = '#fff';
  contexto.fillRect(0, 0, lienzo.width, lienzo.height);
  contexto.drawImage(imagen, 0, 0, lienzo.width, lienzo.height);
  imagen.close();
  return lienzo.toDataURL('image/jpeg', 0.85);
}

@Component({
  selector: 'sc-imagenes-tienda',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="topbar">
      <h1>Imágenes de productos</h1>
    </div>
    <section class="panel config-form">
      <p class="hint">Hasta 2 por producto publicado, de hasta 400 × 400 px. Nada se publica sin que lo aceptes.</p>
      <input class="wide" type="search" maxlength="80" placeholder="Buscar producto publicado" aria-label="Buscar producto publicado"
             [(ngModel)]="busqueda" (ngModelChange)="buscarEnUnRato()" />

      @if (productos.length === 0 && !cargandoLista) {
        <p class="meta">No hay productos publicados{{ busqueda ? ' con esa búsqueda' : '. Se publican desde Productos' }}.</p>
      }
      <ul class="imagenes-lista">
        @for (p of productos; track p.id) {
          <li>
            <div class="row">
              <span class="grow"><strong>{{ p.nombre }}</strong> <span class="meta">{{ p.codigo }}</span></span>
              <button class="btn" type="button" (click)="abrir(p.id)">{{ abierto === p.id ? 'Cerrar' : 'Imágenes' }}</button>
            </div>
            @if (abierto === p.id) {
              <div class="imagenes-apartado">
                <p class="hint">{{ avisoOrigen }}</p>

                @if (propuestaVisible(); as oferta) {
                  <div class="imagenes-propuesta">
                    <p>Te ofrecemos estas imágenes para este producto</p>
                    <img class="imagen-previa" [src]="oferta.url" alt="Imagen propuesta" referrerpolicy="no-referrer" />
                    <p class="hint">
                      Fuente: <a [href]="oferta.enlace" target="_blank" rel="noopener">{{ oferta.fuente }}</a> · {{ oferta.licencia }}
                    </p>
                    <div class="row">
                      <button class="btn btn-primary" type="button" [disabled]="trabajando" (click)="aceptar(p.id)">Aceptar</button>
                      <label class="btn">
                        Subir propia
                        <input type="file" accept="image/*" hidden [disabled]="trabajando"
                               (change)="subir(p.id, ordenDestino(), $event, oferta.url)" />
                      </label>
                    </div>
                  </div>
                } @else if (propuesta?.sinRespuesta) {
                  <p class="meta">No pudimos consultar Open Food Facts. Podés subir tu propia imagen.</p>
                }

                <div class="imagenes-slots">
                  @for (orden of ordenes; track orden) {
                    <div class="imagen-slot">
                      @if (imagenDe(orden); as imagen) {
                        <img class="imagen-previa" [src]="imagen.datos" [alt]="'Imagen ' + orden" />
                        @if (imagen.origen === 'openfoodfacts') {
                          <p class="hint"><a [href]="imagen.enlace" target="_blank" rel="noopener">{{ imagen.fuente }}</a> · {{ imagen.licencia }}</p>
                        }
                      } @else {
                        <div class="imagen-previa imagen-vacia">Sin imagen</div>
                      }
                      <div class="row">
                        <label class="btn">
                          {{ imagenDe(orden) ? 'Reemplazar' : 'Subir propia' }}
                          <input type="file" accept="image/*" hidden [disabled]="trabajando"
                                 (change)="subir(p.id, orden, $event)" />
                        </label>
                        @if (imagenDe(orden)) {
                          <button class="btn" type="button" [disabled]="trabajando" (click)="quitar(p.id, orden)">Quitar</button>
                        }
                      </div>
                    </div>
                  }
                </div>
                <p class="hint">JPG, PNG o WebP de hasta 20 MB. Se achica antes de subir.</p>
                @if (error) { <p class="error">{{ error }}</p> }
                @if (mensaje) { <p class="meta">{{ mensaje }}</p> }
              </div>
            }
          </li>
        }
      </ul>
    </section>
  `
})
export class ImagenesTiendaComponent implements OnInit {
  readonly avisoOrigen = AVISO_ORIGEN;
  readonly ordenes = [1, 2];
  productos: ProductoDto[] = [];
  busqueda = '';
  cargandoLista = false;
  abierto: number | null = null;
  imagenes: ImagenProductoDto[] = [];
  propuesta: PropuestaImagenDto | null = null;
  trabajando = false;
  error = '';
  mensaje = '';
  private espera?: ReturnType<typeof setTimeout>;

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.cargarLista();
  }

  buscarEnUnRato(): void {
    clearTimeout(this.espera);
    this.espera = setTimeout(() => this.cargarLista(), 350);
  }

  abrir(id: number): void {
    this.error = '';
    this.mensaje = '';
    this.imagenes = [];
    this.propuesta = null;
    if (this.abierto === id) {
      this.abierto = null;
      return;
    }
    this.abierto = id;
    this.cargarImagenes(id);
    this.consultarPropuesta(id);
  }

  imagenDe(orden: number): ImagenProductoDto | undefined {
    return this.imagenes.find((i) => i.orden === orden);
  }

  /** La propuesta solo se ofrece si hay una y todavía no se aceptó una de Open Food Facts. */
  propuestaVisible(): PropuestaImagenDto | null {
    const p = this.propuesta;
    if (!p?.disponible || this.imagenes.some((i) => i.origen === 'openfoodfacts')) return null;
    return p;
  }

  /** Primer lugar libre; con los dos ocupados reemplaza el primero. */
  ordenDestino(): number {
    return this.ordenes.find((o) => !this.imagenDe(o)) ?? 1;
  }

  aceptar(id: number): void {
    this.trabajando = true;
    this.api.aceptarPropuestaImagen(id, this.ordenDestino()).subscribe({
      next: () => this.terminar(id, 'Imagen guardada.'),
      error: (e) => this.fallar(e, 'No se pudo guardar la imagen propuesta.')
    });
  }

  async subir(id: number, orden: number, evento: Event, rechazada?: string): Promise<void> {
    const input = evento.target as HTMLInputElement;
    const archivo = input.files?.[0];
    input.value = '';
    if (!archivo) return;
    if (archivo.size > MAXIMO_BYTES_ORIGINAL) {
      this.fallar({}, 'La imagen no puede superar 20 MB.');
      return;
    }
    this.trabajando = true;
    let imagen: string;
    try {
      imagen = await achicar(archivo);
    } catch {
      this.fallar({}, 'No se pudo leer la imagen. Probá con JPG o PNG.');
      return;
    }
    this.api.subirImagenProducto(id, orden, imagen, rechazada).subscribe({
      next: () => {
        if (rechazada) this.propuesta = null;
        this.terminar(id, 'Imagen guardada.');
      },
      error: (e) => this.fallar(e, 'No se pudo subir la imagen.')
    });
  }

  quitar(id: number, orden: number): void {
    this.trabajando = true;
    this.api.borrarImagenProducto(id, orden).subscribe({
      next: () => this.terminar(id, 'Imagen quitada.'),
      error: (e) => this.fallar(e, 'No se pudo quitar la imagen.')
    });
  }

  private terminar(id: number, mensaje: string): void {
    this.trabajando = false;
    this.error = '';
    this.mensaje = mensaje;
    this.cargarImagenes(id);
  }

  private fallar(error: { error?: unknown }, defecto: string): void {
    this.trabajando = false;
    this.mensaje = '';
    this.error = typeof error.error === 'string' ? error.error : defecto;
  }

  private cargarLista(): void {
    this.cargandoLista = true;
    this.api.productos(this.busqueda.trim(), { publicado: true }).subscribe({
      next: (r) => {
        this.productos = r.items;
        this.cargandoLista = false;
      },
      error: () => (this.cargandoLista = false)
    });
  }

  private cargarImagenes(id: number): void {
    this.api.imagenesProducto(id).subscribe({
      next: (r) => {
        if (this.abierto === id) this.imagenes = r.imagenes;
      },
      error: (e) => this.fallar(e, 'No se pudieron cargar las imágenes.')
    });
  }

  private consultarPropuesta(id: number): void {
    this.api.propuestaImagen(id).subscribe({
      next: (r) => {
        if (this.abierto === id) this.propuesta = r;
      },
      error: () => {
        if (this.abierto === id) this.propuesta = null;
      }
    });
  }
}
