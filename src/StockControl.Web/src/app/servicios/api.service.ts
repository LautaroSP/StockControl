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
      productoSector: p.productoSector ?? false
    });
  }

  editarProducto(id: number, p: Partial<ProductoDto>) {
    return this.http.put<ProductoDto>(`${this.base}/productos/${id}`, {
      codigo: p.codigo,
      nombre: p.nombre,
      cantidad: p.cantidad ?? 0,
      costo: p.costo ?? 0,
      precio: p.precio ?? 0,
      productoSector: p.productoSector ?? false
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
}
