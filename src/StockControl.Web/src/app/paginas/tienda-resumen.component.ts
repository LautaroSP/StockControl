import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService, EstadoTienda, VistaTienda } from '../servicios/api.service';
import { IconoComponent } from '../icono.component';

interface Estado {
  texto: string;
  clase: string;
}

@Component({
  selector: 'sc-tienda-resumen',
  standalone: true,
  imports: [RouterLink, IconoComponent],
  template: `
    <div class="topbar">
      <h1>Tienda online</h1>
    </div>
    @if (error) { <p class="error">{{ error }}</p> }
    @if (cargada) {
      <section class="panel config-form">
        @if (!alias) {
          <div class="aviso-abono">
            Tu tienda todavía no tiene alias, así que no tiene link.
            <a class="btn btn-primary" routerLink="/mi-tienda/configuracion">Ir a Configuración</a>
          </div>
        } @else {
          <h2><sc-icono nombre="tienda" /> Estado <span class="badge {{ estado().clase }}">{{ estado().texto }}</span></h2>
          @if (estadoTienda === 'inactiva') {
            <p class="hint">Apagada: el link no abre hasta que la actives en Configuración.</p>
          }
          <div class="row resumen-link">
            <a class="grow" [href]="link()" target="_blank" rel="noopener">{{ link() }}</a>
            <button class="btn" type="button" (click)="copiar()">Copiar</button>
            <a class="btn btn-primary" [href]="link()" target="_blank" rel="noopener">Ver mi tienda</a>
          </div>
          @if (mensaje) { <p class="meta">{{ mensaje }}</p> }
        }

        <h2><sc-icono nombre="productos" /> Productos</h2>
        <p>Publicados: <strong>{{ publicados }}</strong> · Con imagen: <strong>{{ conImagen }}</strong></p>
        <div class="row">
          <a class="btn" routerLink="/productos">Publicar en Productos</a>
          <a class="btn" routerLink="/mi-tienda/imagenes">Imágenes de productos</a>
        </div>
      </section>
    }
  `,
  styles: `
    .resumen-link { align-items: center; flex-wrap: wrap; }
    .resumen-link a.grow { overflow-wrap: anywhere; }
    h2 .badge { margin-left: 8px; vertical-align: middle; }
  `
})
export class TiendaResumenComponent implements OnInit {
  alias = '';
  estadoTienda: EstadoTienda = 'inactiva';
  vista: VistaTienda | null = null;
  publicados = 0;
  conImagen = 0;
  cargada = false;
  error = '';
  mensaje = '';

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.api.configuracionTienda().subscribe({
      next: (r) => {
        this.alias = r.alias;
        this.estadoTienda = r.estado;
        this.vista = r.vista;
        this.publicados = r.publicados;
        this.conImagen = r.conImagen;
        this.cargada = true;
      },
      error: (e) => (this.error = typeof e.error === 'string' ? e.error : 'No se pudo cargar la tienda.')
    });
  }

  estado(): Estado {
    if (this.vista === 'cerrada') return { texto: 'Cerrada por el abono', clase: 'badge-vencido' };
    if (this.vista === 'pausada') return { texto: 'Pausada', clase: 'badge-aviso' };
    if (this.vista === 'catalogo') return { texto: 'Activa', clase: '' };
    return { texto: 'Apagada', clase: 'badge-aviso' };
  }

  link(): string {
    return `${location.origin}/tienda/${this.alias}`;
  }

  copiar(): void {
    void navigator.clipboard?.writeText(this.link()).then(() => (this.mensaje = 'Link copiado.'));
  }
}
