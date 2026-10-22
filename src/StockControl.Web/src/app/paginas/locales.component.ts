import { DatePipe } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { toDataURL } from 'qrcode';
import { ApiService, LocalDto } from '../servicios/api.service';
import { DialogoService } from '../servicios/dialogo.service';
import { SesionService } from '../servicios/sesion.service';
import { TemaComponent } from '../tema.component';

const INTERVALO_MS = 3000;
const MAX_CONSULTAS = 40;

@Component({
  selector: 'sc-locales',
  standalone: true,
  imports: [DatePipe, FormsModule, RouterLink, TemaComponent],
  template: `
    <sc-tema />
    <div class="auth" style="align-items:start;padding-top:72px">
      <div style="width:min(960px,100%)">
        <div class="topbar">
          <div>
            <h1>Mis locales</h1>
            <p class="meta">Hola {{ sesion.nombre() }}. Elegí sucursal para cobrar.</p>
            @if (sesion.rol() === 'dueno' && localesQueCuentan) {
              <p class="meta">
                Tenés {{ localesQueCuentan }} {{ localesQueCuentan === 1 ? 'local' : 'locales' }} · $ {{ pesos(totalMensual) }} por mes
              </p>
            }
          </div>
          <div class="row">
            @if (sesion.rol() === 'dueno') {
              <button class="btn btn-primary" type="button" (click)="contratando = !contratando">Contratar otro local</button>
            }
            @if (sesion.esAdmin()) {
              <a class="btn" routerLink="/admin">Panel de admin</a>
            }
            <button class="btn" type="button" (click)="salir()">Salir</button>
          </div>
        </div>
        @if (error) {
          <p class="error">{{ error }}</p>
        }
        @if (contratando) {
          <form class="panel" (ngSubmit)="contratar()">
            <p>Se factura por local, no por empleado. $ {{ pesos(precioMensual) }} por mes. Esta sucursal no incluye los 7 días de prueba.</p>
            <label>
              Nombre de la sucursal
              <input type="text" name="nombre" [(ngModel)]="nombre" maxlength="200" required />
            </label>
            <div class="row">
              <button class="btn btn-primary" type="submit" [disabled]="ocupado">Pagar con Mercado Pago</button>
              <a routerLink="/contacto">¿No tenés Mercado Pago? Contactá a ventas</a>
            </div>
          </form>
        }
        @if (qr) {
          <div class="panel">
            <p>Pagá $ {{ pesos(precioMensual) }} para habilitar <strong>{{ nombrePago }}</strong>.</p>
            <img [src]="qr" alt="QR para pagar con Mercado Pago" width="220" height="220" />
            <p><a class="btn btn-primary" [href]="urlPago" target="_blank" rel="noopener">Abrir Mercado Pago</a></p>
            <p><a routerLink="/contacto">¿No tenés Mercado Pago? Contactá a ventas</a></p>
          </div>
        }
        <div class="cards">
          @for (l of locales; track l.idLocal) {
            @if (l.estado === 'pendiente') {
              <div class="panel card-local">
                <span class="badge badge-vencido">Esperando pago</span>
                <h2>{{ l.nombre }}</h2>
                <p>Esta sucursal se habilita cuando se acredite el pago.</p>
                @if (sesion.rol() === 'dueno') {
                  <button class="btn" type="button" (click)="pagarPendiente(l)" [disabled]="ocupado">Pagar con Mercado Pago</button>
                }
              </div>
            } @else {
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
          }
        </div>
      </div>
    </div>
  `
})
export class LocalesComponent implements OnInit, OnDestroy {
  locales: LocalDto[] = [];
  totalMensual = 0;
  precioMensual = 14999;
  error = '';
  contratando = false;
  nombre = '';
  ocupado = false;
  qr = '';
  urlPago = '';
  nombrePago = '';
  private espera: ReturnType<typeof setInterval> | null = null;
  private consultas = 0;
  private idEsperando = 0;

  constructor(
    private readonly api: ApiService,
    readonly sesion: SesionService,
    private readonly router: Router,
    private readonly dialogo: DialogoService
  ) {}

  ngOnInit(): void {
    this.sesion.limpiarLocal();
    this.cargar();
  }

  ngOnDestroy(): void {
    this.dejarDeEsperar();
  }

  get localesQueCuentan(): number {
    return this.locales.filter((l) => l.estado !== 'pendiente').length;
  }

  contratar(): void {
    const nombre = this.nombre.trim();
    if (!nombre) {
      this.error = 'El nombre del local es obligatorio.';
      return;
    }
    this.error = '';
    this.ocupado = true;
    this.api.contratarLocal(nombre).subscribe({
      next: (r) => {
        this.ocupado = false;
        this.contratando = false;
        this.nombre = '';
        void this.mostrarPago(r.idLocal, r.nombre, r.urlPago);
        this.cargar();
      },
      error: (err) => {
        this.ocupado = false;
        this.error = typeof err.error === 'string' ? err.error : 'No se pudo iniciar el pago.';
        this.cargar();
      }
    });
  }

  pagarPendiente(local: LocalDto): void {
    this.error = '';
    this.ocupado = true;
    this.api.pagarContratacion(local.idLocal).subscribe({
      next: (r) => {
        this.ocupado = false;
        void this.mostrarPago(r.idLocal, r.nombre, r.urlPago);
      },
      error: (err) => {
        this.ocupado = false;
        this.error = typeof err.error === 'string' ? err.error : 'No se pudo iniciar el pago.';
      }
    });
  }

  entrar(local: LocalDto): void {
    if (local.estado === 'pendiente') return;
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
      error: (err) =>
        (this.error = typeof err.error === 'string' ? err.error : 'No pudiste entrar a ese local.')
    });
  }

  etiqueta(local: LocalDto): string {
    switch (local.estado) {
      case 'pendiente':
        return 'Esperando pago';
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

  async salir(): Promise<void> {
    if (!await this.dialogo.confirmar('¿Cerrar la sesión?')) return;
    this.sesion.salir();
    void this.router.navigateByUrl('/login');
  }

  private cargar(): void {
    this.api.locales().subscribe({
      next: (r) => {
        this.locales = r.items;
        this.totalMensual = r.totalMensual;
        this.precioMensual = r.precioMensual;
        if (this.idEsperando && !r.items.some((l) => l.idLocal === this.idEsperando && l.estado === 'pendiente')) {
          this.qr = '';
          this.urlPago = '';
          this.idEsperando = 0;
          this.dejarDeEsperar();
        }
      },
      error: () => (this.error = 'No se pudieron cargar los locales.')
    });
  }

  private async mostrarPago(idLocal: number, nombre: string, url: string): Promise<void> {
    this.nombrePago = nombre;
    this.urlPago = url;
    this.qr = await toDataURL(url, { width: 220, margin: 2 });
    this.esperar(idLocal);
  }

  private esperar(idLocal: number): void {
    this.dejarDeEsperar();
    this.idEsperando = idLocal;
    this.consultas = 0;
    this.espera = setInterval(() => {
      this.consultas++;
      if (this.consultas > MAX_CONSULTAS) {
        this.dejarDeEsperar();
        return;
      }
      this.cargar();
    }, INTERVALO_MS);
  }

  private dejarDeEsperar(): void {
    if (this.espera) clearInterval(this.espera);
    this.espera = null;
  }
}
