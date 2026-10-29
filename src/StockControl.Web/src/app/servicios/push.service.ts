import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ApiService, SuscripcionPushDto } from './api.service';

const TRABAJADOR = '/sw-avisos.js';

/** Web Push del dispositivo: el aviso llega aunque la pestaña esté cerrada. */
@Injectable({ providedIn: 'root' })
export class PushService {
  private escuchando = false;

  constructor(private readonly api: ApiService, private readonly router: Router) {}

  soportado(): boolean {
    return 'serviceWorker' in navigator && 'PushManager' in window && 'Notification' in window;
  }

  async suscripcion(): Promise<PushSubscription | null> {
    if (!this.soportado() || Notification.permission !== 'granted') return null;
    const registro = await navigator.serviceWorker.getRegistration(TRABAJADOR);
    return (await registro?.pushManager.getSubscription()) ?? null;
  }

  /** Si el dispositivo ya está suscripto, lo vuelve a anotar para este usuario y local. */
  async sincronizar(): Promise<boolean> {
    const actual = await this.suscripcion();
    if (!actual) return false;
    await firstValueFrom(this.api.suscribirPush(datos(actual)));
    return true;
  }

  async activar(clavePublica: string): Promise<void> {
    if (await Notification.requestPermission() !== 'granted')
      throw new Error('El navegador no dio permiso para mostrar avisos. Habilitalo en la configuración del sitio.');
    const registro = await navigator.serviceWorker.register(TRABAJADOR);
    await navigator.serviceWorker.ready;
    const actual = await registro.pushManager.getSubscription();
    const nueva = actual ?? await registro.pushManager.subscribe({ userVisibleOnly: true, applicationServerKey: bytes(clavePublica) });
    await firstValueFrom(this.api.suscribirPush(datos(nueva)));
  }

  async desactivar(): Promise<void> {
    const actual = await this.suscripcion();
    if (!actual) return;
    await firstValueFrom(this.api.desuscribirPush(datos(actual)));
    await actual.unsubscribe();
  }

  /** El clic en la notificación con la app abierta llega como mensaje del trabajador. */
  escucharClics(): void {
    if (!('serviceWorker' in navigator) || this.escuchando) return;
    this.escuchando = true;
    navigator.serviceWorker.addEventListener('message', (e: MessageEvent) => {
      const url = e.data?.url;
      if (typeof url === 'string' && url.startsWith('/') && !url.startsWith('//')) void this.router.navigateByUrl(url);
    });
    navigator.serviceWorker.startMessages();
  }
}

function datos(s: PushSubscription): SuscripcionPushDto {
  const json = s.toJSON();
  return { endpoint: s.endpoint, p256dh: json.keys?.['p256dh'] ?? '', auth: json.keys?.['auth'] ?? '' };
}

function bytes(base64Url: string): Uint8Array {
  const base64 = (base64Url + '='.repeat((4 - (base64Url.length % 4)) % 4)).replace(/-/g, '+').replace(/_/g, '/');
  return Uint8Array.from(atob(base64), (c) => c.charCodeAt(0));
}
