import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService, CotizacionTiendaDto, TiendaPublicaDto } from '../servicios/api.service';
import { CarritoTiendaService, MAXIMO_POR_PRODUCTO } from '../servicios/carrito-tienda.service';
import { TemaComponent } from '../tema.component';
import { NoEncontradaComponent } from './no-encontrada.component';

@Component({
  selector: 'sc-carrito-tienda',
  standalone: true,
  imports: [FormsModule, RouterLink, TemaComponent, NoEncontradaComponent],
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
        <a class="btn" [routerLink]="['/tienda', alias]">Volver a la tienda</a>

        @if (t.vista !== 'catalogo') {
          <section class="panel tienda-aviso">
            <h2>La tienda no está recibiendo pedidos en este momento.</h2>
          </section>
        } @else {
          <h2>Tu carrito</h2>
          @if (quitados) { <p class="error">Sacamos del carrito productos que ya no están en la tienda.</p> }
          @if (!cotizacion || cotizacion.lineas.length === 0) {
            <p class="meta">{{ cargando ? 'Cargando…' : 'El carrito está vacío.' }}</p>
          } @else {
            <ul class="panel carrito-lineas">
              @for (l of cotizacion.lineas; track l.idProducto) {
                <li>
                  <span class="carrito-nombre">{{ l.nombre }}</span>
                  <span class="row tienda-cantidad">
                    <button class="btn" type="button" [attr.aria-label]="'Restar ' + l.nombre" [disabled]="enviando" (click)="poner(l.idProducto, l.cantidad - 1)">−</button>
                    <span>{{ l.cantidad }}</span>
                    <button class="btn" type="button" [attr.aria-label]="'Sumar ' + l.nombre" [disabled]="enviando || l.cantidad >= maximo || l.maximo !== null" (click)="poner(l.idProducto, l.cantidad + 1)">+</button>
                  </span>
                  <span class="carrito-importe">{{ precio(l.subTotal) }}</span>
                  <button class="btn btn-danger" type="button" [disabled]="enviando" (click)="poner(l.idProducto, 0)">Sacar</button>
                  @if (l.maximo !== null) {
                    <span class="error carrito-aviso">
                      {{ l.maximo === 0 ? 'No queda stock.' : 'Quedan ' + l.maximo + '.' }}
                      @if (l.maximo > 0) { <button class="btn" type="button" (click)="poner(l.idProducto, l.maximo)">Pedir {{ l.maximo }}</button> }
                    </span>
                  }
                </li>
              }
              <li class="carrito-total"><span>Subtotal</span><span>{{ precio(cotizacion.total) }}</span></li>
              <li class="carrito-total"><strong>Total</strong><strong>{{ precio(cotizacion.total) }}</strong></li>
            </ul>

            <form class="panel pad carrito-datos" (ngSubmit)="confirmar()">
              <h2>Tus datos</h2>
              <div class="field">
                <label for="carrito-nombre">Nombre</label>
                <input id="carrito-nombre" name="nombre" autocomplete="name" required maxlength="80" [(ngModel)]="nombre" />
              </div>
              <div class="field">
                <label for="carrito-telefono">Teléfono (con código de área)</label>
                <input id="carrito-telefono" name="telefono" type="tel" autocomplete="tel" required maxlength="30" [(ngModel)]="telefono" />
              </div>
              <div class="field">
                <label for="carrito-email">Mail</label>
                <input id="carrito-email" name="email" type="email" autocomplete="email" required maxlength="200" [(ngModel)]="email" />
              </div>
              <div class="carrito-cebo" aria-hidden="true">
                <label for="carrito-sitio">Sitio web</label>
                <input id="carrito-sitio" name="sitio" tabindex="-1" autocomplete="off" [(ngModel)]="sitio" />
              </div>
              <p><strong>Retiro en el local.</strong>
                @if (t.direccion) { {{ t.direccion }}. }
                @if (t.horario) { Horario: {{ t.horario }}. }
              </p>
              <p class="meta">El pago se coordina con el local y se paga al retirar.</p>
              @if (error) { <p class="error">{{ error }}</p> }
              <button class="btn btn-charge" type="submit" [disabled]="enviando || !puedeConfirmar">{{ enviando ? 'Enviando…' : 'Confirmar pedido' }}</button>
            </form>
          }
        }
      </div>
    }
  `
})
export class CarritoTiendaComponent implements OnInit, OnDestroy {
  tienda: TiendaPublicaDto | null = null;
  cotizacion: CotizacionTiendaDto | null = null;
  noEncontrada = false;
  cargando = false;
  enviando = false;
  quitados = false;
  error = '';
  nombre = '';
  telefono = '';
  email = '';
  sitio = '';
  alias = '';
  readonly maximo = MAXIMO_POR_PRODUCTO;
  private readonly tituloAnterior = document.title;

  constructor(
    private readonly ruta: ActivatedRoute,
    private readonly router: Router,
    private readonly api: ApiService,
    private readonly carrito: CarritoTiendaService
  ) {}

  get puedeConfirmar(): boolean {
    return !!this.cotizacion && this.cotizacion.lineas.length > 0 && this.cotizacion.lineas.every((l) => l.maximo === null);
  }

  ngOnInit(): void {
    this.alias = this.ruta.snapshot.paramMap.get('alias') ?? '';
    this.api.tienda(this.alias).subscribe({
      next: (t) => {
        this.tienda = t;
        document.title = `Carrito · ${t.nombre}`;
        if (t.vista === 'catalogo') this.cotizar();
      },
      error: (e) => {
        if (e.status === 404) this.noEncontrada = true;
        else this.error = e.status === 429 ? 'Demasiadas consultas. Esperá un minuto.' : 'No se pudo abrir la tienda.';
      }
    });
  }

  ngOnDestroy(): void {
    document.title = this.tituloAnterior;
  }

  poner(idProducto: number, cantidad: number): void {
    this.carrito.poner(this.alias, idProducto, cantidad);
    this.cotizar();
  }

  confirmar(): void {
    if (!this.puedeConfirmar || this.enviando) return;
    this.enviando = true;
    this.error = '';
    this.api.crearPedidoTienda(this.alias, {
      nombre: this.nombre.trim(),
      telefono: this.telefono.trim(),
      email: this.email.trim(),
      items: this.carrito.items(this.alias),
      sitio: this.sitio
    }).subscribe({
      next: (r) => {
        if (!r?.codigo) {
          this.enviando = false;
          return;
        }
        this.carrito.vaciar(this.alias);
        this.router.navigate(['/tienda', this.alias, 'pedido', r.codigo]);
      },
      error: (e) => {
        this.enviando = false;
        this.error = this.mensajeError(e);
        this.cotizar();
      }
    });
  }

  precio(n: number): string {
    return `$ ${Number(n).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }

  private cotizar(): void {
    const items = this.carrito.items(this.alias);
    if (items.length === 0) {
      this.cotizacion = null;
      return;
    }
    this.cargando = true;
    this.api.cotizarCarritoTienda(this.alias, items).subscribe({
      next: (c) => {
        this.cargando = false;
        this.quitados = c.noDisponibles.length > 0;
        c.noDisponibles.forEach((id) => this.carrito.sacar(this.alias, id));
        this.cotizacion = c;
      },
      error: (e) => {
        this.cargando = false;
        this.error = this.mensajeError(e);
      }
    });
  }

  private mensajeError(e: { status: number; error: unknown }): string {
    if (e.status === 429) return 'Demasiados intentos. Esperá unos minutos y probá de nuevo.';
    if (e.status === 400 && typeof e.error === 'string') return e.error;
    return 'No se pudo completar. Probá de nuevo.';
  }
}
