import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { SesionService } from '../servicios/sesion.service';
import { TemaService } from '../servicios/tema.service';

@Component({
  selector: 'sc-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="app">
      <aside class="sidebar">
        <div class="brand">
          <strong>StockControl</strong>
          <span>{{ sesion.nombreLocal() }}</span>
        </div>
        <nav class="nav">
          <a routerLink="/caja" routerLinkActive="active">Caja</a>
          <a routerLink="/productos" routerLinkActive="active">Productos</a>
          <a routerLink="/locales">Locales</a>
        </nav>
        <div class="sidebar-foot">
          <div>{{ sesion.nombre() }} · {{ sesion.rol() }}</div>
          <button class="btn btn-ghost" type="button" (click)="tema.alternar()">
            {{ tema.tema() === 'oscuro' ? 'Modo claro' : 'Modo oscuro' }}
          </button>
          <a class="btn btn-ghost" routerLink="/login" (click)="sesion.salir()">Salir</a>
        </div>
      </aside>
      <main class="main">
        <router-outlet />
      </main>
    </div>
  `
})
export class ShellComponent {
  constructor(
    readonly sesion: SesionService,
    readonly tema: TemaService
  ) {}
}
