import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService, TiendaPublicaDto } from '../servicios/api.service';
import { TemaComponent } from '../tema.component';
import { NoEncontradaComponent } from './no-encontrada.component';
import { PieTiendaComponent } from '../pie-tienda.component';

@Component({
  selector: 'sc-terminos-tienda',
  standalone: true,
  imports: [RouterLink, TemaComponent, NoEncontradaComponent, PieTiendaComponent],
  template: `
    <sc-tema />
    @if (noEncontrada) {
      <sc-no-encontrada />
    }
    @if (tienda; as t) {
      <div class="tienda">
        <article class="panel legal">
          <h1>Términos de la tienda</h1>
          <p>Quien vende es {{ t.nombre }}, no StockControl.</p>
          @if (t.direccion) { <p>Dirección: {{ t.direccion }}</p> }
          @if (t.horario) { <p>Horario: {{ t.horario }}</p> }
          <p>Los precios son los publicados. El stock que se muestra es si hay o no hay, sin la cantidad. El pedido es retiro en el local, o envío si el local lo ofrece. El pago es el que el local tenga habilitado.</p>
          <p>El catálogo (nombres, precios y fotos) es del local. No se copia para armar otra tienda ni un listado.</p>
          <p>El comprador puede arrepentirse del pedido con el botón de esta tienda. Si el pago fue a Mercado Pago, el reintegro lo hace el local desde esa cuenta. Si fue una transferencia, la devolución es a mano, la hace el local.</p>
          <p class="meta"><a [routerLink]="['/tienda', alias]">Volver a la tienda</a></p>
        </article>
        <sc-pie-tienda [alias]="alias" [nombre]="t.nombre" [direccion]="t.direccion" [horario]="t.horario" />
      </div>
    }
  `
})
export class TerminosTiendaComponent implements OnInit {
  tienda: TiendaPublicaDto | null = null;
  noEncontrada = false;
  alias = '';

  constructor(
    private readonly ruta: ActivatedRoute,
    private readonly api: ApiService
  ) {}

  ngOnInit(): void {
    this.alias = this.ruta.snapshot.paramMap.get('alias') ?? '';
    document.title = 'Términos de la tienda';
    this.api.tienda(this.alias).subscribe({
      next: (t) => { this.tienda = t; },
      error: () => { this.noEncontrada = true; }
    });
  }
}
