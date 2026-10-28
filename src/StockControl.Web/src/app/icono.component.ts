import { Component, Input } from '@angular/core';

@Component({
  selector: 'sc-icono',
  standalone: true,
  template: `
    <svg viewBox="0 0 24 24" aria-hidden="true">
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
      }
    </svg>
  `,
  styles: `
    :host { display: inline-flex; flex: none; }
    svg {
      width: 18px;
      height: 18px;
      fill: none;
      stroke: currentColor;
      stroke-width: 1.8;
      stroke-linecap: round;
      stroke-linejoin: round;
    }
  `
})
export class IconoComponent {
  @Input({ required: true }) nombre = '';
}
