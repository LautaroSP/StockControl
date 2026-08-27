import { DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService, LocalDto } from '../servicios/api.service';
import { SesionService } from '../servicios/sesion.service';
import { TemaService } from '../servicios/tema.service';

@Component({
  selector: 'sc-locales',
  standalone: true,
  imports: [DatePipe],
  template: `
    <div class="auth" style="align-items:start;padding-top:48px">
      <div style="width:min(960px,100%)">
        <div class="topbar">
          <div>
            <h1>Mis locales</h1>
            <p class="meta">Hola {{ sesion.nombre() }}. Elegí sucursal para cobrar.</p>
          </div>
          <div class="row">
            <button class="btn" type="button" (click)="tema.alternar()">
              {{ tema.tema() === 'oscuro' ? 'Modo claro' : 'Modo oscuro' }}
            </button>
            <button class="btn" type="button" (click)="salir()">Salir</button>
          </div>
        </div>
        @if (error) {
          <p class="error">{{ error }}</p>
        }
        <div class="cards">
          @for (l of locales; track l.idLocal) {
            <button class="panel card-local" type="button" (click)="entrar(l)">
              <span class="badge">{{ l.estadoAbono === 'al_dia' ? 'Al día' : l.estadoAbono }}</span>
              <h2>{{ l.nombre }}</h2>
              <p>Vence {{ l.vence ? (l.vence | date:'dd/MM/yyyy') : '—' }}</p>
            </button>
          }
        </div>
      </div>
    </div>
  `
})
export class LocalesComponent implements OnInit {
  locales: LocalDto[] = [];
  error = '';

  constructor(
    private readonly api: ApiService,
    readonly sesion: SesionService,
    private readonly router: Router,
    readonly tema: TemaService
  ) {}

  ngOnInit(): void {
    this.sesion.limpiarLocal();
    this.api.locales().subscribe({
      next: (list) => {
        this.locales = list;
      },
      error: () => (this.error = 'No se pudieron cargar los locales.')
    });
  }

  entrar(local: LocalDto): void {
    this.api.entrar(local.idLocal).subscribe({
      next: (r) => {
        this.sesion.guardarLocal(r.token, local.idLocal, local.nombre);
        this.api.puestos().subscribe({
          next: (p) => {
            if (p.cantidad <= 1) {
              this.api.elegirPuesto(1).subscribe({
                next: (e) => {
                  this.sesion.guardarCaja(e.token, e.nroCaja);
                  void this.router.navigateByUrl('/caja');
                },
                error: (err) =>
                  (this.error = typeof err.error === 'string' ? err.error : 'No se pudo elegir la caja.')
              });
            } else {
              void this.router.navigateByUrl('/elegir-caja');
            }
          },
          error: () => (this.error = 'No se pudieron cargar los puestos.')
        });
      },
      error: () => (this.error = 'No pudiste entrar a ese local.')
    });
  }

  salir(): void {
    this.sesion.salir();
    this.router.navigateByUrl('/login');
  }
}
