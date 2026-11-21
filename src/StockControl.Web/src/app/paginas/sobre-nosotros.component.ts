import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TemaComponent } from '../tema.component';

@Component({
  selector: 'sc-sobre-nosotros',
  standalone: true,
  imports: [RouterLink, TemaComponent],
  template: `
    <sc-tema />
    <div class="auth" style="padding-top:72px;align-items:start">
      <article class="panel legal">
        <div class="brand" style="border:0;padding:0 0 8px">
          <strong>StockControl</strong>
          <span>Sobre nosotros</span>
        </div>
        <h1>Sobre nosotros</h1>
        <p>StockControl empezó como un proyecto para el local de mis padres y fue creciendo hasta lo que es hoy.</p>
        <p>Está basado en lo que necesitan un almacén y un local de refrigeración en el día a día, y está hecho para cualquier local.</p>
        <p>La diferencia es tener todo centralizado: la caja, el stock y la tienda online en un solo lugar, por un precio bajo.</p>
        <p>Detrás de StockControl hay una sola persona. Cuando escribís, te contesto yo: atención personalizada, no bots.</p>
        <div class="row" style="margin-top:16px">
          <a class="btn btn-primary" routerLink="/registro">Probalo 7 días gratis</a>
          <a class="btn" routerLink="/contacto">Escribime</a>
        </div>
        <p class="meta" style="margin-top:16px"><a routerLink="/">Inicio</a> · <a routerLink="/terminos">Términos</a> · <a routerLink="/privacidad">Privacidad</a></p>
      </article>
    </div>
  `
})
export class SobreNosotrosComponent implements OnInit {
  ngOnInit(): void {
    document.title = 'StockControl — sobre nosotros';
  }
}
