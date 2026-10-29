import { Injectable, signal } from '@angular/core';
import { ApiService } from './api.service';

const CADA_MS = 30000;

/** Consulta la campanita cada 30 s mientras la pestaña está a la vista. Suena si llegó un aviso nuevo. */
@Injectable({ providedIn: 'root' })
export class AvisosService {
  readonly puedeVerPedidos = signal(false);
  readonly noLeidos = signal(0);
  readonly ultimo = signal(0);
  private sonido = false;
  private primera = true;
  private timer = 0;
  private readonly alVolver = () => {
    if (!document.hidden) this.consultar();
  };

  constructor(private readonly api: ApiService) {}

  iniciar(): void {
    this.detener();
    this.primera = true;
    this.consultar();
    this.timer = window.setInterval(this.alVolver, CADA_MS);
    document.addEventListener('visibilitychange', this.alVolver);
  }

  detener(): void {
    window.clearInterval(this.timer);
    document.removeEventListener('visibilitychange', this.alVolver);
  }

  consultar(): void {
    this.api.contadorAvisos().subscribe({
      next: (r) => {
        const llegoOtro = !this.primera && r.ultimo > this.ultimo();
        this.primera = false;
        this.puedeVerPedidos.set(r.puedeVerPedidos);
        this.noLeidos.set(r.noLeidos);
        this.ultimo.set(r.ultimo);
        this.sonido = r.sonido;
        if (llegoOtro && this.sonido) this.sonar();
      }
    });
  }

  usarSonido(sonido: boolean): void {
    this.sonido = sonido;
  }

  private sonar(): void {
    try {
      const audio = new AudioContext();
      [880, 1175].forEach((frecuencia, i) => {
        const tono = audio.createOscillator();
        const volumen = audio.createGain();
        const inicio = audio.currentTime + i * 0.18;
        tono.frequency.value = frecuencia;
        volumen.gain.setValueAtTime(0.15, inicio);
        volumen.gain.exponentialRampToValueAtTime(0.001, inicio + 0.16);
        tono.connect(volumen).connect(audio.destination);
        tono.start(inicio);
        tono.stop(inicio + 0.17);
      });
      window.setTimeout(() => void audio.close(), 600);
    } catch {
      // Sin audio el aviso igual se ve en la campanita.
    }
  }
}
