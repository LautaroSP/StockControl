import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ApiService, VentaPedidoDto } from './api.service';
import { CacheCajaService, VentaPendiente } from './cache-caja.service';
import { ConexionService } from './conexion.service';

const ESPERAS_MS = [5_000, 15_000, 60_000, 300_000];
const LATIDO_MS = 5_000;

/** Cola de ventas cobradas sin conexión. Vive en IndexedDB: sobrevive a cerrar la pestaña. */
@Injectable({ providedIn: 'root' })
export class ColaVentasService {
  readonly ventas = signal<VentaPendiente[]>([]);
  readonly sincronizando = signal(false);
  readonly recienSincronizada = signal(false);
  readonly porMandar = computed(() => this.ventas().filter((v) => v.estado === 'pendiente').length);
  readonly rechazadas = computed(() => this.ventas().filter((v) => v.estado === 'rechazada'));
  /** Mientras haya algo sin mandar (o rechazado sin resolver) no se cierra la caja. */
  readonly bloqueaCierre = computed(() => this.ventas().length > 0);
  private reloj: ReturnType<typeof setInterval> | null = null;

  constructor(
    private readonly api: ApiService,
    private readonly cache: CacheCajaService,
    private readonly conexion: ConexionService
  ) {
    window.addEventListener('online', () => void this.sincronizar());
  }

  iniciar(): void {
    void this.refrescar();
    if (this.reloj) return;
    this.reloj = setInterval(() => void this.latido(), LATIDO_MS);
  }

  detener(): void {
    if (this.reloj) clearInterval(this.reloj);
    this.reloj = null;
  }

  textoBloqueo(): string {
    const n = this.ventas().length;
    return `Hay ${n} ${n === 1 ? 'venta' : 'ventas'} sin mandar. Conectá y esperá a que se sincronicen.`;
  }

  async refrescar(): Promise<void> {
    try {
      const todas = await this.cache.ventasPendientes();
      this.ventas.set(todas.sort((a, b) => a.fechaCobro.localeCompare(b.fechaCobro)));
    } catch {
      /* sin IndexedDB no hay cola que mostrar */
    }
  }

  async encolar(pedido: VentaPedidoDto, total: number): Promise<void> {
    await this.cache.guardarVenta({
      idVentaCliente: pedido.idVentaCliente,
      pedido,
      total,
      fechaCobro: pedido.fechaCobro ?? new Date().toISOString(),
      estado: 'pendiente',
      intentos: 0,
      proximoIntento: 0,
      ultimoError: ''
    });
    await this.refrescar();
  }

  async reintentar(id: string): Promise<void> {
    const v = this.ventas().find((x) => x.idVentaCliente === id);
    if (!v) return;
    await this.cache.guardarVenta({ ...v, estado: 'pendiente', intentos: 0, proximoIntento: 0, ultimoError: '' });
    await this.refrescar();
    void this.sincronizar();
  }

  async descartar(id: string): Promise<void> {
    await this.cache.quitarVenta(id);
    await this.refrescar();
  }

  /** Manda las ventas pendientes de a una, en orden. Corta ante un fallo de red o de sesión. */
  async sincronizar(): Promise<void> {
    if (this.sincronizando()) return;
    await this.refrescar();
    const hechas = this.porMandar();
    this.sincronizando.set(true);
    try {
      for (const v of this.ventas().filter((x) => x.estado === 'pendiente' && x.proximoIntento <= Date.now())) {
        if (!(await this.mandar(v))) break;
      }
    } finally {
      this.sincronizando.set(false);
      await this.refrescar();
    }
    if (hechas > 0 && this.porMandar() === 0) this.avisarSincronizada();
  }

  /** false si hay que dejar de mandar (red caída o sesión vencida). */
  private async mandar(v: VentaPendiente): Promise<boolean> {
    try {
      await firstValueFrom(this.api.cobrar({ ...v.pedido, desdeSinConexion: true }));
      this.conexion.marcarVuelta();
      await this.cache.quitarVenta(v.idVentaCliente);
      return true;
    } catch (e) {
      return this.registrarFallo(v, e as HttpErrorResponse);
    }
  }

  private async registrarFallo(v: VentaPendiente, err: HttpErrorResponse): Promise<boolean> {
    if (err.status === 0) {
      this.conexion.marcarCaida();
      return false;
    }
    if (err.status === 401) return false;
    if (err.status >= 400 && err.status < 500) {
      const motivo = typeof err.error === 'string' && err.error ? err.error : 'El servidor no aceptó la venta.';
      await this.cache.guardarVenta({ ...v, estado: 'rechazada', ultimoError: motivo });
      return true;
    }
    const intentos = v.intentos + 1;
    const espera = ESPERAS_MS[Math.min(intentos - 1, ESPERAS_MS.length - 1)];
    await this.cache.guardarVenta({ ...v, intentos, proximoIntento: Date.now() + espera, ultimoError: 'El servidor no respondió.' });
    return false;
  }

  private async latido(): Promise<void> {
    await this.refrescar();
    if (!this.conexion.enLinea() && navigator.onLine) await this.probarConexion();
    if (this.conexion.enLinea() && this.porMandar() > 0) await this.sincronizar();
  }

  private async probarConexion(): Promise<void> {
    try {
      await firstValueFrom(this.api.abonoActual());
      this.conexion.marcarVuelta();
    } catch (e) {
      if ((e as HttpErrorResponse).status !== 0) this.conexion.marcarVuelta();
    }
  }

  private avisarSincronizada(): void {
    this.recienSincronizada.set(true);
    setTimeout(() => this.recienSincronizada.set(false), 4000);
  }
}
