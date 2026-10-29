import { Component, DestroyRef, OnInit, effect, untracked } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ApiService, EstadoPedidoTienda, MetodoPagoDto, PedidoPanelDto, PedidoPanelListaDto } from '../servicios/api.service';
import { AvisosService } from '../servicios/avisos.service';
import { DialogoService } from '../servicios/dialogo.service';
import { SesionService } from '../servicios/sesion.service';
import { PreferenciasAvisosComponent } from './preferencias-avisos.component';

const ESTADOS: Record<EstadoPedidoTienda, { texto: string; clase: string }> = {
  nuevo: { texto: 'Nuevo', clase: 'badge badge-aviso' },
  preparando: { texto: 'Preparando', clase: 'badge' },
  listo: { texto: 'Listo para retirar', clase: 'badge' },
  entregado: { texto: 'Entregado', clase: 'badge' },
  cancelado: { texto: 'Cancelado', clase: 'badge badge-vencido' }
};

@Component({
  selector: 'sc-panel-pedidos',
  standalone: true,
  imports: [FormsModule, PreferenciasAvisosComponent],
  template: `
    <div class="topbar">
      <h1>Pedidos de la tienda</h1>
      <select [(ngModel)]="filtro" (ngModelChange)="cargar(1)" aria-label="Estado">
        <option value="activos">Abiertos</option>
        <option value="nuevo">Nuevos</option>
        <option value="preparando">Preparando</option>
        <option value="listo">Listos</option>
        <option value="entregado">Entregados</option>
        <option value="cancelado">Cancelados</option>
        <option value="todos">Todos</option>
      </select>
    </div>
    @if (error) { <p class="error">{{ error }}</p> }
    @if (mensaje) { <p class="ok">{{ mensaje }}</p> }
    <sc-preferencias-avisos />

    @if (detalle; as d) {
      <section class="panel pad detalle-pedido">
        <div class="row">
          <h2 class="grow">Pedido N° {{ d.numero }} <span [class]="estado(d.estado).clase">{{ estado(d.estado).texto }}</span></h2>
          <button class="btn" type="button" (click)="detalle = null">Cerrar</button>
        </div>
        <p class="meta">
          Hecho el {{ fecha(d.creado) }}
          @if (d.entregado) { · entregado el {{ fecha(d.entregado) }}, la venta entró en la caja }
          @if (d.cancelado) { · cancelado el {{ fecha(d.cancelado) }} }
        </p>
        <p>
          <strong>{{ d.nombre }}</strong> · {{ d.telefonoVisible }} ·
          <a [href]="'mailto:' + d.email">{{ d.email }}</a>
        </p>
        <div class="tabla-scroll">
          <table class="data">
            <thead><tr><th>Producto</th><th class="num">Cant</th><th class="num">Precio</th><th class="num">Subt</th></tr></thead>
            <tbody>
              @for (l of d.lineas; track l.id) {
                <tr>
                  <td>{{ l.nombre }} <span class="meta">{{ l.codigo }}</span></td>
                  <td class="num">{{ l.cantidad }}</td>
                  <td class="num">{{ dinero(l.precioUnitario) }}</td>
                  <td class="num">{{ dinero(l.subTotal) }}</td>
                </tr>
              }
            </tbody>
          </table>
        </div>
        <p><strong>Total {{ dinero(d.total) }}</strong> <span class="meta">· retira en el local, paga al retirar</span></p>
        <div class="row acciones-pedido">
          @if (d.siguientes.includes('preparando')) {
            <button class="btn" type="button" [disabled]="trabajando" (click)="avanzar(d, 'preparando')">Preparando</button>
          }
          @if (d.siguientes.includes('listo')) {
            <button class="btn" type="button" [disabled]="trabajando" (click)="avanzar(d, 'listo')">Listo para retirar</button>
          }
          @if (d.siguientes.includes('entregado')) {
            <select [(ngModel)]="idMedio" aria-label="Medio de pago">
              @for (m of medios; track m.id) { <option [ngValue]="m.id">{{ m.descripcion }}</option> }
            </select>
            <button class="btn btn-primary" type="button" [disabled]="trabajando || !idMedio" (click)="entregar(d)">Entregar y cobrar</button>
          }
          @if (d.siguientes.includes('cancelado')) {
            <button class="btn btn-danger" type="button" [disabled]="trabajando" (click)="cancelar(d)">Cancelar</button>
          }
          <a class="btn" [href]="whatsApp(d)" target="_blank" rel="noopener noreferrer">Escribir al comprador</a>
        </div>
      </section>
    }

    <section class="panel tabla-scroll">
      <table class="data">
        <thead><tr><th>N°</th><th>Fecha</th><th>Comprador</th><th class="num">Productos</th><th class="num">Total</th><th>Estado</th><th></th></tr></thead>
        <tbody>
          @for (p of pedidos; track p.id) {
            <tr class="clickable" [class.selected]="detalle?.id === p.id" (click)="abrir(p.id)">
              <td class="num">{{ p.numero }}</td>
              <td>{{ fecha(p.creado) }}</td>
              <td>{{ p.nombre }}</td>
              <td class="num">{{ p.productos }}</td>
              <td class="num">{{ dinero(p.total) }}</td>
              <td><span [class]="estado(p.estado).clase">{{ estado(p.estado).texto }}</span></td>
              <td class="num"><button class="btn" type="button">Ver</button></td>
            </tr>
          } @empty {
            <tr><td colspan="7" class="meta">No hay pedidos.</td></tr>
          }
        </tbody>
      </table>
      <div class="row pad">
        <span class="meta grow">{{ total }} {{ total === 1 ? 'pedido' : 'pedidos' }}</span>
        @if (total > tamano) {
          <button class="btn" type="button" [disabled]="pagina === 1" (click)="cargar(pagina - 1)">Anterior</button>
          <span class="meta">Página {{ pagina }}</span>
          <button class="btn" type="button" [disabled]="pagina * tamano >= total" (click)="cargar(pagina + 1)">Siguiente</button>
        }
      </div>
    </section>
  `,
  styles: `
    .detalle-pedido { margin-bottom: 16px; }
    .detalle-pedido h2 { margin: 0; font-size: 18px; }
    .detalle-pedido h2 span { margin-left: 8px; vertical-align: middle; }
    .acciones-pedido { flex-wrap: wrap; align-items: center; }
  `
})
export class PanelPedidosComponent implements OnInit {
  pedidos: PedidoPanelListaDto[] = [];
  total = 0;
  pagina = 1;
  tamano = 50;
  filtro = 'activos';
  detalle: PedidoPanelDto | null = null;
  medios: MetodoPagoDto[] = [];
  idMedio = 0;
  trabajando = false;
  error = '';
  mensaje = '';

  constructor(
    private readonly api: ApiService,
    private readonly avisos: AvisosService,
    private readonly sesion: SesionService,
    private readonly dialogo: DialogoService,
    private readonly ruta: ActivatedRoute,
    private readonly destruir: DestroyRef
  ) {
    effect(() => {
      this.avisos.ultimo();
      untracked(() => this.cargar(this.pagina));
    });
  }

  ngOnInit(): void {
    this.api.mediosPago().subscribe({
      next: (r) => {
        this.medios = r.filter((m) => m.activo && (!m.integracion || m.integracion === 'manual'));
        this.idMedio = this.medios[0]?.id ?? 0;
      }
    });
    this.ruta.queryParamMap.pipe(takeUntilDestroyed(this.destruir)).subscribe((q) => {
      const id = Number(q.get('id'));
      if (id > 0) this.abrir(id);
    });
  }

  cargar(pagina: number): void {
    this.api.pedidosPanel(this.filtro, pagina).subscribe({
      next: (r) => {
        this.pedidos = r.items;
        this.total = r.total;
        this.pagina = r.pagina;
        this.tamano = r.tamano;
      },
      error: (e) => (this.error = e.status === 403 ? 'No tenés permiso para ver los pedidos de la tienda.' : 'No se pudieron cargar los pedidos.')
    });
  }

  abrir(id: number): void {
    this.mensaje = '';
    this.error = '';
    this.api.pedidoPanel(id).subscribe({
      next: (d) => (this.detalle = d),
      error: () => (this.error = 'No se pudo abrir el pedido.')
    });
  }

  avanzar(d: PedidoPanelDto, estado: EstadoPedidoTienda): void {
    this.ejecutar(this.api.avanzarPedidoPanel(d.id, estado), `Pedido N° ${d.numero}: ${this.estado(estado).texto.toLowerCase()}.`);
  }

  entregar(d: PedidoPanelDto): void {
    this.ejecutar(this.api.entregarPedidoPanel(d.id, this.idMedio), `Pedido N° ${d.numero} entregado. La venta entró en la caja.`);
  }

  async cancelar(d: PedidoPanelDto): Promise<void> {
    if (!(await this.dialogo.confirmar(`¿Cancelar el pedido N° ${d.numero}? Se libera el stock reservado.`, 'Cancelar pedido', 'Volver'))) return;
    this.ejecutar(this.api.cancelarPedidoPanel(d.id), `Pedido N° ${d.numero} cancelado.`);
  }

  whatsApp(d: PedidoPanelDto): string {
    const local = this.sesion.nombreLocal();
    const texto = d.estado === 'listo'
      ? `Hola ${d.nombre}, tu pedido N° ${d.numero} de ${local} ya está listo para retirar.`
      : `Hola ${d.nombre}, te escribimos de ${local} por tu pedido N° ${d.numero}.`;
    return `https://wa.me/${d.telefono}?text=${encodeURIComponent(texto)}`;
  }

  estado(estado: EstadoPedidoTienda): { texto: string; clase: string } {
    return ESTADOS[estado] ?? { texto: estado, clase: 'badge' };
  }

  fecha(f: string): string {
    return new Date(f).toLocaleString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' });
  }

  dinero(n: number): string {
    return `$ ${Number(n).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }

  private ejecutar(accion: ReturnType<ApiService['pedidoPanel']>, ok: string): void {
    this.trabajando = true;
    this.error = '';
    this.mensaje = '';
    accion.subscribe({
      next: (d) => {
        this.trabajando = false;
        this.detalle = d;
        this.mensaje = ok;
        this.cargar(this.pagina);
      },
      error: (e) => {
        this.trabajando = false;
        this.error = typeof e.error === 'string' ? e.error : 'No se pudo cambiar el pedido.';
      }
    });
  }
}
