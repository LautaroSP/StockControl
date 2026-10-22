import { Component, NgZone, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ApiService, RegistroPedidoDto } from '../servicios/api.service';
import { SesionService } from '../servicios/sesion.service';

@Component({
  selector: 'sc-registro',
  standalone: true,
  imports: [FormsModule, RouterLink],
  template: `
    <div class="auth">
      <div class="panel auth-card">
        <div class="brand" style="border:0;padding:0 0 8px">
          <strong>StockControl</strong>
          <span>Argentina · $ 14.999 / mes por local</span>
        </div>
        <h1>Crear cuenta</h1>
        <p class="meta">Sos el dueño: después sumás empleados desde Usuarios.</p>
        <form (ngSubmit)="registrar()">
          <label class="field">
            Tu nombre
            <input class="wide" type="text" name="nombre" [(ngModel)]="pedido.nombre" maxlength="120" autocomplete="name" />
          </label>
          <label class="field">
            Mail
            <input class="wide" type="email" name="email" [(ngModel)]="pedido.email" maxlength="200" autocomplete="email" />
          </label>
          <label class="field">
            Usuario
            <input class="wide" type="text" name="usuario" [(ngModel)]="pedido.nombreUsuario" maxlength="80" autocomplete="username" />
          </label>
          <label class="field">
            Contraseña (mínimo 8)
            <input class="wide" type="password" name="clave" [(ngModel)]="pedido.clave" maxlength="200" autocomplete="new-password" />
          </label>
          <label class="field">
            Repetir contraseña
            <input class="wide" type="password" name="clave2" [(ngModel)]="clave2" maxlength="200" autocomplete="new-password" />
          </label>
          <label class="field">
            Nombre del local
            <input class="wide" type="text" name="local" [(ngModel)]="pedido.nombreLocal" maxlength="200" />
          </label>
          <input class="campo-oculto" type="text" name="sitio" [(ngModel)]="pedido.sitio" tabindex="-1" autocomplete="off" aria-hidden="true" />
          <div class="row" style="margin:8px 0 12px">
            <label class="row" style="gap:6px">
              <input type="radio" name="modalidad" value="prueba" [(ngModel)]="pedido.modalidad" />
              Probar 7 días gratis
            </label>
            <label class="row" style="gap:6px">
              <input type="radio" name="modalidad" value="pagar" [(ngModel)]="pedido.modalidad" />
              Pagar ahora $ 14.999
            </label>
          </div>
          @if (captchaActivo) {
            <div id="captcha-registro" class="captcha"></div>
          }
          @if (aviso) {
            <p class="meta">{{ aviso }}</p>
          }
          @if (error) {
            <p class="error">{{ error }}</p>
          }
          <button class="btn btn-primary wide" type="submit" [disabled]="cargando || (captchaActivo && !pedido.captcha)">Crear cuenta</button>
        </form>
        <p class="meta" style="margin-top:16px">¿Ya tenés cuenta? <a routerLink="/login">Entrá</a></p>
      </div>
    </div>
  `,
  styles: `
    .captcha { margin: 4px 0 12px; min-height: 65px; }
  `
})
export class RegistroComponent implements OnInit, OnDestroy {
  pedido: RegistroPedidoDto = {
    nombre: '',
    email: '',
    nombreUsuario: '',
    nombreLocal: '',
    modalidad: 'prueba',
    clave: '',
    sitio: '',
    captcha: ''
  };
  clave2 = '';
  error = '';
  aviso = '';
  cargando = false;
  captchaActivo = false;
  private widget: string | null = null;

  constructor(
    private readonly api: ApiService,
    private readonly sesion: SesionService,
    private readonly router: Router,
    private readonly zona: NgZone
  ) {}

  ngOnInit(): void {
    this.api.captchaRegistro().subscribe({
      next: (r) => {
        if (!r.siteKey) return;
        this.captchaActivo = true;
        setTimeout(() => this.mostrarCaptcha(r.siteKey!), 0);
      }
    });
  }

  ngOnDestroy(): void {
    const api = this.turnstile();
    if (api && this.widget) api.remove(this.widget);
  }

  registrar(): void {
    this.error = this.validar();
    this.aviso = '';
    if (this.error) return;
    this.cargando = true;
    this.api.registrar(this.pedido).subscribe({
      next: (r) => {
        if (r?.pendiente) {
          this.cargando = false;
          this.aviso = 'Te enviamos un mail. Abrí el enlace para activar la cuenta.';
          return;
        }
        if (!r?.token || !r.rol || !r.nombre) {
          this.cargando = false;
          return;
        }
        this.sesion.guardarLogin(r.token, r.rol, r.nombre);
        void this.router.navigateByUrl('/locales');
      },
      error: (err) => {
        this.error =
          err.status === 429
            ? 'Demasiados intentos. Esperá un minuto.'
            : typeof err.error === 'string'
              ? err.error
              : 'No se pudo crear la cuenta.';
        this.cargando = false;
      }
    });
  }

  private mostrarCaptcha(siteKey: string): void {
    const pintar = () => this.zona.run(() => {
      const api = this.turnstile();
      const caja = document.getElementById('captcha-registro');
      if (!api || !caja || this.widget) return;
      this.widget = api.render(caja, {
        sitekey: siteKey,
        theme: document.documentElement.getAttribute('data-tema') === 'oscuro' ? 'dark' : 'light',
        callback: (token: string) => this.zona.run(() => { this.pedido.captcha = token; }),
        'expired-callback': () => this.zona.run(() => { this.pedido.captcha = ''; }),
        'error-callback': () => this.zona.run(() => { this.error = 'No se pudo cargar la verificación.'; })
      });
    });
    if (this.turnstile()) {
      pintar();
      return;
    }
    const script = document.createElement('script');
    script.src = 'https://challenges.cloudflare.com/turnstile/v0/api.js?render=explicit';
    script.async = true;
    script.onload = () => pintar();
    script.onerror = () => this.zona.run(() => { this.error = 'No se pudo cargar la verificación.'; });
    document.head.appendChild(script);
  }

  private turnstile(): { render(el: HTMLElement, opts: object): string; remove(id: string): void } | undefined {
    return (window as unknown as { turnstile?: { render(el: HTMLElement, opts: object): string; remove(id: string): void } }).turnstile;
  }

  private validar(): string {
    const p = this.pedido;
    if (!p.nombre.trim() || !p.email.trim() || !p.nombreUsuario.trim() || !p.nombreLocal.trim())
      return 'Completá todos los datos.';
    if (p.clave.length < 8) return 'La contraseña debe tener al menos 8 caracteres.';
    if (p.clave !== this.clave2) return 'Las contraseñas no coinciden.';
    return '';
  }
}
