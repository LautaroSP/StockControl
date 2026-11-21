import { Component, Input } from '@angular/core';

@Component({
  selector: 'sc-icono',
  standalone: true,
  template: `
    <svg viewBox="0 0 24 24" aria-hidden="true" [class.marca]="nombre === 'instagram' || nombre === 'whatsapp'">
      @switch (nombre) {
        @case ('caja') {
          <path d="M4 8h16v10a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2z" />
          <path d="M4 8l2-4h12l2 4M8 13h8" />
        }
        @case ('productos') {
          <path d="M4 8l8-4 8 4-8 4z" />
          <path d="M4 8v8l8 4 8-4V8M12 12v8" />
        }
        @case ('estadisticas') {
          <path d="M5 19V10M10 19V5M15 19v-6M20 19V8" />
        }
        @case ('grupos') {
          <path d="M12 4l8 4-8 4-8-4z" />
          <path d="M4 12l8 4 8-4M4 16l8 4 8-4" />
        }
        @case ('categorias') {
          <rect x="4" y="4" width="7" height="7" rx="1.5" />
          <rect x="13" y="4" width="7" height="7" rx="1.5" />
          <rect x="4" y="13" width="7" height="7" rx="1.5" />
          <rect x="13" y="13" width="7" height="7" rx="1.5" />
        }
        @case ('medios') {
          <rect x="3" y="6" width="18" height="12" rx="2" />
          <path d="M3 10h18" />
        }
        @case ('proveedores') {
          <path d="M3 8h11v8H3zM14 11h4l3 3v2h-7z" />
          <circle cx="7" cy="18" r="1.5" />
          <circle cx="17" cy="18" r="1.5" />
        }
        @case ('usuarios') {
          <circle cx="9" cy="9" r="3" />
          <path d="M4 19c1-3 3-4.5 5-4.5S13 16 14 19" />
          <circle cx="16" cy="9" r="2.2" />
          <path d="M16 14.5c2 .3 3.5 1.6 4.2 4.5" />
        }
        @case ('configuracion') {
          <circle cx="12" cy="12" r="3" />
          <path d="M12 3.5v2.2M12 18.3v2.2M3.5 12h2.2M18.3 12h2.2M6 6l1.6 1.6M16.4 16.4 18 18M18 6l-1.6 1.6M7.6 16.4 6 18" />
        }
        @case ('abono') {
          <rect x="4" y="5" width="16" height="15" rx="2" />
          <path d="M8 3.5V7M16 3.5V7M4 10h16" />
        }
        @case ('informes') {
          <path d="M7 3.5h7l5 5V20a1.5 1.5 0 0 1-1.5 1.5h-10.5A1.5 1.5 0 0 1 5.5 20V5A1.5 1.5 0 0 1 7 3.5z" />
          <path d="M14 3.5V9h5.5M8 13h8M8 17h6" />
        }
        @case ('cajas') {
          <path d="M4 8h16v11H4z" />
          <path d="M4 8l2.5-4h11L20 8M10 12h4" />
        }
        @case ('cambiar') {
          <path d="M5 8a7 7 0 0 1 12-2l2 2" />
          <path d="M19 4v4h-4" />
          <path d="M19 16a7 7 0 0 1-12 2l-2-2" />
          <path d="M5 20v-4h4" />
        }
        @case ('locales') {
          <path d="M4 10.5 12 4l8 6.5V20H4z" />
          <path d="M10 20v-6h4v6" />
        }
        @case ('precios') {
          <path d="M4 13 13 4h6v6L10 19z" />
          <circle cx="16.5" cy="7.5" r="1" />
        }
        @case ('promocion') {
          <circle cx="12" cy="12" r="9" />
          <path d="M9 15.5 15 8.5M9.2 9.2h.01M14.8 14.8h.01" />
        }
        @case ('stock') {
          <path d="M4 7h16v4H4zM4 13h16v4H4z" />
        }
        @case ('empleados') {
          <circle cx="12" cy="8" r="3" />
          <path d="M5 20c1.2-3.5 3.6-5 7-5s5.8 1.5 7 5" />
        }
        @case ('tickets') {
          <path d="M7 3h10v18l-2-1.2L13 21l-2-1.2L9 21l-2-1.2L5 21V3z" />
          <path d="M9 8h6M9 12h6" />
        }
        @case ('carteles') {
          <rect x="4" y="5" width="16" height="12" rx="1.5" />
          <path d="M8 20h8M9 17l2-3 2 2 2-3 2 4" />
        }
        @case ('tienda') {
          <path d="M4 9.5 5.5 4h13L20 9.5a2.7 2.7 0 0 1-5.3 0 2.7 2.7 0 0 1-5.4 0 2.7 2.7 0 0 1-5.3 0z" />
          <path d="M5.5 12v8h13v-8M10 20v-4.5h4V20" />
        }
        @case ('imagenes') {
          <rect x="4" y="5" width="16" height="14" rx="1.5" />
          <circle cx="9" cy="10" r="1.5" />
          <path d="m4 17 5-4.5 3.5 3 2.5-2 5 3.5" />
        }
        @case ('campana') {
          <path d="M6 16.5V11a6 6 0 0 1 12 0v5.5l1.5 2h-15z" />
          <path d="M10 20.5a2 2 0 0 0 4 0" />
        }
        @case ('pedidos') {
          <path d="M6 7h12l-1 13H7z" />
          <path d="M9 7a3 3 0 0 1 6 0M9.5 12h5" />
        }
        @case ('check') {
          <circle cx="12" cy="12" r="9" />
          <path d="M8 12.5 10.5 15 16 9.5" />
        }
        @case ('sol') {
          <circle cx="12" cy="12" r="3.5" />
          <path d="M12 3.5v2M12 18.5v2M3.5 12h2M18.5 12h2M6 6l1.4 1.4M16.6 16.6 18 18M18 6l-1.4 1.4M7.4 16.6 6 18" />
        }
        @case ('luna') {
          <path d="M16.2 3.6A7.2 7.2 0 1 0 20.4 14 5.6 5.6 0 0 1 16.2 3.6z" />
        }
        @case ('qr') {
          <rect x="4" y="4" width="6.5" height="6.5" rx="1" />
          <rect x="13.5" y="4" width="6.5" height="6.5" rx="1" />
          <rect x="4" y="13.5" width="6.5" height="6.5" rx="1" />
          <path d="M14 14h2M18 14h2M14 18h2M18 18h2M16 16h2" />
        }
        @case ('whatsapp') {
          <path class="relleno" d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 01-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 01-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 012.893 6.994c-.003 5.45-4.435 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0012.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 005.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893a11.821 11.821 0 00-3.48-8.413z" />
        }
        @case ('instagram') {
          <defs>
            <radialGradient [attr.id]="igGrad" cx="0.3" cy="1.1" r="1.2">
              <stop offset="0%" stop-color="#fdf497" />
              <stop offset="45%" stop-color="#fd5949" />
              <stop offset="60%" stop-color="#d6249f" />
              <stop offset="90%" stop-color="#285AEB" />
            </radialGradient>
          </defs>
          <rect class="ig-fondo" x="1" y="1" width="22" height="22" rx="6" [attr.fill]="'url(#' + igGrad + ')'" />
          <rect class="ig-linea" x="6.2" y="6.2" width="11.6" height="11.6" rx="3.4" />
          <circle class="ig-linea" cx="12" cy="12" r="3.1" />
          <circle class="ig-punto" cx="16.6" cy="7.4" r="1.15" />
        }
        @case ('correo') {
          <rect x="3" y="5.5" width="18" height="13" rx="2" />
          <path d="m4 7.5 8 6 8-6" />
        }
        @case ('carrito') {
          <path d="M4 5h2l1.2 9h9.6L19 8H7.2" />
          <circle cx="10" cy="19" r="1.4" />
          <circle cx="16.5" cy="19" r="1.4" />
        }
        @case ('chevron-izquierda') {
          <path d="M14.5 6 9 12l5.5 6" />
        }
        @case ('chevron-derecha') {
          <path d="M9.5 6 15 12l-5.5 6" />
        }
      }
    </svg>
  `,
  styles: `
    :host { display: inline-flex; flex: none; vertical-align: middle; }
    svg {
      width: 18px;
      height: 18px;
      fill: none;
      stroke: currentColor;
      stroke-width: 1.8;
      stroke-linecap: round;
      stroke-linejoin: round;
    }
    svg.marca { stroke: none; }
    svg .relleno {
      fill: currentColor;
      stroke: none;
    }
    svg .ig-linea {
      fill: none;
      stroke: #fff;
      stroke-width: 1.7;
    }
    svg .ig-punto {
      fill: #fff;
      stroke: none;
    }
  `
})
export class IconoComponent {
  @Input({ required: true }) nombre = '';
  readonly igGrad = `ig-${Math.random().toString(36).slice(2, 9)}`;
}
