import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService, PedidoTiendaDto } from '../servicios/api.service';
import { DialogoService } from '../servicios/dialogo.service';
import { TemaComponent } from '../tema.component';
import { NoEncontradaComponent } from './no-encontrada.component';

/** El público tiene tope de 60 consultas por minuto por IP. */
const CONSULTA_PAGO_MS = 5000;
/** La transferencia la confirma una persona: no hace falta consultar seguido. */
const CONSULTA_TRANSFERENCIA_MS = 30000;

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
          <p><span [class]="p.estado === 'cancelado' ? 'badge badge-vencido' : 'badge'">{{ estado(p) }}</span></p>
          <p class="meta">Guardá este link para consultar tu pedido. Es la única forma de verlo.</p>
          <ul class="carrito-lineas">
            @for (l of p.lineas; track $index) {
              <li>
                <span class="carrito-nombre">{{ l.cantidad }} × {{ l.nombre }}</span>
                <span class="carrito-importe">{{ precio(l.subTotal) }}</span>
              </li>
            }
            <li class="carrito-total"><span>Subtotal</span><span>{{ precio(p.subtotal) }}</span></li>
            @if (p.modalidad === 'envio') {
              <li class="carrito-total">
                <span>Envío <span class="meta">{{ origen(p.origenDistancia) }}</span></span>
                <span>{{ precio(p.envio) }}</span>
              </li>
            }
            @if (p.gestion > 0) {
              <li class="carrito-total"><span>Gestión</span><span>{{ precio(p.gestion) }}</span></li>
            }
            <li class="carrito-total"><strong>Total</strong><strong>{{ precio(p.total) }}</strong></li>
          </ul>
          @if (p.modalidad === 'envio') {
            <p><strong>Envío a {{ p.direccionEnvio }}.</strong>
              @if (p.distanciaKm !== null) { {{ p.distanciaKm }} km {{ origen(p.origenDistancia) }}. }
            </p>
          } @else {
            <p><strong>Retiro en el local.</strong>
              @if (p.local.direccion) { {{ p.local.direccion }}. }
              @if (p.local.horario) { Horario: {{ p.local.horario }}. }
            </p>
          }
          @switch (p.estadoPago) {
            @case ('pagado') { <p><strong>{{ esTransferencia(p) ? 'El local confirmó tu transferencia.' : 'Pagado con Mercado Pago.' }}</strong></p> }
            @case ('reembolsado') { <p><strong>{{ esTransferencia(p) ? 'El local canceló el pedido y te devuelve la transferencia.' : 'El pago se devolvió por Mercado Pago.' }}</strong></p> }
            @case ('pendiente') {
              @if (p.estado === 'cancelado') {
                <p class="meta">{{ esTransferencia(p) ? 'El local no confirmó la transferencia a tiempo.' : 'No se acreditó el pago a tiempo.' }}</p>
              } @else if (esTransferencia(p)) {
                @if (p.transferencia; as t) {
                  <div class="panel pad pedido-transferencia">
                    <p><strong>Transferí {{ precio(p.total) }}</strong> a esta cuenta:</p>
                    @if (t.cbu) {
                      <p>CBU / CVU: <strong>{{ t.cbu }}</strong> <button class="btn" type="button" (click)="copiar(t.cbu, 'CBU')">Copiar</button></p>
                    }
                    @if (t.alias) {
                      <p>Alias: <strong>{{ t.alias }}</strong> <button class="btn" type="button" (click)="copiar(t.alias, 'Alias')">Copiar</button></p>
                    }
                    <p>Titular: <strong>{{ t.titular }}</strong></p>
                    <p class="meta">Si tu banco lo permite, poné «Pedido {{ p.numero }}» en el concepto. Mandale el comprobante al local. El pedido se prepara cuando el local confirma la transferencia.</p>
                    @if (p.venceElPago) { <p class="meta">Si el local no la confirma antes del {{ fechaHora(p.venceElPago) }}, el pedido se cancela.</p> }
                    @if (copiado) { <p class="ok">{{ copiado }} copiado.</p> }
                  </div>
                }
              } @else {
                <p class="meta">Esperando que Mercado Pago confirme el pago. Si ya pagaste, se actualiza solo en unos segundos.</p>
                @if (p.venceElPago) { <p class="meta">Si no se paga antes de las {{ hora(p.venceElPago) }}, el pedido se cancela.</p> }
              }
            }
            @default { <p class="meta">El pago se coordina con el local y se paga al retirar.</p> }
          }
          @if (error) { <p class="error">{{ error }}</p> }
          <div class="row">
            @if (p.urlPago) {
              <a class="btn btn-charge" [href]="p.urlPago">Pagar con Mercado Pago</a>
            }
            @if (p.local.whatsApp) {
              <a class="btn btn-primary" [href]="whatsApp(p)" target="_blank" rel="noopener noreferrer">
                {{ p.transferencia ? 'Mandar el comprobante por WhatsApp' : 'Avisar al local por WhatsApp' }}
              </a>
            }
            @if (p.puedeCancelar) {
              <button class="btn btn-danger" type="button" [disabled]="cancelando" (click)="cancelar()">Cancelar pedido</button>
            }
          </div>
        </section>
      </div>
    }
    @if (!pedido && error) { <p class="error pad">{{ error }}</p> }
  `,
  styles: `
    .pedido-transferencia { margin: 12px 0; }
    .pedido-transferencia p { margin: 6px 0; }
  `
})
export class PedidoTiendaComponent implements OnInit, OnDestroy {
  pedido: PedidoTiendaDto | null = null;
  noEncontrado = false;
  cancelando = false;
  copiado = '';
  error = '';
  alias = '';
  private codigo = '';
  private reintento?: ReturnType<typeof setTimeout>;
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
    clearTimeout(this.reintento);
  }

  async cancelar(): Promise<void> {
    const texto = this.pedido?.transferencia
      ? '¿Cancelar el pedido? No se puede deshacer. Si ya transferiste, escribile al local para que te devuelva la plata.'
      : '¿Cancelar el pedido? No se puede deshacer.';
    if (!(await this.dialogo.confirmar(texto, 'Cancelar pedido', 'Volver'))) return;
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

  estado(p: PedidoTiendaDto): string {
    if (p.estado === 'nuevo' && p.estadoPago === 'pendiente') return this.esTransferencia(p) ? 'A confirmar transferencia' : 'Esperando el pago';
    if (p.estado === 'nuevo' && p.estadoPago === 'pagado') return 'Pagado · el local lo va a preparar';
    if (p.estado === 'listo' && p.modalidad === 'envio') return 'Listo para enviar';
    return ESTADOS[p.estado] ?? p.estado;
  }

  origen(valor: string): string {
    return valor === 'recorrido' ? 'por recorrido' : 'en línea recta';
  }

  esTransferencia(p: PedidoTiendaDto): boolean {
    return p.medioPago === 'transferencia';
  }

  copiar(valor: string, nombre: string): void {
    void navigator.clipboard?.writeText(valor).then(() => (this.copiado = nombre));
  }

  hora(fecha: string): string {
    return new Date(fecha).toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' });
  }

  fechaHora(fecha: string): string {
    return new Date(fecha).toLocaleString('es-AR', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' });
  }

  whatsApp(p: PedidoTiendaDto): string {
    const productos = p.lineas.map((l) => `${l.cantidad} × ${l.nombre}`).join('\n');
    const pago = p.transferencia ? `\nTe transferí ${this.precio(p.total)}. Te mando el comprobante.` : '';
    const texto = `Hola, soy ${p.nombre}. Hice el pedido N° ${p.numero} en la tienda:\n${productos}\nTotal ${this.precio(p.total)}${pago}\n${location.href}`;
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
        clearTimeout(this.reintento);
        if (p.estadoPago === 'pendiente' && p.estado === 'nuevo')
          this.reintento = setTimeout(() => this.cargar(), this.esTransferencia(p) ? CONSULTA_TRANSFERENCIA_MS : CONSULTA_PAGO_MS);
      },
      error: (e) => {
        if (e.status === 404) this.noEncontrado = true;
        else this.error = e.status === 429 ? 'Demasiadas consultas. Esperá un minuto.' : 'No se pudo abrir el pedido.';
      }
    });
  }
}
