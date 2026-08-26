import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';

export interface ProductoDto {
  id: number;
  idLocal: number;
  codigo: string;
  nombre: string;
  cantidad: number;
  costo: number;
  precio: number;
  productoSector: boolean;
  idGrupoProducto?: number;
  nombreGrupo?: string | null;
}

export interface LocalDto {
  idLocal: number;
  nombre: string;
  estadoAbono: string;
  vence?: string;
}

export interface ItemVenta {
  idProducto: number;
  codigo: string;
  nombre: string;
  cantidad: number;
  precioUnitario: number;
}

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly base = environment.apiUrl;

  constructor(private readonly http: HttpClient) {}

  login(usuario: string, clave: string) {
    return this.http.post<{ token: string; rol: string; nombre: string }>(
      `${this.base}/auth/login`,
      { usuario, clave }
    );
  }

  locales() {
    return this.http.get<LocalDto[]>(`${this.base}/locales`);
  }

  entrar(id: number) {
    return this.http.post<{ token: string }>(`${this.base}/locales/${id}/entrar`, {});
  }

  productos(q = '', opts?: { sector?: boolean; codigo?: string }) {
    let params = new HttpParams().set('tamano', 100);
    if (q) params = params.set('q', q);
    if (opts?.codigo) params = params.set('codigo', opts.codigo);
    if (opts?.sector === true) params = params.set('sector', 'true').set('tipo', 'sector');
    if (opts?.sector === false) params = params.set('sector', 'false').set('tipo', 'comun');
    return this.http.get<{ total: number; items: ProductoDto[] }>(`${this.base}/productos`, { params });
  }

  crearProducto(p: Partial<ProductoDto>) {
    return this.http.post<ProductoDto>(`${this.base}/productos`, {
      codigo: p.codigo,
      nombre: p.nombre,
      cantidad: p.cantidad ?? 0,
      costo: p.costo ?? 0,
      precio: p.precio ?? 0,
      productoSector: p.productoSector ?? false,
      idGrupoProducto: p.idGrupoProducto ?? 0
    });
  }

  editarProducto(id: number, p: Partial<ProductoDto>) {
    return this.http.put<ProductoDto>(`${this.base}/productos/${id}`, {
      codigo: p.codigo,
      nombre: p.nombre,
      cantidad: p.cantidad ?? 0,
      costo: p.costo ?? 0,
      precio: p.precio ?? 0,
      productoSector: p.productoSector ?? false,
      idGrupoProducto: p.idGrupoProducto ?? 0
    });
  }

  sumarStock(id: number, cantidad: number) {
    return this.http.post<ProductoDto>(`${this.base}/productos/${id}/stock`, { cantidad });
  }

  cobrar(items: ItemVenta[], metodoPago: string, descuentoPorcentaje: number, cobrarAlCosto: boolean) {
    return this.http.post<{ idInformeVenta: number; total: number }>(`${this.base}/ventas`, {
      items,
      metodoPago,
      descuentoPorcentaje,
      cobrarAlCosto
    });
  }

  ventas(opts: { desde?: string; hasta?: string; medio?: string; pagina?: number; tamano?: number } = {}) {
    let params = new HttpParams().set('tamano', String(opts.tamano ?? 100));
    if (opts.desde) params = params.set('desde', opts.desde);
    if (opts.hasta) params = params.set('hasta', opts.hasta);
    if (opts.medio) params = params.set('medio', opts.medio);
    if (opts.pagina) params = params.set('pagina', String(opts.pagina));
    return this.http.get<{
      total: number;
      suma: number;
      pagina: number;
      tamano: number;
      items: VentaListaDto[];
    }>(`${this.base}/ventas`, { params });
  }

  venta(id: number) {
    return this.http.get<VentaDetalleDto>(`${this.base}/ventas/${id}`);
  }

  anularVenta(id: number) {
    return this.http.delete(`${this.base}/ventas/${id}`);
  }

  cajas() {
    return this.http.get<{
      proximoNro: number;
      pendientesHoy: { tickets: number; total: number };
      items: CajaListaDto[];
    }>(`${this.base}/cajas`);
  }

  caja(nro: number) {
    return this.http.get<CajaDetalleDto>(`${this.base}/cajas/${nro}`);
  }

  cerrarCaja(fecha?: string) {
    return this.http.post<{ nroCaja: number; filas: { metodoPago: string; cantidadVentas: number; total: number }[] }>(
      `${this.base}/cajas/cerrar`,
      fecha ? { fecha } : {}
    );
  }

  grupos() {
    return this.http.get<GrupoListaDto[]>(`${this.base}/grupos`);
  }

  grupo(id: number) {
    return this.http.get<GrupoDetalleDto>(`${this.base}/grupos/${id}`);
  }

  crearGrupo(body: { nombreGrupo: string; costo: number; precioGrupo: number; ganancia?: number; gananciaIndividual?: boolean }) {
    return this.http.post<GrupoListaDto>(`${this.base}/grupos`, body);
  }

  editarGrupo(id: number, body: { nombreGrupo: string; costo: number; precioGrupo: number; ganancia?: number; gananciaIndividual?: boolean }) {
    return this.http.put<GrupoListaDto>(`${this.base}/grupos/${id}`, body);
  }

  eliminarGrupo(id: number) {
    return this.http.delete(`${this.base}/grupos/${id}`);
  }

  miembrosGrupo(id: number, idsProducto: number[]) {
    return this.http.put<{ idGrupoProducto: number; cantidad: number }>(`${this.base}/grupos/${id}/miembros`, {
      idsProducto
    });
  }
}

export interface GrupoListaDto {
  idGrupoProducto: number;
  nombreGrupo: string;
  costo: number;
  precioGrupo: number;
  cantidad: number;
}

export interface GrupoDetalleDto {
  idGrupoProducto: number;
  nombreGrupo: string;
  costo: number;
  precioGrupo: number;
  miembros: { id: number; codigo: string; nombre: string; costo: number; precio: number }[];
}

export interface VentaListaDto {
  idInformeVenta: number;
  fecha: string;
  total: number;
  metodoPago: string;
  descuento: number;
  precioCosto: string;
  nroCaja?: number | null;
}

export interface VentaDetalleDto {
  idInformeVenta: number;
  fecha: string;
  total: number;
  subtotal: number;
  metodoPago: string;
  descuento: number;
  precioCosto: string;
  nroCaja?: number | null;
  items: {
    idInformeVentaDetalle: number;
    idProducto?: number | null;
    codigo: string;
    nombre: string;
    cantidad: number;
    precio: number;
    costo?: number | null;
    subTotal: number;
  }[];
}

export interface CajaListaDto {
  nroCaja: number;
  fecha: string;
  nombreCierre: string;
  total: number;
  cantidadVentas: number;
}

export interface CajaDetalleDto {
  nroCaja: number;
  fecha: string;
  nombreCierre: string;
  filas: { metodoPago: string; cantidadVentas: number; total: number }[];
}
