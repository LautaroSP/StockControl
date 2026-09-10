import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService, ConfiguracionLocalDto } from '../servicios/api.service';
import { SesionService } from '../servicios/sesion.service';

@Component({
  selector: 'sc-configuracion',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="topbar">
      <h1>Configuración del local</h1>
    </div>
    @if (error) { <p class="error">{{ error }}</p> }
    <section class="panel config-form">
      <h2>Datos del local</h2>
      <label class="field">Nombre del local <input class="wide" [(ngModel)]="form.nombreLocal" /></label>

      <h2>Precios</h2>
      <div class="row">
        <label class="field grow">Factor de ganancia <input class="wide" type="number" min="0.0001" step="0.01" [(ngModel)]="form.factorGanancia" /></label>
        <label class="field grow">IVA <input class="wide" type="number" min="0" step="0.01" [(ngModel)]="form.iva" /></label>
      </div>
      <p class="hint">El precio se calcula como costo × factor de ganancia × IVA.</p>

      <h2>Stock</h2>
      <label class="check"><input type="checkbox" [(ngModel)]="form.stockRigido" /> No permitir vender sin stock suficiente</label>
      <label class="field">Umbral de stock bajo <input class="wide" type="number" min="0" step="1" [(ngModel)]="form.umbralStockBajo" /></label>

      <h2>Empleados</h2>
      <label class="check"><input type="checkbox" [(ngModel)]="form.empleadoPuedeModificarPrecios" /> El empleado puede modificar precios de lista</label>

      <h2>Tickets</h2>
      <label class="field">Formato
        <select class="wide" [(ngModel)]="form.formatoTicket">
          <option value="POS-58">POS 58 mm</option>
          <option value="POS-80">POS 80 mm</option>
          <option value="A4">A4</option>
        </select>
      </label>
      <label class="check"><input type="checkbox" [(ngModel)]="form.imprimirTicketAlCobrar" /> Imprimir ticket al cobrar por defecto</label>

      <h2>Cajas</h2>
      <label class="field">Cantidad de cajas/puestos <input class="wide" type="number" min="1" max="50" [(ngModel)]="form.cantidadCajas" /></label>

      <div class="row" style="margin-top:16px">
        <button class="btn btn-primary grow" type="button" (click)="guardar()">Guardar configuración</button>
      </div>
      @if (mensaje) { <p class="meta">{{ mensaje }}</p> }
    </section>
  `
})
export class ConfiguracionComponent implements OnInit {
  form: ConfiguracionLocalDto & { recalcularPrecios: boolean } = {
    nombreLocal: '', factorGanancia: 1, iva: 1.21, stockRigido: false, umbralStockBajo: 5,
    empleadoPuedeModificarPrecios: false, formatoTicket: 'POS-80', imprimirTicketAlCobrar: true,
    cantidadCajas: 1, recalcularPrecios: false
  };
  original = { factorGanancia: 1, iva: 1.21 };
  error = '';
  mensaje = '';

  constructor(private readonly api: ApiService, private readonly router: Router, private readonly sesion: SesionService) {}

  ngOnInit(): void {
    if (!this.sesion.puedeConfigurarLocal()) {
      void this.router.navigateByUrl('/caja');
      return;
    }
    this.api.configuracionLocal().subscribe({
      next: (r) => {
        this.form = { ...r, recalcularPrecios: false };
        this.original = { factorGanancia: r.factorGanancia, iva: r.iva };
      },
      error: (e) => (this.error = this.mensajeError(e, 'No se pudo cargar la configuración.'))
    });
  }

  guardar(): void {
    if (!this.form.nombreLocal.trim() || this.form.factorGanancia <= 0 || this.form.iva < 0 || this.form.umbralStockBajo < 0 || this.form.cantidadCajas < 1 || this.form.cantidadCajas > 50) {
      this.error = 'Revisá los valores de configuración.';
      return;
    }
    const cambioPrecios = this.form.factorGanancia !== this.original.factorGanancia || this.form.iva !== this.original.iva;
    const recalcular = cambioPrecios && confirm('Cambiaste el factor o el IVA. ¿Querés actualizar los precios de los productos?');
    this.api.guardarConfiguracionLocal({ ...this.form, recalcularPrecios: recalcular }).subscribe({
      next: (r) => {
        this.original = { factorGanancia: this.form.factorGanancia, iva: this.form.iva };
        this.error = '';
        this.mensaje = r.recalculados > 0 ? `Configuración guardada. Productos actualizados: ${r.recalculados}.` : 'Configuración guardada.';
      },
      error: (e) => (this.error = this.mensajeError(e, 'No se pudo guardar la configuración.'))
    });
  }

  private mensajeError(error: { error?: unknown }, defecto: string): string {
    return typeof error.error === 'string' ? error.error : defecto;
  }
}
