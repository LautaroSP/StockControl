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
  fechaModificacion?: string | null;
  usuarioModificacion?: string | null;
  puedeModificarPrecio?: boolean;
}

export type EstadoAbono = 'trial' | 'al_dia' | 'por_vencer' | 'vencido';

export type EstadoDebito = '' | 'pendiente' | 'activa' | 'pausada' | 'cancelada';

export interface LocalDto {
  idLocal: number;
  nombre: string;
  estado: EstadoAbono;
  vence: string | null;
  diasRestantes: number;
  puedeOperarCaja: boolean;
  debito: EstadoDebito;
}

export interface AbonoDto {
  estado: EstadoAbono;
  vence: string | null;
  limite: string | null;
  diasRestantes: number;
  puedeOperarCaja: boolean;
  enGracia: boolean;
  pagoPendiente: boolean;
  debito: EstadoDebito;
  email: string;
  precioMensual: number;
  ventasWhatsApp: string | null;
}

export interface RegistroPedidoDto {
  nombre: string;
  email: string;
  nombreUsuario: string;
  nombreLocal: string;
  modalidad: 'prueba' | 'pagar';
  clave: string;
  sitio: string;
}

export interface ItemVenta {
  idProducto: number;
  codigo: string;
  nombre: string;
  cantidad: number;
  precioUnitario: number;
}

export interface ConfiguracionLocalDto {
  nombreLocal: string;
  factorGanancia: number;
  iva: number;
  stockRigido: boolean;
  umbralStockBajo: number;
  empleadoPuedeModificarPrecios: boolean;
  formatoTicket: string;
  imprimirTicketAlCobrar: boolean;
  cantidadCajas: number;
}

export interface ConfiguracionTicketDto {
  formatoTicket: string;
  imprimirTicketAlCobrar: boolean;
  nombreLocal: string;
}

export interface UsuarioDto {
  id: number;
  nombre: string;
  nombreUsuario: string;
  rol: string;
  activo: boolean;
  locales: { idLocal: number; nombre: string }[];
}

export interface PagoVenta {
  idMetodoPago: number;
  importe: number;
}

export interface MetodoPagoDto {
  id: number;
  descripcion: string;
  activo: boolean;
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
    return this.http.get<{ totalMensual: number; precioMensual: number; items: LocalDto[] }>(`${this.base}/locales`);
  }

  registrar(pedido: RegistroPedidoDto) {
    return this.http.post<{ token: string; rol: string; nombre: string }>(`${this.base}/registro`, pedido);
  }

  abonoActual() {
    return this.http.get<AbonoDto>(`${this.base}/locales/actual/abono`);
  }

  pagarAbono() {
    return this.http.post<{ urlPago: string }>(`${this.base}/locales/actual/abono/pagar`, {});
  }

  activarDebito(mailPagador: string) {
    return this.http.post<{ urlPago: string }>(`${this.base}/locales/actual/abono/debito`, { mailPagador });
  }

  cancelarDebito() {
    return this.http.delete(`${this.base}/locales/actual/abono/debito`);
  }

  entrar(id: number) {
    return this.http.post<{ token: string }>(`${this.base}/locales/${id}/entrar`, {});
  }

  productos(q = '', opts?: { sector?: boolean; codigo?: string; idGrupo?: number; sinGrupo?: boolean; stockBajo?: boolean; idProveedor?: number }) {
    let params = new HttpParams().set('tamano', 100);
    if (opts?.idProveedor) params = params.set('idProveedor', String(opts.idProveedor));
    if (q) params = params.set('q', q);
    if (opts?.codigo) params = params.set('codigo', opts.codigo);
    if (opts?.sector === true) params = params.set('sector', 'true').set('tipo', 'sector');
    if (opts?.sector === false) params = params.set('sector', 'false').set('tipo', 'comun');
    if (opts?.idGrupo) params = params.set('idGrupo', String(opts.idGrupo));
    if (opts?.sinGrupo) params = params.set('sinGrupo', 'true');
    if (opts?.stockBajo) params = params.set('stockBajo', 'true');
    return this.http.get<{ total: number; umbralStockBajo: number; puedeModificarPrecio: boolean; items: ProductoDto[] }>(`${this.base}/productos`, { params });
  }

  usuarios() {
    return this.http.get<UsuarioDto[]>(`${this.base}/usuarios`);
  }

  crearUsuario(body: { nombre: string; nombreUsuario: string; rol: string; idsLocal: number[]; clave: string }) {
    return this.http.post<UsuarioDto>(`${this.base}/usuarios`, body);
  }

  editarUsuario(id: number, body: { nombre: string; nombreUsuario: string; rol: string; idsLocal: number[] }) {
    return this.http.put<UsuarioDto>(`${this.base}/usuarios/${id}`, body);
  }

  cambiarEstadoUsuario(id: number, activo: boolean) {
    return this.http.post(`${this.base}/usuarios/${id}/${activo ? 'activar' : 'desactivar'}`, {});
  }

  resetearClaveUsuario(id: number, clave: string) {
    return this.http.post(`${this.base}/usuarios/${id}/resetear-clave`, { clave });
  }

  gruposFiltro() {
    return this.http.get<{ idGrupoProducto: number; nombreGrupo: string }[]>(`${this.base}/productos/grupos`);
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

  cobrar(items: ItemVenta[], pagos: PagoVenta[], descuentoPorcentaje: number, cobrarAlCosto: boolean) {
    return this.http.post<{ idInformeVenta: number; total: number }>(`${this.base}/ventas`, {
      items,
      pagos,
      descuentoPorcentaje,
      cobrarAlCosto
    });
  }

  editarPrecioProducto(id: number, precio: number) {
    return this.http.put<ProductoDto>(`${this.base}/productos/${id}/precio`, { precio });
  }

  mediosPago() {
    return this.http.get<MetodoPagoDto[]>(`${this.base}/medios-pago`);
  }

  crearMedioPago(descripcion: string) {
    return this.http.post<MetodoPagoDto>(`${this.base}/medios-pago`, { descripcion });
  }

  editarMedioPago(id: number, descripcion: string) {
    return this.http.put<MetodoPagoDto>(`${this.base}/medios-pago/${id}`, { descripcion });
  }

  desactivarMedioPago(id: number) {
    return this.http.delete(`${this.base}/medios-pago/${id}`);
  }

  activarMedioPago(id: number) {
    return this.http.post<MetodoPagoDto>(`${this.base}/medios-pago/${id}/activar`, {});
  }

  ventas(opts: { desde?: string; hasta?: string; medio?: string; nroCaja?: number; idCierre?: number; pagina?: number; tamano?: number } = {}) {
    let params = new HttpParams().set('tamano', String(opts.tamano ?? 100));
    if (opts.desde) params = params.set('desde', opts.desde);
    if (opts.hasta) params = params.set('hasta', opts.hasta);
    if (opts.medio) params = params.set('medio', opts.medio);
    if (opts.nroCaja) params = params.set('nroCaja', String(opts.nroCaja));
    if (opts.idCierre) params = params.set('idCierre', String(opts.idCierre));
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
      nroCaja: number;
      pendientesHoy: { tickets: number; total: number };
      items: CajaListaDto[];
    }>(`${this.base}/cajas`);
  }

  cajasAbiertas(fecha?: string) {
    let params = new HttpParams();
    if (fecha) params = params.set('fecha', fecha);
    return this.http.get<{ nroCaja: number; tickets: number; total: number }[]>(`${this.base}/cajas/abiertas`, { params });
  }

  consultaCajas(opts: { desde?: string; hasta?: string; quien?: string; medio?: string; pagina?: number; tamano?: number } = {}) {
    let params = new HttpParams().set('tamano', String(opts.tamano ?? 100));
    if (opts.desde) params = params.set('desde', opts.desde);
    if (opts.hasta) params = params.set('hasta', opts.hasta);
    if (opts.quien) params = params.set('quien', opts.quien);
    if (opts.medio) params = params.set('medio', opts.medio);
    if (opts.pagina) params = params.set('pagina', String(opts.pagina));
    return this.http.get<{
      total: number;
      pagina: number;
      tamano: number;
      items: CajaListaDto[];
      unificables: GrupoUnificableDto[];
    }>(`${this.base}/cajas/consulta`, { params });
  }

  unificarCajas(idsCierre: number[]) {
    return this.http.post<{
      idCierre: number;
      eliminados: number[];
      filas: { metodoPago: string; cantidadVentas: number; total: number }[];
    }>(`${this.base}/cajas/unificar`, { idsCierre });
  }

  filtrosCajas() {
    return this.http.get<{ personas: string[]; medios: string[] }>(`${this.base}/cajas/filtros`);
  }

  cierre(idCierre: number) {
    return this.http.get<CajaDetalleDto>(`${this.base}/cajas/cierres/${idCierre}`);
  }

  /** @deprecated preferir cierre(idCierre) */
  caja(nro: number) {
    return this.cierre(nro);
  }

  puestos() {
    return this.http.get<{
      cantidad: number;
      ocupaciones: { nroCaja: number; nombreUsuario: string; idUsuario: number }[];
    }>(`${this.base}/cajas/puestos`);
  }

  elegirPuesto(nro: number) {
    return this.http.post<{ token: string; nroCaja: number; aviso?: string | null }>(
      `${this.base}/cajas/puestos/${nro}/elegir`,
      {}
    );
  }

  cantidadCajas() {
    return this.http.get<{ cantidad: number }>(`${this.base}/configuracion/cajas`);
  }

  setCantidadCajas(cantidad: number) {
    return this.http.put<{ cantidad: number }>(`${this.base}/configuracion/cajas`, { cantidad });
  }

  configuracionLocal() {
    return this.http.get<ConfiguracionLocalDto>(`${this.base}/configuracion/local`);
  }

  configuracionTicket() {
    return this.http.get<ConfiguracionTicketDto>(`${this.base}/configuracion/ticket`);
  }

  guardarConfiguracionLocal(configuracion: ConfiguracionLocalDto & { recalcularPrecios: boolean }) {
    return this.http.put<{ recalculados: number }>(`${this.base}/configuracion/local`, configuracion);
  }

  cerrarCaja(fecha?: string, desglose: 'medio' | 'usuario' = 'medio', todas = false) {
    return this.http.post<{
      idCierre: number;
      nroCaja: number;
      filas: { metodoPago: string; cantidadVentas: number; total: number }[];
    }>(`${this.base}/cajas/cerrar`, {
      ...(fecha ? { fecha } : {}),
      desglose,
      todas
    });
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

  generarCarteles(idsProducto: number[], tamano: TamanoCartel) {
    return this.http.post<CartelesDto>(`${this.base}/carteles`, { idsProducto, tamano });
  }

  plantillasCartel() {
    return this.http.get<PlantillasCartelDto>(`${this.base}/carteles/plantillas`);
  }

  subirPlantillaCartel(tamano: TamanoCartel, imagen: string) {
    return this.http.put<{ tamano: TamanoCartel; plantilla: string }>(`${this.base}/carteles/plantillas/${tamano}`, { imagen });
  }

  quitarPlantillaCartel(tamano: TamanoCartel) {
    return this.http.delete(`${this.base}/carteles/plantillas/${tamano}`);
  }

  estadisticas(params?: { idLocal?: number; todos?: boolean; desde?: string; hasta?: string; mes?: string }) {
    let p = new HttpParams();
    if (params?.idLocal) p = p.set('idLocal', params.idLocal);
    if (params?.todos) p = p.set('todos', 'true');
    if (params?.desde) p = p.set('desde', params.desde);
    if (params?.hasta) p = p.set('hasta', params.hasta);
    if (params?.mes) p = p.set('mes', params.mes);
    return this.http.get<EstadisticasRespuestaDto>(`${this.base}/estadisticas`, { params: p });
  }

  proveedores() {
    return this.http.get<ProveedorListaDto[]>(`${this.base}/proveedores`);
  }

  proveedor(id: number) {
    return this.http.get<ProveedorFichaDto>(`${this.base}/proveedores/${id}`);
  }

  crearProveedor(body: ProveedorFormDto) {
    return this.http.post<ProveedorListaDto>(`${this.base}/proveedores`, body);
  }

  editarProveedor(id: number, body: ProveedorFormDto) {
    return this.http.put<ProveedorListaDto>(`${this.base}/proveedores/${id}`, body);
  }

  eliminarProveedor(id: number) {
    return this.http.delete<{ desactivado: boolean }>(`${this.base}/proveedores/${id}`);
  }

  activarProveedor(id: number) {
    return this.http.post<ProveedorListaDto>(`${this.base}/proveedores/${id}/activar`, {});
  }

  plantillaProveedor() {
    return this.http.get(`${this.base}/proveedores/plantilla`, { responseType: 'blob' });
  }

  revisarCatalogo(id: number, archivo: string) {
    return this.http.post<RevisionCatalogoDto>(`${this.base}/proveedores/${id}/catalogo/revisar`, { archivo });
  }

  confirmarCatalogo(id: number, filas: FilaCatalogoDto[]) {
    return this.http.post<{ creados: number; actualizados: number; invalidas: FilaInvalidaDto[] }>(
      `${this.base}/proveedores/${id}/catalogo/confirmar`,
      { filas }
    );
  }

  crearOrdenCompra(id: number, lineas: { idProducto: number; cantidad: number }[]) {
    return this.http.post<OrdenGeneradaDto>(`${this.base}/proveedores/${id}/ordenes`, { lineas });
  }

  excelOrdenCompra(id: number) {
    return this.http.get(`${this.base}/ordenes-compra/${id}/excel`, { responseType: 'blob' });
  }

  ordenesProveedor(id: number, pagina = 1) {
    const params = new HttpParams().set('pagina', pagina).set('tamano', 20);
    return this.http.get<{ total: number; pagina: number; tamano: number; items: OrdenResumenDto[] }>(
      `${this.base}/proveedores/${id}/ordenes`,
      { params }
    );
  }

  ordenCompra(id: number) {
    return this.http.get<{ orden: OrdenCompraDto; nombreProveedor: string }>(`${this.base}/ordenes-compra/${id}`);
  }

  recibirOrden(id: number, lineas: { idLinea: number; cantidad: number }[]) {
    return this.http.post<{ orden: OrdenCompraDto; productosActualizados: number; lineasSinProducto: number }>(
      `${this.base}/ordenes-compra/${id}/recibir`,
      { lineas }
    );
  }
}

export interface OrdenResumenDto {
  id: number;
  fecha: string;
  nombreUsuario: string;
  cantidadLineas: number;
  recibida: boolean;
  fechaRecepcion: string | null;
  nombreUsuarioRecepcion: string;
}

export interface ProveedorFormDto {
  nombre: string;
  tipoProductos: string;
  telefono: string;
}

export interface ProveedorListaDto extends ProveedorFormDto {
  id: number;
  activo: boolean;
  cantidadProductos?: number;
  ultimaOrden?: string | null;
}

export interface LineaOrdenCompraDto {
  id?: number;
  idProducto: number;
  codigo: string;
  nombre: string;
  cantidad: number;
  cantidadRecibida?: number | null;
}

export interface OrdenCompraDto {
  id: number;
  idProveedor: number;
  fecha: string;
  nombreUsuario: string;
  recibida: boolean;
  fechaRecepcion: string | null;
  nombreUsuarioRecepcion: string;
  lineas: LineaOrdenCompraDto[];
}

export interface ProveedorFichaDto extends ProveedorFormDto {
  id: number;
  activo: boolean;
  urlContacto: string | null;
  cantidadProductos: number;
  ultimaOrden: OrdenCompraDto | null;
}

export interface OrdenGeneradaDto {
  orden: OrdenCompraDto;
  nombreLocal: string;
  textoWhatsApp: string;
  urlWhatsApp: string | null;
}

export interface FilaCatalogoDto {
  fila: number;
  codigo: string;
  nombre: string;
  costo: number | null;
  cantidad: number | null;
}

export interface FilaRevisadaDto {
  fila: number;
  codigo: string;
  nombre: string;
  costo: number;
  cantidad: number;
  accion: 'crear' | 'actualizar';
  precio: number;
  aviso: string;
}

export interface FilaInvalidaDto {
  fila: number;
  codigo: string;
  motivo: string;
}

export interface RevisionCatalogoDto {
  filas: FilaCatalogoDto[];
  validas: FilaRevisadaDto[];
  invalidas: FilaInvalidaDto[];
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
  idCierre?: number | null;
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
  idCierre?: number | null;
  pagos: { idMetodoPago: number; descripcionMetodoPago: string; importe: number }[];
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
  idCierre: number;
  nroCaja: number;
  fecha: string;
  nombreCierre: string;
  total: number;
  cantidadVentas: number;
}

export interface GrupoUnificableDto {
  nroCaja: number;
  nombreCierre: string;
  dia: string;
  idsCierre: number[];
}

export interface CajaDetalleDto {
  idCierre?: number;
  nroCaja: number;
  fecha: string;
  nombreCierre: string;
  tipoDesglose?: string;
  filas: { metodoPago: string; cantidadVentas: number; total: number }[];
}

export type TamanoCartel = 'grande' | 'chico';

export interface CartelDto {
  codigo: string;
  nombre: string;
  precio: number;
}

export interface CartelesDto {
  tamano: TamanoCartel;
  nombreLocal: string;
  plantilla: string | null;
  hojas: CartelDto[][];
}

export interface PlantillasCartelDto {
  grande: string | null;
  chico: string | null;
}

export interface ItemTopProductoDto {
  idProducto?: number | null;
  codigo: string;
  nombre: string;
  cantidad: number;
  total: number;
  esSector: boolean;
}

export interface ItemVentaDelDiaDto {
  codigo: string;
  nombre: string;
  cantidadVentas: number;
  stockRestante?: number | null;
  esSector: boolean;
  esGenerico: boolean;
}

export interface ItemMixPagoDto {
  descripcion: string;
  total: number;
  porcentaje: number;
  cantidadOperaciones: number;
}

export interface ItemVentaGrupoDto {
  nombreGrupo: string;
  cantidad: number;
  total: number;
  porcentaje: number;
}

export interface ItemComparacionLocalDto {
  idLocal: number;
  nombreLocal: string;
  totalVendido: number;
  cantidadTickets: number;
  ticketPromedio: number;
}

export interface ItemStockBajoDto {
  idProducto: number;
  idLocal: number;
  nombreLocal: string;
  codigo: string;
  nombre: string;
  cantidad: number;
  umbral: number;
}

export interface ResumenEstadisticasDto {
  totalVendido: number;
  cantidadTickets: number;
  ticketPromedio: number;
  margenEstimado: number;
  porcentajeMargen: number;
  masVendidoUnidades?: ItemTopProductoDto | null;
  masVendidoDinero?: ItemTopProductoDto | null;
  topPorUnidades: ItemTopProductoDto[];
  topPorDinero: ItemTopProductoDto[];
  mixMediosPago: ItemMixPagoDto[];
  ventasPorGrupo: ItemVentaGrupoDto[];
  comparativaSucursales: ItemComparacionLocalDto[];
  stockBajo: ItemStockBajoDto[];
  ventasDelDia: ItemVentaDelDiaDto[];
}

export interface EstadisticasRespuestaDto {
  resumen: ResumenEstadisticasDto;
  periodo: {
    desde?: string;
    hasta?: string;
    hoy?: string;
  };
  locales: { idLocal: number; nombre: string }[];
}

