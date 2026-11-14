import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'sc-pie-tienda',
  standalone: true,
  imports: [RouterLink],
  template: `
    <footer class="tienda-pie">
      <p>
        <strong>{{ nombre }}</strong>
        @if (direccion) { · {{ direccion }} }
        @if (horario) { · {{ horario }} }
      </p>
      <p>Quien vende es este local, no StockControl.</p>
      <p>
        <a [routerLink]="['/tienda', alias, 'terminos']">Términos de la tienda</a>
        · <a [routerLink]="['/tienda', alias, 'arrepentimiento']">Arrepentimiento</a>
      </p>
    </footer>
  `
})
export class PieTiendaComponent {
  @Input({ required: true }) alias = '';
  @Input() nombre = '';
  @Input() direccion = '';
  @Input() horario = '';
}
