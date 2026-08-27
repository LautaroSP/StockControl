import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from '../servicios/api.service';
import { SesionService } from '../servicios/sesion.service';

@Component({
  selector: 'sc-elegir-caja',
  standalone: true,
  template: `
    <div class="auth" style="align-items:start;padding-top:48px">
      <div style="width:min(640px,100%)">
        <div class="topbar">
          <div>
            <h1>Elegir caja</h1>
            <p class="meta">{{ sesion.nombreLocal() }} · {{ sesion.nombre() }}</p>
          </div>
          <a class="btn" href="/locales" (click)="locales($event)">Cambiar local</a>
        </div>
        @if (error) {
          <p class="error">{{ error }}</p>
        }
        @if (aviso) {
          <p class="meta" style="margin-bottom:12px">{{ aviso }}</p>
        }
        <div class="cards">
          @for (n of puestos; track n) {
            <button class="panel card-local" type="button" (click)="elegir(n)">
              <h2>Caja {{ n }}</h2>
              <p>{{ ocupacion(n) }}</p>
            </button>
          }
        </div>
      </div>
    </div>
  `
})
export class ElegirCajaComponent implements OnInit {
  puestos: number[] = [];
  ocupaciones: { nroCaja: number; nombreUsuario: string }[] = [];
  error = '';
  aviso = '';

  constructor(
    private readonly api: ApiService,
    readonly sesion: SesionService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.api.puestos().subscribe({
      next: (r) => {
        this.puestos = Array.from({ length: r.cantidad }, (_, i) => i + 1);
        this.ocupaciones = r.ocupaciones;
      },
      error: () => (this.error = 'No se pudieron cargar los puestos.')
    });
  }

  ocupacion(n: number): string {
    const otros = this.ocupaciones.filter((o) => o.nroCaja === n);
    if (otros.length === 0) return 'Libre';
    return otros.map((o) => o.nombreUsuario).join(', ');
  }

  elegir(n: number): void {
    this.error = '';
    this.aviso = '';
    this.api.elegirPuesto(n).subscribe({
      next: (r) => {
        this.sesion.guardarCaja(r.token, r.nroCaja);
        if (r.aviso) this.aviso = r.aviso;
        void this.router.navigateByUrl('/caja');
      },
      error: (err) => (this.error = typeof err.error === 'string' ? err.error : 'No se pudo elegir la caja.')
    });
  }

  locales(ev: Event): void {
    ev.preventDefault();
    this.sesion.limpiarLocal();
    void this.router.navigateByUrl('/locales');
  }
}
