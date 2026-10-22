import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../servicios/api.service';
import { TemaComponent } from '../tema.component';

@Component({
  selector: 'sc-confirmar',
  standalone: true,
  imports: [RouterLink, TemaComponent],
  template: `
    <sc-tema />
    <div class="auth" style="padding-top:72px">
      <div class="panel auth-card">
        <div class="brand" style="border:0;padding:0 0 8px">
          <strong>StockControl</strong>
          <span>Confirmación</span>
        </div>
        <h1>Confirmación</h1>
        @if (listo) {
          <p>La cuenta ya está activa.</p>
          <a class="btn btn-primary" routerLink="/login">Entrar</a>
        } @else if (error) {
          <p class="error">{{ error }}</p>
          <p class="meta"><a routerLink="/registro">Volver al registro</a></p>
        } @else {
          <p class="meta">Confirmando el mail...</p>
        }
      </div>
    </div>
  `
})
export class ConfirmarComponent implements OnInit {
  listo = false;
  error = '';

  constructor(
    private readonly ruta: ActivatedRoute,
    private readonly api: ApiService
  ) {}

  ngOnInit(): void {
    document.title = 'StockControl — confirmación';
    const token = this.ruta.snapshot.queryParamMap.get('token') ?? '';
    if (!token) {
      this.error = 'El enlace no sirve. Registrate de nuevo.';
      return;
    }
    this.api.confirmarCorreo(token).subscribe({
      next: () => { this.listo = true; },
      error: (e) => {
        this.error = typeof e.error === 'string' ? e.error : 'No se pudo confirmar el mail.';
      }
    });
  }
}

