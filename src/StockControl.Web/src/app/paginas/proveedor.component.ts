import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  ApiService,
  LineaOrdenCompraDto,
  OrdenGeneradaDto,
  OrdenResumenDto,
  ProductoDto,
  ProductoProveedorDto,
  ProveedorFichaDto
} from '../servicios/api.service';
import { descargar } from '../servicios/descargar';
import { RecibirOrdenComponent } from './recibir-orden.component';

@Component({
  selector: 'sc-proveedor',
  standalone: true,
  imports: [FormsModule, RouterLink, RecibirOrdenComponent],
  template: `
    <div class="topbar">
      <h1>{{ proveedor?.nombre ?? 'Proveedor' }}</h1>
      <a class="btn" routerLink="/proveedores">Volver a Proveedores</a>
    </div>
    @if (error) {
      <p class="error">{{ error }}</p>
    }
    @if (mensaje) {
      <p class="meta">{{ mensaje }}</p>
    }
    @if (urlPendiente) {
      <p class="error">
        El navegador bloqueó la ventana de WhatsApp.
        <a [href]="urlPendiente" target="_blank" rel="noopener">Abrir WhatsApp</a>
      </p>
    }
    @if (proveedor) {
      <section class="panel pad" style="margin-bottom:12px">
        <p style="margin:0 0 4px">{{ proveedor.tipoProductos || 'Sin tipo de productos' }}</p>
        <p class="meta" style="margin:0 0 8px">
          {{ proveedor.telefono || 'Sin WhatsApp' }} · {{ proveedor.cantidadProductos }} productos
          @if (!proveedor.activo) { · Inactivo }
        </p>
        <div class="row">
          @if (proveedor.urlContacto) {
            <a class="btn" [href]="proveedor.urlContacto" target="_blank" rel="noopener">Contactar</a>
          }
          @if (proveedor.activo) {
            <a class="btn" [routerLink]="['/proveedores', proveedor.id, 'catalogo']">Cargar productos (Excel)</a>
          }
        </div>
      </section>

      <div class="split">
        <section class="panel pad">
          <h2 style="margin:0 0 8px;font-size:16px">Última orden</h2>
          @if (proveedor.ultimaOrden; as ultima) {
            <p class="meta" style="margin:0 0 8px">
              {{ fecha(ultima.fecha) }} · {{ ultima.nombreUsuario }} · {{ ultima.recibida ? 'Recibida' : 'Pedida' }}
            </p>
            <table class="data">
              <thead><tr><th>Producto</th><th class="num">Cantidad</th></tr></thead>
              <tbody>
                @for (l of ultima.lineas; track l.id) {
                  <tr><td>{{ l.descripcion || l.nombre }}</td><td class="num">{{ l.cantidad }}</td></tr>
                }
              </tbody>
            </table>
            @if (proveedor.activo) {
              <button class="btn" type="button" style="margin-top:8px" (click)="repetir()">Repetir</button>
            }
          } @else {
            <p class="meta">Todavía no hay órdenes.</p>
          }
        </section>

        @if (proveedor.activo) {
          <section class="panel pad" id="nueva-orden">
            <h2 style="margin:0 0 8px;font-size:16px">Nueva orden</h2>
            <input
              class="wide"
              type="search"
              [ngModel]="q"
              (ngModelChange)="buscar($event)"
              placeholder="Buscar un producto nuevo para este proveedor"
            />
            @if (resultados.length) {
              <div class="scroll" style="max-height:200px;overflow:auto;margin-top:8px">
                <table class="data">
                  <tbody>
                    @for (p of resultados; track p.id) {
                      <tr>
                        <td>{{ p.codigo }}</td>
                        <td>{{ p.nombre }}</td>
                        <td class="num">Stock {{ p.cantidad }}</td>
                        <td class="num">
                          <button class="btn btn-primary" type="button" (click)="agregar(p)">Agregar</button>
                        </td>
                      </tr>
                    }
                  </tbody>
                </table>
              </div>
            }
            <datalist id="presentaciones">
              <option value="Pack"></option>
              <option value="Caja"></option>
              <option value="Bulto"></option>
            </datalist>
            <table class="data" style="margin-top:12px">
              <thead><tr><th>Producto</th><th class="num">Cantidad</th><th>Presentación</th><th class="num">Unid. por pack</th><th></th></tr></thead>
              <tbody>
                @for (l of lineas; track $index) {
                  <tr>
                    <td>{{ l.nombre }}</td>
                    <td class="num"><input type="number" min="0.001" style="width:80px" [(ngModel)]="l.cantidad" /></td>
                    <td><input type="text" list="presentaciones" maxlength="30" style="width:110px" placeholder="Unidad" [(ngModel)]="l.nombrePack" /></td>
                    <td class="num">
                      <input type="number" min="2" max="10000" style="width:80px" [disabled]="!l.nombrePack.trim()" [(ngModel)]="l.unidadesPack" />
                    </td>
                    <td class="num"><button class="btn" type="button" (click)="quitar(l)">Quitar</button></td>
                  </tr>
                } @empty {
                  <tr><td colspan="5" class="meta">Elegí un producto de la lista de abajo o buscá uno nuevo.</td></tr>
                }
              </tbody>
            </table>
            <div class="row" style="margin-top:12px;gap:12px;align-items:center">
              <label class="check">
                <input type="checkbox" [(ngModel)]="porWhatsApp" [disabled]="!proveedor.urlContacto" /> WhatsApp
              </label>
              <label class="check"><input type="checkbox" [(ngModel)]="porExcel" /> Excel</label>
              <label class="check"><input type="checkbox" [(ngModel)]="porPdf" /> PDF</label>
              <button class="btn btn-primary grow" type="button" [disabled]="!lineas.length || generando" (click)="generar()">
                {{ generando ? 'Generando...' : 'Generar orden' }}
              </button>
            </div>
          </section>
        }
      </div>

      <section class="panel" style="margin-top:12px">
        <div class="toolbar" style="padding:12px 12px 0">
          <h2 style="margin:0;font-size:16px" class="grow">Órdenes</h2>
          <span class="meta">{{ totalOrdenes }} en total</span>
          <button class="btn" type="button" [disabled]="paginaOrdenes <= 1" (click)="cargarOrdenes(paginaOrdenes - 1)">Anterior</button>
          <button class="btn" type="button" [disabled]="paginaOrdenes * 20 >= totalOrdenes" (click)="cargarOrdenes(paginaOrdenes + 1)">
            Siguiente
          </button>
        </div>
        <table class="data">
          <thead><tr><th>#</th><th>Fecha</th><th class="num">Productos</th><th>Estado</th><th></th></tr></thead>
          <tbody>
            @for (o of ordenes; track o.id) {
              <tr>
                <td>{{ o.id }}</td>
                <td>{{ fecha(o.fecha) }} · {{ o.nombreUsuario }}</td>
                <td class="num">{{ o.cantidadLineas }}</td>
                <td>
                  @if (o.recibida) {
                    Recibida el {{ fecha(o.fechaRecepcion!) }} por {{ o.nombreUsuarioRecepcion }}
                  } @else {
                    Pedida
                  }
                </td>
                <td class="num">
                  <button class="btn" [class.btn-primary]="!o.recibida" type="button" (click)="ordenAbierta = o.id">
                    {{ o.recibida ? 'Ver' : 'Marcar recibida' }}
                  </button>
                </td>
              </tr>
            } @empty {
              <tr><td colspan="5" class="meta">Todavía no hay órdenes.</td></tr>
            }
          </tbody>
        </table>
      </section>

      <section class="panel" style="margin-top:12px">
        <div class="toolbar" style="padding:12px 12px 0">
          <h2 style="margin:0;font-size:16px" class="grow">Productos de {{ proveedor.nombre }}</h2>
          <span class="meta">{{ totalVinculados }} en total</span>
        </div>
        <div style="padding:8px 12px">
          <input
            class="wide"
            type="search"
            [ngModel]="qVinculados"
            (ngModelChange)="buscarVinculados($event)"
            placeholder="Buscar entre los productos de este proveedor"
          />
        </div>
        <table class="data">
          <thead><tr><th>Producto</th><th class="num">Stock</th><th>Presentación</th><th></th></tr></thead>
          <tbody>
            @for (v of vinculados; track v.idProducto) {
              <tr>
                <td>{{ v.nombre }}</td>
                <td class="num">{{ v.stock }}</td>
                <td>
                  @if (editandoPack?.idProducto === v.idProducto) {
                    <input type="text" list="presentaciones" maxlength="30" style="width:110px" placeholder="Unidad" [(ngModel)]="edNombrePack" />
                    <input type="number" min="2" max="10000" style="width:80px" [disabled]="!edNombrePack.trim()" [(ngModel)]="edUnidadesPack" />
                    <span class="meta">unidades</span>
                  } @else {
                    {{ v.nombrePack ? v.nombrePack + ' de ' + v.unidadesPack : 'Por unidad' }}
                  }
                </td>
                <td class="num">
                  @if (editandoPack?.idProducto === v.idProducto) {
                    <button class="btn btn-primary" type="button" (click)="guardarPack(v)">Guardar</button>
                    <button class="btn" type="button" (click)="editandoPack = null">Cancelar</button>
                  } @else {
                    @if (proveedor.activo) {
                      <button class="btn btn-primary" type="button" (click)="agregarVinculado(v)">Agregar a la orden</button>
                    }
                    <button class="btn" type="button" (click)="editarPack(v)">Editar pack</button>
                    <button class="btn btn-danger" type="button" (click)="desvincular(v)">Quitar</button>
                  }
                </td>
              </tr>
            } @empty {
              <tr><td colspan="4" class="meta">Este proveedor todavía no tiene productos. Se suman solos al mandarle una orden.</td></tr>
            }
          </tbody>
        </table>
        @if (totalVinculados > 20) {
          <div class="row pad">
            <button class="btn" type="button" [disabled]="paginaVinculados <= 1" (click)="cargarVinculados(paginaVinculados - 1)">Anterior</button>
            <span class="meta">Página {{ paginaVinculados }}</span>
            <button class="btn" type="button" [disabled]="paginaVinculados * 20 >= totalVinculados" (click)="cargarVinculados(paginaVinculados + 1)">Siguiente</button>
          </div>
        }
      </section>
    }

    @if (ordenAbierta) {
      <sc-recibir-orden [idOrden]="ordenAbierta" (recibida)="alRecibir($event)" (cerrar)="ordenAbierta = 0" />
    }

    @if (impresa) {
      <div class="orden-print">
        <h1>Orden de compra #{{ impresa.orden.id }}</h1>
        <p><strong>Local:</strong> {{ impresa.nombreLocal }}</p>
        <p><strong>Proveedor:</strong> {{ proveedor?.nombre }}</p>
        <p><strong>Fecha:</strong> {{ fecha(impresa.orden.fecha) }}</p>
        <table>
          <thead><tr><th>Producto</th><th class="num">Cantidad</th></tr></thead>
          <tbody>
            @for (l of impresa.orden.lineas; track l.id) {
              <tr><td>{{ l.descripcion || l.nombre }}</td><td class="num">{{ l.cantidad }}</td></tr>
            }
          </tbody>
        </table>
      </div>
    }
  `
})
export class ProveedorComponent implements OnInit {
  proveedor: ProveedorFichaDto | null = null;
  q = '';
  resultados: ProductoDto[] = [];
  lineas: LineaOrdenCompraDto[] = [];
  porWhatsApp = false;
  porExcel = false;
  porPdf = false;
  generando = false;
  impresa: OrdenGeneradaDto | null = null;
  ordenes: OrdenResumenDto[] = [];
  totalOrdenes = 0;
  paginaOrdenes = 1;
  ordenAbierta = 0;
  vinculados: ProductoProveedorDto[] = [];
  totalVinculados = 0;
  paginaVinculados = 1;
  qVinculados = '';
  editandoPack: ProductoProveedorDto | null = null;
  edNombrePack = '';
  edUnidadesPack = 2;
  private temporizadorVinculados?: ReturnType<typeof setTimeout>;
  urlPendiente = '';
  error = '';
  mensaje = '';
  private id = 0;
  private temporizador?: ReturnType<typeof setTimeout>;

  constructor(
    private readonly api: ApiService,
    private readonly ruta: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.id = Number(this.ruta.snapshot.paramMap.get('id'));
    this.cargar();
  }

  cargar(): void {
    this.api.proveedor(this.id).subscribe({
      next: (r) => {
        this.proveedor = r;
        this.porWhatsApp = this.porWhatsApp && !!r.urlContacto;
        if (this.ruta.snapshot.fragment === 'orden') this.enfocarOrden();
      },
      error: () => (this.error = 'No se pudo cargar el proveedor.')
    });
    this.cargarOrdenes(this.paginaOrdenes);
    this.cargarVinculados(this.paginaVinculados);
  }

  cargarOrdenes(pagina: number): void {
    this.api.ordenesProveedor(this.id, pagina).subscribe({
      next: (r) => {
        this.ordenes = r.items;
        this.totalOrdenes = r.total;
        this.paginaOrdenes = r.pagina;
      },
      error: () => (this.error = 'No se pudieron cargar las órdenes.')
    });
  }

  alRecibir(mensaje: string): void {
    this.ordenAbierta = 0;
    this.error = '';
    this.mensaje = mensaje;
    this.cargar();
  }

  private enfocarOrden(): void {
    setTimeout(() => {
      const seccion = document.getElementById('nueva-orden');
      seccion?.scrollIntoView({ block: 'start' });
      (seccion?.querySelector('input') as HTMLInputElement | null)?.focus();
    });
  }

  buscar(texto: string): void {
    this.q = texto;
    clearTimeout(this.temporizador);
    if (!texto.trim()) {
      this.resultados = [];
      return;
    }
    this.temporizador = setTimeout(() => {
      this.api.productos(this.q).subscribe({
        next: (r) => (this.resultados = r.items),
        error: () => (this.error = 'No se pudieron buscar productos.')
      });
    }, 250);
  }

  agregar(p: ProductoDto): void {
    this.sumarLinea(p.id, p.nombre, '', 1);
  }

  agregarVinculado(v: ProductoProveedorDto): void {
    this.sumarLinea(v.idProducto, v.nombre, v.nombrePack, v.unidadesPack);
  }

  private sumarLinea(idProducto: number, nombre: string, nombrePack: string, unidadesPack: number): void {
    const linea = this.lineas.find((l) => l.idProducto === idProducto && l.nombrePack === nombrePack);
    if (linea) linea.cantidad += 1;
    else this.lineas = [...this.lineas, { idProducto, nombre, cantidad: 1, nombrePack, unidadesPack }];
  }

  buscarVinculados(texto: string): void {
    this.qVinculados = texto;
    clearTimeout(this.temporizadorVinculados);
    this.temporizadorVinculados = setTimeout(() => this.cargarVinculados(1), 250);
  }

  cargarVinculados(pagina: number): void {
    this.api.productosProveedor(this.id, this.qVinculados.trim(), pagina).subscribe({
      next: (r) => {
        this.vinculados = r.items;
        this.totalVinculados = r.total;
        this.paginaVinculados = r.pagina;
      },
      error: () => (this.error = 'No se pudieron cargar los productos del proveedor.')
    });
  }

  editarPack(v: ProductoProveedorDto): void {
    this.editandoPack = v;
    this.edNombrePack = v.nombrePack;
    this.edUnidadesPack = v.unidadesPack;
  }

  guardarPack(v: ProductoProveedorDto): void {
    const nombre = this.edNombrePack.trim();
    const unidades = nombre ? Number(this.edUnidadesPack) : 1;
    this.api.fijarPackProveedor(this.id, v.idProducto, nombre, unidades).subscribe({
      next: () => {
        this.editandoPack = null;
        this.error = '';
        this.cargarVinculados(this.paginaVinculados);
      },
      error: (e) => (this.error = typeof e.error === 'string' ? e.error : 'No se pudo guardar el pack.')
    });
  }

  desvincular(v: ProductoProveedorDto): void {
    this.api.desvincularProducto(this.id, v.idProducto).subscribe({
      next: () => {
        this.cargarVinculados(this.paginaVinculados);
        this.cargar();
      },
      error: () => (this.error = 'No se pudo quitar el producto del proveedor.')
    });
  }

  quitar(linea: LineaOrdenCompraDto): void {
    this.lineas = this.lineas.filter((l) => l !== linea);
  }

  repetir(): void {
    this.lineas = (this.proveedor?.ultimaOrden?.lineas ?? []).map((l) => ({ ...l }));
  }

  generar(): void {
    if (this.lineas.some((l) => !(Number(l.cantidad) > 0))) {
      this.error = 'Cada producto tiene que tener cantidad mayor a 0.';
      return;
    }
    const ventana = this.porWhatsApp ? window.open('', '_blank') : null;
    this.generando = true;
    this.error = '';
    this.mensaje = '';
    this.urlPendiente = '';
    const pedido = this.lineas.map((l) => {
      const nombrePack = (l.nombrePack ?? '').trim();
      return { idProducto: l.idProducto, cantidad: Number(l.cantidad), nombrePack, unidadesPack: nombrePack ? Number(l.unidadesPack) : 1 };
    });
    this.api.crearOrdenCompra(this.id, pedido).subscribe({
      next: (r) => {
        this.generando = false;
        this.lineas = [];
        this.mensaje = `Orden #${r.orden.id} guardada.`;
        this.abrirWhatsApp(ventana, r.urlWhatsApp);
        if (this.porExcel) this.bajarExcel(r.orden.id);
        if (this.porPdf) this.imprimir(r);
        this.cargar();
      },
      error: (e) => {
        ventana?.close();
        this.generando = false;
        this.error = typeof e.error === 'string' ? e.error : 'No se pudo generar la orden.';
      }
    });
  }

  fecha(iso: string): string {
    return new Date(iso).toLocaleString('es-AR', { dateStyle: 'short', timeStyle: 'short' });
  }

  private abrirWhatsApp(ventana: Window | null, url: string | null): void {
    if (!this.porWhatsApp || !url) {
      ventana?.close();
      return;
    }
    if (ventana) ventana.location.href = url;
    else this.urlPendiente = url;
  }

  private bajarExcel(idOrden: number): void {
    this.api.excelOrdenCompra(idOrden).subscribe({
      next: (archivo) => descargar(archivo, `orden-${idOrden}.xlsx`),
      error: () => (this.error = 'La orden quedó guardada, pero no se pudo bajar el Excel.')
    });
  }

  private imprimir(orden: OrdenGeneradaDto): void {
    this.impresa = orden;
    setTimeout(() => {
      try {
        window.print();
      } catch {
        this.error = 'No se pudo abrir el diálogo de impresión.';
      }
      this.impresa = null;
    }, 50);
  }
}
