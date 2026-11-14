import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService, TiendaPublicaDto } from '../servicios/api.service';
import { TemaComponent } from '../tema.component';
import { NoEncontradaComponent } from './no-encontrada.component';
import { PieTiendaComponent } from '../pie-tienda.component';

@Component({
  selector: 'sc-arrepentimiento-tienda',
  standalone: true,
  imports: [FormsModule, RouterLink, TemaComponent, NoEncontradaComponent, PieTiendaComponent],
  template: `
    <sc-tema />
    @if (noEncontrada) {
      <sc-no-encontrada />
    }
    @if (tienda; as t) {
      <div class="tienda">
        <article class="panel legal">
          <h1>Arrepentimiento</h1>
          <p>El vendedor es {{ t.nombre }}. El pedido queda para el local. El reintegro, si ya pagaste, lo hace el local.</p>
          @if (codigo) {
            <p>Tu trámite es <strong>{{ codigo }}</strong>. Guardalo.</p>
          } @else {
            <form (ngSubmit)="enviar()">
              <label class="field">Nombre <input class="wide" type="text" name="nombre" maxlength="120" [(ngModel)]="nombre" /></label>
              <label class="field">Mail <input class="wide" type="email" name="correo" maxlength="200" [(ngModel)]="correo" /></label>
              <label class="field">Mensaje <textarea class="wide" name="mensaje" rows="4" maxlength="2000" [(ngModel)]="mensaje"></textarea></label>
              <input class="campo-oculto" type="text" name="sitio" [(ngModel)]="sitio" tabindex="-1" autocomplete="off" aria-hidden="true" />
              @if (error) { <p class="error">{{ error }}</p> }
              <button class="btn btn-primary" type="submit" [disabled]="enviando">{{ enviando ? 'Enviando...' : 'Pedir el arrepentimiento' }}</button>
            </form>
          }
          <p class="meta"><a [routerLink]="['/tienda', alias]">Volver a la tienda</a></p>
        </article>
        <sc-pie-tienda [alias]="alias" [nombre]="t.nombre" [direccion]="t.direccion" [horario]="t.horario" />
      </div>
    }
  `
})
export class ArrepentimientoTiendaComponent implements OnInit {
  tienda: TiendaPublicaDto | null = null;
  noEncontrada = false;
  alias = '';
  nombre = '';
  correo = '';
  mensaje = '';
  sitio = '';
  codigo = '';
  error = '';
  enviando = false;

  constructor(
    private readonly ruta: ActivatedRoute,
    private readonly api: ApiService
  ) {}

  ngOnInit(): void {
    this.alias = this.ruta.snapshot.paramMap.get('alias') ?? '';
    document.title = 'Arrepentimiento de la tienda';
    this.api.tienda(this.alias).subscribe({
      next: (t) => { this.tienda = t; },
      error: () => { this.noEncontrada = true; }
    });
  }

  enviar(): void {
    this.error = '';
    this.enviando = true;
    this.api.pedirArrepentimientoTienda(this.alias, {
      nombre: this.nombre,
      correo: this.correo,
      mensaje: this.mensaje,
      sitio: this.sitio
    }).subscribe({
      next: (r) => {
        this.enviando = false;
        this.codigo = r.codigo;
      },
      error: (err) => {
        this.enviando = false;
        this.error = typeof err.error === 'string' ? err.error : 'No se pudo enviar el pedido.';
      }
    });
  }
}
