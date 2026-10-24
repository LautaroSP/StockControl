import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, OrdenCompraDto } from '../servicios/api.service';
import { DialogoService } from '../servicios/dialogo.service';

interface FilaRecepcion {
  idLinea: number;
  nombre: string;
  unidadesPack: number;
  pedida: number;
  recibida: number | null;
}

@Component({
  selector: 'sc-recibir-orden',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="modal-back show">
      <div class="modal" style="max-width:560px">
        <h2 style="margin:0 0 8px;font-size:18px">Orden #{{ idOrden }} · {{ nombreProveedor }}</h2>
        @if (orden) {
          @if (orden.recibida) {
            <p class="meta" style="margin:0 0 8px">
              Recibida el {{ fecha(orden.fechaRecepcion!) }} por {{ orden.nombreUsuarioRecepcion }}
            </p>
          } @else {
            <p class="meta" style="margin:0 0 8px">Ajustá lo que llegó. Se suma al stock de cada producto.</p>
          }
          <div class="scroll" style="max-height:50vh;overflow:auto">
            <table class="data">
              <thead><tr><th>Producto</th><th class="num">Pedida</th><th class="num">Recibida</th></tr></thead>
              <tbody>
                @for (f of filas; track f.idLinea) {
                  <tr>
                    <td>
                      {{ f.nombre }}
                      @if (f.unidadesPack > 1) { <div class="meta">{{ f.unidadesPack }} unidades cada uno · suma {{ (f.recibida || 0) * f.unidadesPack }} al stock</div> }
                    </td>
                    <td class="num">{{ f.pedida }}</td>
                    <td class="num">
                      @if (orden.recibida) {
                        {{ f.recibida }}
                      } @else {
                        <input type="number" min="0" style="width:90px" [(ngModel)]="f.recibida" />
                      }
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }
        @if (error) {
          <p class="error">{{ error }}</p>
        }
        <div class="row" style="margin-top:12px">
          @if (orden && !orden.recibida) {
            <button class="btn btn-primary grow" type="button" [disabled]="guardando" (click)="confirmar()">
              {{ guardando ? 'Recibiendo...' : 'Confirmar recepción' }}
            </button>
          }
          <button class="btn" type="button" (click)="cerrar.emit()">Cerrar</button>
        </div>
      </div>
    </div>
  `
})
export class RecibirOrdenComponent implements OnInit {
  @Input({ required: true }) idOrden = 0;
  @Output() recibida = new EventEmitter<string>();
  @Output() cerrar = new EventEmitter<void>();

  orden: OrdenCompraDto | null = null;
  nombreProveedor = '';
  filas: FilaRecepcion[] = [];
  guardando = false;
  error = '';

  constructor(private readonly api: ApiService, private readonly dialogo: DialogoService) {}

  ngOnInit(): void {
    this.api.ordenCompra(this.idOrden).subscribe({
      next: (r) => {
        this.orden = r.orden;
        this.nombreProveedor = r.nombreProveedor;
        this.filas = r.orden.lineas.map((l) => ({
          idLinea: l.id ?? 0,
          nombre: l.descripcion || l.nombre,
          unidadesPack: l.unidadesPack,
          pedida: l.cantidad,
          recibida: l.cantidadRecibida ?? l.cantidad
        }));
      },
      error: () => (this.error = 'No se pudo cargar la orden.')
    });
  }

  async confirmar(): Promise<void> {
    if (this.filas.some((f) => f.recibida === null || !(Number(f.recibida) >= 0))) {
      this.error = 'La cantidad recibida no puede ser negativa ni quedar vacía.';
      return;
    }
    if (!await this.dialogo.confirmar('Se suma al stock lo recibido y la orden queda cerrada. ¿Confirmar?')) return;
    this.guardando = true;
    this.error = '';
    const lineas = this.filas.map((f) => ({ idLinea: f.idLinea, cantidad: Number(f.recibida) }));
    this.api.recibirOrden(this.idOrden, lineas).subscribe({
      next: (r) => {
        this.guardando = false;
        let mensaje = `Orden #${this.idOrden} recibida: stock sumado a ${r.productosActualizados} productos.`;
        if (r.lineasSinProducto) mensaje += ` ${r.lineasSinProducto} líneas sin producto.`;
        this.recibida.emit(mensaje);
      },
      error: (e) => {
        this.guardando = false;
        this.error = typeof e.error === 'string' ? e.error : 'No se pudo recibir la orden.';
      }
    });
  }

  fecha(iso: string): string {
    return new Date(iso).toLocaleString('es-AR', { dateStyle: 'short', timeStyle: 'short' });
  }
}
