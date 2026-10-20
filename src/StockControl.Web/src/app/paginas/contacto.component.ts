import { Component } from '@angular/core';
import { Location } from '@angular/common';
import { TemaService } from '../servicios/tema.service';

@Component({
  selector: 'sc-contacto',
  standalone: true,
  template: `
    <div class="auth">
      <div class="panel auth-card">
        <div class="brand" style="border:0;padding:0 0 8px">
          <strong>StockControl</strong>
          <span>Contacto</span>
        </div>
        <h1>Contacto</h1>
        <p>Esta pantalla es la misma para todos los locales.</p>
        <p class="meta">Los datos de contacto todavía no están cargados.</p>
        <button class="btn" type="button" (click)="volver()">Volver</button>
        <p class="meta" style="margin-top:16px">
          <button class="btn" type="button" (click)="tema.alternar()">
            {{ tema.tema() === 'oscuro' ? 'Modo claro' : 'Modo oscuro' }}
          </button>
        </p>
      </div>
    </div>
  `
})
export class ContactoComponent {
  constructor(
    private readonly lugar: Location,
    readonly tema: TemaService
  ) {}

  volver(): void {
    this.lugar.back();
  }
}
