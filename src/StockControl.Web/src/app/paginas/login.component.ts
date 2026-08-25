import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../servicios/api.service';
import { SesionService } from '../servicios/sesion.service';
import { TemaService } from '../servicios/tema.service';

@Component({
  selector: 'sc-login',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="auth">
      <div class="panel auth-card">
        <div class="brand" style="border:0;padding:0 0 8px">
          <strong>StockControl</strong>
          <span>Argentina · $ 9.999 / mes por local</span>
        </div>
        <h1>Entrar</h1>
        <p class="meta">Usuario y contraseña del local.</p>
        <form (ngSubmit)="entrar()">
          <label class="field">
            Usuario
            <input class="wide" name="usuario" [(ngModel)]="usuario" autocomplete="username" />
          </label>
          <label class="field">
            Clave
            <input class="wide" type="password" name="clave" [(ngModel)]="clave" autocomplete="current-password" />
          </label>
          @if (error) {
            <p class="error">{{ error }}</p>
          }
          <button class="btn btn-primary wide" type="submit" [disabled]="cargando">Entrar</button>
        </form>
        <p class="meta" style="margin-top:16px">
          <button class="btn" type="button" (click)="tema.alternar()">
            {{ tema.tema() === 'oscuro' ? 'Modo claro' : 'Modo oscuro' }}
          </button>
        </p>
      </div>
    </div>
  `
})
export class LoginComponent {
  usuario = '';
  clave = '';
  error = '';
  cargando = false;

  constructor(
    private readonly api: ApiService,
    private readonly sesion: SesionService,
    private readonly router: Router,
    readonly tema: TemaService
  ) {
    if (sesion.hayToken())
      router.navigateByUrl(sesion.idLocal() ? '/caja' : '/locales');
  }

  entrar(): void {
    this.error = '';
    this.cargando = true;
    this.api.login(this.usuario.trim(), this.clave).subscribe({
      next: (r) => {
        this.sesion.guardarLogin(r.token, r.rol, r.nombre);
        this.router.navigateByUrl('/locales');
      },
      error: (err) => {
        this.error = err.status === 0
          ? 'No se pudo conectar con la API. ¿Está corriendo en el puerto 5263?'
          : 'Usuario o clave incorrectos.';
        this.cargando = false;
      }
    });
  }
}
