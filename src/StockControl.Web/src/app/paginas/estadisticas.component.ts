import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  ApiService,
  ItemComparacionLocalDto,
  ItemMixPagoDto,
  ItemStockBajoDto,
  ItemTopProductoDto,
  ItemVentaDelDiaDto,
  ItemVentaGrupoDto,
  LocalDto,
  ResumenEstadisticasDto
} from '../servicios/api.service';
import { SesionService } from '../servicios/sesion.service';

interface OpcionMes {
  valor: string;
  etiqueta: string;
}

@Component({
  selector: 'sc-estadisticas',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="topbar">
      <div>
        <h1 style="margin:0 0 6px">Estadísticas</h1>
        <div class="tabs">
          <button
            class="tab"
            [class.on]="pestana === 'general'"
            type="button"
            (click)="pestana = 'general'"
          >
            Rendimiento general
          </button>
          <button
            class="tab"
            [class.on]="pestana === 'dia'"
            type="button"
            (click)="pestana = 'dia'"
          >
            Ventas del día
          </button>
        </div>
      </div>

      <div class="row" style="flex-wrap:wrap; gap:8px; align-items:center">
        <!-- Selector de Local -->
        <select [(ngModel)]="localSeleccionado" (ngModelChange)="cargar()">
          @if (locales.length > 1) {
            <option value="todos">Todos mis locales</option>
          }
          @for (loc of locales; track loc.idLocal) {
            <option [value]="loc.idLocal">{{ loc.nombre }}</option>
          }
        </select>

        @if (pestana === 'general') {
          <select [(ngModel)]="modoPeriodo" (ngModelChange)="alCambiarModoPeriodo()">
            <option value="mes">Por mes</option>
            <option value="rango">Rango de fechas</option>
          </select>

          @if (modoPeriodo === 'mes') {
            <select [(ngModel)]="mesSeleccionado" (ngModelChange)="cargar()">
              @for (m of mesesDisponibles; track m.valor) {
                <option [value]="m.valor">{{ m.etiqueta }}</option>
              }
            </select>
          } @else {
            <input
              type="text"
              inputmode="numeric"
              placeholder="dd/mm/aaaa"
              style="width:110px"
              [(ngModel)]="fechaDesde"
            />
            <input
              type="text"
              inputmode="numeric"
              placeholder="dd/mm/aaaa"
              style="width:110px"
              [(ngModel)]="fechaHasta"
            />
          }
        }

        <button class="btn btn-primary" type="button" [disabled]="cargando" (click)="cargar()">
          {{ cargando ? 'Cargando...' : 'Actualizar' }}
        </button>
      </div>
    </div>

    @if (error) {
      <p class="error" style="margin-bottom:16px">{{ error }}</p>
    }

    <!-- ==================== PESTAÑA: VENTAS DEL DÍA ==================== -->
    @if (pestana === 'dia') {
      <div class="kpis">
        <div class="panel kpi">
          <div class="n">{{ totalUnidadesDia() }}</div>
          <div class="l">Unidades vendidas hoy</div>
        </div>
        <div class="panel kpi">
          <div class="n">{{ resumen.ventasDelDia.length }}</div>
          <div class="l">Variedad de productos</div>
        </div>
        <div class="panel kpi">
          <div class="n">{{ localSeleccionado === 'todos' ? 'Todas las sucursales' : nombreLocalActual() }}</div>
          <div class="l">Alcance de cajas (todas)</div>
        </div>
      </div>

      <section class="panel pad">
        <div class="toolbar" style="padding:0 0 12px; display:flex; justify-content:space-between; align-items:center; flex-wrap:wrap; gap:8px">
          <div>
            <strong>Productos vendidos hoy en todas las cajas</strong>
            <span class="meta" style="margin-left:8px">Ordenado por mayor cantidad de ventas</span>
          </div>
          <input
            type="search"
            placeholder="Buscar por código o producto..."
            style="width:260px"
            [(ngModel)]="filtroBusquedaDia"
          />
        </div>

        @if (ventasDiaFiltradas().length === 0) {
          <p class="meta" style="margin-top:8px">No se registraron ventas para los filtros actuales hoy.</p>
        } @else {
          <table class="data">
            <thead>
              <tr>
                <th style="width:140px">Código</th>
                <th>Nombre del producto</th>
                <th class="num" style="width:180px">Cantidad de ventas</th>
                <th class="num" style="width:160px">Stock restante</th>
              </tr>
            </thead>
            <tbody>
              @for (item of ventasDiaFiltradas(); track item.codigo) {
                <tr>
                  <td><code>{{ item.codigo }}</code></td>
                  <td>
                    {{ item.nombre }}
                    @if (item.esSector) {
                      <span class="meta" style="font-size:11px; margin-left:6px">(Sector)</span>
                    }
                    @if (item.esGenerico) {
                      <span class="meta" style="font-size:11px; margin-left:6px">(Genérico)</span>
                    }
                  </td>
                  <td class="num">
                    <strong style="font-size:14px">{{ item.cantidadVentas }}</strong>
                  </td>
                  <td class="num">
                    @if (item.stockRestante == null) {
                      <span class="meta">—</span>
                    } @else {
                      <span
                        [style.color]="item.stockRestante <= 0 ? 'var(--danger)' : (item.stockRestante <= 5 ? 'var(--warn)' : 'inherit')"
                        [style.font-weight]="item.stockRestante <= 5 ? 'bold' : 'normal'"
                      >
                        {{ item.stockRestante }}
                      </span>
                    }
                  </td>
                </tr>
              }
            </tbody>
          </table>
        }
      </section>
    }

    <!-- ==================== PESTAÑA: RENDIMIENTO GENERAL ==================== -->
    @if (pestana === 'general') {
      <div class="kpis">
        <div class="panel kpi">
          <div class="n">{{ dinero(resumen.totalVendido) }}</div>
          <div class="l">Total vendido en el período</div>
        </div>
        <div class="panel kpi">
          <div class="n">{{ resumen.cantidadTickets }}</div>
          <div class="l">Tickets emitidos</div>
        </div>
        <div class="panel kpi">
          <div class="n">{{ dinero(resumen.ticketPromedio) }}</div>
          <div class="l">Ticket promedio</div>
        </div>
        <div class="panel kpi">
          <div class="n">{{ dinero(resumen.margenEstimado) }}</div>
          <div class="l">
            Margen estimado
            @if (resumen.porcentajeMargen > 0) {
              <span>({{ resumen.porcentajeMargen }}%)</span>
            }
          </div>
        </div>
      </div>

      <div class="split" style="margin-bottom:16px">
        <!-- Top 20 Productos -->
        <section class="panel pad">
          <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:10px">
            <strong>Top productos más vendidos</strong>
            <div class="tabs" style="font-size:12px">
              <button
                class="tab"
                [class.on]="vistaTop === 'unidades'"
                type="button"
                (click)="vistaTop = 'unidades'"
              >
                Por unidades
              </button>
              <button
                class="tab"
                [class.on]="vistaTop === 'dinero'"
                type="button"
                (click)="vistaTop = 'dinero'"
              >
                Por $ recaudado
              </button>
            </div>
          </div>

          @if (vistaTop === 'unidades') {
            @if (resumen.topPorUnidades.length === 0) {
              <p class="meta">Sin ventas de productos en este período.</p>
            } @else {
              <table class="data">
                <thead>
                  <tr>
                    <th style="width:36px">#</th>
                    <th>Producto</th>
                    <th class="num">Unidades</th>
                    <th class="num">Total $</th>
                  </tr>
                </thead>
                <tbody>
                  @for (p of resumen.topPorUnidades; track p.codigo; let i = $index) {
                    <tr>
                      <td class="meta">{{ i + 1 }}</td>
                      <td>{{ p.nombre }}</td>
                      <td class="num"><strong>{{ p.cantidad }}</strong></td>
                      <td class="num">{{ dinero(p.total) }}</td>
                    </tr>
                  }
                </tbody>
              </table>
            }
          } @else {
            @if (resumen.topPorDinero.length === 0) {
              <p class="meta">Sin ventas en este período.</p>
            } @else {
              <table class="data">
                <thead>
                  <tr>
                    <th style="width:36px">#</th>
                    <th>Producto</th>
                    <th class="num">Total $</th>
                    <th class="num">Unidades</th>
                  </tr>
                </thead>
                <tbody>
                  @for (p of resumen.topPorDinero; track p.codigo; let i = $index) {
                    <tr>
                      <td class="meta">{{ i + 1 }}</td>
                      <td>
                        {{ p.nombre }}
                        @if (p.esSector) {
                          <span class="meta" style="font-size:11px; margin-left:4px">(Sector)</span>
                        }
                      </td>
                      <td class="num"><strong>{{ dinero(p.total) }}</strong></td>
                      <td class="num">{{ p.esSector ? '—' : p.cantidad }}</td>
                    </tr>
                  }
                </tbody>
              </table>
            }
          }
        </section>

        <!-- Mix de Medios de Pago & Sucursales -->
        <div style="display:flex; flex-direction:column; gap:16px">
          <!-- Mix Medios de Pago -->
          <section class="panel pad">
            <strong>Mix de medios de pago</strong>
            @if (resumen.mixMediosPago.length === 0) {
              <p class="meta" style="margin-top:8px">No hay operaciones registradas en el período.</p>
            } @else {
              <div style="margin-top:12px">
                @for (m of resumen.mixMediosPago; track m.descripcion) {
                  <div class="bar-row">
                    <span style="font-weight:500">{{ m.descripcion }}</span>
                    <div class="bar">
                      <b [style.width.%]="m.porcentaje"></b>
                    </div>
                    <span class="num" style="min-width:110px">{{ dinero(m.total) }} ({{ m.porcentaje }}%)</span>
                  </div>
                }
              </div>
            }
          </section>

          <!-- Comparativa de Sucursales -->
          @if (resumen.comparativaSucursales.length > 1 || localSeleccionado === 'todos') {
            <section class="panel pad">
              <strong>Comparativa por sucursal</strong>
              <table class="data" style="margin-top:8px">
                <thead>
                  <tr>
                    <th>Local</th>
                    <th class="num">Tickets</th>
                    <th class="num">Ticket Prom.</th>
                    <th class="num">Total Vendido</th>
                  </tr>
                </thead>
                <tbody>
                  @for (s of resumen.comparativaSucursales; track s.idLocal) {
                    <tr>
                      <td>{{ s.nombreLocal }}</td>
                      <td class="num">{{ s.cantidadTickets }}</td>
                      <td class="num">{{ dinero(s.ticketPromedio) }}</td>
                      <td class="num"><strong>{{ dinero(s.totalVendido) }}</strong></td>
                    </tr>
                  }
                </tbody>
              </table>
            </section>
          }
        </div>
      </div>

      <div class="split">
        <!-- Ventas por Grupo de Precio -->
        <section class="panel pad">
          <strong>Ventas por grupo de precio</strong>
          @if (resumen.ventasPorGrupo.length === 0) {
            <p class="meta" style="margin-top:8px">No hay ventas registradas.</p>
          } @else {
            <table class="data" style="margin-top:8px">
              <thead>
                <tr>
                  <th>Grupo / Familia</th>
                  <th class="num">Cant.</th>
                  <th class="num">Total $</th>
                  <th class="num">%</th>
                </tr>
              </thead>
              <tbody>
                @for (g of resumen.ventasPorGrupo; track g.nombreGrupo) {
                  <tr>
                    <td>{{ g.nombreGrupo }}</td>
                    <td class="num">{{ g.cantidad }}</td>
                    <td class="num"><strong>{{ dinero(g.total) }}</strong></td>
                    <td class="num">{{ g.porcentaje }}%</td>
                  </tr>
                }
              </tbody>
            </table>
          }
        </section>

        <!-- Stock Bajo Actual -->
        <section class="panel pad">
          <div style="display:flex; justify-content:space-between; align-items:center">
            <strong>Stock bajo actual</strong>
            <span class="meta">{{ resumen.stockBajo.length }} en alerta</span>
          </div>
          @if (resumen.stockBajo.length === 0) {
            <p class="meta" style="margin-top:8px">Todos los productos cuentan con stock superior al umbral.</p>
          } @else {
            <table class="data" style="margin-top:8px">
              <thead>
                <tr>
                  @if (localSeleccionado === 'todos') {
                    <th>Local</th>
                  }
                  <th>Código</th>
                  <th>Producto</th>
                  <th class="num">Stock</th>
                  <th class="num">Umbral</th>
                </tr>
              </thead>
              <tbody>
                @for (b of resumen.stockBajo; track b.idProducto) {
                  <tr>
                    @if (localSeleccionado === 'todos') {
                      <td class="meta">{{ b.nombreLocal }}</td>
                    }
                    <td><code>{{ b.codigo }}</code></td>
                    <td>{{ b.nombre }}</td>
                    <td class="num" style="color:var(--danger); font-weight:bold">{{ b.cantidad }}</td>
                    <td class="num meta">{{ b.umbral }}</td>
                  </tr>
                }
              </tbody>
            </table>
          }
        </section>
      </div>
    }
  `
})
export class EstadisticasComponent implements OnInit {
  pestana: 'general' | 'dia' = 'dia';
  vistaTop: 'unidades' | 'dinero' = 'unidades';

  locales: Pick<LocalDto, 'idLocal' | 'nombre'>[] = [];
  localSeleccionado: string | number = '';

  modoPeriodo: 'mes' | 'rango' = 'mes';
  mesesDisponibles: OpcionMes[] = [];
  mesSeleccionado = '';
  fechaDesde = '';
  fechaHasta = '';

  filtroBusquedaDia = '';

  cargando = false;
  error = '';

  resumen: ResumenEstadisticasDto = {
    totalVendido: 0,
    cantidadTickets: 0,
    ticketPromedio: 0,
    margenEstimado: 0,
    porcentajeMargen: 0,
    topPorUnidades: [],
    topPorDinero: [],
    mixMediosPago: [],
    ventasPorGrupo: [],
    comparativaSucursales: [],
    stockBajo: [],
    ventasDelDia: []
  };

  constructor(
    private readonly api: ApiService,
    readonly sesion: SesionService
  ) {
    this.armarMeses();
  }

  ngOnInit(): void {
    this.localSeleccionado = this.sesion.idLocal() || '';
    this.api.locales().subscribe({
      next: ({ items: ls }) => {
        this.locales = ls;
        if (!this.localSeleccionado && ls.length > 0) {
          this.localSeleccionado = ls[0].idLocal;
        }
        this.cargar();
      },
      error: () => {
        this.cargar();
      }
    });
  }

  armarMeses(): void {
    const nombresMeses = [
      'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
      'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'
    ];
    const hoy = new Date();
    const meses: OpcionMes[] = [];
    for (let i = 0; i < 6; i++) {
      const d = new Date(hoy.getFullYear(), hoy.getMonth() - i, 1);
      const año = d.getFullYear();
      const mesNum = (d.getMonth() + 1).toString().padStart(2, '0');
      meses.push({
        valor: `${año}-${mesNum}`,
        etiqueta: `${nombresMeses[d.getMonth()]} ${año}`
      });
    }
    this.mesesDisponibles = meses;
    this.mesSeleccionado = meses[0].valor;
  }

  alCambiarModoPeriodo(): void {
    if (this.modoPeriodo === 'mes') {
      this.cargar();
    }
  }

  nombreLocalActual(): string {
    const loc = this.locales.find((l) => l.idLocal === Number(this.localSeleccionado));
    return loc ? loc.nombre : this.sesion.nombreLocal();
  }

  totalUnidadesDia(): number {
    return this.resumen.ventasDelDia.reduce((acc, v) => acc + Number(v.cantidadVentas), 0);
  }

  ventasDiaFiltradas(): ItemVentaDelDiaDto[] {
    const q = this.filtroBusquedaDia.trim().toLowerCase();
    if (!q) return this.resumen.ventasDelDia;
    return this.resumen.ventasDelDia.filter(
      (v) => v.codigo.toLowerCase().includes(q) || v.nombre.toLowerCase().includes(q)
    );
  }

  cargar(): void {
    this.cargando = true;
    this.error = '';

    const params: { idLocal?: number; todos?: boolean; desde?: string; hasta?: string; mes?: string } = {};

    if (this.localSeleccionado === 'todos') {
      params.todos = true;
    } else if (this.localSeleccionado) {
      params.idLocal = Number(this.localSeleccionado);
    }

    if (this.modoPeriodo === 'mes') {
      params.mes = this.mesSeleccionado;
    } else {
      const d = this.parsearFecha(this.fechaDesde);
      const h = this.parsearFecha(this.fechaHasta);
      if (d) params.desde = d;
      if (h) params.hasta = h;
    }

    this.api.estadisticas(params).subscribe({
      next: (resp) => {
        this.cargando = false;
        this.resumen = resp.resumen || {
          totalVendido: 0,
          cantidadTickets: 0,
          ticketPromedio: 0,
          margenEstimado: 0,
          porcentajeMargen: 0,
          topPorUnidades: [],
          topPorDinero: [],
          mixMediosPago: [],
          ventasPorGrupo: [],
          comparativaSucursales: [],
          stockBajo: [],
          ventasDelDia: []
        };
        if (resp.locales && resp.locales.length > 0 && this.locales.length === 0) {
          this.locales = resp.locales;
        }
      },
      error: (err) => {
        this.cargando = false;
        this.error = err.error?.message || err.error || 'No se pudieron cargar las estadísticas.';
      }
    });
  }

  parsearFecha(texto?: string): string | undefined {
    if (!texto) return undefined;
    const parts = texto.trim().split('/');
    if (parts.length === 3) {
      const d = parts[0].padStart(2, '0');
      const m = parts[1].padStart(2, '0');
      const y = parts[2];
      return `${y}-${m}-${d}`;
    }
    return texto.trim();
  }

  dinero(n: number): string {
    return `$ ${Number(n || 0).toLocaleString('es-AR', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    })}`;
  }
}
