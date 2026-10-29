import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService, PedidoTiendaDto } from '../servicios/api.service';
import { DialogoService } from '../servicios/dialogo.service';
import { TemaComponent } from '../tema.component';
import { NoEncontradaComponent } from './no-encontrada.component';

const ESTADOS: Record<PedidoTiendaDto['estado'], string> = {
  nuevo: 'Nuevo · a coordinar con el local',
  preparando: 'El local lo está preparando',
  listo: 'Listo para retirar',
  entregado: 'Entregado',
  cancelado: 'Cancelado'
};

@Component({
  selector: 'sc-pedido-tienda',
  standalone: true,
  imports: [RouterLink, TemaComponent, NoEncontradaComponent],
  template: `
    @if (noEncontrado) {
      <sc-no-encontrada />
    }
    @if (pedido; as p) {
      <div class="tienda">
        <header class="tienda-cabecera">
          <h1>{{ p.local.nombre }}</h1>
          <sc-tema [fijo]="false" />
        </header>
        <a class="btn" [routerLink]="['/tienda', alias]">Volver a la tienda</a>

        <section class="panel pad pedido-tienda">
          <h2>Pedido N° {{ p.numero }}</h2>
          <p><span [class]="p.estado === 'cancelado' ? 'badge badge-vencido' : 'badge'">{{ estado(p.estado) }}</span></p>
          <p class="meta">Guardá este link para consultar tu pedido. Es la única forma de verlo.</p>
          <ul class="carrito-lineas">
            @for (l of p.lineas; track $index) {
              <li>
                <span class="carrito-nombre">{{ l.cantidad }} × {{ l.nombre }}</span>
                <span class="carrito-importe">{{ precio(l.subTotal) }}</span>
              </li>
            }
            <li class="carrito-total"><span>Subtotal</span><span>{{ precio(p.subtotal) }}</span></li>
            <li class="carrito-total"><strong>Total</strong><strong>{{ precio(p.total) }}</strong></li>
          </ul>
          <p><strong>Retiro en el local.</strong>
            @if (p.local.direccion) { {{ p.local.direccion }}. }
            @if (p.local.horario) { Horario: {{ p.local.horario }}. }
          </p>
          <p class="meta">El pago se coordina con el local y se paga al retirar.</p>
          @if (error) { <p class="error">{{ error }}</p> }
          <div class="row">
            @if (p.local.whatsApp) {
              <a class="btn btn-primary" [href]="whatsApp(p)" target="_blank" rel="noopener noreferrer">Avisar al local por WhatsApp</a>
            }
            @if (p.puedeCancelar) {
              <button class="btn btn-danger" type="button" [disabled]="cancelando" (click)="cancelar()">Cancelar pedido</button>
            }
          </div>
        </section>
      </div>
    }
    @if (!pedido && error) { <p class="error pad">{{ error }}</p> }
  `
})
export class PedidoTiendaComponent implements OnInit, OnDestroy {
  pedido: PedidoTiendaDto | null = null;
  noEncontrado = false;
  cancelando = false;
  error = '';
  alias = '';
  private codigo = '';
  private readonly tituloAnterior = document.title;

  constructor(
    private readonly ruta: ActivatedRoute,
    private readonly api: ApiService,
    private readonly dialogo: DialogoService
  ) {}

  ngOnInit(): void {
    this.alias = this.ruta.snapshot.paramMap.get('alias') ?? '';
    this.codigo = this.ruta.snapshot.paramMap.get('codigo') ?? '';
    this.cargar();
  }

  ngOnDestroy(): void {
    document.title = this.tituloAnterior;
  }

  async cancelar(): Promise<void> {
    if (!(await this.dialogo.confirmar('¿Cancelar el pedido? No se puede deshacer.', 'Cancelar pedido', 'Volver'))) return;
    this.cancelando = true;
    this.api.cancelarPedidoTienda(this.alias, this.codigo).subscribe({
      next: () => {
        this.cancelando = false;
        this.cargar();
      },
      error: (e) => {
        this.cancelando = false;
        this.error = e.status === 400 && typeof e.error === 'string' ? e.error : 'No se pudo cancelar. Probá de nuevo.';
      }
    });
  }

  estado(estado: PedidoTiendaDto['estado']): string {
    return ESTADOS[estado] ?? estado;
  }

  whatsApp(p: PedidoTiendaDto): string {
    const productos = p.lineas.map((l) => `${l.cantidad} × ${l.nombre}`).join('\n');
    const texto = `Hola, soy ${p.nombre}. Hice el pedido N° ${p.numero} en la tienda:\n${productos}\nTotal ${this.precio(p.total)}\n${location.href}`;
    return `https://wa.me/${p.local.whatsApp}?text=${encodeURIComponent(texto)}`;
  }

  precio(n: number): string {
    return `$ ${Number(n).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }

  private cargar(): void {
    this.api.pedidoTienda(this.alias, this.codigo).subscribe({
      next: (p) => {
        this.pedido = p;
        this.error = '';
        document.title = `Pedido N° ${p.numero} · ${p.local.nombre}`;
      },
      error: (e) => {
        if (e.status === 404) this.noEncontrado = true;
        else this.error = e.status === 429 ? 'Demasiadas consultas. Esperá un minuto.' : 'No se pudo abrir el pedido.';
      }
    });
  }
}
