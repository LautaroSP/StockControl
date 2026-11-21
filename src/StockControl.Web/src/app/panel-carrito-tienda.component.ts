import { Component, EventEmitter, Input, Output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CotizacionTiendaDto } from './servicios/api.service';
import { MAXIMO_POR_PRODUCTO } from './servicios/carrito-tienda.service';
import { IconoComponent } from './icono.component';

/** Panel lateral del carrito en catálogo y ficha: siempre visible, se pliega con el chevron. */
@Component({
  selector: 'sc-panel-carrito-tienda',
  standalone: true,
  imports: [RouterLink, IconoComponent],
  template: `
    <aside class="carrito-panel" [class.carrito-panel-plegado]="!abierto" aria-label="Carrito">
      <header class="carrito-panel-cabecera">
        @if (abierto) {
          <h2><sc-icono nombre="carrito" /> Tu carrito</h2>
        } @else {
          <span class="carrito-panel-resumen" aria-hidden="true">
            <sc-icono nombre="carrito" />
            @if (unidades > 0) { <span class="carrito-panel-cuenta">{{ unidades }}</span> }
          </span>
        }
        <button
          class="btn carrito-panel-toggle"
          type="button"
          [attr.aria-expanded]="abierto"
          [attr.aria-label]="abierto ? 'Ocultar carrito' : 'Mostrar carrito'"
          (click)="alternar()"
        >
          <sc-icono [nombre]="abierto ? 'chevron-derecha' : 'chevron-izquierda'" />
        </button>
      </header>
      @if (abierto) {
        <div class="carrito-panel-lista">
          @if (!cotizacion || cotizacion.lineas.length === 0) {
            <p class="meta">{{ unidades > 0 ? 'Cargando…' : 'El carrito está vacío.' }}</p>
          } @else {
            <ul>
              @for (l of cotizacion.lineas; track l.idProducto) {
                <li>
                  <span class="carrito-nombre">{{ l.nombre }}</span>
                  <span class="row tienda-cantidad">
                    <button class="btn" type="button" [attr.aria-label]="'Restar ' + l.nombre" (click)="cambiar.emit({ idProducto: l.idProducto, cantidad: l.cantidad - 1 })">−</button>
                    <span>{{ l.cantidad }}</span>
                    <button class="btn" type="button" [attr.aria-label]="'Sumar ' + l.nombre" [disabled]="l.cantidad >= maximo || l.maximo !== null" (click)="cambiar.emit({ idProducto: l.idProducto, cantidad: l.cantidad + 1 })">+</button>
                  </span>
                  <span class="carrito-importe">
                    @if (l.ahorro > 0) { <s class="precio-tachado">{{ precio(l.precioLista * l.cantidad) }}</s> }
                    {{ precio(l.subTotal) }}
                  </span>
                </li>
              }
            </ul>
          }
        </div>
        <footer class="carrito-panel-pie">
          <span>Total <strong>{{ precio(total) }}</strong></span>
          @if (total > 0) {
            <a class="btn btn-primary" [routerLink]="['/tienda', alias, 'carrito']">Pedir</a>
          } @else {
            <button class="btn btn-primary" type="button" disabled>Pedir</button>
          }
        </footer>
      }
    </aside>
  `
})
export class PanelCarritoTiendaComponent {
  @Input({ required: true }) alias = '';
  @Input() cotizacion: CotizacionTiendaDto | null = null;
  @Input() unidades = 0;
  @Input() total = 0;
  @Input() abierto = true;
  @Output() readonly abiertoChange = new EventEmitter<boolean>();
  @Output() readonly cambiar = new EventEmitter<{ idProducto: number; cantidad: number }>();

  readonly maximo = MAXIMO_POR_PRODUCTO;

  alternar(): void {
    this.abiertoChange.emit(!this.abierto);
  }

  precio(n: number): string {
    return `$ ${Number(n).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }
}
