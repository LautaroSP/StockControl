import { Component, Input } from '@angular/core';
import { VentaDetalleDto } from '../servicios/api.service';

@Component({
  selector: 'sc-ticket',
  standalone: true,
  host: {
    class: 'ticket-print',
    '[class.ticket-pos58]': "formato === 'POS-58'",
    '[class.ticket-pos80]': "formato !== 'POS-58' && formato !== 'A4'",
    '[class.ticket-a4]': "formato === 'A4'"
  },
  template: `
    <h1>{{ nombreLocal }}</h1>
    <p>{{ fechaHora() }}</p>
    @for (i of venta.items; track i.idInformeVentaDetalle) {
      <div class="ticket-linea">
        <div>{{ i.codigo }} {{ i.nombre }}</div>
        <div class="ticket-fila">
          <span>{{ i.cantidad }} × {{ dinero(i.precio) }}</span>
          <span>{{ dinero(i.subTotal) }}</span>
        </div>
      </div>
    }
    <p class="ticket-total">Total {{ dinero(venta.total) }}</p>
    @if (venta.descuento > 0) {
      <p>Descuento {{ venta.descuento }}%</p>
    }
    @if (alCosto()) {
      <p>Cobrado al costo</p>
    }
    @for (m of medios(); track $index) {
      <p>{{ m }}</p>
    }
  `
})
export class TicketComponent {
  @Input({ required: true }) venta!: VentaDetalleDto;
  @Input() nombreLocal = 'Local';
  @Input() formato = 'POS-80';

  dinero(n: number): string {
    return `$ ${Number(n).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }

  fechaHora(): string {
    const f = new Date(this.venta.fecha);
    return `${f.toLocaleDateString('es-AR')} ${f.toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' })}`;
  }

  alCosto(): boolean {
    return (this.venta.precioCosto || '').toUpperCase() === 'SI';
  }

  medios(): string[] {
    if (this.venta.pagos?.length) {
      return this.venta.pagos.map((p) => `${p.descripcionMetodoPago} ${this.dinero(p.importe)}`);
    }
    return this.venta.metodoPago ? [this.venta.metodoPago] : [];
  }
}
