import { Component, HostListener } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { DialogoService } from './servicios/dialogo.service';
import { TemaService } from './servicios/tema.service';

@Component({
  selector: 'sc-root',
  standalone: true,
  imports: [RouterOutlet],
  template: `
    <router-outlet />
    @if (dialogo.visible()) {
      <div class="dialogo-fondo">
        <div class="dialogo" role="dialog" aria-modal="true">
          <p>{{ dialogo.mensaje }}</p>
          @if (dialogo.conCampo) {
            <input class="wide" [type]="dialogo.oculto ? 'password' : 'text'" [value]="dialogo.valor" (input)="dialogo.escribir($event)" (keydown.enter)="dialogo.aceptar()" autofocus />
          }
          <div class="row dialogo-acciones">
            <button class="btn" type="button" (click)="dialogo.cancelar()">{{ dialogo.textoCancelar }}</button>
            <button class="btn btn-primary" type="button" (click)="dialogo.aceptar()" [attr.autofocus]="dialogo.conCampo ? null : ''">{{ dialogo.textoAceptar }}</button>
          </div>
        </div>
      </div>
    }
  `
})
export class AppComponent {
  constructor(tema: TemaService, readonly dialogo: DialogoService) {
    tema.aplicar();
  }

  @HostListener('document:keydown.escape')
  cancelar(): void {
    if (this.dialogo.visible()) this.dialogo.cancelar();
  }
}
