import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService, CotizacionTiendaDto, LugarEncontradoDto, MedioPagoTienda, TiendaPublicaDto } from '../servicios/api.service';
import { CarritoTiendaService, MAXIMO_POR_PRODUCTO } from '../servicios/carrito-tienda.service';
import { MapaComponent, Ubicacion } from '../mapa.component';
import { TemaComponent } from '../tema.component';
import { NoEncontradaComponent } from './no-encontrada.component';

@Component({
  selector: 'sc-carrito-tienda',
  standalone: true,
  imports: [FormsModule, RouterLink, TemaComponent, NoEncontradaComponent, MapaComponent],
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
              @if (modalidad === 'envio' && cotizacion.envio.disponible) {
                <li class="carrito-total">
                  <span>Envío <span class="meta">{{ origen(cotizacion.envio.origen) }}</span></span>
                  <span>{{ precio(cotizacion.envio.importe) }}</span>
                </li>
              }
              @if (gestion > 0) {
                <li class="carrito-total"><span>Gestión</span><span>{{ precio(gestion) }}</span></li>
              }
              <li class="carrito-total"><strong>Total</strong><strong>{{ precio(total) }}</strong></li>
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
              @if (t.haceEnvios) {
                <fieldset class="carrito-medios">
                  <legend>¿Cómo lo recibís?</legend>
                  <label class="check"><input type="radio" name="entrega" value="retiro" [ngModel]="modalidad" (ngModelChange)="elegirEntrega($event)" /> Retiro en el local</label>
                  <label class="check"><input type="radio" name="entrega" value="envio" [ngModel]="modalidad" (ngModelChange)="elegirEntrega($event)" /> Envío</label>
                </fieldset>
              }
              @if (modalidad === 'envio') {
                <div class="field">
                  <label for="carrito-direccion">Dirección de entrega</label>
                  <input id="carrito-direccion" name="direccion" maxlength="200" [(ngModel)]="direccion" />
                </div>
                <div class="row">
                  <input class="grow" type="search" maxlength="200" placeholder="Calle, número y localidad" aria-label="Buscar dirección"
                         name="busqueda" [(ngModel)]="busqueda" (keydown.enter)="$event.preventDefault(); buscar()" />
                  <button class="btn" type="button" [disabled]="buscando" (click)="buscar()">{{ buscando ? 'Buscando...' : 'Buscar' }}</button>
                </div>
                @if (lugares.length > 0) {
                  <ul class="lista-lugares">
                    @for (l of lugares; track $index) {
                      <li><button class="btn" type="button" (click)="elegir(l)">{{ l.nombre }}</button></li>
                    }
                  </ul>
                }
                @if (punto) {
                  <sc-mapa [latitud]="punto.latitud" [longitud]="punto.longitud" [editable]="true" etiqueta="Dirección de entrega" (movido)="marcar($event)" />
                  <p class="hint">Mové el marcador para ajustar. El envío se vuelve a calcular al soltarlo.</p>
                } @else if (t.latitud !== null && t.longitud !== null) {
                  <button class="btn" type="button" (click)="marcar({ latitud: t.latitud, longitud: t.longitud })">Marcar en el mapa</button>
                }
                @if (!cotizacion.envio.disponible && punto) {
                  <p class="error">{{ cotizacion.envio.motivo }}</p>
                }
                @if (cotizacion.envio.disponible && cotizacion.envio.distanciaKm !== null) {
                  <p class="meta">{{ cotizacion.envio.distanciaKm }} km {{ origen(cotizacion.envio.origen) }}.</p>
                }
              } @else {
                <p><strong>Retiro en el local.</strong>
                  @if (t.direccion) { {{ t.direccion }}. }
                  @if (t.horario) { Horario: {{ t.horario }}. }
                </p>
              }
              @if (cotizacion.mercadoPago || cotizacion.transferencia) {
                <fieldset class="carrito-medios">
                  <legend>¿Cómo pagás?</legend>
                  @if (cotizacion.mercadoPago) {
                    <label class="check"><input type="radio" name="medio" value="mercadopago" [(ngModel)]="medioPago" /> Ahora con Mercado Pago</label>
                  }
                  @if (cotizacion.transferencia) {
                    <label class="check"><input type="radio" name="medio" value="transferencia" [(ngModel)]="medioPago" /> Transferencia bancaria</label>
                  }
                  <label class="check"><input type="radio" name="medio" value="coordinar" [(ngModel)]="medioPago" /> {{ modalidad === 'envio' ? 'Al recibir, a coordinar con el local' : 'Al retirar, a coordinar con el local' }}</label>
                </fieldset>
              }
              @switch (medioPago) {
                @case ('mercadopago') { <p class="meta">Te llevamos a Mercado Pago. Tenés 30 minutos para pagar; si no, el pedido se cancela solo.</p> }
                @case ('transferencia') { <p class="meta">Al confirmar ves el CBU o alias del local. El local confirma cuando le llega la transferencia.</p> }
                @default { <p class="meta">{{ modalidad === 'envio' ? 'El pago se coordina con el local y se paga al recibir.' : 'El pago se coordina con el local y se paga al retirar.' }}</p> }
              }
              @if (error) { <p class="error">{{ error }}</p> }
              <button class="btn btn-charge" type="submit" [disabled]="enviando || !puedeConfirmar">
                {{ enviando ? 'Enviando…' : medioPago === 'mercadopago' ? 'Pagar ' + precio(total) : 'Confirmar pedido' }}
              </button>
            </form>
          }
        }
      </div>
    }
  `,
  styles: `
    .lista-lugares { list-style: none; padding: 0; margin: 8px 0; display: grid; gap: 6px; }
    .lista-lugares .btn { text-align: left; width: 100%; }
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
  modalidad: 'retiro' | 'envio' = 'retiro';
  direccion = '';
  busqueda = '';
  lugares: LugarEncontradoDto[] = [];
  punto: Ubicacion | null = null;
  buscando = false;
  readonly maximo = MAXIMO_POR_PRODUCTO;
  private readonly tituloAnterior = document.title;

  constructor(
    private readonly ruta: ActivatedRoute,
    private readonly router: Router,
    private readonly api: ApiService,
    private readonly carrito: CarritoTiendaService
  ) {}

  medioPago: MedioPagoTienda = 'coordinar';

  get puedeConfirmar(): boolean {
    if (!this.cotizacion || this.cotizacion.lineas.length === 0 || this.cotizacion.lineas.some((l) => l.maximo !== null))
      return false;
    if (this.modalidad === 'envio')
      return !!this.cotizacion.envio?.disponible && this.direccion.trim().length >= 4 && !!this.punto;
    return true;
  }

  get envioImporte(): number {
    return this.modalidad === 'envio' && this.cotizacion?.envio?.disponible ? this.cotizacion.envio.importe : 0;
  }

  get gestion(): number {
    return this.pagoPrevio?.gestion ?? 0;
  }

  get total(): number {
    return (this.pagoPrevio?.total ?? this.cotizacion?.total ?? 0) + this.envioImporte;
  }

  private get pagoPrevio(): { gestion: number; total: number } | null {
    if (this.medioPago === 'mercadopago') return this.cotizacion?.mercadoPago ?? null;
    if (this.medioPago === 'transferencia') return this.cotizacion?.transferencia ?? null;
    return null;
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
      medioPago: this.medioPago,
      sitio: this.sitio,
      modalidad: this.modalidad,
      direccionEnvio: this.modalidad === 'envio' ? this.direccion.trim() : '',
      latitud: this.modalidad === 'envio' ? this.punto?.latitud ?? null : null,
      longitud: this.modalidad === 'envio' ? this.punto?.longitud ?? null : null
    }).subscribe({
      next: (r) => {
        if (!r?.codigo) {
          this.enviando = false;
          return;
        }
        this.carrito.vaciar(this.alias);
        if (r.urlPago) location.assign(r.urlPago);
        else this.router.navigate(['/tienda', this.alias, 'pedido', r.codigo]);
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

  origen(valor: string | null): string {
    return valor === 'recorrido' ? 'por recorrido' : 'en línea recta';
  }

  elegirEntrega(valor: 'retiro' | 'envio'): void {
    this.modalidad = valor;
    this.cotizar();
  }

  buscar(): void {
    const q = this.busqueda.trim();
    if (q.length < 4) {
      this.error = 'Escribí la dirección con calle, número y localidad.';
      return;
    }
    this.buscando = true;
    this.api.buscarUbicacionTienda(this.alias, q).subscribe({
      next: (r) => {
        this.buscando = false;
        this.lugares = r;
        this.error = r.length === 0 ? 'No encontramos esa dirección. Probá con la localidad o marcá en el mapa.' : '';
      },
      error: (e) => {
        this.buscando = false;
        this.error = e.status === 429 ? 'Esperá un segundo entre búsquedas.' : 'No se pudo buscar la dirección.';
      }
    });
  }

  elegir(lugar: LugarEncontradoDto): void {
    this.direccion = lugar.nombre.slice(0, 200);
    this.lugares = [];
    this.marcar(lugar);
  }

  marcar(lugar: Ubicacion): void {
    this.punto = { latitud: lugar.latitud, longitud: lugar.longitud };
    this.cotizar();
  }

  private cotizar(): void {
    const items = this.carrito.items(this.alias);
    if (items.length === 0) {
      this.cotizacion = null;
      return;
    }
    this.cargando = true;
    this.api.cotizarCarritoTienda(this.alias, items, this.modalidad === 'envio' ? this.punto : null).subscribe({
      next: (c) => {
        this.cargando = false;
        this.quitados = c.noDisponibles.length > 0;
        c.noDisponibles.forEach((id) => this.carrito.sacar(this.alias, id));
        this.cotizacion = c;
        if (!this.pagoPrevio) this.medioPago = 'coordinar';
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
