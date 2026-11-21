import { Component, OnInit } from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService, CategoriaDto, FiltroProductos, GrupoListaDto, PedidoAumento, ProductoDto, ProveedorListaDto, RevisionListaDto, UltimoAumentoDto, VistaPreviaAumentoDto } from '../servicios/api.service';
import { descargar } from '../servicios/descargar';
import { SesionService } from '../servicios/sesion.service';
import { DialogoService } from '../servicios/dialogo.service';

@Component({
  selector: 'sc-productos',
  standalone: true,
  imports: [FormsModule, NgTemplateOutlet, RouterLink],
  template: `
    <div class="topbar">
      <h1>Productos</h1>
      <div class="row">
        @if (sesion.esDueno()) {
          <button class="btn btn-primary" type="button" (click)="nuevo()">Nuevo producto</button>
          <button class="btn" type="button" (click)="abrirAumento()">Aumentar</button>
          <button class="btn" type="button" [disabled]="marcados.size === 0" (click)="abrirAsignarCategoria()">
            Asignar categoría{{ marcados.size > 0 ? ' (' + marcados.size + ')' : '' }}
          </button>
          @if (ultimoAumento && !ultimoAumento.deshecho) {
            <button class="btn" type="button" (click)="deshacerUltimoAumento()">Deshacer último aumento</button>
          }
          <a class="btn" routerLink="/carteles">Carteles</a>
        }
        <button class="btn" type="button" [disabled]="!elegido" (click)="abrirStock()">Sumar stock</button>
      </div>
    </div>
    <div class="toolbar" style="margin-bottom:12px">
      <input class="grow" type="search" [ngModel]="q" (ngModelChange)="aplicarBuscar($event)" placeholder="Buscar nombre o código" />
      <select [ngModel]="tipo" (ngModelChange)="aplicarTipo($event)">
        <option value="">Tipo: todos</option>
        <option value="comun">Común</option>
        <option value="sector">Sector</option>
      </select>
      <select [ngModel]="grupo" (ngModelChange)="aplicarGrupo($event)">
        <option value="">Grupo: todos</option>
        <option value="sin">Sin grupo</option>
        @for (g of gruposFiltro; track g.idGrupoProducto) {
          <option [value]="g.idGrupoProducto">{{ g.nombreGrupo }}</option>
        }
      </select>
      <label class="check"><input type="checkbox" [(ngModel)]="stockBajo" (ngModelChange)="cargar()" /> Stock bajo</label>
      @if (sesion.esDueno()) {
        <label class="check"><input type="checkbox" [(ngModel)]="agrupar" /> Agrupar</label>
      }
      @if (sesion.puedeConfigurarLocal()) {
        <select [ngModel]="tienda" (ngModelChange)="aplicarTienda($event)" aria-label="Filtrar por tienda">
          <option value="">Tienda: todos</option>
          <option value="si">Publicados</option>
          <option value="no">No publicados</option>
        </select>
      }
      <span class="meta">{{ total }} en lista</span>
    </div>
      @if (sesion.puedeConfigurarLocal()) {
      <div class="row" style="margin-bottom:12px">
        <button class="btn" type="button" [disabled]="publicablesVisibles().length === 0 && !hayMasQueLaPantalla()" (click)="publicarLista(true)">Publicar la lista en la tienda</button>
        <button class="btn" type="button" [disabled]="visibles.length === 0" (click)="publicarLista(false)">Ocultar la lista de la tienda</button>
        <span class="meta">Sector y genérico no se publican: no tienen precio fijo.</span>
      </div>
    }
    <section class="panel tabla-scroll">
      <table class="data">
        <thead>
          <tr>
            @if (sesion.esDueno()) {
              <th><input type="checkbox" [checked]="todosVisiblesMarcados()" (change)="marcarVisibles($any($event.target).checked)" aria-label="Marcar los de la pantalla" /></th>
            }
            <th>Código</th>
            <th>Nombre</th>
            <th class="num">Stock</th>
            @if (sesion.esDueno()) {
              <th class="num">Costo</th>
            }
            <th class="num">Precio</th>
            <th>Tipo</th>
            @if (sesion.esDueno()) {
              <th>Grupo</th>
              <th>Categoría</th>
            }
            <th>Última modificación</th>
            @if (sesion.puedeConfigurarLocal()) {
              <th>Tienda</th>
            }
          </tr>
        </thead>
        <tbody>
          @if (agrupar && sesion.esDueno()) {
            @for (seccion of secciones; track seccion.nombre) {
              <tr><th colspan="11"><button class="colapsar" type="button" (click)="alternarSeccion(seccion.nombre)"><span class="flecha" [class.abierta]="!seccionesCerradas.has(seccion.nombre)">▸</span>{{ seccion.nombre }} ({{ seccion.productos.length }})</button></th></tr>
              @if (!seccionesCerradas.has(seccion.nombre)) {
                @for (p of seccion.productos; track p.id) {
                  <ng-container [ngTemplateOutlet]="filaProducto" [ngTemplateOutletContext]="{ p: p }" />
                }
              }
            }
          } @else {
          @for (p of visibles; track p.id) {
            <tr
              class="clickable"
              [class.low]="!p.productoSector && p.cantidad <= umbralStockBajo"
              [class.selected]="elegido?.id === p.id"
              (click)="elegido = p"
              (dblclick)="editar(p)"
            >
              @if (sesion.esDueno()) {
                <td (click)="$event.stopPropagation()"><input type="checkbox" [checked]="marcada(p.id)" (change)="marcar(p.id, $any($event.target).checked)" [attr.aria-label]="'Marcar ' + p.nombre" /></td>
              }
              <td>{{ p.codigo }}</td>
              <td>{{ p.nombre }}</td>
              <td class="num">{{ p.productoSector ? '—' : p.cantidad }}</td>
              @if (sesion.esDueno()) {
                <td class="num">{{ dinero(p.costo) }}</td>
              }
              <td class="num">{{ p.productoSector ? 'en caja' : dinero(p.precio) }}</td>
              <td>{{ p.productoSector ? 'Sector' : 'Común' }}</td>
              @if (sesion.esDueno()) {
                <td>{{ p.nombreGrupo || '—' }}</td>
                <td><ng-container [ngTemplateOutlet]="celdaCategoria" [ngTemplateOutletContext]="{ p: p }" /></td>
              }
              <td><ng-container [ngTemplateOutlet]="celdaModificacion" [ngTemplateOutletContext]="{ p: p }" /></td>
              @if (sesion.puedeConfigurarLocal()) {
                <td><ng-container [ngTemplateOutlet]="celdaTienda" [ngTemplateOutletContext]="{ p: p }" /></td>
              }
            </tr>
          }
          }
        </tbody>
      </table>
      <ng-template #filaProducto let-p="p">
        <tr class="clickable" [class.low]="!p.productoSector && p.cantidad <= umbralStockBajo" [class.selected]="elegido?.id === p.id" (click)="elegido = p" (dblclick)="editar(p)">
          @if (sesion.esDueno()) {
            <td (click)="$event.stopPropagation()"><input type="checkbox" [checked]="marcada(p.id)" (change)="marcar(p.id, $any($event.target).checked)" [attr.aria-label]="'Marcar ' + p.nombre" /></td>
          }
          <td>{{ p.codigo }}</td><td>{{ p.nombre }}</td><td class="num">{{ p.productoSector ? '—' : p.cantidad }}</td>
          @if (sesion.esDueno()) { <td class="num">{{ dinero(p.costo) }}</td> }
          <td class="num">{{ p.productoSector ? 'en caja' : dinero(p.precio) }}</td><td>{{ p.productoSector ? 'Sector' : 'Común' }}</td>
          @if (sesion.esDueno()) {
            <td>{{ p.nombreGrupo || '—' }}</td>
            <td><ng-container [ngTemplateOutlet]="celdaCategoria" [ngTemplateOutletContext]="{ p: p }" /></td>
          }
          <td><ng-container [ngTemplateOutlet]="celdaModificacion" [ngTemplateOutletContext]="{ p: p }" /></td>
          @if (sesion.puedeConfigurarLocal()) {
            <td><ng-container [ngTemplateOutlet]="celdaTienda" [ngTemplateOutletContext]="{ p: p }" /></td>
          }
        </tr>
      </ng-template>
      <ng-template #celdaCategoria let-p="p">
        {{ p.nombreCategoria || '—' }}
        @if (p.categoriaDelGrupo) { <span class="meta">(grupo)</span> }
      </ng-template>
      <ng-template #celdaModificacion let-p="p">
        {{ fechaHora(p.fechaModificacion) }}
        <div class="meta">{{ p.usuarioModificacion || '—' }}</div>
      </ng-template>
      <ng-template #celdaTienda let-p="p">
        <input type="checkbox" [checked]="p.publicado" [disabled]="!p.publicable && !p.publicado"
               [attr.aria-label]="'Publicar ' + p.nombre + ' en la tienda'"
               [title]="p.publicable ? 'Publicar en la tienda' : 'Sector y genérico no se publican'"
               (click)="$event.stopPropagation()" (change)="publicar(p, $any($event.target).checked)" />
      </ng-template>
    </section>
    @if (sesion.esDueno()) {
      <p class="meta" style="margin-top:12px">Dueño: alta, edición (doble clic) y aumento de precio o costo. Empleado: lista y sumar stock.</p>
    } @else {
      <p class="meta" style="margin-top:12px">Podés sumar stock. No cambiás precios ni creás productos.</p>
    }
    @if (error) {
      <p class="error">{{ error }}</p>
    }

    <div class="modal-back" [class.show]="formAbierto">
      <div class="modal">
        <h2 style="margin:0 0 8px;font-size:18px">{{ editando ? 'Editar producto' : 'Nuevo producto' }}</h2>
        @if (sesion.esDueno()) {
          <label class="field">Código <input class="wide" type="text" [(ngModel)]="form.codigo" [disabled]="!!editando" /></label>
          <label class="field">Nombre <input class="wide" type="text" [(ngModel)]="form.nombre" /></label>
          <label class="field">Stock <input class="wide" type="number" [(ngModel)]="form.cantidad" /></label>
          <label class="field">Costo <input class="wide" type="number" [ngModel]="form.costo" (ngModelChange)="cambiarCosto($event)" /></label>
          <label class="field">Precio <input class="wide" type="number" [(ngModel)]="form.precio" [disabled]="form.productoSector || !!form.idGrupoProducto" /></label>
          <label class="check"><input type="checkbox" [ngModel]="form.productoSector" (ngModelChange)="cambiarSector($event)" /> Producto sector</label>
          <label class="field">Precio por kilo <input class="wide" type="number" min="0" step="0.01" [(ngModel)]="form.precioPorKilo" /></label>
          <p class="meta">En 0 no aparece en el modo balanza. Sirve para común y sector.</p>
          <label class="field">Descripción para la tienda (opcional)
            <textarea class="wide" rows="3" maxlength="500" [(ngModel)]="form.descripcion"></textarea>
          </label>
          @if (!form.productoSector) {
            <p class="meta" style="margin:8px 0 4px">Grupo: {{ form.nombreGrupo || 'Sin grupo' }}</p>
            <div class="row">
              <button class="btn" type="button" (click)="abrirGrupos()">Buscar grupo</button>
              <button class="btn" type="button" [disabled]="!(form.idGrupoProducto)" (click)="sacarGrupo()">Sacar grupo</button>
            </div>
          }
          <label class="field" style="margin-top:8px">
            Categoría
            <select class="wide" [(ngModel)]="form.idCategoria">
              <option [ngValue]="0">{{ opcionSinCategoria }}</option>
              @for (c of categorias; track c.id) {
                <option [ngValue]="c.id">{{ c.nombre }}</option>
              }
            </select>
          </label>
        } @else {
          <label class="field">Precio de venta <input class="wide" type="number" min="0" [(ngModel)]="form.precio" /></label>
        }
        @if (error) {
          <p class="error">{{ error }}</p>
        }
        <div class="row" style="margin-top:12px">
          <button class="btn btn-primary grow" type="button" (click)="guardar()">Guardar</button>
          <button class="btn" type="button" (click)="formAbierto = false">Cancelar</button>
        </div>
      </div>
    </div>

    <div class="modal-back" [class.show]="gruposAbiertos">
      <div class="modal">
        <h2 style="margin:0 0 8px;font-size:18px">Elegir grupo</h2>
        <table class="data">
          <thead>
            <tr>
              <th>Grupo</th>
              <th class="num">Precio</th>
            </tr>
          </thead>
          <tbody>
            @for (g of grupos; track g.idGrupoProducto) {
              <tr class="clickable" (click)="elegirGrupo(g)">
                <td>{{ g.nombreGrupo }}</td>
                <td class="num">{{ dinero(g.precioGrupo) }}</td>
              </tr>
            }
          </tbody>
        </table>
        <div class="row" style="margin-top:12px">
          <button class="btn grow" type="button" (click)="gruposAbiertos = false">Cerrar</button>
        </div>
      </div>
    </div>

    <div class="modal-back" [class.show]="stockAbierto">
      <div class="modal">
        <h2 style="margin:0 0 8px;font-size:18px">Sumar stock</h2>
        <p class="meta">{{ elegido?.nombre }}</p>
        <label class="field">Cantidad <input class="wide" type="number" min="0.001" [(ngModel)]="cantidadStock" /></label>
        <div class="row" style="margin-top:12px">
          <button class="btn btn-primary grow" type="button" (click)="sumar()">Sumar</button>
          <button class="btn" type="button" (click)="stockAbierto = false">Cancelar</button>
        </div>
      </div>
    </div>
    <div class="modal-back" [class.show]="asignarCategoriaAbierto">
      <div class="modal">
        <h2 style="margin:0 0 8px;font-size:18px">Asignar categoría</h2>
        <p class="meta">{{ marcados.size }} producto{{ marcados.size === 1 ? '' : 's' }} seleccionado{{ marcados.size === 1 ? '' : 's' }}.</p>
        <label class="field">
          Categoría
          <select class="wide" [(ngModel)]="idCategoriaAsignar">
            <option [ngValue]="0">Sin categoría</option>
            @for (c of categorias; track c.id) {
              <option [ngValue]="c.id">{{ etiquetaCategoria(c) }}</option>
            }
          </select>
        </label>
        @if (error) {
          <p class="error">{{ error }}</p>
        }
        <div class="row" style="margin-top:12px">
          <button class="btn btn-primary grow" type="button" (click)="asignarCategoria()">Asignar</button>
          <button class="btn" type="button" (click)="asignarCategoriaAbierto = false">Cancelar</button>
        </div>
      </div>
    </div>
    <div class="modal-back" [class.show]="aumentoAbierto">
      <div class="modal">
        <h2 style="margin:0 0 8px;font-size:18px">Aumentar precio y costo</h2>
        @if (listaRevision) {
          <p class="meta">Lista del proveedor: {{ listaRevision.encontrados.length }} se actualizan, {{ listaRevision.noEncontrados.length }} no se encontraron, {{ listaRevision.salteados.length }} se saltean. Todavía no se guardó nada.</p>
          <div style="max-height:45vh;overflow:auto">
            <table class="data">
              <thead><tr><th>Producto</th><th class="num">Costo antes</th><th class="num">Costo nuevo</th><th class="num">Precio antes</th><th class="num">Precio nuevo</th></tr></thead>
              <tbody>
                @for (f of listaRevision.encontrados; track f.fila) {
                  <tr>
                    <td>{{ f.nombre }}@if (f.unidadesPack > 1) { <span class="meta"> (pack de {{ f.unidadesPack }})</span> }@if (f.aviso) { <span class="meta"> {{ f.aviso }}</span> }</td>
                    <td class="num">{{ dinero(f.costoAntes) }}</td>
                    <td class="num">{{ dinero(f.costoNuevo) }}</td>
                    <td class="num">{{ dinero(f.precioAntes) }}</td>
                    <td class="num">{{ dinero(f.precioNuevo) }}</td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
          @if (listaRevision.noEncontrados.length > 0) {
            <p class="meta"><strong>No encontrados</strong> (no se crean): {{ codigosNoEncontrados }}</p>
          }
          @if (listaRevision.salteados.length > 0) {
            <p class="meta"><strong>Salteados:</strong></p>
            <ul class="meta">
              @for (s of listaRevision.salteados; track s.fila) {
                <li>Fila {{ s.fila }} {{ s.codigo }}: {{ s.motivo }}</li>
              }
            </ul>
          }
        } @else if (!vistaPrevia) {
        <label class="field">
          Alcance
          <select class="wide" [(ngModel)]="alcance">
            <option value="marcados">Productos marcados ({{ marcados.size }})</option>
            <option value="categoria">Toda una categoría</option>
            <option value="proveedor">Todo un proveedor</option>
          </select>
        </label>
        @if (alcance === 'proveedor') {
          <label class="field">
            Proveedor
            <select class="wide" [(ngModel)]="idProveedorAumento">
              <option [ngValue]="0">Elegí</option>
              @for (v of proveedores; track v.id) {
                <option [ngValue]="v.id">{{ v.nombre }}</option>
              }
            </select>
          </label>
          <div class="row" style="margin-bottom:8px">
            <button class="btn" type="button" (click)="bajarPlantillaLista()">Plantilla de lista</button>
            <label class="btn">
              Subir lista en Excel
              <input type="file" accept=".xlsx" hidden (change)="subirLista($event)" />
            </label>
          </div>
          <p class="meta">Con el Excel (código de barras y costo) se actualizan costo y precio. No crea productos ni suma stock. Para un aumento por porcentaje o pesos, usá las opciones de abajo.</p>
        }
        @if (alcance === 'categoria') {
          <label class="field">
            Categoría
            <select class="wide" [(ngModel)]="idCategoriaAumento">
              <option [ngValue]="0">Elegí</option>
              @for (c of categorias; track c.id) {
                <option [ngValue]="c.id">{{ c.nombre }}</option>
              }
            </select>
          </label>
        }
        <label class="field">
          Precio
          <select class="wide" [(ngModel)]="tipoPrecio">
            <option value="ninguno">No cambiar</option>
            <option value="porcentaje">Porcentaje</option>
            <option value="importe">Pesos</option>
          </select>
        </label>
        @if (tipoPrecio !== 'ninguno') {
          <label class="field">{{ tipoPrecio === 'porcentaje' ? 'Porcentaje de precio' : 'Pesos de precio' }}
            <input class="wide" type="number" min="0" step="0.01" [(ngModel)]="valorPrecio" />
          </label>
        }
        <label class="field">
          Costo
          <select class="wide" [(ngModel)]="tipoCosto">
            <option value="ninguno">No cambiar</option>
            <option value="porcentaje">Porcentaje</option>
            <option value="importe">Pesos</option>
          </select>
        </label>
        @if (tipoCosto !== 'ninguno') {
          <label class="field">{{ tipoCosto === 'porcentaje' ? 'Porcentaje de costo' : 'Pesos de costo' }}
            <input class="wide" type="number" min="0" step="0.01" [(ngModel)]="valorCosto" />
          </label>
        }
        <label class="field">
          Redondear precio hacia arriba
          <select class="wide" [(ngModel)]="redondeoPrecio">
            <option [ngValue]="0">No redondear</option>
            <option [ngValue]="10">A $ 10</option>
            <option [ngValue]="50">A $ 50</option>
            <option [ngValue]="100">A $ 100</option>
          </select>
        </label>
        <p class="meta">El precio y el costo se aumentan por separado. Si el producto está en un grupo, sube el grupo y todos sus miembros. El sector no tiene precio de lista: solo sube el costo. El precio por kilo no cambia.</p>
        } @else {
          <p class="meta">Vista previa: {{ vistaPrevia.total === 1 ? "se cambia 1 producto o grupo" : "se cambian " + vistaPrevia.total + " productos o grupos" }}. Todavía no se guardó nada.</p>
          <div style="max-height:50vh;overflow:auto">
            <table class="data">
              <thead><tr><th>Producto</th><th class="num">Precio antes</th><th class="num">Precio después</th><th class="num">Costo antes</th><th class="num">Costo después</th></tr></thead>
              <tbody>
                @for (f of vistaPrevia.filas; track $index) {
                  <tr>
                    <td>{{ f.nombre }}@if (f.idGrupoProducto) { <span class="meta"> (grupo)</span> }</td>
                    <td class="num">{{ dinero(f.precioAntes) }}</td>
                    <td class="num">{{ dinero(f.precioDespues) }}</td>
                    <td class="num">{{ dinero(f.costoAntes) }}</td>
                    <td class="num">{{ dinero(f.costoDespues) }}</td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
          @if (vistaPrevia.total > vistaPrevia.filas.length) {
            <p class="meta">Se muestran las primeras {{ vistaPrevia.filas.length }} de {{ vistaPrevia.total }}.</p>
          }
        }
        @if (error) {
          <p class="error">{{ error }}</p>
        }
        <div class="row" style="margin-top:12px">
          @if (listaRevision) {
            <button class="btn btn-primary grow" type="button" [disabled]="listaRevision.encontrados.length === 0" (click)="confirmarLista()">Confirmar lista</button>
            <button class="btn" type="button" (click)="listaRevision = null">Volver</button>
          } @else if (!vistaPrevia) {
            <button class="btn grow" type="button" (click)="verVistaPrevia()">Ver vista previa</button>
            <button class="btn btn-primary grow" type="button" (click)="aplicarAumento()">Aumentar</button>
            <button class="btn" type="button" (click)="aumentoAbierto = false">Cancelar</button>
          } @else {
            <button class="btn btn-primary grow" type="button" (click)="aplicarAumento()">Aplicar</button>
            <button class="btn" type="button" (click)="vistaPrevia = null">Volver</button>
          }
        </div>
      </div>
    </div>
    <div class="toast" [class.show]="toast">{{ toast }}</div>
  `
})
export class ProductosComponent implements OnInit {
  productos: ProductoDto[] = [];
  visibles: ProductoDto[] = [];
  grupos: GrupoListaDto[] = [];
  categorias: CategoriaDto[] = [];
  gruposFiltro: { idGrupoProducto: number; nombreGrupo: string }[] = [];
  total = 0;
  elegido: ProductoDto | null = null;
  q = '';
  tipo = '';
  grupo = '';
  stockBajo = false;
  tienda = '';
  agrupar = false;
  umbralStockBajo = 5;
  puedeModificarPrecios = false;
  seccionesCerradas = new Set<string>();
  error = '';
  toast = '';
  formAbierto = false;
  stockAbierto = false;
  aumentoAbierto = false;
  asignarCategoriaAbierto = false;
  idCategoriaAsignar = 0;
  marcados = new Set<number>();
  alcance: 'marcados' | 'categoria' | 'proveedor' = 'marcados';
  idCategoriaAumento = 0;
  idProveedorAumento = 0;
  proveedores: ProveedorListaDto[] = [];
  listaRevision: RevisionListaDto | null = null;
  private archivoLista = '';
  tipoPrecio = 'porcentaje';
  valorPrecio = 0;
  tipoCosto = 'ninguno';
  valorCosto = 0;
  redondeoPrecio = 0;
  vistaPrevia: VistaPreviaAumentoDto | null = null;
  ultimoAumento: UltimoAumentoDto | null = null;
  gruposAbiertos = false;
  editando: ProductoDto | null = null;
  cantidadStock = 1;
  factorGanancia = 1;
  iva = 1.21;
  form: Partial<ProductoDto> = {
    codigo: '',
    nombre: '',
    cantidad: 0,
    costo: 0,
    precio: 0,
    productoSector: false,
    precioPorKilo: 0,
    idGrupoProducto: 0,
    nombreGrupo: null,
    idCategoria: 0
  };

  constructor(
    private readonly api: ApiService,
    readonly sesion: SesionService,
    private readonly dialogo: DialogoService
  ) {}

  ngOnInit(): void {
    this.api.gruposFiltro().subscribe({ next: (r) => (this.gruposFiltro = r) });
    if (this.sesion.esDueno()) {
      this.api.configuracionLocal().subscribe({
        next: (c) => {
          this.factorGanancia = Number(c.factorGanancia) || 1;
          this.iva = Number(c.iva) || 1.21;
          if (this.formAbierto) this.aplicarPrecioCalculado();
        }
      });
      this.api.categorias().subscribe({ next: (r) => (this.categorias = r) });
      this.api.grupos().subscribe({ next: (r) => (this.grupos = r) });
      this.api.proveedores().subscribe({ next: (r) => (this.proveedores = r.filter((v) => v.activo)) });
      this.cargarUltimoAumento();
    }
    this.cargar();
  }

  get opcionSinCategoria(): string {
    const grupo = this.grupos.find((g) => g.idGrupoProducto === this.form.idGrupoProducto);
    const categoria = grupo?.idCategoria ? this.categorias.find((c) => c.id === grupo.idCategoria) : undefined;
    return categoria ? `La del grupo (${categoria.nombre})` : 'Sin categoría';
  }

  get secciones(): { nombre: string; productos: ProductoDto[] }[] {
    const mapa = new Map<string, ProductoDto[]>();
    for (const p of this.visibles) {
      const nombre = p.productoSector ? 'Sector' : p.nombreGrupo || 'Sin grupo';
      const productos = mapa.get(nombre) ?? [];
      productos.push(p);
      mapa.set(nombre, productos);
    }
    return [...mapa.entries()].sort(([a], [b]) => a.localeCompare(b, 'es')).map(([nombre, productos]) => ({ nombre, productos }));
  }

  alternarSeccion(nombre: string): void {
    if (this.seccionesCerradas.has(nombre)) this.seccionesCerradas.delete(nombre);
    else this.seccionesCerradas.add(nombre);
    this.seccionesCerradas = new Set(this.seccionesCerradas);
  }

  dinero(n: number): string {
    return `$ ${Number(n).toLocaleString('es-AR', { minimumFractionDigits: 0, maximumFractionDigits: 0 })}`;
  }

  fechaHora(iso?: string | null): string {
    if (!iso) return '—';
    const d = new Date(iso);
    return `${String(d.getDate()).padStart(2, '0')}/${String(d.getMonth() + 1).padStart(2, '0')}/${d.getFullYear()} ${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`;
  }

  private filtro(): FiltroProductos {
    return {
      sector: this.tipo === 'sector' ? true : this.tipo === 'comun' ? false : undefined,
      idGrupo: this.grupo && this.grupo !== 'sin' ? Number(this.grupo) : undefined,
      sinGrupo: this.grupo === 'sin',
      stockBajo: this.stockBajo,
      publicado: this.tienda === 'si' ? true : this.tienda === 'no' ? false : undefined
    };
  }

  cargar(): void {
    this.api.productos(this.q, this.filtro()).subscribe({
      next: (r) => {
        this.productos = r.items;
        this.visibles = r.items;
        this.total = r.total;
        this.umbralStockBajo = r.umbralStockBajo;
        this.puedeModificarPrecios = r.puedeModificarPrecio;
      },
      error: () => (this.error = 'No se pudieron cargar los productos.')
    });
  }

  aplicarTipo(tipo: string): void {
    this.tipo = tipo;
    this.cargar();
  }

  aplicarBuscar(q: string): void {
    this.q = q;
    this.cargar();
  }

  aplicarGrupo(grupo: string): void {
    this.grupo = grupo;
    this.cargar();
  }

  aplicarTienda(tienda: string): void {
    this.tienda = tienda;
    this.cargar();
  }

  publicablesVisibles(): ProductoDto[] {
    return this.visibles.filter((p) => p.publicable && !p.publicado);
  }

  publicar(p: ProductoDto, publicado: boolean): void {
    this.api.publicarProductos([p.id], publicado).subscribe({
      next: () => {
        p.publicado = publicado;
        this.error = '';
        this.mostrar(publicado ? 'Publicado en la tienda' : 'Oculto en la tienda');
      },
      error: (err) => {
        this.error = typeof err.error === 'string' ? err.error : 'No se pudo cambiar la publicación.';
        this.cargar();
      }
    });
  }

  hayMasQueLaPantalla(): boolean {
    return this.total > this.visibles.length;
  }

  async publicarLista(publicado: boolean): Promise<void> {
    const ids = (publicado ? this.publicablesVisibles() : this.visibles.filter((p) => p.publicado)).map((p) => p.id);
    const verbo = publicado ? 'publicar' : 'ocultar';
    let todaLaLista = false;
    if (this.hayMasQueLaPantalla()) {
      const eleccion = await this.dialogo.elegir(
        `La lista tiene ${this.total} productos y en pantalla se ven ${this.visibles.length}. ¿Querés ${verbo} toda la lista? Si no, se ${publicado ? 'publican' : 'ocultan'} solo los de la pantalla.`,
        'Toda la lista',
        'Solo la pantalla');
      if (eleccion === null) return;
      todaLaLista = eleccion === 'aceptar';
    } else {
      if (ids.length === 0) return;
      const texto = publicado
        ? `¿Publicar ${ids.length} productos de la lista en la tienda?`
        : `¿Ocultar ${ids.length} productos de la lista de la tienda?`;
      if (!await this.dialogo.confirmar(texto)) return;
    }
    if (!todaLaLista && ids.length === 0) {
      this.mostrar(publicado ? 'No hay productos para publicar en pantalla' : 'No hay productos para ocultar en pantalla');
      return;
    }
    const pedido = todaLaLista
      ? this.api.publicarListaProductos(publicado, this.q, this.filtro())
      : this.api.publicarProductos(ids, publicado);
    pedido.subscribe({
      next: (r) => {
        this.error = '';
        this.mostrar(`${r.actualizados} productos ${publicado ? 'publicados' : 'ocultos'}`);
        this.cargar();
      },
      error: (err) => (this.error = typeof err.error === 'string' ? err.error : 'No se pudo cambiar la publicación.')
    });
  }

  filtrar(): void {
    this.cargar();
  }

  nuevo(): void {
    this.editando = null;
    this.form = {
      codigo: '',
      nombre: '',
      cantidad: 0,
      costo: 0,
      precio: 0,
      productoSector: false,
      precioPorKilo: 0,
      idGrupoProducto: 0,
      nombreGrupo: null,
      idCategoria: 0
    };
    this.formAbierto = true;
  }

  cambiarCosto(costo: number | string): void {
    this.form.costo = Number(costo) || 0;
    this.aplicarPrecioCalculado();
  }

  cambiarSector(sector: boolean): void {
    this.form.productoSector = sector;
    if (sector) {
      this.form.precio = 0;
      this.form.costo = 0;
      this.form.idGrupoProducto = 0;
      this.form.nombreGrupo = null;
      return;
    }
    this.aplicarPrecioCalculado();
  }

  private aplicarPrecioCalculado(): void {
    if (this.form.productoSector || this.form.idGrupoProducto) return;
    const costo = Number(this.form.costo) || 0;
    this.form.precio = Math.round(costo * this.factorGanancia * this.iva * 100) / 100;
  }

  editar(p: ProductoDto): void {
    if (!this.sesion.esDueno() && (!this.puedeModificarPrecios || p.productoSector)) return;
    this.editando = p;
    this.form = { ...p, idCategoria: p.idCategoria ?? 0 };
    this.formAbierto = true;
  }

  abrirGrupos(): void {
    this.api.grupos().subscribe({
      next: (r) => {
        this.grupos = r;
        this.gruposAbiertos = true;
      },
      error: () => (this.error = 'No se pudieron cargar los grupos.')
    });
  }

  elegirGrupo(g: GrupoListaDto): void {
    this.form.idGrupoProducto = g.idGrupoProducto;
    this.form.nombreGrupo = g.nombreGrupo;
    this.form.costo = g.costo;
    this.form.precio = g.precioGrupo;
    this.gruposAbiertos = false;
  }

  sacarGrupo(): void {
    this.form.idGrupoProducto = 0;
    this.form.nombreGrupo = null;
  }

  guardar(): void {
    const pedido = this.form;
    const obs = this.editando && !this.sesion.esDueno()
      ? this.api.editarPrecioProducto(this.editando.id, Number(pedido.precio) || 0)
      : this.editando
        ? this.api.editarProducto(this.editando.id, pedido)
        : this.api.crearProducto(pedido);
    obs.subscribe({
      next: () => {
        this.formAbierto = false;
        this.error = '';
        if (pedido.productoSector) this.tipo = 'sector';
        this.mostrar('Guardado');
        this.cargar();
      },
      error: (err) => (this.error = typeof err.error === 'string' ? err.error : 'No se pudo guardar.')
    });
  }

  marcada(id: number): boolean {
    return this.marcados.has(id);
  }

  marcar(id: number, on: boolean): void {
    if (on) this.marcados.add(id);
    else this.marcados.delete(id);
    this.marcados = new Set(this.marcados);
  }

  todosVisiblesMarcados(): boolean {
    return this.visibles.length > 0 && this.visibles.every((p) => this.marcados.has(p.id));
  }

  marcarVisibles(on: boolean): void {
    for (const p of this.visibles) {
      if (on) this.marcados.add(p.id);
      else this.marcados.delete(p.id);
    }
    this.marcados = new Set(this.marcados);
  }

  abrirAumento(): void {
    this.alcance = this.marcados.size > 0 ? 'marcados' : 'categoria';
    this.tipoPrecio = 'porcentaje';
    this.valorPrecio = 0;
    this.tipoCosto = 'ninguno';
    this.valorCosto = 0;
    this.idCategoriaAumento = 0;
    this.idProveedorAumento = 0;
    this.listaRevision = null;
    this.redondeoPrecio = 0;
    this.vistaPrevia = null;
    this.error = '';
    this.aumentoAbierto = true;
  }

  abrirAsignarCategoria(): void {
    this.idCategoriaAsignar = 0;
    this.error = '';
    this.asignarCategoriaAbierto = true;
  }

  etiquetaCategoria(c: CategoriaDto): string {
    if (!c.idPadre) return c.nombre;
    const padre = this.categorias.find((x) => x.id === c.idPadre);
    return padre ? `${padre.nombre} › ${c.nombre}` : c.nombre;
  }

  asignarCategoria(): void {
    if (this.marcados.size === 0) {
      this.error = 'Marcá al menos un producto.';
      return;
    }
    const ids = [...this.marcados];
    this.api.asignarCategoriaProductos(ids, this.idCategoriaAsignar).subscribe({
      next: (r) => {
        this.asignarCategoriaAbierto = false;
        this.marcados = new Set();
        this.error = '';
        this.mostrar(r.actualizados === 1 ? 'Categoría asignada a 1 producto' : `Categoría asignada a ${r.actualizados} productos`);
        this.cargar();
      },
      error: (err) => (this.error = typeof err.error === 'string' ? err.error : 'No se pudo asignar la categoría.')
    });
  }

  private armarPedidoAumento(): PedidoAumento | null {
    const precio = Number(this.valorPrecio) || 0;
    const costo = Number(this.valorCosto) || 0;
    const subePrecio = this.tipoPrecio !== 'ninguno' && precio > 0;
    const subeCosto = this.tipoCosto !== 'ninguno' && costo > 0;
    if (!subePrecio && !subeCosto) {
      this.error = 'Indicá un aumento de precio o de costo.';
      return null;
    }
    const pedido: PedidoAumento = {
      tipoPrecio: subePrecio ? this.tipoPrecio : 'ninguno',
      valorPrecio: subePrecio ? precio : 0,
      tipoCosto: subeCosto ? this.tipoCosto : 'ninguno',
      valorCosto: subeCosto ? costo : 0,
      redondeoPrecio: subePrecio ? this.redondeoPrecio : 0
    };
    if (this.alcance === 'proveedor') {
      if (!this.idProveedorAumento) {
        this.error = 'Elegí un proveedor.';
        return null;
      }
      pedido.idProveedor = this.idProveedorAumento;
      return pedido;
    }
    if (this.alcance === 'categoria') {
      if (!this.idCategoriaAumento) {
        this.error = 'Elegí una categoría.';
        return null;
      }
      pedido.idCategoria = this.idCategoriaAumento;
      return pedido;
    }
    if (this.marcados.size === 0) {
      this.error = 'Marcá al menos un producto.';
      return null;
    }
    pedido.ids = [...this.marcados];
    return pedido;
  }

  verVistaPrevia(): void {
    const pedido = this.armarPedidoAumento();
    if (!pedido) return;
    this.api.vistaPreviaAumento(pedido).subscribe({
      next: (r) => {
        this.vistaPrevia = r;
        this.error = '';
      },
      error: (err) => (this.error = typeof err.error === 'string' ? err.error : 'No se pudo armar la vista previa.')
    });
  }

  aplicarAumento(): void {
    const pedido = this.armarPedidoAumento();
    if (!pedido) return;
    this.api.aumentarProductos(pedido).subscribe({
      next: (r) => {
        this.aumentoAbierto = false;
        this.vistaPrevia = null;
        this.marcados = new Set();
        this.error = '';
        this.mostrar(r.cantidad === 1 ? 'Aumentaste 1 producto' : `Aumentaste ${r.cantidad} productos`);
        this.cargarUltimoAumento();
        this.cargar();
      },
      error: (err) => (this.error = typeof err.error === 'string' ? err.error : 'No se pudo aumentar.')
    });
  }

  get codigosNoEncontrados(): string {
    return (this.listaRevision?.noEncontrados ?? []).map((n) => n.codigo || `fila ${n.fila}`).join(', ');
  }

  bajarPlantillaLista(): void {
    this.api.plantillaListaProveedor().subscribe({
      next: (archivo) => descargar(archivo, 'plantilla-lista-costos.xlsx'),
      error: () => (this.error = 'No se pudo bajar la plantilla.')
    });
  }

  subirLista(evento: Event): void {
    const input = evento.target as HTMLInputElement;
    const archivo = input.files?.[0];
    input.value = '';
    if (!archivo) return;
    if (!this.idProveedorAumento) {
      this.error = 'Elegí un proveedor.';
      return;
    }
    if (archivo.size > 2 * 1024 * 1024) {
      this.error = 'El Excel no puede pesar más de 2 MB.';
      return;
    }
    this.error = '';
    const lector = new FileReader();
    lector.onload = () => {
      this.archivoLista = String(lector.result);
      this.api.revisarListaProveedor(this.idProveedorAumento, this.archivoLista).subscribe({
        next: (r) => (this.listaRevision = r),
        error: (err) => (this.error = typeof err.error === 'string' ? err.error : 'No se pudo leer el Excel.')
      });
    };
    lector.readAsDataURL(archivo);
  }

  confirmarLista(): void {
    if (!this.listaRevision) return;
    this.api.aplicarListaProveedor(this.idProveedorAumento, this.archivoLista).subscribe({
      next: (r) => {
        this.aumentoAbierto = false;
        this.listaRevision = null;
        this.archivoLista = '';
        this.error = '';
        this.mostrar(r.actualizados === 1 ? 'Actualizaste 1 producto' : `Actualizaste ${r.actualizados} productos`);
        this.cargar();
      },
      error: (err) => (this.error = typeof err.error === 'string' ? err.error : 'No se pudo aplicar la lista.')
    });
  }

  private cargarUltimoAumento(): void {
    this.api.ultimoAumento().subscribe({ next: (u) => (this.ultimoAumento = u) });
  }

  async deshacerUltimoAumento(): Promise<void> {
    const u = this.ultimoAumento;
    if (!u || u.deshecho) return;
    const cuando = new Date(u.fecha).toLocaleString('es-AR');
    if (!await this.dialogo.confirmar(`¿Deshacer el aumento del ${cuando} hecho por ${u.usuario}? Solo vuelven los productos que nadie tocó después.`)) return;
    this.api.deshacerAumento().subscribe({
      next: (r) => {
        const omitidos = r.omitidos.map((o) => o.nombre);
        const aviso = omitidos.length > 0
          ? `Se restauraron ${r.restaurados}. No se tocaron por haber cambiado después: ${omitidos.join(', ')}`
          : `Se restauraron ${r.restaurados}`;
        this.mostrar(aviso);
        this.cargarUltimoAumento();
        this.cargar();
      },
      error: (err) => this.mostrar(typeof err.error === 'string' ? err.error : 'No se pudo deshacer.')
    });
  }
  abrirStock(): void {
    if (!this.elegido) return;
    this.cantidadStock = 1;
    this.stockAbierto = true;
  }

  sumar(): void {
    if (!this.elegido) return;
    this.api.sumarStock(this.elegido.id, Number(this.cantidadStock)).subscribe({
      next: () => {
        this.stockAbierto = false;
        this.mostrar('Stock actualizado');
        this.cargar();
      },
      error: (err) => (this.error = typeof err.error === 'string' ? err.error : 'No se pudo sumar stock.')
    });
  }

  private mostrar(t: string): void {
    this.toast = t;
    setTimeout(() => (this.toast = ''), 2000);
  }
}
