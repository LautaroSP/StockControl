import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ApiService, ImagenTiendaDto, ProductoTiendaDto, TiendaPublicaDto } from '../servicios/api.service';
import { TemaComponent } from '../tema.component';
import { MapaComponent } from '../mapa.component';
import { NoEncontradaComponent } from './no-encontrada.component';

@Component({
  selector: 'sc-tienda',
  standalone: true,
  imports: [FormsModule, TemaComponent, MapaComponent, NoEncontradaComponent],
  template: `
    @if (noEncontrada) {
      <sc-no-encontrada />
    }
    @if (tienda; as t) {
      <div class="tienda">
        <header class="tienda-cabecera">
          <h1>{{ t.nombre }}</h1>
          <sc-tema [fijo]="false" />
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
            <div class="panel pad">
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
                  <img class="tienda-foto" [src]="urlFoto(foto.id)" [alt]="p.nombre" width="400" height="400" loading="lazy" />
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
  private alias = '';
  private espera?: ReturnType<typeof setTimeout>;
  private readonly tituloAnterior = document.title;

  constructor(
    private readonly ruta: ActivatedRoute,
    private readonly api: ApiService
  ) {}

  ngOnInit(): void {
    this.alias = this.ruta.snapshot.paramMap.get('alias') ?? '';
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

  urlFoto(idImagen: number): string {
    return this.api.urlImagenTienda(this.alias, idImagen);
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
