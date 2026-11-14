import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ApiService } from '../servicios/api.service';
import { SesionService } from '../servicios/sesion.service';
import { TemaComponent } from '../tema.component';

@Component({
  selector: 'sc-aceptar-terminos',
  standalone: true,
  imports: [FormsModule, RouterLink, TemaComponent],
  template: `
    <sc-tema />
    <div class="auth" style="padding-top:72px">
      <div class="panel auth-card">
        <h1>Términos nuevos</h1>
        <p>Para seguir usando StockControl tenés que aceptar la versión vigente.</p>
        <label class="check" style="margin:12px 0 16px">
          <input type="checkbox" name="acepta" [(ngModel)]="acepta" />
          <span>Acepto los <a routerLink="/terminos" (click)="$event.stopPropagation()">términos</a> y la <a routerLink="/privacidad" (click)="$event.stopPropagation()">política de privacidad</a>.</span>
        </label>
        @if (error) { <p class="error">{{ error }}</p> }
        <button class="btn btn-primary wide" type="button" [disabled]="cargando" (click)="aceptar()">Seguir</button>
      </div>
    </div>
  `
})
export class AceptarTerminosComponent {
  acepta = false;
  error = '';
  cargando = false;

  constructor(
    private readonly api: ApiService,
    private readonly sesion: SesionService,
    private readonly router: Router
  ) {}

  aceptar(): void {
    if (!this.acepta) {
      this.error = 'Tenés que aceptar los términos y la política de privacidad.';
      return;
    }
    this.error = '';
    this.cargando = true;
    this.api.aceptarTerminos().subscribe({
      next: () => {
        this.sesion.terminosVigentes.set(true);
        void this.router.navigateByUrl('/locales');
      },
      error: (err) => {
        this.cargando = false;
        this.error = typeof err.error === 'string' ? err.error : 'No se pudo guardar la aceptación.';
      }
    });
  }
}
