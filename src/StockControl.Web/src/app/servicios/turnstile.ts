import { NgZone } from '@angular/core';

type ApiTurnstile = { render(el: HTMLElement, opts: object): string; remove(id: string): void };

export function montarCaptcha(zona: NgZone, elementoId: string, siteKey: string, alCambiar: (token: string) => void): () => void {
  let widget = '';
  const pintar = () => {
    const api = (window as unknown as { turnstile?: ApiTurnstile }).turnstile;
    const caja = document.getElementById(elementoId);
    if (!api || !caja || widget) return;
    widget = api.render(caja, {
      sitekey: siteKey,
      callback: (token: string) => zona.run(() => alCambiar(token)),
      'expired-callback': () => zona.run(() => alCambiar(''))
    });
  };
  if ((window as unknown as { turnstile?: ApiTurnstile }).turnstile) pintar();
  else {
    const script = document.createElement('script');
    script.src = 'https://challenges.cloudflare.com/turnstile/v0/api.js?render=explicit';
    script.async = true;
    script.onload = () => pintar();
    document.head.appendChild(script);
  }
  return () => {
    const api = (window as unknown as { turnstile?: ApiTurnstile }).turnstile;
    if (widget && api) api.remove(widget);
  };
}
