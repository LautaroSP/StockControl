import { Component, Input } from '@angular/core';
import { IconoComponent } from './icono.component';
import { TemaService } from './servicios/tema.service';

@Component({
  selector: 'sc-tema',
  standalone: true,
  imports: [IconoComponent],
  host: { '[class.fijo]': 'fijo' },
  template: `
    <button
      class="tema-switch"
      type="button"
      [attr.data-tema]="tema.tema()"
      [attr.aria-label]="tema.tema() === 'oscuro' ? 'Modo claro' : 'Modo oscuro'"
      (click)="tema.alternar()">
      <span class="tema-pista">
        <sc-icono nombre="sol" />
        <sc-icono nombre="luna" />
      </span>
    </button>
  `,
  styles: `
    :host { display: inline-flex; }
    :host.fijo .tema-switch {
      position: fixed;
      top: 14px;
      left: 14px;
      z-index: 40;
    }
    .tema-switch {
      width: 64px;
      height: 32px;
      padding: 0;
      border: 1px solid var(--line);
      border-radius: 999px;
      background: var(--panel);
      cursor: pointer;
    }
    .tema-pista {
      display: grid;
      grid-template-columns: 1fr 1fr;
      align-items: center;
      justify-items: center;
      height: 100%;
      position: relative;
      color: var(--ink-soft);
    }
    .tema-pista::before {
      content: "";
      position: absolute;
      top: 3px;
      bottom: 3px;
      left: 3px;
      width: calc(50% - 3px);
      border-radius: 999px;
      background: var(--moss);
    }
    .tema-switch[data-tema="oscuro"] .tema-pista::before { transform: translateX(100%); }
    .tema-pista sc-icono { position: relative; z-index: 1; }
    .tema-switch[data-tema="claro"] .tema-pista sc-icono:first-child,
    .tema-switch[data-tema="oscuro"] .tema-pista sc-icono:last-child { color: #fff; }
    @media (prefers-reduced-motion: no-preference) {
      .tema-pista::before { transition: transform 160ms ease; }
    }
  `
})
export class TemaComponent {
  @Input() fijo = true;

  constructor(readonly tema: TemaService) {}
}
