import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, ComprobanteFiscalDto, ConfiguracionFiscalDto } from '../servicios/api.service';

@Component({
  selector: 'sc-facturacion',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="topbar">
      <h1>Facturación electrónica (ARCA)</h1>
    </div>
    @if (error) { <p class="error">{{ error }}</p> }
    @if (mensaje) { <p class="meta">{{ mensaje }}</p> }
    @if (config) {
      <section class="panel" style="padding:12px;margin-bottom:16px">
        <h2 style="font-size:16px;margin:0 0 8px">Datos del local</h2>
        <p class="meta">Monotributo · Factura C a consumidor final.</p>
        @if (!config.arcaDisponible) {
          <p class="error">Este servidor todavía no tiene el certificado de StockControl cargado: se pueden guardar los datos, pero no emitir.</p>
        }
        <div class="toolbar">
          <label>CUIT <input type="text" inputmode="numeric" maxlength="13" placeholder="20-12345678-9" [(ngModel)]="config.cuit" /></label>
          <label>Punto de venta <input type="number" min="1" max="99999" style="width:96px" [(ngModel)]="config.puntoVenta" /></label>
        </div>
        <div class="toolbar">
          <label><input type="checkbox" [(ngModel)]="config.activa" /> Facturar con ARCA en este local</label>
          <label><input type="checkbox" [(ngModel)]="config.facturaGlobalPorDefecto" /> Proponer una factura global por caja</label>
        </div>
        <p class="meta">Tope por factura a consumidor final: {{ dinero(config.tope) }} · Ambiente: {{ config.ambiente }}</p>
        <button class="btn btn-primary" type="button" (click)="guardar()">Guardar</button>
      </section>

      <section class="panel" style="padding:12px;margin-bottom:16px">
        <h2 style="font-size:16px;margin:0 0 8px">Cómo delegar la facturación a StockControl</h2>
        <ol class="meta" style="margin:0;padding-left:20px">
          <li>Entrá a ARCA con tu CUIT y clave fiscal.</li>
          <li>Abrí «Administrador de Relaciones de Clave Fiscal» y elegí «Nueva relación».</li>
          <li>Servicio: ARCA › WebService › Facturación Electrónica.</li>
          <li>Representante: el CUIT de StockControl (te lo pasamos al activar el servicio).</li>
          <li>Confirmá. Después cargá acá tu CUIT y el punto de venta (tipo «Web Services»).</li>
        </ol>
      </section>

      <section class="panel">
        <div class="toolbar" style="padding:12px"><strong style="font-size:14px">Comprobantes</strong></div>
        @if (comprobantes.length === 0) {
          <p class="meta" style="padding:0 12px 12px">Todavía no hay comprobantes.</p>
        } @else {
          <table class="data">
            <thead>
              <tr><th>Fecha</th><th>Tipo</th><th>Número</th><th class="num">Importe</th><th>CAE</th><th>Estado</th><th></th></tr>
            </thead>
            <tbody>
              @for (c of comprobantes; track c.id) {
                <tr>
                  <td>{{ fecha(c.fecha) }}</td>
                  <td>{{ c.tipo === 'NotaCreditoC' ? 'Nota de crédito C' : 'Factura C' }}</td>
                  <td>{{ c.numero > 0 ? c.puntoVenta + '-' + c.numero : '—' }}</td>
                  <td class="num">{{ dinero(c.importe) }}</td>
                  <td>{{ c.cae || '—' }}</td>
                  <td>{{ c.estado }}{{ c.observacion ? ' · ' + c.observacion : '' }}</td>
                  <td>
                    @if (c.estado === 'Pendiente') {
                      <button class="btn" type="button" (click)="reintentar(c)">Reintentar</button>
                    }
                  </td>
                </tr>
              }
            </tbody>
          </table>
        }
      </section>
    }
  `
})
export class FacturacionComponent implements OnInit {
  config: ConfiguracionFiscalDto | null = null;
  comprobantes: ComprobanteFiscalDto[] = [];
  error = '';
  mensaje = '';

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.api.fiscalConfiguracion().subscribe({
      next: (c) => (this.config = c),
      error: () => (this.error = 'No se pudo cargar la configuración fiscal.')
    });
    this.cargarComprobantes();
  }

  guardar(): void {
    if (!this.config) return;
    this.error = '';
    this.mensaje = '';
    this.api.guardarFiscalConfiguracion(this.config).subscribe({
      next: () => (this.mensaje = 'Datos fiscales guardados.'),
      error: (err) => (this.error = typeof err.error === 'string' ? err.error : 'No se pudo guardar.')
    });
  }

  reintentar(c: ComprobanteFiscalDto): void {
    this.error = '';
    this.api.reintentarComprobante(c.id).subscribe({
      next: () => this.cargarComprobantes(),
      error: (err) => (this.error = typeof err.error === 'string' ? err.error : 'No se pudo reintentar.')
    });
  }

  dinero(n: number): string {
    return `$ ${Number(n).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }

  fecha(iso: string): string {
    return new Date(iso).toLocaleDateString('es-AR');
  }

  private cargarComprobantes(): void {
    this.api.fiscalComprobantes().subscribe({
      next: (r) => (this.comprobantes = r.items),
      error: () => (this.comprobantes = [])
    });
  }
}
