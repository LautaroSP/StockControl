import { AfterViewInit, Component, ElementRef, HostListener, OnDestroy, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { toDataURL } from 'qrcode';
import {
  ApiService,
  ClienteDto,
  CobroMercadoPagoDto,
  CuentaDto,
  ItemVenta,
  MetodoPagoDto,
  ComboVigenteDto,
  OfertaVigenteDto,
  PedidoEsperaDto,
  ProductoDto,
  VentaDetalleDto
} from '../servicios/api.service';
import { DialogoService } from '../servicios/dialogo.service';
import { cantidadCobrada, descuentoCombos, precioConOferta, precioUnitarioConOferta } from '../servicios/oferta-caja';
import { CarritoCaja, LineaCarritoPendiente, SesionService } from '../servicios/sesion.service';
import { EstadoVistaCliente, VistaClienteService } from '../servicios/vista-cliente.service';
import { TicketComponent } from './ticket.component';

const FIJOS = 4;

@Component({
  selector: 'sc-caja',
  standalone: true,
  imports: [FormsModule, RouterLink, TicketComponent],
  template: `
    <div class="topbar">
      <h1>Caja</h1>
      <div class="row">
        <span class="meta">Stock bajo en naranja</span>
        <input
          #lector
          style="width:200px"
          type="text"
          [(ngModel)]="scanner"
          (keydown)="teclaLector($event)"
          placeholder="Código + Enter"
          autofocus
        />
        <button class="btn" type="button" (click)="agregarGenerico()">Genérico</button>
      </div>
    </div>
    @if (bloqueada()) {
      <section class="panel pad caja-bloqueada">
        <h2 style="margin:0 0 8px;font-size:18px">Local vencido</h2>
        <p>No se puede cobrar hasta reactivar el abono ($ {{ precioMensual() }} por mes).</p>
        <p class="meta">Podés seguir cerrando caja y consultando informes y productos.</p>
        @if (sesion.puedeConfigurarLocal()) {
          <a class="btn btn-primary" routerLink="/activar">Pagar</a>
        } @else {
          <a class="btn btn-primary" routerLink="/contacto">Contactar a ventas</a>
        }
      </section>
    } @else {
    <div class="caja-grid">
      <section class="panel list-wrap">
        <div class="toolbar" style="padding:12px">
          <input class="grow" type="search" [ngModel]="buscar" (ngModelChange)="aplicarBuscar($event)" placeholder="Buscar por nombre o código" />
          <select [ngModel]="filtroTipo" (ngModelChange)="aplicarTipo($event)">
            <option value="">Tipo: todos</option>
            <option value="comun">Común</option>
            <option value="sector">Sector</option>
          </select>
          <select [ngModel]="filtroGrupo" (ngModelChange)="aplicarGrupo($event)">
            <option value="">Grupo: todos</option>
            <option value="sin">Sin grupo</option>
            @for (g of gruposFiltro; track g.idGrupoProducto) {
              <option [value]="g.idGrupoProducto">{{ g.nombreGrupo }}</option>
            }
          </select>
          <label class="check"><input type="checkbox" [(ngModel)]="stockBajo" (ngModelChange)="cargar()" /> Stock bajo</label>
          <span class="meta">{{ totalLista }} en lista</span>
        </div>
        <div class="scroll">
          <table class="data">
            <thead>
              <tr>
                <th>Código</th>
                <th>Nombre</th>
                <th class="num">Stock</th>
                <th class="num">Precio</th>
              </tr>
            </thead>
            <tbody>
              @for (p of visibles; track p.id) {
                 <tr class="clickable" [class.low]="!p.productoSector && p.cantidad <= umbralStockBajo" (click)="agregarProducto(p)">
                  <td>{{ p.codigo }}</td>
                  <td>{{ p.nombre }}</td>
                  <td class="num">{{ p.productoSector ? '—' : p.cantidad }}</td>
                  <td class="num">{{ p.productoSector ? 'en caja' : dinero(p.precio) }}</td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </section>
      <aside class="panel cart">
        <button class="btn" type="button" (click)="abrirVistaCliente()">Vista de cliente</button>
        <h2>Carrito</h2>
        <div class="scroll" style="min-height:180px;border:1px solid var(--line);border-radius:8px">
          <table class="data">
            <thead>
              <tr>
                <th>Producto</th>
                <th class="num">Cant</th>
                <th class="num">Subt</th>
              </tr>
            </thead>
            <tbody>
              @for (l of activo.lineas; track $index) {
                <tr class="clickable" (dblclick)="sacar(l)">
                  <td>
                    {{ l.nombre }}
                    @if (l.precioAnterior != null) {
                      <span class="badge badge-aviso">Antes {{ dinero(l.precioAnterior) }}</span>
                    }
                    @if (ofertaDe(l); as o) {
                      <span class="badge badge-aviso">{{ o.descripcion }}</span>
                      @if (o.tipo === 'llevaPaga') {
                        <span class="meta">llevás {{ l.cantidad }}, pagás {{ cobradas(l) }}</span>
                      }
                    }
                    @if (l.generico || l.sector) {
                      <input
                        class="precio"
                        type="number"
                        min="0"
                        step="0.01"
                        aria-label="Precio"
                        placeholder="Precio"
                        [(ngModel)]="l.precioUnitario"
                        (keydown.enter)="enterCantidad($event)"
                        (dblclick)="$event.stopPropagation()"
                      />
                    }
                  </td>
                  <td class="num">
                    <input
                      #cant
                      class="cant"
                      type="number"
                      min="0"
                      step="1"
                      [(ngModel)]="l.cantidad"
                      (keydown.enter)="enterCantidad($event)"
                      (dblclick)="$event.stopPropagation()"
                    />
                  </td>
                  <td class="num">{{ dinero(subtotalLinea(l)) }}</td>
                </tr>
              }
            </tbody>
          </table>
        </div>
        @if (descuentoDeCombos > 0) {
          <p class="meta">Combo aplicado: ahorrás {{ dinero(descuentoDeCombos) }}</p>
        }
        <div class="total-box">
          <div>
            <div class="lbl">Ítems</div>
            <div>{{ items }}</div>
          </div>
          <div>
            <div class="lbl">Total</div>
            <div class="val">{{ dinero(total) }}</div>
          </div>
        </div>
        <div class="checks">
          <label class="check" #chkDesc>
            <input type="checkbox" [ngModel]="activo.usarDesc" (ngModelChange)="aplicarDesc($event, txtDesc)" />
            Descuento %
          </label>
          <input
            #txtDesc
            style="width:72px"
            type="number"
            min="0"
            max="100"
            [(ngModel)]="activo.descuento"
            [disabled]="!activo.usarDesc"
            (keydown.enter)="enterDescuento($event)"
            (focusout)="salirDescuento($event)"
          />
          @if (sesion.esDueno()) {
            <label class="check">
              <input type="checkbox" [ngModel]="activo.alCosto" (ngModelChange)="aplicarAlCosto($event)" />
              Cobrar al costo
            </label>
          }
          <label class="check">
            <input type="checkbox" [(ngModel)]="activo.imprimirTicket" />
            Imprimir ticket
          </label>
          @if (hayOfertaOpcional() || hayCombo()) {
            <label class="check">
              <input type="checkbox" [(ngModel)]="activo.sinOfertas" />
              No aplicar ofertas
            </label>
          }
        </div>
        <div class="payment-list" (keydown)="teclaMedio($event)">
          @for (p of activo.pagos; track $index) {
            <div class="row">
              <select [(ngModel)]="p.idMetodoPago">
                @for (m of mediosPago; track m.id) {
                  @if (m.activo && medioVisible(m)) { <option [ngValue]="m.id">{{ m.descripcion }}</option> }
                }
              </select>
              <input class="grow" type="number" min="0.01" [ngModel]="importePago(p, $index)" (ngModelChange)="p.importe = $event" [disabled]="activo.pagos.length === 1 || p.resto" />
              @if (activo.pagos.length > 1) {
                <label class="check"><input type="checkbox" [ngModel]="p.resto" (ngModelChange)="marcarResto($index, $event)" /> Resto</label>
                <button class="btn" type="button" (click)="quitarPago($index)">Quitar</button>
              }
            </div>
          }
          <button class="btn" type="button" [disabled]="mediosPago.length < 2" (click)="agregarPago()">Agregar otro medio</button>
        </div>
        <div class="tabs">
          @for (c of carritos; track c.id; let i = $index) {
            <button class="tab" type="button" [class.on]="i === indiceActivo" [disabled]="cobrando" (click)="seleccionar(i)">
              {{ titulo(i) }}
            </button>
          }
          <button class="btn" type="button" [disabled]="cobrando" (click)="crearCarrito()">+</button>
          <button class="btn" type="button" [disabled]="cobrando || indiceActivo < fijos" (click)="cerrarCarrito()">×</button>
        </div>
        @if (esperando) {
          <div class="panel pad">
            <p>Esperando el pago · {{ dinero(esperando.importe) }}</p>
            @if (esperando.modo === 'qr_pantalla' && qrEspera) {
              <img [src]="qrEspera" alt="QR de Mercado Pago" style="width:220px;height:220px;background:#fff" />
            }
            @if (esperando.modo === 'qr_impreso') {
              <p class="meta">El cliente escanea el QR impreso de este puesto.</p>
            }
            @if (esperando.modo === 'point') {
              <p class="meta">El monto está en el lector.</p>
            }
            <p class="meta">{{ textoEspera(esperando.estado) }}</p>
            <button class="btn" type="button" (click)="cancelarEspera()">Cancelar cobro</button>
          </div>
        } @else {
        <div class="row">
          <button #btnCobrar class="btn btn-charge grow" type="button" [disabled]="cobrando || activo.lineas.length === 0" (click)="cobrar()">
            COBRAR
          </button>
          <button class="btn" type="button" (click)="cancelar()">Cancelar</button>
        </div>
        @if (puedeFiar) {
          <div class="row">
            <button class="btn grow" type="button" [disabled]="cobrando || activo.lineas.length === 0" (click)="abrirFiar()">Dejar a cuenta</button>
          </div>
        }
        @if (fiando) {
          <div class="panel pad">
            <p><strong>Dejar a cuenta · {{ dinero(total) }}</strong></p>
            <input type="search" [(ngModel)]="buscarCliente" (ngModelChange)="buscarClientes()" placeholder="Buscar cliente" />
            @for (c of clientes; track c.id) {
              <div class="row">
                <span class="grow">{{ c.nombre }} @if (c.telefono) { <span class="meta">{{ c.telefono }}</span> }</span>
                <button class="btn btn-primary" type="button" [disabled]="cobrando" (click)="fiar(c.id)">Elegir</button>
              </div>
            }
            <div class="row">
              <input class="grow" type="text" maxlength="80" [(ngModel)]="nuevoNombre" placeholder="Cliente nuevo" />
              <input type="tel" maxlength="30" style="width:140px" [(ngModel)]="nuevoTelefono" placeholder="Teléfono (opcional)" />
            </div>
            <div class="row">
              <button class="btn btn-primary grow" type="button" [disabled]="cobrando || !nuevoNombre.trim()" (click)="fiar()">Crear y dejar a cuenta</button>
              <button class="btn" type="button" (click)="cerrarFiar()">Volver</button>
            </div>
          </div>
        }
        <div class="row">
          <button class="btn grow" type="button" [disabled]="cobrando || activo.lineas.length === 0" (click)="guardarEnEspera()">
            Guardar en espera
          </button>
          <button class="btn grow" type="button" [disabled]="cobrando" (click)="alternarEspera()">
            Pedidos en espera ({{ pedidosEspera.length }})
          </button>
        </div>
        @if (verEspera) {
          <div class="panel pad">
            @for (p of pedidosEspera; track p.id) {
              <div class="row">
                <span class="grow">{{ p.nota || 'Sin nota' }} · {{ p.productos }} {{ p.productos === 1 ? 'producto' : 'productos' }} · {{ hora(p.fecha) }}</span>
                <button class="btn" type="button" (click)="cargarEspera(p.id)">Cargar</button>
                <button class="btn btn-danger" type="button" (click)="borrarEspera(p)">Borrar</button>
              </div>
            } @empty {
              <p class="meta">No hay pedidos en espera en este puesto.</p>
            }
          </div>
        }
        }
        @if (mensaje) {
          <p class="meta">{{ mensaje }}</p>
        }
        @if (error) {
          <p class="error">{{ error }}</p>
        }
      </aside>
    </div>
    }
    <div class="toast" [class.show]="toast">{{ toast }}</div>
    @if (ticket) {
      <sc-ticket [venta]="ticket" [nombreLocal]="nombreTicket" [formato]="formatoTicket" [pendiente]="ticketPendiente" />
    }
  `
})
export class CajaComponent implements OnInit, AfterViewInit, OnDestroy {
  readonly fijos = FIJOS;
  @ViewChild('lector') private lector?: ElementRef<HTMLInputElement>;
  @ViewChild('chkDesc') private chkDesc?: ElementRef<HTMLElement>;
  @ViewChild('btnCobrar') private btnCobrar?: ElementRef<HTMLButtonElement>;
  @ViewChildren('cant') private cantidades?: QueryList<ElementRef<HTMLInputElement>>;
  productos: ProductoDto[] = [];
  visibles: ProductoDto[] = [];
  totalLista = 0;
  mediosPago: MetodoPagoDto[] = [];
  private proximoId = 1;
  private imprimirDefault = true;
  carritos: CarritoCaja[] = [this.vacio(), this.vacio(), this.vacio(), this.vacio()];
  indiceActivo = 0;
  buscar = '';
  filtroTipo = '';
  filtroGrupo = '';
  stockBajo = false;
  umbralStockBajo = 5;
  gruposFiltro: { idGrupoProducto: number; nombreGrupo: string }[] = [];
  scanner = '';
  cobrando = false;
  mensaje = '';
  mpVinculado = false;
  esperando: CobroMercadoPagoDto | null = null;
  qrEspera = '';
  pedidosEspera: PedidoEsperaDto[] = [];
  verEspera = false;
  private ofertas = new Map<number, OfertaVigenteDto>();
  private combos: ComboVigenteDto[] = [];
  puedeFiar = false;
  fiando = false;
  buscarCliente = '';
  clientes: ClienteDto[] = [];
  nuevoNombre = '';
  nuevoTelefono = '';
  ticketPendiente: number | null = null;
  private relojEspera: ReturnType<typeof setInterval> | null = null;
  private relojOcio: ReturnType<typeof setTimeout> | null = null;
  private relojVista: ReturnType<typeof setInterval> | null = null;
  private qrVista = '';
  private totalUltimoCobro = 0;
  private jsonVista = '';
  private cantidadesVista: number[] = [];
  private idCarritoVista = 0;
  private ultimaVista = { linea: -1, marca: 0 };
  private graciasVista: { total: number; medio: string; hasta: number } | null = null;
  private medioUltimoCobro = '';
  private latidoVista = 0;
  private descuentoPorEnter = false;
  error = '';
  toast = '';
  ticket: VentaDetalleDto | null = null;
  formatoTicket = 'POS-80';
  nombreTicket = '';

  constructor(
    private readonly api: ApiService,
    readonly sesion: SesionService,
    private readonly dialogo: DialogoService,
    private readonly vista: VistaClienteService
  ) {}

  get activo(): CarritoCaja {
    return this.carritos[this.indiceActivo] ?? this.carritos[0];
  }

  get items(): number {
    return this.activo.lineas.reduce((n, l) => n + Number(l.cantidad), 0);
  }

  get total(): number {
    return Math.round((this.activo.lineas.reduce((n, l) => n + this.subtotalLinea(l), 0) - this.descuentoDeCombos) * 100) / 100;
  }

  get descuentoDeCombos(): number {
    const a = this.activo;
    if (a.sinOfertas || a.alCosto || this.descuentoCaja() > 0) return 0;
    const lineas = a.lineas
      .filter((l) => !l.sector && !l.generico)
      .map((l) => ({ idProducto: l.idProducto, cantidad: Number(l.cantidad), subtotal: this.subtotalLinea(l) }));
    return descuentoCombos(this.combos, lineas);
  }

  hayCombo(): boolean {
    const ids = new Set(this.activo.lineas.map((l) => l.idProducto));
    return this.combos.some((c) => c.lineas.every((x) => ids.has(x.idProducto)));
  }

  ofertaDe(l: LineaCarritoPendiente): OfertaVigenteDto | undefined {
    const o = this.ofertaVigente(l);
    if (!o) return undefined;
    if (this.activo.sinOfertas && o.permiteNoAplicarEnCaja) return undefined;
    if (this.activo.alCosto && o.anulaConAlCosto) return undefined;
    if (this.descuentoCaja() > 0 && o.anulaConDescuentoCaja) return undefined;
    return o;
  }

  hayOfertaOpcional(): boolean {
    return this.activo.lineas.some((l) => this.ofertaVigente(l)?.permiteNoAplicarEnCaja);
  }

  private ofertaVigente(l: LineaCarritoPendiente): OfertaVigenteDto | undefined {
    return l.sector || l.generico ? undefined : this.ofertas.get(l.idProducto);
  }

  private descuentoCaja(): number {
    return this.activo.usarDesc ? Number(this.activo.descuento) || 0 : 0;
  }

  cobradas(l: LineaCarritoPendiente): number {
    return cantidadCobrada(this.ofertaDe(l), Number(l.cantidad));
  }

  subtotalLinea(l: LineaCarritoPendiente): number {
    return Math.round(this.precioLinea(l) * this.cobradas(l) * 100) / 100;
  }

  precioLinea(l: LineaCarritoPendiente): number {
    if (l.sector || l.generico) return Number(l.precioUnitario);
    if (this.activo.alCosto) return precioConOferta(this.ofertaDe(l), Number(l.costo));
    const desc = this.activo.usarDesc ? Math.min(100, Math.max(0, Number(this.activo.descuento) || 0)) : 0;
    const lista = precioUnitarioConOferta(this.ofertaDe(l), Number(l.precioUnitario), Number(l.cantidad));
    const costo = Number(l.costo);
    const margen = Math.max(0, lista - costo);
    return Math.round((lista - (margen * desc) / 100) * 100) / 100;
  }

  aplicarDesc(v: boolean, el: HTMLInputElement): void {
    this.activo.usarDesc = v;
    if (!v) {
      this.activo.descuento = 0;
      this.focoLector();
      return;
    }
    queueMicrotask(() => {
      el.focus();
      el.select();
    });
  }

  async aplicarAlCosto(v: boolean): Promise<void> {
    if (v && !await this.dialogo.confirmar('¿Cobrar esta venta al costo?')) {
      this.activo.alCosto = false;
      this.focoLector();
      return;
    }
    this.activo.alCosto = v;
    this.focoLector();
  }

  titulo(indice: number): string {
    const n = indice + 1;
    return indice < FIJOS ? `${n} F${n}` : String(n);
  }

  seleccionar(indice: number): void {
    if (this.cobrando || indice < 0 || indice >= this.carritos.length) return;
    this.indiceActivo = indice;
    this.focoLector();
  }

  crearCarrito(): void {
    this.carritos = [...this.carritos, this.vacio()];
    this.indiceActivo = this.carritos.length - 1;
    this.focoLector();
  }

  async cerrarCarrito(): Promise<void> {
    if (this.indiceActivo < FIJOS) return;
    if (this.activo.lineas.length > 0 && !await this.dialogo.confirmar('Este carrito tiene productos. ¿Desea cerrarlo?')) {
      this.focoLector();
      return;
    }
    const i = this.indiceActivo;
    this.carritos = this.carritos.filter((_, n) => n !== i);
    this.indiceActivo = Math.min(i, this.carritos.length - 1);
    this.focoLector();
  }

  @HostListener('window:keydown', ['$event'])
  atajo(e: KeyboardEvent): void {
    if (this.dialogo.visible()) return;
    if (e.key !== 'F1' && e.key !== 'F2' && e.key !== 'F3' && e.key !== 'F4') return;
    e.preventDefault();
    this.seleccionar(Number(e.key.slice(1)) - 1);
  }

  @HostListener('window:focus')
  @HostListener('document:visibilitychange')
  alVolver(): void {
    if (document.visibilityState === 'hidden') return;
    this.cargarOfertas();
    this.focoLector();
  }

  @HostListener('document:keydown')
  @HostListener('document:pointerdown')
  actividad(): void {
    this.reiniciarOcio();
  }

  teclaLector(e: KeyboardEvent): void {
    if (e.key === 'Enter') {
      e.preventDefault();
      this.escanear();
      return;
    }
    if (!this.esMas(e)) return;
    e.preventDefault();
    this.focoMedio();
  }

  teclaMedio(e: KeyboardEvent): void {
    if (!this.esMas(e)) return;
    const primero = (e.currentTarget as HTMLElement).querySelector('select');
    if (e.target !== primero) return;
    e.preventDefault();
    this.btnCobrar?.nativeElement.focus();
  }

  enterCantidad(e: Event): void {
    e.preventDefault();
    this.focoLector();
  }

  enterDescuento(e: Event): void {
    e.preventDefault();
    this.descuentoPorEnter = true;
    this.focoLector();
  }

  salirDescuento(e: FocusEvent): void {
    if (this.descuentoPorEnter) {
      this.descuentoPorEnter = false;
      return;
    }
    const destino = e.relatedTarget as Node | null;
    if (this.chkDesc?.nativeElement.contains(destino)) return;
    if (destino === this.lector?.nativeElement) return;
    if (!this.activo.usarDesc) return;
    this.focoMedio();
  }

  ngOnInit(): void {
    const guardados = this.sesion.tomarCarritos();
    if (guardados && guardados.carritos.length >= FIJOS) {
      this.carritos = guardados.carritos;
      this.indiceActivo = Math.min(guardados.indice, this.carritos.length - 1);
      this.proximoId = Math.max(...this.carritos.map((c) => c.id), FIJOS) + 1;
    }
    const pend = this.sesion.tomarCarritoPendiente();
    if (pend?.length) this.activo.lineas = pend.map((l) => ({ ...l }));
    this.api.cuentaMercadoPago().subscribe({ next: (r) => (this.mpVinculado = r.vinculada) });
    this.api.mediosPago().subscribe({
      next: (r) => {
        this.mediosPago = r;
        const id = r.find((m) => m.activo && this.medioVisible(m))?.id ?? r[0]?.id;
        if (!id) return;
        for (const c of this.carritos) {
          if (c.pagos.length === 0) c.pagos = [{ idMetodoPago: id, importe: 0, resto: false }];
        }
      },
      error: () => (this.error = 'No se pudieron cargar los medios de pago.')
    });
    this.nombreTicket = this.sesion.nombreLocal();
    this.api.configuracionTicket().subscribe({
      next: (c) => {
        this.imprimirDefault = c.imprimirTicketAlCobrar;
        this.formatoTicket = c.formatoTicket || 'POS-80';
        this.nombreTicket = c.nombreLocal || this.nombreTicket;
        if (!guardados) {
          for (const cart of this.carritos) cart.imprimirTicket = this.imprimirDefault;
        }
      }
    });
    this.api.gruposFiltro().subscribe({ next: (r) => (this.gruposFiltro = r) });
    this.reanudarEspera();
    this.refrescarEspera();
    this.cargarOfertas();
    this.api.permisosCuentas().subscribe({ next: (p) => (this.puedeFiar = p.puedeFiar) });
    this.cargar();
    this.vista.abrir(this.sesion.idLocal(), this.sesion.nroCaja());
    this.relojVista = setInterval(() => this.emitirVista(), 250);
  }

  ngAfterViewInit(): void {
    this.focoLector();
    this.reiniciarOcio();
  }

  ngOnDestroy(): void {
    this.pararEspera();
    if (this.relojVista) clearInterval(this.relojVista);
    this.vista.cerrar();
    if (this.relojOcio) clearTimeout(this.relojOcio);
    this.sesion.guardarCarritos(this.carritos, this.indiceActivo);
  }

  medioVisible(medio: MetodoPagoDto): boolean {
    return !medio.integracion || medio.integracion === 'manual' || this.mpVinculado;
  }

  textoEspera(estado: string): string {
    if (estado === 'pagado') return 'Pagado';
    if (estado === 'cancelado') return 'Cancelado';
    if (estado === 'vencido') return 'Vencido';
    return 'Esperando';
  }

  dinero(n: number): string {
    return `$ ${Number(n).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }

  cargar(): void {
    const sector = this.filtroTipo === 'sector' ? true : this.filtroTipo === 'comun' ? false : undefined;
    this.api.productos(this.buscar, {
      sector,
      idGrupo: this.filtroGrupo && this.filtroGrupo !== 'sin' ? Number(this.filtroGrupo) : undefined,
      sinGrupo: this.filtroGrupo === 'sin',
      stockBajo: this.stockBajo
    }).subscribe({
      next: (r) => {
        this.productos = r.items;
        this.visibles = r.items;
        this.totalLista = r.total;
        this.umbralStockBajo = r.umbralStockBajo;
        this.error = '';
      },
      error: () => (this.error = 'No se pudieron cargar los productos.')
    });
  }

  aplicarTipo(tipo: string): void {
    this.filtroTipo = tipo;
    this.cargar();
  }

  aplicarBuscar(q: string): void {
    this.buscar = q;
    this.cargar();
  }

  filtrar(): void {
    this.cargar();
  }

  escanear(): void {
    const codigo = this.scanner.trim();
    if (!codigo) {
      this.focoLector();
      return;
    }
    this.scanner = '';
    const local = this.productos.find((x) => x.codigo.toLowerCase() === codigo.toLowerCase());
    if (local) {
      void this.agregarProducto(local).finally(() => this.focoLector());
      return;
    }
    this.api.productos('', { codigo }).subscribe({
      next: (r) => {
        const p = r.items[0];
        if (p) void this.agregarProducto(p).finally(() => this.focoLector());
        else {
          this.error = `No hay producto ${codigo}`;
          this.focoLector();
        }
      },
      error: () => {
        this.error = `No hay producto ${codigo}`;
        this.focoLector();
      }
    });
  }

  async agregarProducto(p: ProductoDto): Promise<void> {
    this.error = '';
    if (p.productoSector) {
      const raw = await this.dialogo.pedir(`Precio de ${p.nombre}`);
      const precio = Number((raw ?? '').replace(',', '.'));
      if (!raw || !(precio > 0)) return;
      this.activo.lineas.push({
        idProducto: p.id,
        codigo: p.codigo,
        nombre: p.nombre,
        cantidad: 1,
        precioUnitario: precio,
        costo: p.costo,
        sector: true,
        generico: false
      });
      return;
    }
    const ya = this.activo.lineas.find((l) => l.idProducto === p.id && !l.generico && !l.sector);
    if (ya) ya.cantidad = Number(ya.cantidad) + 1;
    else {
      this.activo.lineas.push({
        idProducto: p.id,
        codigo: p.codigo,
        nombre: p.nombre,
        cantidad: 1,
        precioUnitario: p.precio,
        costo: p.costo,
        sector: false,
        generico: false
      });
    }
  }

  async agregarGenerico(): Promise<void> {
    const nombre = await this.dialogo.pedir('Nombre del genérico', 'Producto suelto');
    if (!nombre) {
      this.focoLector();
      return;
    }
    const raw = await this.dialogo.pedir('Precio');
    const precio = Number((raw ?? '').replace(',', '.'));
    if (!raw || !(precio > 0)) {
      this.focoLector();
      return;
    }
    this.activo.lineas.push({
      idProducto: 0,
      codigo: `GENERIC-${crypto.randomUUID().slice(0, 8)}`,
      nombre,
      cantidad: 1,
      precioUnitario: precio,
      costo: 0,
      sector: false,
      generico: true
    });
    setTimeout(() => this.enfocarCantidad());
  }

  sacar(l: LineaCarritoPendiente): void {
    this.activo.lineas = this.activo.lineas.filter((x) => x !== l);
    this.focoLector();
  }

  cancelar(): void {
    this.vaciarActivo();
    this.error = '';
    this.mensaje = '';
    this.focoLector();
  }

  cobrar(): void {
    if (this.activo.lineas.length === 0) return;
    this.cobrando = true;
    this.error = '';
    this.guardarDatosCobroVista();
    const items = this.itemsActivo();
    const idEspera = this.activo.idPedidoEspera;
    const desc = this.activo.usarDesc ? Number(this.activo.descuento) || 0 : 0;
    const pagos = this.activo.pagos.map((p, i) => ({ idMetodoPago: p.idMetodoPago, importe: this.importePago(p, i) }));
    const imprimir = this.activo.imprimirTicket;
    if (pagos.some((p) => this.esMercadoPago(p.idMetodoPago))) {
      const importeQr = pagos.filter((p) => this.esQr(p.idMetodoPago)).reduce((s, p) => s + Number(p.importe), 0);
      if (importeQr > 0 && importeQr < 15) {
        this.error = 'El QR de Mercado Pago pide al menos $ 15.';
        this.cobrando = false;
        return;
      }
      this.api.iniciarCobroMercadoPago(items, pagos, desc, this.sesion.esDueno() && this.activo.alCosto, idEspera, !!this.activo.sinOfertas).subscribe({
        next: (r) => {
          this.activo.idCobro = r.id;
          this.mostrarEspera(r, imprimir);
        },
        error: (err) => {
          this.error = typeof err.error === 'string' ? err.error : 'No se pudo iniciar el cobro.';
          this.cobrando = false;
          if (err.status === 409) this.refrescarEspera();
        }
      });
      return;
    }
    this.api.cobrar(items, pagos, desc, this.sesion.esDueno() && this.activo.alCosto, idEspera, !!this.activo.sinOfertas).subscribe({
      next: (r) => {
        this.toast = `Cobrado ${this.dinero(r.total)}`;
        this.avisarGracias();
        this.cerrarTrasCobro();
        this.cobrando = false;
        this.cargar();
        if (idEspera) this.refrescarEspera();
        if (imprimir) this.imprimirVenta(r.idInformeVenta, r.total);
        else {
          this.focoLector();
          setTimeout(() => (this.toast = ''), 2500);
        }
      },
      error: (err) => {
        this.error = typeof err.error === 'string' ? err.error : 'No se pudo cobrar.';
        this.cobrando = false;
        if (err.status === 402) this.api.abonoActual().subscribe({ next: (a) => this.sesion.abono.set(a) });
        if (err.status === 409) this.refrescarEspera();
      }
    });
  }

  async guardarEnEspera(): Promise<void> {
    if (this.activo.lineas.length === 0) return;
    const nota = await this.dialogo.pedir('Nota del pedido (ej. María, pedido de las 11)');
    if (nota === null) {
      this.focoLector();
      return;
    }
    const carrito = this.activo;
    const anterior = carrito.idPedidoEspera;
    const desc = carrito.usarDesc ? Number(carrito.descuento) || 0 : 0;
    this.api.guardarPedidoEspera(this.itemsActivo(), nota.slice(0, 80), desc, this.sesion.esDueno() && carrito.alCosto).subscribe({
      next: () => {
        const listo = () => this.refrescarEspera();
        if (anterior) this.api.borrarPedidoEspera(anterior).subscribe({ next: listo, error: listo });
        else listo();
        if (carrito === this.activo) this.cerrarTrasCobro();
        this.error = '';
        this.mensaje = '';
        this.toast = 'Pedido guardado en espera';
        setTimeout(() => (this.toast = ''), 2500);
        this.focoLector();
      },
      error: (err) => (this.error = typeof err.error === 'string' ? err.error : 'No se pudo guardar el pedido.')
    });
  }

  abrirFiar(): void {
    this.fiando = true;
    this.buscarCliente = '';
    this.nuevoNombre = '';
    this.nuevoTelefono = '';
    this.buscarClientes();
  }

  cerrarFiar(): void {
    this.fiando = false;
    this.focoLector();
  }

  buscarClientes(): void {
    this.api.clientes(this.buscarCliente.trim()).subscribe({ next: (r) => (this.clientes = r) });
  }

  fiar(idCliente?: number): void {
    if (this.activo.lineas.length === 0) return;
    this.cobrando = true;
    this.error = '';
    const imprimir = this.activo.imprimirTicket;
    this.api.fiar({
      items: this.itemsActivo(),
      descuentoPorcentaje: this.descuentoCaja(),
      cobrarAlCosto: this.sesion.esDueno() && this.activo.alCosto,
      sinOfertas: !!this.activo.sinOfertas,
      idCliente,
      clienteNuevo: idCliente ? undefined : { nombre: this.nuevoNombre.trim(), telefono: this.nuevoTelefono.trim() }
    }).subscribe({
      next: (c) => {
        this.cobrando = false;
        this.fiando = false;
        this.cerrarTrasCobro();
        this.cargar();
        this.toast = `A cuenta de ${c.cliente}: ${this.dinero(c.importe)}`;
        if (imprimir) this.imprimirCuenta(c);
        else {
          this.focoLector();
          setTimeout(() => (this.toast = ''), 2500);
        }
      },
      error: (err) => {
        this.cobrando = false;
        this.error = typeof err.error === 'string' ? err.error : 'No se pudo dejar a cuenta.';
        if (err.status === 402) this.api.abonoActual().subscribe({ next: (a) => this.sesion.abono.set(a) });
      }
    });
  }

  private imprimirCuenta(c: CuentaDto): void {
    this.ticket = {
      idInformeVenta: c.id,
      fecha: c.fecha,
      total: c.importe,
      subtotal: c.importe,
      metodoPago: `A cuenta de ${c.cliente}`,
      descuento: 0,
      precioCosto: 'NO',
      pagos: [],
      items: c.lineas.map((l) => ({
        idInformeVentaDetalle: l.id,
        idProducto: l.idProducto,
        codigo: l.codigo,
        nombre: l.nombre,
        cantidad: l.cantidad,
        precio: l.precio,
        subTotal: l.subTotal,
        descripcionOferta: l.descripcionOferta
      }))
    };
    this.ticketPendiente = c.saldo;
    setTimeout(() => {
      try {
        window.print();
      } catch {
        /* el diálogo no corrió */
      }
      this.ticket = null;
      this.ticketPendiente = null;
      this.focoLector();
      setTimeout(() => (this.toast = ''), 4000);
    }, 50);
  }

  alternarEspera(): void {
    this.verEspera = !this.verEspera;
    if (this.verEspera) this.refrescarEspera();
  }

  async cargarEspera(id: number): Promise<void> {
    if (this.activo.lineas.length > 0 && !await this.dialogo.confirmar('El carrito actual se reemplaza por el pedido. ¿Seguir?')) {
      this.focoLector();
      return;
    }
    this.api.cargarPedidoEspera(id).subscribe({
      next: (p) => {
        const c = this.activo;
        c.lineas = p.lineas.map((l) => ({ ...l }));
        c.usarDesc = p.descuentoPorcentaje > 0;
        c.descuento = p.descuentoPorcentaje;
        c.alCosto = p.alCosto && this.sesion.esDueno();
        c.idPedidoEspera = p.id;
        this.verEspera = false;
        this.error = p.faltantes.length ? `No se cargó porque ya no está en el catálogo: ${p.faltantes.join(', ')}.` : '';
        this.mensaje = p.lineas.some((l) => l.precioAnterior != null) ? 'Hay precios que cambiaron desde que se guardó.' : '';
        this.focoLector();
      },
      error: (err) => {
        this.error = typeof err.error === 'string' ? err.error : 'No se pudo cargar el pedido.';
        this.refrescarEspera();
      }
    });
  }

  async borrarEspera(p: PedidoEsperaDto): Promise<void> {
    if (!await this.dialogo.confirmar(`¿Borrar el pedido "${p.nota || 'Sin nota'}"?`)) return;
    this.api.borrarPedidoEspera(p.id).subscribe({
      next: () => this.quitarEspera(p.id),
      error: () => this.quitarEspera(p.id)
    });
  }

  hora(fecha: string): string {
    return new Date(fecha).toLocaleString('es-AR', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' });
  }

  private quitarEspera(id: number): void {
    for (const c of this.carritos) {
      if (c.idPedidoEspera === id) c.idPedidoEspera = undefined;
    }
    this.refrescarEspera();
  }

  private cargarOfertas(): void {
    this.api.ofertasVigentes().subscribe({ next: (r) => (this.ofertas = new Map(r.map((o) => [o.idProducto, o]))) });
    this.api.combosVigentes().subscribe({ next: (r) => (this.combos = r) });
  }

  private refrescarEspera(): void {
    this.api.pedidosEspera().subscribe({ next: (r) => (this.pedidosEspera = r) });
  }

  private itemsActivo(): ItemVenta[] {
    return this.activo.lineas.map((l) => ({
      idProducto: l.idProducto,
      codigo: l.codigo,
      nombre: l.nombre,
      cantidad: Number(l.cantidad),
      precioUnitario: Number(l.precioUnitario)
    }));
  }

  bloqueada(): boolean {
    return this.sesion.abono()?.puedeOperarCaja === false;
  }

  precioMensual(): string {
    return Math.trunc(this.sesion.abono()?.precioMensual ?? 0).toLocaleString('es-AR');
  }

  aplicarGrupo(grupo: string): void {
    this.filtroGrupo = grupo;
    this.cargar();
  }

  agregarPago(): void {
    const disponible = this.mediosPago.find((m) => m.activo && !this.activo.pagos.some((p) => p.idMetodoPago === m.id));
    if (disponible) this.activo.pagos = [...this.activo.pagos, { idMetodoPago: disponible.id, importe: 0, resto: false }];
  }

  quitarPago(indice: number): void {
    this.activo.pagos = this.activo.pagos.filter((_, i) => i !== indice);
  }

  marcarResto(indice: number, resto: boolean): void {
    this.activo.pagos = this.activo.pagos.map((p, i) => ({ ...p, resto: resto && i === indice }));
  }

  importePago(pago: CarritoCaja['pagos'][number], indice: number): number {
    if (this.activo.pagos.length === 1) return this.total;
    if (!pago.resto) return Number(pago.importe);
    const usado = this.activo.pagos
      .filter((_, i) => i !== indice)
      .reduce((suma, p) => suma + Number(p.importe), 0);
    return Math.max(0, Math.round((this.total - usado) * 100) / 100);
  }

  private esMercadoPago(id: number): boolean {
    const integracion = this.mediosPago.find((m) => m.id === id)?.integracion;
    return integracion === 'qr_impreso' || integracion === 'qr_pantalla' || integracion === 'point';
  }

  private esQr(id: number): boolean {
    const integracion = this.mediosPago.find((m) => m.id === id)?.integracion;
    return integracion === 'qr_impreso' || integracion === 'qr_pantalla';
  }

  private reanudarEspera(): void {
    this.api.cobroMercadoPagoAbierto().subscribe({
      next: (r) => {
        if (!r) return;
        const i = this.carritos.findIndex((c) => c.idCobro === r.id);
        if (i >= 0) this.indiceActivo = i;
        this.activo.idCobro = r.id;
        this.mostrarEspera(r, this.activo.imprimirTicket);
      }
    });
  }

  private mostrarEspera(r: CobroMercadoPagoDto, imprimir: boolean): void {
    if (r.estado !== 'esperando') {
      this.cerrarEspera(r, imprimir);
      return;
    }
    this.esperando = r;
    this.cobrando = true;
    this.qrEspera = '';
    if (r.modo === 'qr_pantalla' && r.qrData) {
      void toDataURL(r.qrData, { margin: 1, width: 220 }).then((url) => (this.qrEspera = url));
      void toDataURL(r.qrData, { margin: 1, width: 600 }).then((url) => (this.qrVista = url));
    }
    this.pararEspera();
    this.relojEspera = setInterval(() => this.consultarEspera(imprimir), 3000);
  }

  private consultarEspera(imprimir: boolean): void {
    if (!this.esperando) return;
    this.api.consultarCobroMercadoPago(this.esperando.id).subscribe({
      next: (r) => {
        if (r.estado === 'esperando') {
          this.esperando = r;
          return;
        }
        this.cerrarEspera(r, imprimir);
      }
    });
  }

  private cerrarEspera(r: CobroMercadoPagoDto, imprimir: boolean): void {
    this.pararEspera();
    this.cobrando = false;
    this.esperando = null;
    this.qrEspera = '';
    this.qrVista = '';
    for (const c of this.carritos) {
      if (c.idCobro === r.id) c.idCobro = undefined;
    }
    if (r.estado === 'pagado' && r.idInformeVenta) {
      this.avisarGracias();
      this.toast = `Cobrado ${this.dinero(r.importe)}`;
      this.cerrarTrasCobro();
      this.cargar();
      this.refrescarEspera();
      if (imprimir) this.imprimirVenta(r.idInformeVenta, r.importe);
      else {
        this.focoLector();
        setTimeout(() => (this.toast = ''), 2500);
      }
      return;
    }
    this.error = r.estado === 'vencido' ? 'El cobro venció.' : 'El cobro se canceló.';
    this.focoLector();
  }

  cancelarEspera(): void {
    if (!this.esperando) return;
    const id = this.esperando.id;
    this.api.cancelarCobroMercadoPago(id).subscribe({
      next: (r) => {
        this.cerrarEspera(r, false);
        this.error = '';
        this.mensaje = 'Cobro cancelado. El carrito sigue.';
      },
      error: (err) => (this.error = typeof err.error === 'string' ? err.error : 'No se pudo cancelar el cobro.')
    });
  }

  private pararEspera(): void {
    if (this.relojEspera) clearInterval(this.relojEspera);
    this.relojEspera = null;
  }

  private esMas(e: KeyboardEvent): boolean {
    return e.key === '+' || e.code === 'NumpadAdd';
  }

  private enfocarCantidad(): void {
    const campo = this.cantidades?.last?.nativeElement;
    if (!campo) return;
    campo.focus();
    campo.select();
  }

  private focoLector(): void {
    if (!this.puedeRobarFoco()) return;
    this.lector?.nativeElement.focus();
  }

  private focoMedio(): void {
    if (!this.puedeRobarFoco()) return;
    const medio = document.querySelector('.payment-list select');
    if (!(medio instanceof HTMLSelectElement)) return;
    medio.focus();
    const abrir = medio as HTMLSelectElement & { showPicker?: () => void };
    try {
      abrir.showPicker?.();
    } catch {
      /* el navegador no abre el combo sin un gesto */
    }
  }

  private puedeRobarFoco(): boolean {
    return !this.dialogo.visible() && !this.bloqueada() && document.visibilityState !== 'hidden';
  }

  private reiniciarOcio(): void {
    if (this.relojOcio) clearTimeout(this.relojOcio);
    this.relojOcio = setTimeout(() => {
      if (!this.puedeRobarFoco()) {
        this.reiniciarOcio();
        return;
      }
      this.focoLector();
    }, 10000);
  }

  private vacio(): CarritoCaja {
    const idMedio = this.mediosPago.find((m) => m.activo)?.id ?? this.mediosPago[0]?.id;
    return {
      id: this.proximoId++,
      lineas: [],
      pagos: idMedio ? [{ idMetodoPago: idMedio, importe: 0, resto: false }] : [],
      usarDesc: false,
      descuento: 0,
      alCosto: false,
      imprimirTicket: this.imprimirDefault
    };
  }

  private vaciarActivo(): void {
    const c = this.activo;
    c.lineas = [];
    c.usarDesc = false;
    c.descuento = 0;
    c.alCosto = false;
    c.idPedidoEspera = undefined;
    c.sinOfertas = false;
    if (c.pagos.length > 1) c.pagos = c.pagos.slice(0, 1);
  }

  private imprimirVenta(id: number, total: number): void {
    this.api.venta(id).subscribe({
      next: (venta) => {
        this.ticket = venta;
        setTimeout(() => {
          try {
            window.print();
          } catch {
            /* el diálogo no corrió */
          }
          this.ticket = null;
          this.toast = `Cobrado ${this.dinero(total)}. Si el ticket no salió, la venta igual está guardada.`;
          this.focoLector();
          setTimeout(() => (this.toast = ''), 4000);
        }, 50);
      },
      error: () => {
        this.toast = `Cobrado ${this.dinero(total)}. No se pudo armar el ticket; la venta está guardada.`;
        this.focoLector();
        setTimeout(() => (this.toast = ''), 4000);
      }
    });
  }

  abrirVistaCliente(): void {
    window.open(`/vista-cliente?caja=${this.sesion.nroCaja()}`, '_blank');
  }

  private guardarDatosCobroVista(): void {
    const ids = new Set(this.activo.pagos.map((p) => p.idMetodoPago));
    const nombre = this.mediosPago.find((m) => m.id === [...ids][0])?.descripcion ?? '';
    this.medioUltimoCobro = ids.size === 1 ? nombre : 'Varios medios';
    this.totalUltimoCobro = this.total;
  }

  private avisarGracias(): void {
    this.graciasVista = { total: this.totalUltimoCobro, medio: this.medioUltimoCobro, hasta: Date.now() + 6000 };
  }

  private emitirVista(): void {
    const ahora = Date.now();
    if (this.graciasVista && ahora >= this.graciasVista.hasta) this.graciasVista = null;
    const estado = this.graciasVista ? this.estadoGracias(this.graciasVista) : this.estadoCarrito(ahora);
    const json = JSON.stringify(estado);
    if (json === this.jsonVista && ahora - this.latidoVista < 2000) return;
    this.jsonVista = json;
    this.latidoVista = ahora;
    this.vista.emitir(estado);
  }

  private estadoGracias(g: { total: number; medio: string }): EstadoVistaCliente {
    return { ...this.estadoBase('gracias'), total: g.total, medio: g.medio };
  }

  private estadoBase(tipo: EstadoVistaCliente['tipo']): EstadoVistaCliente {
    return { tipo, lineas: [], descuentoPorcentaje: 0, ahorroCombos: 0, total: 0, ultimaLinea: -1, marcaUltima: 0, qrDataUrl: '', medio: '' };
  }

  private estadoCarrito(ahora: number): EstadoVistaCliente {
    const a = this.activo;
    this.marcarUltimaLinea(ahora);
    const lineas = a.lineas
      .map((l) => ({
        nombre: l.nombre,
        cantidad: Number(l.cantidad),
        precioUnitario: this.precioLinea(l),
        subtotal: this.subtotalLinea(l),
        oferta: this.textoOfertaVista(l)
      }))
      .reverse();
    const ultima = this.ultimaVista.linea;
    const conQr = !!this.esperando && this.esperando.modo === 'qr_pantalla' && !!this.qrVista;
    return {
      ...this.estadoBase(conQr ? 'cobrando' : 'carrito'),
      lineas,
      descuentoPorcentaje: a.alCosto ? 0 : this.descuentoCaja(),
      ahorroCombos: this.descuentoDeCombos,
      total: this.total,
      ultimaLinea: ultima < 0 ? -1 : lineas.length - 1 - ultima,
      marcaUltima: this.ultimaVista.marca,
      qrDataUrl: conQr ? this.qrVista : ''
    };
  }

  private textoOfertaVista(l: LineaCarritoPendiente): string {
    const o = this.ofertaDe(l);
    if (!o) return '';
    if (o.tipo === 'llevaPaga') return `${o.descripcion} · llevás ${l.cantidad}, pagás ${this.cobradas(l)}`;
    return o.descripcion;
  }

  private marcarUltimaLinea(ahora: number): void {
    const actual = this.activo.lineas.map((l) => Number(l.cantidad));
    if (this.activo.id !== this.idCarritoVista) {
      this.idCarritoVista = this.activo.id;
      this.ultimaVista = { linea: -1, marca: 0 };
    } else if (actual.length >= this.cantidadesVista.length) {
      const cambiada = actual.map((c, i) => c !== this.cantidadesVista[i]).lastIndexOf(true);
      if (cambiada >= 0) this.ultimaVista = { linea: cambiada, marca: ahora };
    }
    this.cantidadesVista = actual;
  }

  private cerrarTrasCobro(): void {
    if (this.indiceActivo >= FIJOS) {
      const i = this.indiceActivo;
      this.carritos = this.carritos.filter((_, n) => n !== i);
      this.indiceActivo = Math.min(i, this.carritos.length - 1);
      return;
    }
    this.vaciarActivo();
  }
}
