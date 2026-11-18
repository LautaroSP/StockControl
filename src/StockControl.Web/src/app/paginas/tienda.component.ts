import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService, CotizacionTiendaDto, ImagenTiendaDto, ProductoTiendaDto, TiendaPublicaDto } from '../servicios/api.service';
import { CarritoTiendaService, MAXIMO_POR_PRODUCTO } from '../servicios/carrito-tienda.service';
import { TemaComponent } from '../tema.component';
import { TemaService } from '../servicios/tema.service';
import { colorSiClaro, tintaDe, tintaDeFicha } from '../servicios/paleta';
import { MapaComponent } from '../mapa.component';
import { NoEncontradaComponent } from './no-encontrada.component';
import { PieTiendaComponent } from '../pie-tienda.component';

@Component({
  selector: 'sc-tienda',
  standalone: true,
  imports: [FormsModule, RouterLink, TemaComponent, MapaComponent, NoEncontradaComponent, PieTiendaComponent],
  template: `
    @if (noEncontrada) {
      <sc-no-encontrada />
    }
    @if (tienda; as t) {
      <div class="tienda" [class.tienda-marca]="!!propio(t.colorMarca)" [class.tienda-fondo]="!!propio(t.colorFondo)" [class.tienda-ficha]="!!propio(t.colorFicha)" [style.--acento]="propio(t.colorMarca)" [style.--fondo]="propio(t.colorFondo)" [style.--tinta]="propio(t.colorFondo) ? tinta(t) : null" [style.--ficha]="propio(t.colorFicha)" [style.--tinta-ficha]="propio(t.colorFicha) ? tintaFicha(t) : null">
        <header class="tienda-cabecera">
          <h1>{{ t.nombre }}</h1>
          <div class="row">
            @if (t.vista === 'catalogo') {
              <button class="btn btn-primary" type="button" [attr.aria-expanded]="carritoAbierto" (click)="carritoAbierto = !carritoAbierto">
                Carrito · {{ precio(totalCarrito) }}
              </button>
            }
            <sc-tema [fijo]="false" />
          </div>
        </header>

        @if (t.vista === 'cerrada') {
          <section class="panel tienda-aviso">
            <h2>Tienda temporalmente cerrada</h2>
          </section>
        } @else if (t.vista === 'pausada') {
          <section class="panel tienda-aviso">
            <h2>{{ mensajePausada(t.horario) }}</h2>
          </section>
        } @else {
          <section class="tienda-datos">
            <div class="panel pad tienda-presentacion">
              @if (t.logo) {
                <img class="tienda-logo" [src]="urlLogo(t.logo)" alt="" />
              }
              @if (t.direccion) { <p><strong>Dirección:</strong> {{ t.direccion }}</p> }
              @if (t.horario) { <p><strong>Horario:</strong> {{ t.horario }}</p> }
              <p>{{ t.haceEnvios ? 'Hace envíos y retiro en el local.' : 'Solo retiro en el local.' }}</p>
              @if (t.whatsApp) {
                <a class="btn btn-primary" [href]="'https://wa.me/' + t.whatsApp" target="_blank" rel="noopener">Escribir por WhatsApp</a>
              }
            </div>
            @if (t.latitud !== null && t.longitud !== null) {
              <div class="panel tienda-mapa">
                <sc-mapa [latitud]="t.latitud" [longitud]="t.longitud" />
                <a class="btn" [href]="comoLlegar(t.latitud, t.longitud)" target="_blank" rel="noopener">Cómo llegar</a>
              </div>
            }
          </section>

          <section class="toolbar tienda-filtros">
            <input class="grow" type="search" placeholder="Buscar por nombre o código" maxlength="80" aria-label="Buscar productos"
                   [(ngModel)]="busqueda" (ngModelChange)="buscarEnUnRato()" (keydown.enter)="buscar()" />
          </section>
          @if (t.categorias.length > 0) {
            <div class="tienda-categorias" role="group" aria-label="Filtrar por categoría">
              <button class="chip" type="button" [attr.aria-pressed]="categorias.size === 0" (click)="todas()">Todas</button>
              @for (c of t.categorias; track c.id) {
                <button class="chip" type="button" [attr.aria-pressed]="categorias.has(c.id)" (click)="alternarCategoria(c.id)">{{ c.nombre }}</button>
              }
            </div>
          }

          @if (error) { <p class="error">{{ error }}</p> }
          @if (productos.length === 0 && !cargando) {
            <p class="meta">{{ busqueda || categorias.size ? 'No hay productos con esa búsqueda.' : 'Todavía no hay productos publicados.' }}</p>
          }
          <ul class="tienda-productos">
            @for (p of productos; track p.id) {
              <li class="panel">
                @if (fotoActual(p); as foto) {
                  <img class="tienda-foto" [src]="urlFoto(foto.id)" [alt]="p.nombre" width="250" height="300" loading="lazy" />
                  @if (p.imagenes.length > 1) {
                    <span class="tienda-miniaturas">
                      @for (i of p.imagenes; track i.id; let n = $index) {
                        <button type="button" [class.activa]="i.id === foto.id" [attr.aria-label]="'Ver imagen ' + (n + 1)" (click)="elegirFoto(p.id, n)">{{ n + 1 }}</button>
                      }
                    </span>
                  }
                  @if (foto.origen === 'openfoodfacts') {
                    <span class="tienda-atribucion">Foto: <a [href]="foto.enlace" target="_blank" rel="noopener">{{ foto.fuente }}</a> · {{ foto.licencia }}</span>
                  }
                } @else {
                  <span class="tienda-foto tienda-foto-vacia">Sin imagen</span>
                }
                <span class="tienda-nombre">{{ p.nombre }}</span>
                <span class="tienda-precio">{{ precio(p.precio) }}</span>
                <span [class]="p.hayStock ? 'badge' : 'badge badge-vencido'">{{ p.hayStock ? 'Hay stock' : 'Sin stock' }}</span>
                @if (enCarrito(p.id); as n) {
                  <span class="row tienda-cantidad">
                    <button class="btn" type="button" [attr.aria-label]="'Restar ' + p.nombre" (click)="poner(p.id, n - 1)">−</button>
                    <span aria-live="polite">{{ n }}</span>
                    <button class="btn" type="button" [attr.aria-label]="'Sumar ' + p.nombre" [disabled]="n >= maximo" (click)="poner(p.id, n + 1)">+</button>
                  </span>
                } @else {
                  <button class="btn btn-primary" type="button" [disabled]="!p.hayStock" (click)="poner(p.id, 1)">Agregar</button>
                }
              </li>
            }
          </ul>
          @if (paginas > 1) {
            <nav class="row tienda-paginas" aria-label="Páginas">
              <button class="btn" type="button" [disabled]="pagina <= 1" (click)="irA(pagina - 1)">Anterior</button>
              <span class="meta">Página {{ pagina }} de {{ paginas }}</span>
              <button class="btn" type="button" [disabled]="pagina >= paginas" (click)="irA(pagina + 1)">Siguiente</button>
            </nav>
          }
        }
        <sc-pie-tienda [alias]="alias" [nombre]="t.nombre" [direccion]="t.direccion" [horario]="t.horario" />
        @if (t.vista === 'catalogo' && carritoAbierto) {
          <button class="carrito-fondo" type="button" aria-label="Cerrar carrito" (click)="carritoAbierto = false"></button>
          <aside class="carrito-panel" aria-label="Carrito">
            <header class="carrito-panel-cabecera">
              <h2>Tu carrito</h2>
              <button class="btn" type="button" (click)="carritoAbierto = false">Cerrar</button>
            </header>
            <div class="carrito-panel-lista">
              @if (!cotizacion || cotizacion.lineas.length === 0) {
                <p class="meta">{{ unidades > 0 ? 'Cargando…' : 'El carrito está vacío.' }}</p>
              } @else {
                <ul>
                  @for (l of cotizacion.lineas; track l.idProducto) {
                    <li>
                      <span class="carrito-nombre">{{ l.nombre }}</span>
                      <span class="row tienda-cantidad">
                        <button class="btn" type="button" [attr.aria-label]="'Restar ' + l.nombre" (click)="poner(l.idProducto, l.cantidad - 1)">−</button>
                        <span>{{ l.cantidad }}</span>
                        <button class="btn" type="button" [attr.aria-label]="'Sumar ' + l.nombre" [disabled]="l.cantidad >= maximo || l.maximo !== null" (click)="poner(l.idProducto, l.cantidad + 1)">+</button>
                      </span>
                      <span class="carrito-importe">{{ precio(l.subTotal) }}</span>
                    </li>
                  }
                </ul>
              }
            </div>
            <footer class="carrito-panel-pie">
              <span>Total <strong>{{ precio(totalCarrito) }}</strong></span>
              @if (totalCarrito > 0) {
                <a class="btn btn-primary" [routerLink]="['/tienda', alias, 'carrito']">Pedir</a>
              } @else {
                <button class="btn btn-primary" type="button" disabled>Pedir</button>
              }
            </footer>
          </aside>
        }
      </div>
    }
  `
})
export class TiendaComponent implements OnInit, OnDestroy {
  tienda: TiendaPublicaDto | null = null;
  noEncontrada = false;
  productos: ProductoTiendaDto[] = [];
  fotoElegida: Record<number, number> = {};
  busqueda = '';
  categorias = new Set<number>();
  pagina = 1;
  paginas = 1;
  cargando = false;
  error = '';
  alias = '';
  unidades = 0;
  cantidades: Record<number, number> = {};
  cotizacion: CotizacionTiendaDto | null = null;
  carritoAbierto = false;
  readonly maximo = MAXIMO_POR_PRODUCTO;
  readonly tinta = tintaDe;
  readonly tintaFicha = tintaDeFicha;

  propio(color: string | null): string | null {
    return colorSiClaro(this.tema.tema(), color);
  }
  private espera?: ReturnType<typeof setTimeout>;
  private readonly tituloAnterior = document.title;

  constructor(
    private readonly ruta: ActivatedRoute,
    private readonly api: ApiService,
    private readonly carrito: CarritoTiendaService,
    readonly tema: TemaService
  ) {}

  ngOnInit(): void {
    this.alias = this.ruta.snapshot.paramMap.get('alias') ?? '';
    this.leerCarrito();
    this.api.tienda(this.alias).subscribe({
      next: (t) => {
        this.tienda = t;
        document.title = t.nombre;
        if (t.vista === 'catalogo') this.cargar();
      },
      error: (e) => {
        if (e.status === 404) this.noEncontrada = true;
        else this.error = e.status === 429 ? 'Demasiadas consultas. Esperá un minuto.' : 'No se pudo abrir la tienda.';
      }
    });
  }

  ngOnDestroy(): void {
    clearTimeout(this.espera);
    document.title = this.tituloAnterior;
  }

  buscarEnUnRato(): void {
    clearTimeout(this.espera);
    this.espera = setTimeout(() => this.buscar(), 350);
  }

  buscar(): void {
    clearTimeout(this.espera);
    this.pagina = 1;
    this.cargar();
  }

  todas(): void {
    if (this.categorias.size === 0) return;
    this.categorias = new Set();
    this.buscar();
  }

  alternarCategoria(id: number): void {
    const elegidas = new Set(this.categorias);
    if (elegidas.has(id)) elegidas.delete(id);
    else elegidas.add(id);
    this.categorias = elegidas;
    this.buscar();
  }

  irA(pagina: number): void {
    this.pagina = pagina;
    this.cargar();
    window.scrollTo({ top: 0 });
  }

  mensajePausada(horario: string): string {
    const base = 'En este momento la tienda no se encuentra disponible';
    return horario ? `${base}, intentá en los horarios ${horario}.` : `${base}.`;
  }

  comoLlegar(latitud: number, longitud: number): string {
    return `https://www.google.com/maps/dir/?api=1&destination=${latitud},${longitud}`;
  }

  fotoActual(p: ProductoTiendaDto): ImagenTiendaDto | undefined {
    return p.imagenes[this.fotoElegida[p.id] ?? 0] ?? p.imagenes[0];
  }

  elegirFoto(idProducto: number, posicion: number): void {
    this.fotoElegida = { ...this.fotoElegida, [idProducto]: posicion };
  }

  enCarrito(idProducto: number): number {
    return this.cantidades[idProducto] ?? 0;
  }

  get totalCarrito(): number {
    return this.cotizacion?.total ?? 0;
  }

  poner(idProducto: number, cantidad: number): void {
    const eraNuevo = this.enCarrito(idProducto) === 0 && cantidad > 0;
    this.carrito.poner(this.alias, idProducto, cantidad);
    this.leerCarrito();
    if (eraNuevo) this.carritoAbierto = true;
  }

  private leerCarrito(): void {
    const items = this.carrito.items(this.alias);
    this.cantidades = Object.fromEntries(items.map((i) => [i.idProducto, i.cantidad]));
    this.unidades = items.reduce((total, i) => total + i.cantidad, 0);
    this.cotizar();
  }

  private cotizar(): void {
    const items = this.carrito.items(this.alias);
    if (items.length === 0) {
      this.cotizacion = null;
      return;
    }
    this.api.cotizarCarritoTienda(this.alias, items).subscribe({
      next: (c) => {
        c.noDisponibles.forEach((id) => this.carrito.sacar(this.alias, id));
        const quedan = this.carrito.items(this.alias);
        this.cantidades = Object.fromEntries(quedan.map((i) => [i.idProducto, i.cantidad]));
        this.unidades = quedan.reduce((total, i) => total + i.cantidad, 0);
        this.cotizacion = c;
      }
    });
  }

  urlFoto(idImagen: number): string {
    return this.api.urlImagenTienda(this.alias, idImagen);
  }

  urlLogo(logo: string): string {
    return this.api.urlLogoTienda(this.alias, logo);
  }

  precio(n: number): string {
    return `$ ${Number(n).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }

  private cargar(): void {
    this.cargando = true;
    this.api.productosTienda(this.alias, { q: this.busqueda.trim(), categorias: [...this.categorias], pagina: this.pagina }).subscribe({
      next: (r) => {
        this.productos = r.items;
        this.paginas = Math.max(1, Math.ceil(r.total / r.tamano));
        this.cargando = false;
        this.error = '';
      },
      error: (e) => {
        this.cargando = false;
        this.error = e.status === 429 ? 'Demasiadas consultas. Esperá un minuto.' : 'No se pudieron cargar los productos.';
      }
    });
  }
}
