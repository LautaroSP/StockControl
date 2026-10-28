import { Component, Input } from '@angular/core';
import { TicketDto } from '../servicios/api.service';

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
    <header class="ticket-cabecera">
      <h1>{{ nombreLocal }}</h1>
      <p>{{ fechaHora() }}@if (ticket.nroCaja) { · Caja {{ ticket.nroCaja }} }</p>
    </header>
    <div class="ticket-sep"></div>
    @for (l of ticket.lineas; track $index) {
      <div class="ticket-linea">
        <div class="ticket-nombre">{{ l.nombre }}</div>
        <div class="ticket-fila ticket-detalle">
          <span>{{ cantidad(l.cantidad) }} × {{ dinero(l.precio) }}</span>
          <span class="ticket-importe">{{ dinero(l.subTotal) }}</span>
        </div>
        @if (l.ahorro) {
          <div class="ticket-fila ticket-ahorro">
            <span>{{ l.ahorro.descripcion }}</span>
            <span class="ticket-importe">−{{ dinero(l.ahorro.importe) }}</span>
          </div>
        }
        @if (l.nota) {
          <div class="ticket-ahorro">Oferta {{ l.nota }}</div>
        }
      </div>
    }
    @if (ticket.ahorros.length) {
      <div class="ticket-sep"></div>
      @for (a of ticket.ahorros; track $index) {
        <div class="ticket-fila ticket-ahorro">
          <span>{{ a.descripcion }}</span>
          <span class="ticket-importe">−{{ dinero(a.importe) }}</span>
        </div>
      }
    }
    <div class="ticket-sep"></div>
    <div class="ticket-fila ticket-total">
      <span>TOTAL</span>
      <span class="ticket-importe">{{ dinero(ticket.total) }}</span>
    </div>
    <div class="ticket-sep"></div>
    @for (m of ticket.medios; track $index) {
      <div class="ticket-fila">
        <span>{{ m.descripcion }}</span>
        <span class="ticket-importe">{{ dinero(m.importe) }}</span>
      </div>
    }
    @if (pendiente !== null) {
      <div class="ticket-fila ticket-pendiente">
        <span>Pendiente de pago</span>
        <span class="ticket-importe">{{ dinero(pendiente) }}</span>
      </div>
    }
    <footer class="ticket-pie">
      <p>{{ cantidad(ticket.items) }} {{ ticket.items === 1 ? 'ítem' : 'ítems' }}</p>
      <p>¡Gracias por su compra!</p>
      <p class="ticket-legal">Ticket no válido como factura</p>
    </footer>
  `
})
export class TicketComponent {
  @Input({ required: true }) ticket!: TicketDto;
  @Input() nombreLocal = 'Local';
  @Input() formato = 'POS-80';
  @Input() pendiente: number | null = null;

  dinero(n: number): string {
    return `$ ${Number(n).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }

  cantidad(n: number): string {
    return Number(n).toLocaleString('es-AR', { maximumFractionDigits: 3 });
  }

  fechaHora(): string {
    const f = new Date(this.ticket.fecha);
    return `${f.toLocaleDateString('es-AR')} ${f.toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' })}`;
  }
}
