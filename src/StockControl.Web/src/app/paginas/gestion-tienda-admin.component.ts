import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, GestionTiendaAdminDto } from '../servicios/api.service';
import { DialogoService } from '../servicios/dialogo.service';

/** Admin: gestión de la tienda por local y mes. La cobrada la separó Mercado Pago; la deuda se salda aparte. */
@Component({
  selector: 'sc-gestion-tienda-admin',
  standalone: true,
  imports: [FormsModule],
  template: `
    <section class="panel pad" style="margin-bottom:16px">
      <div class="row">
        <strong class="grow">Gestión de la tienda</strong>
        <label class="field" style="margin:0">
          Mes
          <input type="month" [(ngModel)]="mes" (ngModelChange)="cargar()" />
        </label>
      </div>
      @if (error) { <p class="error">{{ error }}</p> }
      @if (datos; as d) {
        <p class="meta">
          $ {{ pesos(d.importe) }} por pedido pagado fuera de Mercado Pago. Los cancelados con devolución no cuentan.
          Cobrada por Mercado Pago: <strong>$ {{ pesos(d.total) }}</strong> ·
          Pendiente: <strong>$ {{ pesos(d.totalPendiente) }}</strong> ·
          Saldada: <strong>$ {{ pesos(d.totalSaldada) }}</strong>
        </p>
        <div class="tabla-scroll">
          <table class="data">
            <thead>
              <tr>
                <th>Local</th>
                <th class="num">Pedidos MP</th>
                <th class="num">Cobrada</th>
                <th class="num">Fuera de MP</th>
                <th class="num">Pendiente</th>
                <th class="num">Saldada</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              @for (i of d.items; track i.idLocal) {
                <tr>
                  <td>{{ i.nombre }}</td>
                  <td class="num">{{ i.pedidos }}</td>
                  <td class="num">$ {{ pesos(i.gestion) }}</td>
                  <td class="num">{{ i.fueraDeMp }}</td>
                  <td class="num">$ {{ pesos(i.pendiente) }}</td>
                  <td class="num">$ {{ pesos(i.saldada) }}</td>
                  <td>
                    @if (i.pendiente > 0) {
                      <button class="btn" type="button" (click)="saldar(i.idLocal)">Saldar</button>
                    }
                    @if (i.puedeAnularSaldo) {
                      <button class="btn" type="button" (click)="anular(i.idLocal)">Anular saldo</button>
                    }
                  </td>
                </tr>
              } @empty {
                <tr><td colspan="7" class="meta">Sin gestión en este mes.</td></tr>
              }
            </tbody>
          </table>
        </div>
      }
    </section>
  `
})
export class GestionTiendaAdminComponent implements OnInit {
  mes = `${new Date().getFullYear()}-${String(new Date().getMonth() + 1).padStart(2, '0')}`;
  datos: GestionTiendaAdminDto | null = null;
  error = '';

  constructor(
    private readonly api: ApiService,
    private readonly dialogo: DialogoService
  ) {}

  ngOnInit(): void {
    this.cargar();
  }

  cargar(): void {
    if (!/^\d{4}-\d{2}$/.test(this.mes)) return;
    this.api.gestionTiendaAdmin(this.mes).subscribe({
      next: (d) => {
        this.datos = d;
        this.error = '';
      },
      error: (e) => (this.error = typeof e.error === 'string' ? e.error : 'No se pudo cargar la gestión.')
    });
  }

  async saldar(idLocal: number): Promise<void> {
    const clave = await this.dialogo.pedir('Clave para saldar la gestión pendiente de este local.', '', true);
    if (!clave) return;
    this.api.saldarGestionAdmin(idLocal, clave).subscribe({
      next: () => this.cargar(),
      error: (e) => (this.error = typeof e.error === 'string' ? e.error : 'No se pudo saldar la gestión.')
    });
  }

  async anular(idLocal: number): Promise<void> {
    const clave = await this.dialogo.pedir('Clave para anular el último saldo de gestión de este local.', '', true);
    if (!clave) return;
    this.api.anularSaldoGestionAdmin(idLocal, clave).subscribe({
      next: () => this.cargar(),
      error: (e) => (this.error = typeof e.error === 'string' ? e.error : 'No se pudo anular el saldo.')
    });
  }

  pesos(n: number): string {
    return Number(n).toLocaleString('es-AR', { maximumFractionDigits: 2 });
  }
}
