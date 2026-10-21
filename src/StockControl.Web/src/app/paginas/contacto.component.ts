import { Component, OnInit } from '@angular/core';
import { Location } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../servicios/api.service';
import { TemaService } from '../servicios/tema.service';

@Component({
  selector: 'sc-contacto',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="auth">
      <div class="panel auth-card" style="max-width:440px">
        <div class="brand" style="border:0;padding:0 0 8px">
          <strong>StockControl</strong>
          <span>Contacto</span>
        </div>
        <h1>Contacto</h1>
        <p>Esta pantalla es la misma para todos los locales.</p>
        @if (url) {
          <p><a [href]="url">{{ telefono }}</a></p>
        } @else {
          <p class="meta">Todavía no hay un teléfono de ventas.</p>
        }
        @if (email) {
          <p><a [href]="'mailto:' + email">{{ email }}</a></p>
        }
        @if (formularioActivo) {
          <form (ngSubmit)="enviar()">
            <label class="field">Nombre <input class="wide" type="text" name="nombre" maxlength="120" autocomplete="name" [(ngModel)]="nombre" /></label>
            <label class="field">Correo <input class="wide" type="email" name="correo" maxlength="200" autocomplete="email" [(ngModel)]="correo" /></label>
            <label class="field">Mensaje <textarea class="wide" name="mensaje" rows="4" maxlength="2000" [(ngModel)]="mensaje"></textarea></label>
            <input class="campo-oculto" type="text" name="sitio" [(ngModel)]="sitio" tabindex="-1" autocomplete="off" aria-hidden="true" />
            <button class="btn btn-primary" type="submit" [disabled]="enviando">{{ enviando ? 'Enviando...' : 'Enviar' }}</button>
          </form>
          @if (aviso) { <p class="meta">{{ aviso }}</p> }
          @if (error) { <p class="error">{{ error }}</p> }
        } @else {
          <p class="meta">El formulario está desactivado. Escribinos por teléfono o por mail.</p>
        }
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
export class ContactoComponent implements OnInit {
  telefono = '';
  email = '';
  url: string | null = null;
  formularioActivo = true;
  nombre = '';
  correo = '';
  mensaje = '';
  sitio = '';
  enviando = false;
  aviso = '';
  error = '';

  constructor(
    private readonly lugar: Location,
    private readonly api: ApiService,
    readonly tema: TemaService
  ) {}

  ngOnInit(): void {
    this.api.contactoVentas().subscribe({
      next: (r) => {
        this.telefono = r.telefono;
        this.email = r.email;
        this.url = r.url;
        this.formularioActivo = r.formularioActivo;
      }
    });
  }

  enviar(): void {
    if (!this.nombre.trim() || !this.correo.trim() || !this.mensaje.trim()) {
      this.error = 'Completá nombre, correo y mensaje.';
      this.aviso = '';
      return;
    }
    this.enviando = true;
    this.error = '';
    this.api.enviarContacto({
      nombre: this.nombre.trim(),
      correo: this.correo.trim(),
      mensaje: this.mensaje.trim(),
      sitio: this.sitio
    }).subscribe({
      next: () => {
        this.nombre = '';
        this.correo = '';
        this.mensaje = '';
        this.sitio = '';
        this.enviando = false;
        this.aviso = 'Mensaje enviado.';
      },
      error: (e) => {
        this.enviando = false;
        this.aviso = '';
        this.error = typeof e.error === 'string' ? e.error : 'No se pudo enviar el mensaje.';
      }
    });
  }

  volver(): void {
    this.lugar.back();
  }
}
