import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService, MetodoPagoDto } from '../servicios/api.service';
import { DialogoService } from '../servicios/dialogo.service';
import { SesionService } from '../servicios/sesion.service';

@Component({
  selector: 'sc-medios-pago',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="topbar">
      <h1>Medios de pago</h1>
      <button class="btn btn-primary" type="button" (click)="crear()">Nuevo medio</button>
    </div>
    @if (error) { <p class="error">{{ error }}</p> }
    <section class="panel">
      <table class="data">
        <thead><tr><th>Descripción</th><th>Cobro</th><th>Estado</th><th></th></tr></thead>
        <tbody>
          @for (medio of medios; track medio.id) {
            <tr>
              <td>{{ medio.descripcion }}</td>
              <td>
                <select [ngModel]="medio.integracion || 'manual'" (ngModelChange)="integracion(medio, $event)">
                  <option value="manual">Manual</option>
                  <option value="qr_impreso">QR impreso</option>
                  <option value="qr_pantalla">QR en pantalla</option>
                  <option value="point">Point</option>
                </select>
              </td>
              <td>{{ medio.activo ? 'Activo' : 'Inactivo' }}</td>
              <td class="num">
                @if (medio.activo) {
                  <button class="btn" type="button" (click)="editar(medio)">Editar</button>
                  <button class="btn btn-danger" type="button" (click)="desactivar(medio)">Desactivar</button>
                } @else {
                  <button class="btn" type="button" (click)="activar(medio)">Activar</button>
                }
              </td>
            </tr>
          }
        </tbody>
      </table>
    </section>
  `
})
export class MediosPagoComponent implements OnInit {
  medios: MetodoPagoDto[] = [];
  error = '';

  constructor(private readonly api: ApiService, private readonly router: Router, private readonly sesion: SesionService, private readonly dialogo: DialogoService) {}

  ngOnInit(): void {
    if (!this.sesion.esDueno()) {
      void this.router.navigateByUrl('/caja');
      return;
    }
    this.cargar();
  }

  cargar(): void {
    this.api.mediosPago().subscribe({ next: (r) => (this.medios = r), error: () => (this.error = 'No se pudieron cargar los medios.') });
  }

  async crear(): Promise<void> {
    const descripcion = (await this.dialogo.pedir('Descripción del medio de pago', 'Transferencia'))?.trim();
    if (!descripcion) return;
    this.api.crearMedioPago(descripcion, 'manual').subscribe({ next: () => this.cargar(), error: (e) => (this.error = this.mensaje(e)) });
  }

  async editar(medio: MetodoPagoDto): Promise<void> {
    const descripcion = (await this.dialogo.pedir('Descripción del medio de pago', medio.descripcion))?.trim();
    if (!descripcion || descripcion === medio.descripcion) return;
    this.api.editarMedioPago(medio.id, descripcion, medio.integracion || 'manual').subscribe({ next: () => this.cargar(), error: (e) => (this.error = this.mensaje(e)) });
  }

  async desactivar(medio: MetodoPagoDto): Promise<void> {
    if (!await this.dialogo.confirmar(`¿Desactivar ${medio.descripcion}?`)) return;
    this.api.desactivarMedioPago(medio.id).subscribe({ next: () => this.cargar(), error: (e) => (this.error = this.mensaje(e)) });
  }

  integracion(medio: MetodoPagoDto, integracion: string): void {
    if (integracion === medio.integracion) return;
    this.api.editarMedioPago(medio.id, medio.descripcion, integracion).subscribe({
      next: () => this.cargar(),
      error: (e) => (this.error = this.mensaje(e))
    });
  }

  activar(medio: MetodoPagoDto): void {
    this.api.activarMedioPago(medio.id).subscribe({ next: () => this.cargar(), error: (e) => (this.error = this.mensaje(e)) });
  }

  private mensaje(error: { error?: unknown }): string {
    return typeof error.error === 'string' ? error.error : 'No se pudo guardar el medio.';
  }
}
