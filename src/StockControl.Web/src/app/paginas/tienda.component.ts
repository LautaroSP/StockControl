import { Component, HostListener, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService, CotizacionTiendaDto, ImagenTiendaDto, OrdenTienda, ProductoTiendaDto, TiendaPublicaDto } from '../servicios/api.service';
import { CarritoTiendaService, MAXIMO_POR_PRODUCTO } from '../servicios/carrito-tienda.service';
import { TemaComponent } from '../tema.component';
import { TemaService } from '../servicios/tema.service';
import { colorSiClaro, tintaDe, tintaDeFicha } from '../servicios/paleta';
import { MapaComponent } from '../mapa.component';
import { NoEncontradaComponent } from './no-encontrada.component';
import { PieTiendaComponent } from '../pie-tienda.component';
import { ContactoTiendaComponent } from '../contacto-tienda.component';
import { IconoComponent } from '../icono.component';
import { PanelCarritoTiendaComponent } from '../panel-carrito-tienda.component';

const FILAS_POR_CARGA = 4;
const ANCHO_MIN_TARJETA = 200;
const GAP_GRILLA = 12;

@Component({
  selector: 'sc-tienda',
  standalone: true,
  imports: [FormsModule, RouterLink, TemaComponent, MapaComponent, NoEncontradaComponent, PieTiendaComponent, ContactoTiendaComponent, IconoComponent, PanelCarritoTiendaComponent],
  template: `
    @if (noEncontrada) {
      <sc-no-encontrada />
    }
    @if (tienda; as t) {
      <div class="tienda" [class.tienda-con-carrito]="t.vista === 'catalogo' && carritoAbierto" [class.tienda-marca]="!!propio(t.colorMarca)" [class.tienda-fondo]="!!propio(t.colorFondo)" [class.tienda-ficha]="!!propio(t.colorFicha)" [style.--acento]="propio(t.colorMarca)" [style.--fondo]="propio(t.colorFondo)" [style.--tinta]="propio(t.colorFondo) ? tinta(t) : null" [style.--ficha]="propio(t.colorFicha)" [style.--tinta-ficha]="propio(t.colorFicha) ? tintaFicha(t) : null">
        <header class="tienda-cabecera">
          <h1>{{ t.nombre }}</h1>
          <div class="row">
            @if (t.vista === 'catalogo') {
              <button class="btn btn-primary" type="button" [attr.aria-expanded]="carritoAbierto" (click)="carritoAbierto = !carritoAbierto">
                <sc-icono nombre="carrito" /> {{ precio(totalCarrito) }}
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
              <sc-contacto-tienda [whatsApp]="t.whatsApp" [correo]="t.correo" [instagram]="t.instagram" />
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
            <select aria-label="Ordenar productos" [ngModel]="orden" (ngModelChange)="cambiarOrden($event)">
              <option value="nombre|asc">Nombre A-Z</option>
              <option value="nombre|desc">Nombre Z-A</option>
              <option value="precio|asc">Menor precio</option>
              <option value="precio|desc">Mayor precio</option>
            </select>
          </section>
          @if (t.categorias.length > 0 && t.filtroCategorias !== 'menu') {
            <div class="tienda-categorias" role="group" aria-label="Filtrar por categoría">
              <button class="chip" type="button" [attr.aria-pressed]="categorias.size === 0" (click)="todas()">Todas</button>
              @for (c of categoriasChips; track c.id) {
                <button class="chip" type="button" [attr.aria-pressed]="categorias.has(c.id)" (click)="alternarCategoria(c.id)">{{ c.nombre }}</button>
              }
            </div>
          }

          <div class="tienda-cuerpo" [class.tienda-cuerpo-menu]="t.filtroCategorias === 'menu' && t.categorias.length > 0">
            @if (t.filtroCategorias === 'menu' && t.categorias.length > 0) {
              <aside class="tienda-menu-categorias" aria-label="Categorías">
                <button type="button" class="menu-cat" [class.activa]="categorias.size === 0" (click)="todas()">Todas</button>
                @for (padre of categoriasRaiz; track padre.id) {
                  <div class="menu-bloque">
                    <button type="button" class="menu-cat" [class.activa]="categorias.has(padre.id)" (click)="elegirCategoria(padre.id)">
                      {{ padre.nombre }} <span class="meta">({{ padre.cantidad }})</span>
                    </button>
                    @if (hijosDe(padre.id); as hijos) {
                      @if (hijos.length) {
                        <button type="button" class="menu-toggle" (click)="alternarAbierta(padre.id)" [attr.aria-expanded]="abierta(padre.id)">{{ abierta(padre.id) ? '−' : '+' }}</button>
                        @if (abierta(padre.id) || categorias.has(padre.id) || hijoActivo(padre.id)) {
                          <div class="menu-hijos">
                            @for (h of hijos; track h.id) {
                              <button type="button" class="menu-cat menu-hijo" [class.activa]="categorias.has(h.id)" (click)="elegirCategoria(h.id)">
                                {{ h.nombre }} <span class="meta">({{ h.cantidad }})</span>
                              </button>
                            }
                          </div>
                        }
                      }
                    }
                  </div>
                }
              </aside>
            }
            <div class="tienda-listado" [class.tienda-listado-cargando]="cargando && reiniciando">
              @if (error) { <p class="error">{{ error }}</p> }
              @if (productos.length === 0 && !cargando) {
                <p class="meta">{{ busqueda || categorias.size ? 'No hay productos con esa búsqueda.' : 'Todavía no hay productos publicados.' }}</p>
              }
              <ul class="tienda-productos">
                @for (p of productos; track p.id) {
                  <li class="panel">
                    @if (fotoActual(p); as foto) {
                      @if (fotoRota(foto.id)) {
                        <span class="tienda-foto tienda-foto-vacia">Sin imagen</span>
                      } @else {
                        <img class="tienda-foto" [src]="urlFoto(foto.id)" [alt]="p.nombre" width="250" height="300" loading="lazy" (error)="marcarFotoRota(foto.id)" />
                      }
                      @if (p.imagenes.length > 1) {
                        <span class="tienda-miniaturas">
                          @for (i of p.imagenes; track i.id; let n = $index) {
                            <button type="button" [class.activa]="i.id === foto.id" [attr.aria-label]="'Ver imagen ' + (n + 1)" (click)="elegirFoto(p.id, n)">{{ n + 1 }}</button>
                          }
                        </span>
                      }
                      @if (foto.origen === 'openfoodfacts' && !fotoRota(foto.id)) {
                        <span class="tienda-atribucion">Foto: <a [href]="foto.enlace" target="_blank" rel="noopener">{{ foto.fuente }}</a> · {{ foto.licencia }}</span>
                      }
                    } @else {
                      <span class="tienda-foto tienda-foto-vacia">Sin imagen</span>
                    }
                    <a class="tienda-nombre" [routerLink]="['/tienda', alias, 'producto', p.id]">{{ p.nombre }}</a>
                    <span class="tienda-precio">
                      @if (p.precioLista > p.precio) { <s class="precio-tachado">{{ precio(p.precioLista) }}</s> }
                      {{ precio(p.precio) }}
                    </span>
                    @if (p.oferta) {
                      <span class="badge badge-oferta"><sc-icono nombre="promocion" /> Promociones!</span>
                      <span class="badge badge-oferta">{{ p.oferta }}</span>
                    }
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
              @if (cargando && productos.length > 0) {
                <p class="meta" style="text-align:center">Cargando más…</p>
              }
            </div>
          </div>
        }
        <sc-pie-tienda [alias]="alias" [nombre]="t.nombre" [direccion]="t.direccion" [horario]="t.horario" />
        @if (t.vista === 'catalogo') {
          <sc-panel-carrito-tienda
            [alias]="alias"
            [cotizacion]="cotizacion"
            [unidades]="unidades"
            [total]="totalCarrito"
            [abierto]="carritoAbierto"
            (abiertoChange)="carritoAbierto = $event"
            (cambiar)="poner($event.idProducto, $event.cantidad)"
          />
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
  private fotosRotas = new Set<number>();
  busqueda = '';
  orden = 'nombre|asc';
  categorias = new Set<number>();
  private abiertas = new Set<number>();
  pagina = 1;
  tamano = 24;
  total = 0;
  cargando = false;
  reiniciando = false;
  error = '';
  alias = '';
  unidades = 0;
  cantidades: Record<number, number> = {};
  cotizacion: CotizacionTiendaDto | null = null;
  carritoAbierto = true;
  readonly maximo = MAXIMO_POR_PRODUCTO;
  readonly tinta = tintaDe;
  readonly tintaFicha = tintaDeFicha;
  private cargaId = 0;

  get hayMas(): boolean {
    return this.productos.length < this.total;
  }

  get categoriasRaiz() {
    return (this.tienda?.categorias ?? []).filter((c) => !c.idPadre);
  }

  /** En chips: raíces (y hojas sueltas sin padre). */
  get categoriasChips() {
    return (this.tienda?.categorias ?? []).filter((c) => !c.idPadre);
  }

  hijosDe(idPadre: number) {
    return (this.tienda?.categorias ?? []).filter((c) => c.idPadre === idPadre);
  }

  hijoActivo(idPadre: number): boolean {
    return this.hijosDe(idPadre).some((h) => this.categorias.has(h.id));
  }

  abierta(id: number): boolean {
    return this.abiertas.has(id);
  }

  alternarAbierta(id: number): void {
    if (this.abiertas.has(id)) this.abiertas.delete(id);
    else this.abiertas.add(id);
    this.abiertas = new Set(this.abiertas);
  }

  elegirCategoria(id: number): void {
    this.categorias = new Set([id]);
    const padre = this.tienda?.categorias.find((c) => c.id === id);
    if (padre && !padre.idPadre) this.abiertas.add(id);
    this.buscar();
  }

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
    this.cargar(true);
  }

  cambiarOrden(valor: string): void {
    this.orden = valor;
    this.buscar();
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

  @HostListener('window:scroll')
  alScroll(): void {
    if (this.cargando || !this.hayMas) return;
    const resto = document.documentElement.scrollHeight - (window.scrollY + window.innerHeight);
    if (resto < 480) this.cargar(false);
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

  fotoRota(idImagen: number): boolean {
    return this.fotosRotas.has(idImagen);
  }

  marcarFotoRota(idImagen: number): void {
    this.fotosRotas.add(idImagen);
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
    this.carrito.poner(this.alias, idProducto, cantidad);
    this.leerCarrito();
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

  private cargar(reiniciar = true): void {
    if (!reiniciar) {
      if (this.cargando || !this.hayMas) return;
    }
    const id = reiniciar ? ++this.cargaId : this.cargaId;
    if (reiniciar) {
      this.pagina = 1;
      this.tamano = this.tamanoPorFilas();
      this.reiniciando = true;
    } else {
      this.pagina += 1;
    }
    this.cargando = true;
    const [orden, sentido] = this.orden.split('|') as [OrdenTienda, 'asc' | 'desc'];
    this.api.productosTienda(this.alias, {
      q: this.busqueda.trim(),
      categorias: [...this.categorias],
      pagina: this.pagina,
      tamano: this.tamano,
      orden,
      sentido
    }).subscribe({
      next: (r) => {
        if (id !== this.cargaId) return;
        this.total = r.total;
        this.tamano = r.tamano;
        this.productos = reiniciar ? r.items : [...this.productos, ...r.items];
        this.cargando = false;
        this.reiniciando = false;
        this.error = '';
        if (reiniciar) {
          this.mantenerVistaListado();
          setTimeout(() => this.alScroll());
        }
      },
      error: (e) => {
        if (id !== this.cargaId) return;
        if (!reiniciar) this.pagina = Math.max(1, this.pagina - 1);
        this.cargando = false;
        this.reiniciando = false;
        this.error = e.status === 429 ? 'Demasiadas consultas. Esperá un minuto.' : 'No se pudieron cargar los productos.';
      }
    });
  }

  /** Evita el salto al tope cuando el listado se achica al filtrar. */
  private mantenerVistaListado(): void {
    const listado = document.querySelector('.tienda-listado') as HTMLElement | null;
    if (!listado) return;
    const top = listado.getBoundingClientRect().top + window.scrollY - 16;
    if (window.scrollY > top) window.scrollTo(0, top);
  }

  /** columnas × 4 filas, según el ancho de la grilla (mismo criterio que CSS auto-fill 200px). */
  private tamanoPorFilas(): number {
    const lista = document.querySelector('.tienda-productos') as HTMLElement | null;
    const ancho = lista?.clientWidth || Math.max(320, window.innerWidth - (this.carritoAbierto ? 360 : 48));
    const columnas = window.matchMedia('(max-width: 600px)').matches
      ? 2
      : Math.max(2, Math.floor((ancho + GAP_GRILLA) / (ANCHO_MIN_TARJETA + GAP_GRILLA)));
    return Math.min(48, Math.max(8, columnas * FILAS_POR_CARGA));
  }
}
