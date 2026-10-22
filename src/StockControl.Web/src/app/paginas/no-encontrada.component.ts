import { Component, OnDestroy, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TemaService } from '../servicios/tema.service';

@Component({
  selector: 'sc-no-encontrada',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="auth">
      <div class="panel auth-card">
        <div class="brand" style="border:0;padding:0 0 8px">
          <strong>StockControl</strong>
          <span>Caja y stock</span>
        </div>
        <h1>No encontramos esta página.</h1>
        <p class="meta">El enlace no corresponde a una pantalla de la app.</p>
        <a class="btn btn-primary" routerLink="/">Volver al inicio</a>
      </div>
    </div>
  `
})
export class NoEncontradaComponent implements OnInit, OnDestroy {
  private readonly tituloAnterior = document.title;

  constructor(private readonly tema: TemaService) {
    this.tema.aplicar();
  }

  ngOnInit(): void {
    document.title = 'StockControl — página no encontrada';
  }

  ngOnDestroy(): void {
    document.title = this.tituloAnterior;
  }
}
