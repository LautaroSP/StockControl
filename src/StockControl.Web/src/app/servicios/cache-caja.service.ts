import { Injectable } from '@angular/core';
import { ComboVigenteDto, MetodoPagoDto, OfertaVigenteDto, ProductoDto, VentaPedidoDto } from './api.service';
import { SesionService } from './sesion.service';

export type EstadoVentaPendiente = 'pendiente' | 'rechazada';

export interface VentaPendiente {
  idVentaCliente: string;
  pedido: VentaPedidoDto;
  total: number;
  fechaCobro: string;
  estado: EstadoVentaPendiente;
  intentos: number;
  proximoIntento: number;
  ultimoError: string;
}

const ALMACENES = ['productos', 'mediosPago', 'ofertas', 'combos', 'ventasPendientes', 'meta'] as const;
type Almacen = (typeof ALMACENES)[number];

const CLAVES: Record<Almacen, string> = {
  productos: 'id',
  mediosPago: 'id',
  ofertas: 'idProducto',
  combos: 'id',
  ventasPendientes: 'idVentaCliente',
  meta: 'clave'
};

/** Lee y escribe IndexedDB: catálogo, medios, ofertas, combos y cola de ventas. Una base por local. */
@Injectable({ providedIn: 'root' })
export class CacheCajaService {
  private conexiones = new Map<string, Promise<IDBDatabase>>();

  constructor(private readonly sesion: SesionService) {}

  async guardarCatalogo(productos: ProductoDto[], umbralStockBajo: number): Promise<void> {
    await this.reemplazar('productos', productos);
    await this.poner('meta', { clave: 'umbralStockBajo', valor: umbralStockBajo });
    await this.poner('meta', { clave: 'ultimaCarga', valor: Date.now() });
  }

  productos(): Promise<ProductoDto[]> {
    return this.todos<ProductoDto>('productos');
  }

  async umbralStockBajo(): Promise<number> {
    return (await this.meta<number>('umbralStockBajo')) ?? 5;
  }

  async ultimaCarga(): Promise<number> {
    return (await this.meta<number>('ultimaCarga')) ?? 0;
  }

  /** Solo los medios activos y manuales: sin conexión no hay Mercado Pago. */
  guardarMedios(medios: MetodoPagoDto[]): Promise<void> {
    return this.reemplazar('mediosPago', medios.filter((m) => m.activo && (!m.integracion || m.integracion === 'manual')));
  }

  mediosPago(): Promise<MetodoPagoDto[]> {
    return this.todos<MetodoPagoDto>('mediosPago');
  }

  guardarOfertas(ofertas: OfertaVigenteDto[]): Promise<void> {
    return this.reemplazar('ofertas', ofertas);
  }

  ofertas(): Promise<OfertaVigenteDto[]> {
    return this.todos<OfertaVigenteDto>('ofertas');
  }

  guardarCombos(combos: ComboVigenteDto[]): Promise<void> {
    return this.reemplazar('combos', combos);
  }

  combos(): Promise<ComboVigenteDto[]> {
    return this.todos<ComboVigenteDto>('combos');
  }

  /** Baja el stock de la copia local para no vender de más en este puesto. Es informativo. */
  async descontarStock(items: { idProducto: number; cantidad: number }[]): Promise<void> {
    const productos = await this.productos();
    for (const item of items) {
      const p = productos.find((x) => x.id === item.idProducto);
      if (!p || p.productoSector) continue;
      p.cantidad = Math.max(0, p.cantidad - item.cantidad);
      await this.poner('productos', p);
    }
  }

  poner(almacen: Almacen, valor: unknown): Promise<void> {
    return this.transaccion(almacen, 'readwrite', (s) => s.put(valor)).then(() => undefined);
  }

  quitar(almacen: Almacen, clave: string): Promise<void> {
    return this.transaccion(almacen, 'readwrite', (s) => s.delete(clave)).then(() => undefined);
  }

  ventasPendientes(): Promise<VentaPendiente[]> {
    return this.todos<VentaPendiente>('ventasPendientes');
  }

  guardarVenta(venta: VentaPendiente): Promise<void> {
    return this.poner('ventasPendientes', venta);
  }

  quitarVenta(idVentaCliente: string): Promise<void> {
    return this.quitar('ventasPendientes', idVentaCliente);
  }

  private async meta<T>(clave: string): Promise<T | undefined> {
    const fila = await this.transaccion<{ valor: T } | undefined>('meta', 'readonly', (s) => s.get(clave));
    return fila?.valor;
  }

  private todos<T>(almacen: Almacen): Promise<T[]> {
    return this.transaccion<T[]>(almacen, 'readonly', (s) => s.getAll());
  }

  private async reemplazar(almacen: Almacen, filas: unknown[]): Promise<void> {
    const db = await this.abrir();
    await new Promise<void>((resolver, rechazar) => {
      const tx = db.transaction(almacen, 'readwrite');
      const s = tx.objectStore(almacen);
      s.clear();
      for (const fila of filas) s.put(fila);
      tx.oncomplete = () => resolver();
      tx.onerror = () => rechazar(tx.error);
      tx.onabort = () => rechazar(tx.error);
    });
  }

  private async transaccion<T>(almacen: Almacen, modo: IDBTransactionMode, accion: (s: IDBObjectStore) => IDBRequest): Promise<T> {
    const db = await this.abrir();
    return new Promise<T>((resolver, rechazar) => {
      const tx = db.transaction(almacen, modo);
      const pedido = accion(tx.objectStore(almacen));
      tx.oncomplete = () => resolver(pedido.result as T);
      tx.onerror = () => rechazar(tx.error);
      tx.onabort = () => rechazar(tx.error);
    });
  }

  private abrir(): Promise<IDBDatabase> {
    const nombre = `stockcontrol-caja-${this.sesion.idLocal()}`;
    let conexion = this.conexiones.get(nombre);
    if (!conexion) {
      conexion = new Promise<IDBDatabase>((resolver, rechazar) => {
        const pedido = indexedDB.open(nombre, 1);
        pedido.onupgradeneeded = () => {
          for (const a of ALMACENES) pedido.result.createObjectStore(a, { keyPath: CLAVES[a] });
        };
        pedido.onsuccess = () => resolver(pedido.result);
        pedido.onerror = () => rechazar(pedido.error);
      });
      this.conexiones.set(nombre, conexion);
    }
    return conexion;
  }
}
