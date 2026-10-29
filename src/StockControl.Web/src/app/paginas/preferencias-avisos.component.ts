import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../servicios/api.service';
import { AvisosService } from '../servicios/avisos.service';
import { PushService } from '../servicios/push.service';

function mensaje(e: unknown): string {
  const delServidor = (e as { error?: unknown })?.error;
  if (typeof delServidor === 'string' && delServidor) return delServidor;
  if (e instanceof Error && e.message.startsWith('El navegador')) return e.message;
  return 'No se pudo cambiar el aviso de este dispositivo.';
}

@Component({
  selector: 'sc-preferencias-avisos',
  standalone: true,
  imports: [FormsModule],
  template: `
    <details class="panel pad preferencias-avisos">
      <summary><strong>Mis avisos</strong> <span class="meta">· sonido y avisos en este dispositivo</span></summary>
      <p class="meta">La campanita siempre avisa. Esto es solo tuyo, para este local.</p>
      <label class="check"><input type="checkbox" [(ngModel)]="sonido" (ngModelChange)="guardar()" /> Sonar cuando llega un aviso con el panel abierto</label>

      <h3>Con la pestaña cerrada</h3>
      @if (!push.soportado()) {
        <p class="meta">Este navegador no recibe avisos con la pestaña cerrada. En iPhone, agregá StockControl a la pantalla de inicio y abrilo desde ahí.</p>
      } @else if (!clavePush) {
        <p class="meta">Los avisos al dispositivo todavía no están habilitados en el servidor.</p>
      } @else {
        <label class="check"><input type="checkbox" [(ngModel)]="pushNuevos" (ngModelChange)="guardar()" /> Pedidos nuevos</label>
        <label class="check"><input type="checkbox" [(ngModel)]="pushCancelados" (ngModelChange)="guardar()" /> Pedidos que cancela el comprador</label>
        <div class="row">
          @if (activo) {
            <span class="ok grow">Este dispositivo recibe avisos.</span>
            <button class="btn" type="button" [disabled]="trabajando" (click)="desactivar()">Dejar de recibir acá</button>
          } @else {
            <span class="meta grow">Este dispositivo no recibe avisos.</span>
            <button class="btn btn-primary" type="button" [disabled]="trabajando" (click)="activar()">Recibir avisos acá</button>
          }
        </div>
        <p class="meta">Funciona en la compu y en Android. En iPhone, solo con StockControl agregado a la pantalla de inicio.</p>
      }
      @if (error) { <p class="error">{{ error }}</p> }
    </details>
  `,
  styles: `
    .preferencias-avisos { margin-bottom: 16px; }
    .preferencias-avisos summary { cursor: pointer; }
    .preferencias-avisos h3 { margin: 14px 0 6px; font-size: 14px; }
    .preferencias-avisos .check { margin: 6px 0; }
  `
})
export class PreferenciasAvisosComponent implements OnInit {
  sonido = true;
  pushNuevos = true;
  pushCancelados = true;
  clavePush: string | null = null;
  activo = false;
  trabajando = false;
  error = '';

  constructor(
    readonly push: PushService,
    private readonly api: ApiService,
    private readonly avisos: AvisosService
  ) {}

  ngOnInit(): void {
    this.api.preferenciasAviso().subscribe({
      next: (p) => {
        this.sonido = p.sonido;
        this.pushNuevos = p.pushNuevos;
        this.pushCancelados = p.pushCancelados;
        this.clavePush = p.clavePush;
        if (p.clavePush) void this.sincronizar();
      }
    });
  }

  guardar(): void {
    this.error = '';
    this.avisos.usarSonido(this.sonido);
    this.api.guardarPreferenciasAviso({ sonido: this.sonido, pushNuevos: this.pushNuevos, pushCancelados: this.pushCancelados }).subscribe({
      error: () => (this.error = 'No se pudieron guardar tus avisos.')
    });
  }

  async activar(): Promise<void> {
    await this.trabajar(async () => {
      await this.push.activar(this.clavePush!);
      this.activo = true;
    });
  }

  async desactivar(): Promise<void> {
    await this.trabajar(async () => {
      await this.push.desactivar();
      this.activo = false;
    });
  }

  private async sincronizar(): Promise<void> {
    try {
      this.activo = await this.push.sincronizar();
    } catch {
      this.activo = false;
    }
  }

  private async trabajar(accion: () => Promise<void>): Promise<void> {
    this.trabajando = true;
    this.error = '';
    try {
      await accion();
    } catch (e) {
      this.error = mensaje(e);
    } finally {
      this.trabajando = false;
    }
  }
}
