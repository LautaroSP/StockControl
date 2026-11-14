import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../servicios/api.service';
import { TemaComponent } from '../tema.component';

@Component({
  selector: 'sc-arrepentimiento',
  standalone: true,
  imports: [FormsModule, RouterLink, TemaComponent],
  template: `
    <sc-tema />
    <div class="auth" style="padding-top:72px">
      <div class="panel auth-card" style="max-width:440px">
        <h1>Arrepentimiento</h1>
        <p>Si contrataste el abono de StockControl en línea y te arrepentís, dejanos el pedido. El reintegro lo hace el servicio. No es la baja de todos los meses: eso está en Abono, adentro de la cuenta.</p>
        @if (codigo) {
          <p>Tu trámite es <strong>{{ codigo }}</strong>. Guardalo.</p>
        } @else {
          <form (ngSubmit)="enviar()">
            <label class="field">Nombre <input class="wide" type="text" name="nombre" maxlength="120" [(ngModel)]="nombre" /></label>
            <label class="field">Mail <input class="wide" type="email" name="correo" maxlength="200" [(ngModel)]="correo" /></label>
            <label class="field">Local <input class="wide" type="text" name="local" maxlength="200" [(ngModel)]="local" /></label>
            <label class="field">Mensaje <textarea class="wide" name="mensaje" rows="4" maxlength="2000" [(ngModel)]="mensaje"></textarea></label>
            <input class="campo-oculto" type="text" name="sitio" [(ngModel)]="sitio" tabindex="-1" autocomplete="off" aria-hidden="true" />
            @if (error) { <p class="error">{{ error }}</p> }
            <button class="btn btn-primary wide" type="submit" [disabled]="enviando">{{ enviando ? 'Enviando...' : 'Pedir el arrepentimiento' }}</button>
          </form>
        }
        <p class="meta"><a routerLink="/terminos">Términos</a> · <a routerLink="/">Inicio</a></p>
      </div>
    </div>
  `
})
export class ArrepentimientoComponent implements OnInit {
  nombre = '';
  correo = '';
  local = '';
  mensaje = '';
  sitio = '';
  codigo = '';
  error = '';
  enviando = false;

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    document.title = 'StockControl — arrepentimiento';
  }

  enviar(): void {
    this.error = '';
    this.enviando = true;
    this.api.pedirArrepentimiento({
      nombre: this.nombre,
      correo: this.correo,
      local: this.local,
      mensaje: this.mensaje,
      sitio: this.sitio
    }).subscribe({
      next: (r) => {
        this.enviando = false;
        this.codigo = r.codigo;
      },
      error: (err) => {
        this.enviando = false;
        this.error = typeof err.error === 'string' ? err.error : 'No se pudo enviar el pedido.';
      }
    });
  }
}
