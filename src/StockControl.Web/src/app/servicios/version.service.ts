import { Injectable, OnDestroy, signal } from '@angular/core';

/** Compara el build que cargó el navegador con /version.json. Si cambió, avisa para recargar. */
@Injectable({ providedIn: 'root' })
export class VersionService implements OnDestroy {
  readonly hayActualizacion = signal(false);
  private propia = '';
  private timer?: ReturnType<typeof setInterval>;
  private readonly cadaMs = 60_000;

  iniciar(): void {
    if (this.propia) return;
    void this.primera();
    this.timer = setInterval(() => void this.chequear(), this.cadaMs);
    document.addEventListener('visibilitychange', this.alVolver);
  }

  actualizar(): void {
    location.reload();
  }

  ngOnDestroy(): void {
    clearInterval(this.timer);
    document.removeEventListener('visibilitychange', this.alVolver);
  }

  private readonly alVolver = (): void => {
    if (document.visibilityState === 'visible') void this.chequear();
  };

  private async primera(): Promise<void> {
    const id = await this.leer();
    if (id) this.propia = id;
  }

  private async chequear(): Promise<void> {
    if (!this.propia) {
      await this.primera();
      return;
    }
    const id = await this.leer();
    if (id && id !== this.propia) this.hayActualizacion.set(true);
  }

  private async leer(): Promise<string | null> {
    try {
      const r = await fetch(`/version.json?t=${Date.now()}`, { cache: 'no-store' });
      if (!r.ok) return null;
      const data = (await r.json()) as { id?: string };
      return typeof data.id === 'string' && data.id.length > 0 ? data.id : null;
    } catch {
      return null;
    }
  }
}
