import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../servicios/api.service';
import { TemaComponent } from '../tema.component';

@Component({
  selector: 'sc-recuperar',
  standalone: true,
  imports: [FormsModule, RouterLink, TemaComponent],
  template: `
    <sc-tema />
    <div class="auth" style="padding-top:72px">
      <div class="panel auth-card">
        <div class="brand" style="border:0;padding:0 0 8px">
          <strong>StockControl</strong>
          <span>Recuperar la contraseña</span>
        </div>
        @if (cambiada) {
          <h1>Listo</h1>
          <p>Ya podés entrar con la contraseña nueva. Las sesiones abiertas se cerraron.</p>
          <a class="btn btn-primary wide" routerLink="/login">Entrar</a>
        } @else if (token) {
          <h1>Contraseña nueva</h1>
          <form (ngSubmit)="guardar()">
            <label class="field">Contraseña nueva (mínimo 8)
              <input class="wide" type="password" name="clave" minlength="8" autocomplete="new-password" [(ngModel)]="clave" />
            </label>
            <label class="field">Repetila
              <input class="wide" type="password" name="clave2" autocomplete="new-password" [(ngModel)]="clave2" />
            </label>
            @if (error) { <p class="error">{{ error }}</p> }
            <button class="btn btn-primary wide" type="submit" [disabled]="enviando">Guardar</button>
          </form>
        } @else {
          <h1>¿Olvidaste tu contraseña?</h1>
          <p class="meta">Para el dueño y el socio. Si sos empleado, pedile al dueño que te la cambie desde Usuarios.</p>
          @if (aviso) {
            <p>{{ aviso }}</p>
          } @else {
            <form (ngSubmit)="pedir()">
              <label class="field">Mail o usuario
                <input class="wide" type="text" name="identificador" maxlength="200" autocomplete="username" [(ngModel)]="identificador" />
              </label>
              @if (error) { <p class="error">{{ error }}</p> }
              <button class="btn btn-primary wide" type="submit" [disabled]="enviando">{{ enviando ? 'Enviando...' : 'Mandame el link' }}</button>
            </form>
          }
        }
        <p class="meta" style="margin-top:12px"><a routerLink="/login">Volver a entrar</a> · <a routerLink="/">Inicio</a></p>
      </div>
    </div>
  `
})
export class RecuperarComponent implements OnInit {
  token = '';
  identificador = '';
  clave = '';
  clave2 = '';
  aviso = '';
  error = '';
  enviando = false;
  cambiada = false;

  constructor(
    private readonly ruta: ActivatedRoute,
    private readonly api: ApiService
  ) {}

  ngOnInit(): void {
    document.title = 'StockControl — recuperar la contraseña';
    this.token = this.ruta.snapshot.queryParamMap.get('token') ?? '';
  }

  pedir(): void {
    if (!this.identificador.trim()) {
      this.error = 'Escribí tu mail o tu usuario.';
      return;
    }
    this.enviando = true;
    this.error = '';
    this.api.recuperarClave(this.identificador.trim()).subscribe({
      next: (r) => {
        this.enviando = false;
        this.aviso = r.mensaje;
      },
      error: (e) => {
        this.enviando = false;
        this.error = e.status === 429
          ? 'Demasiados pedidos. Probá de nuevo en unos minutos.'
          : typeof e.error === 'string' ? e.error : 'No se pudo mandar el pedido.';
      }
    });
  }

  guardar(): void {
    if (this.clave.length < 8) {
      this.error = 'La contraseña debe tener al menos 8 caracteres.';
      return;
    }
    if (this.clave !== this.clave2) {
      this.error = 'Las dos contraseñas no coinciden.';
      return;
    }
    this.enviando = true;
    this.error = '';
    this.api.nuevaClave(this.token, this.clave).subscribe({
      next: () => {
        this.enviando = false;
        this.cambiada = true;
      },
      error: (e) => {
        this.enviando = false;
        this.error = e.status === 429
          ? 'Demasiados intentos. Probá de nuevo en unos minutos.'
          : typeof e.error === 'string' ? e.error : 'No se pudo cambiar la contraseña.';
      }
    });
  }
}
