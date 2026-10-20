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
            @if (sesion.rol() === 'dueno' && locales.length) {
              <p class="meta">
                Tenés {{ locales.length }} {{ locales.length === 1 ? 'local' : 'locales' }} · $ {{ pesos(totalMensual) }} por mes
              </p>
            }
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
              <span class="badge" [class.badge-vencido]="l.estado === 'vencido'" [class.badge-aviso]="l.estado === 'por_vencer'">
                {{ etiqueta(l) }}
              </span>
              <h2>{{ l.nombre }}</h2>
              <p>Vence {{ l.vence ? (l.vence | date: 'dd/MM/yyyy HH:mm') : '—' }}</p>
              @if (l.debito === 'activa') {
                <p class="meta">Débito automático</p>
              }
            </button>
          }
        </div>
      </div>
    </div>
  `
})
export class LocalesComponent implements OnInit {
  locales: LocalDto[] = [];
  totalMensual = 0;
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
      next: (r) => {
        this.locales = r.items;
        this.totalMensual = r.totalMensual;
      },
      error: () => (this.error = 'No se pudieron cargar los locales.')
    });
  }

  entrar(local: LocalDto): void {
    this.api.entrar(local.idLocal).subscribe({
      next: (r) => {
        this.sesion.guardarLocal(r.token, local.idLocal, local.nombre);
        const destino = local.estado === 'vencido' && this.sesion.puedeConfigurarLocal() ? '/activar' : '/caja';
        this.api.puestos().subscribe({
          next: (p) => {
            if (p.cantidad <= 1) {
              this.api.elegirPuesto(1).subscribe({
                next: (e) => {
                  this.sesion.guardarCaja(e.token, e.nroCaja);
                  void this.router.navigateByUrl(destino);
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

  etiqueta(local: LocalDto): string {
    switch (local.estado) {
      case 'trial':
        return `Prueba · ${local.diasRestantes} días`;
      case 'por_vencer':
        return `Por vencer · ${local.diasRestantes} días`;
      case 'vencido':
        return 'Vencido';
      default:
        return 'Al día';
    }
  }

  pesos(n: number): string {
    return Math.trunc(n).toLocaleString('es-AR');
  }

  salir(): void {
    this.sesion.salir();
    this.router.navigateByUrl('/login');
  }
}
