import { Component, Input } from '@angular/core';
import { VentaDetalleDto } from '../servicios/api.service';

@Component({
  selector: 'sc-detalle-venta',
  standalone: true,
  template: `
    <table class="data">
      <thead>
        <tr>
          <th>Producto</th>
          <th class="num">Cant × Precio</th>
          @if (verCosto) {
            <th class="num">Costo</th>
          }
          <th class="num">Subt</th>
        </tr>
      </thead>
      <tbody>
        @if (venta.ticket; as t) {
          @for (l of t.lineas; track $index; let i = $index) {
            <tr>
              <td>{{ l.nombre }}@if (l.nota) { <span class="meta"> · {{ l.nota }}</span> }</td>
              <td class="num">{{ l.cantidad }} × {{ dinero(l.precio) }}</td>
              @if (verCosto) {
                <td class="num">{{ costo(i) }}</td>
              }
              <td class="num">{{ dinero(l.subTotal) }}</td>
            </tr>
            @if (l.ahorro) {
              <tr>
                <td class="meta" [attr.colspan]="verCosto ? 3 : 2" style="padding-left:24px">{{ l.ahorro.descripcion }}</td>
                <td class="num">−{{ dinero(l.ahorro.importe) }}</td>
              </tr>
            }
          }
          @for (a of t.ahorros; track $index) {
            <tr>
              <td class="meta" [attr.colspan]="verCosto ? 3 : 2">{{ a.descripcion }}</td>
              <td class="num">−{{ dinero(a.importe) }}</td>
            </tr>
          }
          <tr>
            <td [attr.colspan]="verCosto ? 3 : 2"><strong>Total</strong></td>
            <td class="num"><strong>{{ dinero(t.total) }}</strong></td>
          </tr>
        }
      </tbody>
    </table>
  `
})
export class DetalleVentaComponent {
  @Input({ required: true }) venta!: VentaDetalleDto;
  @Input() verCosto = false;

  costo(indice: number): string {
    const c = this.venta.items[indice]?.costo;
    return c == null ? '—' : this.dinero(c);
  }

  dinero(n: number): string {
    return `$ ${Number(n).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }
}
