import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService, CotizacionTiendaDto, ProductoTiendaDto, TiendaPublicaDto } from '../servicios/api.service';
import { CarritoTiendaService, MAXIMO_POR_PRODUCTO } from '../servicios/carrito-tienda.service';
import { TemaComponent } from '../tema.component';
import { TemaService } from '../servicios/tema.service';
import { colorSiClaro, tintaDe, tintaDeFicha } from '../servicios/paleta';
import { NoEncontradaComponent } from './no-encontrada.component';
import { PieTiendaComponent } from '../pie-tienda.component';
import { IconoComponent } from '../icono.component';
import { ContactoTiendaComponent } from '../contacto-tienda.component';
import { PanelCarritoTiendaComponent } from '../panel-carrito-tienda.component';

@Component({
  selector: 'sc-ficha-producto-tienda',
  standalone: true,
  imports: [RouterLink, TemaComponent, NoEncontradaComponent, PieTiendaComponent, IconoComponent, ContactoTiendaComponent, PanelCarritoTiendaComponent],
  template: `
    @if (noEncontrado) {
      <sc-no-encontrada />
    }
    @if (error) { <p class="error">{{ error }}</p> }
    @if (tienda; as t) {
      <div class="tienda" [class.tienda-con-carrito]="t.vista === 'catalogo' && carritoAbierto" [class.tienda-marca]="!!propio(t.colorMarca)" [class.tienda-fondo]="!!propio(t.colorFondo)" [class.tienda-ficha]="!!propio(t.colorFicha)" [style.--acento]="propio(t.colorMarca)" [style.--fondo]="propio(t.colorFondo)" [style.--tinta]="propio(t.colorFondo) ? tinta(t) : null" [style.--ficha]="propio(t.colorFicha)" [style.--tinta-ficha]="propio(t.colorFicha) ? tintaFicha(t) : null">
        <header class="tienda-cabecera">
          <h1><a class="tienda-nombre" [routerLink]="['/tienda', alias]">{{ t.nombre }}</a></h1>
          <div class="row">
            @if (t.vista === 'catalogo') {
              <button class="btn btn-primary" type="button" [attr.aria-expanded]="carritoAbierto" (click)="carritoAbierto = !carritoAbierto">
                <sc-icono nombre="carrito" /> {{ precio(totalCarrito) }}
              </button>
            }
            <sc-tema [fijo]="false" />
          </div>
        </header>

        @if (producto; as p) {
          <a class="btn" [routerLink]="['/tienda', alias]">Volver a la tienda</a>
          <article class="panel pad tienda-ficha-producto">
            <div>
              @if (fotoActual(); as foto) {
                <img class="tienda-foto" [src]="urlFoto(foto.id)" [alt]="p.nombre" />
                @if (p.imagenes.length > 1) {
                  <span class="tienda-miniaturas">
                    @for (i of p.imagenes; track i.id; let n = $index) {
                      <button type="button" [class.activa]="n === elegida" [attr.aria-label]="'Ver imagen ' + (n + 1)" (click)="elegida = n">{{ n + 1 }}</button>
                    }
                  </span>
                }
                @if (foto.origen === 'openfoodfacts') {
                  <span class="tienda-atribucion">Foto: <a [href]="foto.enlace" target="_blank" rel="noopener">{{ foto.fuente }}</a> · {{ foto.licencia }}</span>
                }
              } @else {
                <span class="tienda-foto tienda-foto-vacia">Sin imagen</span>
              }
            </div>
            <div class="tienda-ficha-datos">
              <h2>{{ p.nombre }}</h2>
              <p class="tienda-precio">
                @if (p.precioLista > p.precio) { <s class="precio-tachado">{{ precio(p.precioLista) }}</s> }
                {{ precio(p.precio) }}
              </p>
              @if (p.oferta) {
                <p class="row">
                  <span class="badge badge-oferta"><sc-icono nombre="promocion" /> Promociones!</span>
                  <span class="badge badge-oferta">{{ p.oferta }}</span>
                </p>
              }
              <p><span [class]="p.hayStock ? 'badge' : 'badge badge-vencido'">{{ p.hayStock ? 'Hay stock' : 'Sin stock' }}</span></p>
              @if (p.descripcion) { <p class="tienda-ficha-descripcion">{{ p.descripcion }}</p> }
              @if (enCarrito > 0) {
                <span class="row tienda-cantidad">
                  <button class="btn" type="button" aria-label="Restar" (click)="poner(enCarrito - 1)">−</button>
                  <span aria-live="polite">{{ enCarrito }}</span>
                  <button class="btn" type="button" aria-label="Sumar" [disabled]="enCarrito >= maximo" (click)="poner(enCarrito + 1)">+</button>
                </span>
              } @else {
                <button class="btn btn-primary" type="button" [disabled]="!p.hayStock" (click)="poner(1)">Agregar</button>
              }
              <sc-contacto-tienda
                class="tienda-ficha-consulta"
                titulo="¿Tenés dudas sobre este producto?"
                textoWhatsApp="Contactá a la tienda"
                [whatsApp]="t.whatsApp"
                [correo]="t.correo"
                [instagram]="t.instagram"
                [producto]="p.nombre"
              />
            </div>
          </article>
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
            (cambiar)="cambiarLinea($event.idProducto, $event.cantidad)"
          />
        }
      </div>
    }
  `,
  styles: `
    .tienda-ficha-consulta {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      margin-top: 1.25rem;
      padding-top: 1rem;
      border-top: 1px solid color-mix(in srgb, var(--tinta-ficha, var(--tinta, currentColor)) 18%, transparent);
    }
  `
})
export class FichaProductoTiendaComponent implements OnInit, OnDestroy {
  alias = '';
  tienda: TiendaPublicaDto | null = null;
  producto: ProductoTiendaDto | null = null;
  noEncontrado = false;
  error = '';
  elegida = 0;
  enCarrito = 0;
  unidades = 0;
  cotizacion: CotizacionTiendaDto | null = null;
  carritoAbierto = true;
  readonly maximo = MAXIMO_POR_PRODUCTO;
  readonly tinta = tintaDe;
  readonly tintaFicha = tintaDeFicha;
  private readonly tituloAnterior = document.title;

  constructor(
    private readonly ruta: ActivatedRoute,
    private readonly api: ApiService,
    private readonly carrito: CarritoTiendaService,
    readonly tema: TemaService
  ) {}

  ngOnInit(): void {
    this.alias = this.ruta.snapshot.paramMap.get('alias') ?? '';
    const id = Number(this.ruta.snapshot.paramMap.get('id'));
    this.api.tienda(this.alias).subscribe({
      next: (t) => {
        this.tienda = t;
        this.leerCarrito();
      },
      error: (e) => {
        if (e.status === 404) this.noEncontrado = true;
        else this.error = e.status === 429 ? 'Demasiadas consultas. Esperá un minuto.' : 'No se pudo abrir la tienda.';
      }
    });
    this.api.productoTienda(this.alias, id).subscribe({
      next: (p) => {
        this.producto = p;
        document.title = p.nombre;
        this.leerCarrito();
      },
      error: (e) => {
        if (e.status === 404) this.noEncontrado = true;
        else this.error = e.status === 429 ? 'Demasiadas consultas. Esperá un minuto.' : 'No se pudo abrir el producto.';
      }
    });
  }

  ngOnDestroy(): void {
    document.title = this.tituloAnterior;
  }

  propio(color: string | null): string | null {
    return colorSiClaro(this.tema.tema(), color);
  }

  get totalCarrito(): number {
    return this.cotizacion?.total ?? 0;
  }

  fotoActual() {
    return this.producto?.imagenes[this.elegida] ?? this.producto?.imagenes[0];
  }

  urlFoto(idImagen: number): string {
    return this.api.urlImagenTienda(this.alias, idImagen);
  }

  poner(cantidad: number): void {
    if (!this.producto) return;
    this.carrito.poner(this.alias, this.producto.id, cantidad);
    this.leerCarrito();
  }

  cambiarLinea(idProducto: number, cantidad: number): void {
    this.carrito.poner(this.alias, idProducto, cantidad);
    this.leerCarrito();
  }

  precio(n: number): string {
    return `$ ${Number(n).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }

  private leerCarrito(): void {
    const items = this.carrito.items(this.alias);
    this.enCarrito = items.find((i) => i.idProducto === this.producto?.id)?.cantidad ?? 0;
    this.unidades = items.reduce((total, i) => total + i.cantidad, 0);
    if (items.length === 0) {
      this.cotizacion = null;
      return;
    }
    this.api.cotizarCarritoTienda(this.alias, items).subscribe({
      next: (c) => {
        c.noDisponibles.forEach((id) => this.carrito.sacar(this.alias, id));
        const quedan = this.carrito.items(this.alias);
        this.enCarrito = quedan.find((i) => i.idProducto === this.producto?.id)?.cantidad ?? 0;
        this.unidades = quedan.reduce((total, i) => total + i.cantidad, 0);
        this.cotizacion = c;
      }
    });
  }
}
