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
  publicado?: boolean;
  publicable?: boolean;
}

export type EstadoTienda = 'inactiva' | 'activa' | 'pausada';

export type VistaTienda = 'catalogo' | 'pausada' | 'cerrada';

export interface ConfiguracionTiendaDto {
  alias: string;
  estado: EstadoTienda;
  direccion: string;
  horario: string;
  whatsApp: string;
  haceEnvios: boolean;
  latitud: number | null;
  longitud: number | null;
}

export interface LugarEncontradoDto {
  nombre: string;
  latitud: number;
  longitud: number;
}

export interface TiendaPublicaDto {
  alias: string;
  nombre: string;
  vista: VistaTienda;
  horario: string;
  direccion: string;
  whatsApp: string;
  haceEnvios: boolean;
  latitud: number | null;
  longitud: number | null;
  grupos: { id: number; nombre: string }[];
}

export interface ImagenTiendaDto {
  id: number;
  origen: string;
  fuente: string;
  enlace: string;
  licencia: string;
}

export interface ProductoTiendaDto {
  id: number;
  codigo: string;
  nombre: string;
  precio: number;
  idGrupo: number;
  hayStock: boolean;
  imagenes: ImagenTiendaDto[];
}

export interface ImagenProductoDto {
  orden: number;
  origen: string;
  fuente: string;
  enlace: string;
  licencia: string;
  datos: string | null;
}

export interface PropuestaImagenDto {
  disponible: boolean;
  sinRespuesta: boolean;
  url: string;
  fuente: string;
  enlace: string;
  licencia: string;
}

export type EstadoAbono = 'pendiente' | 'trial' | 'al_dia' | 'por_vencer' | 'vencido';

export type EstadoDebito = '' | 'pendiente' | 'activa' | 'pausada' | 'cancelada';

export interface LocalAdminDto {
  idLocal: number;
  nombre: string;
  estado: EstadoAbono;
  vence: string | null;
}

export interface UsuarioAdminDto {
  id: number;
  nombre: string;
  nombreUsuario: string;
  email: string;
  rol: string;
  activo: boolean;
  locales: LocalAdminDto[];
}

export interface LocalPlataformaDto {
  idLocal: number;
  nombre: string;
  activo: boolean;
  cliente: string;
  estado: string;
  vence: string | null;
  empleados: number;
}

export interface MensajeContactoDto {
  id: number;
  nombre: string;
  correo: string;
  mensaje: string;
  creado: string;
  leido: string | null;
  vence: string | null;
}

export interface ResumenLocalesAdmin {
  totalMes: number;
  precioMensual: number;
  localesPagos: number;
  telefono: string;
  email: string;
  items: LocalPlataformaDto[];
}

export interface UsuarioCreadoDto {
  id: number;
  nombre: string;
  nombreUsuario: string;
  rol: string;
  activo: boolean;
}

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
  captcha: string;
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
  factorGananciaSector: number;
  factorGananciaGenerico: number;
  iva: number;
  stockRigido: boolean;
  umbralStockBajo: number;
  empleadoPuedeModificarPrecios: boolean;
  empleadoPuedeFiar: boolean;
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
  integracion: string;
}

export interface CuentaMercadoPagoDto {
  vinculada: boolean;
  puestos: { nroCaja: number; listo: boolean; urlQr: string; idLector: string }[];
}

export type TipoOferta = 'descuento' | 'llevaPaga' | 'precioDesde' | 'precioExacto';

export interface OpcionesOfertaDto {
  anulaConDescuentoCaja: boolean;
  anulaConAlCosto: boolean;
  permiteNoAplicarEnCaja: boolean;
}

export interface OfertaVigenteDto extends OpcionesOfertaDto {
  idProducto: number;
  tipo: TipoOferta;
  porcentaje: number;
  lleva: number;
  paga: number;
  precioUnidad: number;
  descripcion: string;
}

export interface OfertaPedidoDto extends OpcionesOfertaDto {
  idProducto: number;
  tipo: TipoOferta;
  porcentaje: number;
  lleva: number;
  paga: number;
  precioUnidad: number;
  fechaDesde: string;
  fechaHasta: string;
  sinFin: boolean;
}

export interface OfertaDto extends OfertaPedidoDto {
  id: number;
  codigo: string;
  nombre: string;
  descripcion: string;
  vigente: boolean;
}

export interface ComboLineaDto {
  idProducto: number;
  cantidad: number;
}

export interface ComboVigenteDto {
  id: number;
  nombre: string;
  precioTotal: number;
  lineas: ComboLineaDto[];
}

export interface ComboPedidoDto {
  nombre: string;
  precioTotal: number;
  fechaDesde: string;
  fechaHasta: string;
  sinFin: boolean;
  lineas: ComboLineaDto[];
}

export interface ComboDto extends Omit<ComboPedidoDto, 'lineas'> {
  id: number;
  vigente: boolean;
  lineas: (ComboLineaDto & { nombre: string })[];
}

export interface PedidoEsperaDto {
  id: number;
  nota: string;
  fecha: string;
  productos: number;
}

export interface LineaPedidoCargadaDto {
  idProducto: number;
  codigo: string;
  nombre: string;
  cantidad: number;
  precioUnitario: number;
  costo: number;
  sector: boolean;
  generico: boolean;
  precioAnterior: number | null;
}

export interface PedidoCargadoDto {
  id: number;
  nota: string;
  descuentoPorcentaje: number;
  alCosto: boolean;
  lineas: LineaPedidoCargadaDto[];
  faltantes: string[];
}

export interface CobroMercadoPagoDto {
  id: number;
  estado: string;
  modo: string;
  importe: number;
  qrData: string;
  idInformeVenta: number | null;
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

  contratarLocal(nombre: string) {
    return this.http.post<{ idLocal: number; nombre: string; urlPago: string }>(`${this.base}/locales/contratar`, { nombre });
  }

  pagarContratacion(idLocal: number) {
    return this.http.post<{ idLocal: number; nombre: string; urlPago: string }>(`${this.base}/locales/${idLocal}/contratar/pago`, {});
  }

  usuariosAdmin() {
    return this.http.get<UsuarioAdminDto[]>(`${this.base}/admin/usuarios`);
  }

  usuariosDelDueno(id: number) {
    return this.http.get<UsuarioCreadoDto[]>(`${this.base}/admin/usuarios/${id}/creados`);
  }

  localesAdmin() {
    return this.http.get<ResumenLocalesAdmin>(`${this.base}/admin/locales`);
  }

  altaLocalAdmin(idUsuario: number, nombre: string) {
    return this.http.post(`${this.base}/admin/usuarios/${idUsuario}/locales`, { nombre });
  }

  altaLocalConDueno(pedido: { nombre: string; idDueno: number; nombreDueno: string; email: string; nombreUsuario: string; claveDueno: string; idLocalOrigen: number; copiarStock: boolean }) {
    return this.http.post<{ idLocal: number; nombre: string; cantidadProductos: number }>(`${this.base}/admin/locales`, pedido);
  }

  marcarPagoAdmin(idLocal: number, clave: string, canal: string) {
    return this.http.post<{ idLocal: number; estado: string; vence: string }>(`${this.base}/admin/locales/${idLocal}/pago`, { clave, canal });
  }

  cambiarVencimientoAdmin(idLocal: number, vence: string, clave: string) {
    return this.http.post(`${this.base}/admin/locales/${idLocal}/vencimiento`, { vence, clave });
  }

  desactivarLocal(idLocal: number, clave: string) {
    return this.http.post(`${this.base}/admin/locales/${idLocal}/desactivar`, { clave });
  }

  activarLocal(idLocal: number, clave: string) {
    return this.http.post(`${this.base}/admin/locales/${idLocal}/activar`, { clave });
  }

  guardarTelefonoVentas(telefono: string, email: string, clave: string) {
    return this.http.put<{ telefono: string; email: string }>(`${this.base}/admin/ventas/telefono`, { telefono, email, clave });
  }

  contactoVentas() {
    return this.http.get<{ telefono: string; url: string | null; email: string; formularioActivo: boolean; apertura: string }>(`${this.base}/ventas/contacto`);
  }

  enviarContacto(body: { nombre: string; correo: string; mensaje: string; sitio: string; apertura: string }) {
    return this.http.post(`${this.base}/ventas/contacto`, body);
  }

  mensajesContacto() {
    return this.http.get<{ activo: boolean; items: MensajeContactoDto[] }>(`${this.base}/admin/contacto`);
  }

  marcarContactoLeido(id: number) {
    return this.http.post<{ id: number; leido: string; vence: string }>(`${this.base}/admin/contacto/${id}/leido`, {});
  }

  guardarFormularioContacto(activo: boolean, clave: string) {
    return this.http.put<{ activo: boolean }>(`${this.base}/admin/contacto`, { activo, clave });
  }

  anularPagoAdmin(idLocal: number, clave: string) {
    return this.http.post<{ idLocal: number; estado: string; vence: string }>(`${this.base}/admin/locales/${idLocal}/anular-pago`, { clave });
  }

  hacerAdmin(id: number, clave: string) {
    return this.http.post(`${this.base}/admin/usuarios/${id}/admin`, { clave });
  }

  cambiarClaveAdmin(id: number, clave: string, claveSecreta: string) {
    return this.http.post(`${this.base}/admin/usuarios/${id}/clave`, { clave, claveSecreta });
  }

  bloquearUsuario(id: number, clave: string) {
    return this.http.post(`${this.base}/admin/usuarios/${id}/bloquear`, { clave });
  }

  eliminarUsuario(id: number, clave: string) {
    return this.http.delete(`${this.base}/admin/usuarios/${id}`, { body: { clave } });
  }

  captchaRegistro() {
    return this.http.get<{ siteKey: string | null }>(`${this.base}/registro/captcha`);
  }

  registrar(pedido: RegistroPedidoDto) {
    return this.http.post<{ token?: string; rol?: string; nombre?: string; pendiente?: boolean }>(`${this.base}/registro`, pedido);
  }

  confirmarCorreo(token: string) {
    return this.http.post<{ confirmado: boolean }>(`${this.base}/registro/confirmar`, { token });
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

  productos(q = '', opts?: { sector?: boolean; codigo?: string; idGrupo?: number; sinGrupo?: boolean; stockBajo?: boolean; idProveedor?: number; publicado?: boolean }) {
    let params = new HttpParams().set('tamano', 100);
    if (opts?.publicado !== undefined) params = params.set('publicado', String(opts.publicado));
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
    return this.http.get<{ usuarios: UsuarioDto[]; locales: { idLocal: number; nombre: string }[] }>(`${this.base}/usuarios`);
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

  cobrar(items: ItemVenta[], pagos: PagoVenta[], descuentoPorcentaje: number, cobrarAlCosto: boolean, idPedidoEspera?: number, sinOfertas = false) {
    return this.http.post<{ idInformeVenta: number; total: number }>(`${this.base}/ventas`, {
      items,
      pagos,
      descuentoPorcentaje,
      cobrarAlCosto,
      idPedidoEspera,
      sinOfertas
    });
  }

  permisosCuentas() {
    return this.http.get<{ puedeFiar: boolean; puedeAnular: boolean }>(`${this.base}/cuentas/permisos`);
  }

  clientes(q: string) {
    return this.http.get<ClienteDto[]>(`${this.base}/clientes`, { params: new HttpParams().set('q', q) });
  }

  fiar(cuenta: {
    items: ItemVenta[];
    descuentoPorcentaje: number;
    cobrarAlCosto: boolean;
    sinOfertas: boolean;
    idCliente?: number;
    clienteNuevo?: { nombre: string; telefono: string };
  }) {
    return this.http.post<CuentaDto>(`${this.base}/cuentas`, cuenta);
  }

  cuentas(opts: { cliente?: string; estado?: string; idCliente?: number; pagina?: number }) {
    let params = new HttpParams().set('pagina', String(opts.pagina ?? 1));
    if (opts.idCliente) params = params.set('idCliente', String(opts.idCliente));
    if (opts.cliente) params = params.set('cliente', opts.cliente);
    if (opts.estado) params = params.set('estado', opts.estado);
    return this.http.get<{ total: number; saldo: number; pagina: number; tamano: number; items: CuentaListaDto[] }>(`${this.base}/cuentas`, { params });
  }

  cuenta(id: number) {
    return this.http.get<CuentaDto>(`${this.base}/cuentas/${id}`);
  }

  pagarCuenta(id: number, importe: number, idMetodoPago: number) {
    return this.http.post<{ idInformeVenta: number; saldo: number; estado: string }>(`${this.base}/cuentas/${id}/pagos`, { importe, idMetodoPago });
  }

  cuentaIncobrable(id: number) {
    return this.http.post(`${this.base}/cuentas/${id}/incobrable`, {});
  }

  ofertas(pagina = 1) {
    return this.http.get<{ total: number; pagina: number; tamano: number; items: OfertaDto[] }>(`${this.base}/ofertas`, {
      params: new HttpParams().set('pagina', String(pagina))
    });
  }

  ofertasVigentes() {
    return this.http.get<OfertaVigenteDto[]>(`${this.base}/ofertas/vigentes`);
  }

  crearOferta(oferta: OfertaPedidoDto) {
    return this.http.post<{ id: number }>(`${this.base}/ofertas`, oferta);
  }

  editarOferta(id: number, oferta: OfertaPedidoDto) {
    return this.http.put(`${this.base}/ofertas/${id}`, oferta);
  }

  borrarOferta(id: number) {
    return this.http.delete(`${this.base}/ofertas/${id}`);
  }

  combos() {
    return this.http.get<ComboDto[]>(`${this.base}/combos`);
  }

  combosVigentes() {
    return this.http.get<ComboVigenteDto[]>(`${this.base}/combos/vigentes`);
  }

  crearCombo(combo: ComboPedidoDto) {
    return this.http.post<{ id: number }>(`${this.base}/combos`, combo);
  }

  editarCombo(id: number, combo: ComboPedidoDto) {
    return this.http.put(`${this.base}/combos/${id}`, combo);
  }

  borrarCombo(id: number) {
    return this.http.delete(`${this.base}/combos/${id}`);
  }

  pedidosEspera() {
    return this.http.get<PedidoEsperaDto[]>(`${this.base}/pedidos-espera`);
  }

  guardarPedidoEspera(items: ItemVenta[], nota: string, descuentoPorcentaje: number, alCosto: boolean) {
    return this.http.post<{ id: number }>(`${this.base}/pedidos-espera`, { items, nota, descuentoPorcentaje, alCosto });
  }

  cargarPedidoEspera(id: number) {
    return this.http.get<PedidoCargadoDto>(`${this.base}/pedidos-espera/${id}`);
  }

  borrarPedidoEspera(id: number) {
    return this.http.delete(`${this.base}/pedidos-espera/${id}`);
  }

  editarPrecioProducto(id: number, precio: number) {
    return this.http.put<ProductoDto>(`${this.base}/productos/${id}/precio`, { precio });
  }

  mediosPago() {
    return this.http.get<MetodoPagoDto[]>(`${this.base}/medios-pago`);
  }

  crearMedioPago(descripcion: string, integracion: string) {
    return this.http.post<MetodoPagoDto>(`${this.base}/medios-pago`, { descripcion, integracion });
  }

  editarMedioPago(id: number, descripcion: string, integracion: string) {
    return this.http.put<MetodoPagoDto>(`${this.base}/medios-pago/${id}`, { descripcion, integracion });
  }

  cuentaMercadoPago() {
    return this.http.get<CuentaMercadoPagoDto>(`${this.base}/mercadopago/cuenta`);
  }

  conectarMercadoPago() {
    return this.http.get<{ url: string }>(`${this.base}/mercadopago/conectar`);
  }

  prepararPuestoMercadoPago(nro: number) {
    return this.http.post<CuentaMercadoPagoDto['puestos'][number]>(`${this.base}/mercadopago/puestos/${nro}`, {});
  }

  guardarLectorMercadoPago(nro: number, idLector: string) {
    return this.http.put(`${this.base}/mercadopago/puestos/${nro}`, { idLector });
  }

  cobroMercadoPagoAbierto() {
    return this.http.get<CobroMercadoPagoDto | null>(`${this.base}/ventas/mercadopago/abierto`);
  }

  iniciarCobroMercadoPago(items: ItemVenta[], pagos: PagoVenta[], descuentoPorcentaje: number, cobrarAlCosto: boolean, idPedidoEspera?: number, sinOfertas = false) {
    return this.http.post<CobroMercadoPagoDto>(`${this.base}/ventas/mercadopago`, {
      items, pagos, descuentoPorcentaje, cobrarAlCosto, idPedidoEspera, sinOfertas
    });
  }

  consultarCobroMercadoPago(id: number) {
    return this.http.get<CobroMercadoPagoDto>(`${this.base}/ventas/mercadopago/${id}`);
  }

  cancelarCobroMercadoPago(id: number) {
    return this.http.post<CobroMercadoPagoDto>(`${this.base}/ventas/mercadopago/${id}/cancelar`, {});
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

  configuracionTienda() {
    return this.http.get<ConfiguracionTiendaDto & { vista: VistaTienda | null; publicados: number; conImagen: number }>(`${this.base}/configuracion/tienda`);
  }

  guardarConfiguracionTienda(tienda: ConfiguracionTiendaDto) {
    return this.http.put<{ alias: string; estado: EstadoTienda; vista: VistaTienda | null }>(`${this.base}/configuracion/tienda`, tienda);
  }

  buscarUbicacion(q: string) {
    return this.http.get<LugarEncontradoDto[]>(`${this.base}/configuracion/tienda/ubicacion`, { params: new HttpParams().set('q', q) });
  }

  publicarProductos(ids: number[], publicado: boolean) {
    return this.http.put<{ actualizados: number }>(`${this.base}/productos/publicados`, { ids, publicado });
  }

  imagenesProducto(id: number) {
    return this.http.get<{ publicado: boolean; maximo: number; imagenes: ImagenProductoDto[] }>(`${this.base}/productos/${id}/imagenes`);
  }

  propuestaImagen(id: number) {
    return this.http.get<PropuestaImagenDto>(`${this.base}/productos/${id}/imagenes/propuesta`);
  }

  aceptarPropuestaImagen(id: number, orden: number) {
    return this.http.post<{ orden: number }>(`${this.base}/productos/${id}/imagenes/propuesta/aceptar`, { orden });
  }

  subirImagenProducto(id: number, orden: number, imagen: string, rechazada?: string) {
    return this.http.put<{ orden: number }>(`${this.base}/productos/${id}/imagenes/${orden}`, { imagen, rechazada: rechazada ?? null });
  }

  borrarImagenProducto(id: number, orden: number) {
    return this.http.delete<void>(`${this.base}/productos/${id}/imagenes/${orden}`);
  }

  urlImagenTienda(alias: string, idImagen: number): string {
    return `${this.base}/tienda/${encodeURIComponent(alias)}/imagenes/${idImagen}`;
  }

  tienda(alias: string) {
    return this.http.get<TiendaPublicaDto>(`${this.base}/tienda/${encodeURIComponent(alias)}`);
  }

  productosTienda(alias: string, opts: { q?: string; idGrupo?: number; pagina?: number }) {
    let params = new HttpParams().set('pagina', String(opts.pagina ?? 1));
    if (opts.q) params = params.set('q', opts.q);
    if (opts.idGrupo) params = params.set('idGrupo', String(opts.idGrupo));
    return this.http.get<{ total: number; pagina: number; tamano: number; items: ProductoTiendaDto[] }>(
      `${this.base}/tienda/${encodeURIComponent(alias)}/productos`,
      { params }
    );
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

  crearOrdenCompra(id: number, lineas: { idProducto: number; cantidad: number; nombrePack: string; unidadesPack: number }[]) {
    return this.http.post<OrdenGeneradaDto>(`${this.base}/proveedores/${id}/ordenes`, { lineas });
  }

  productosProveedor(id: number, q: string, pagina = 1) {
    let params = new HttpParams().set('pagina', pagina).set('tamano', 20);
    if (q) params = params.set('q', q);
    return this.http.get<{ total: number; pagina: number; tamano: number; items: ProductoProveedorDto[] }>(
      `${this.base}/proveedores/${id}/productos`,
      { params }
    );
  }

  fijarPackProveedor(id: number, idProducto: number, nombrePack: string, unidadesPack: number) {
    return this.http.put(`${this.base}/proveedores/${id}/productos/${idProducto}`, { nombrePack, unidadesPack });
  }

  desvincularProducto(id: number, idProducto: number) {
    return this.http.delete(`${this.base}/proveedores/${id}/productos/${idProducto}`);
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
  nombre: string;
  cantidad: number;
  cantidadRecibida?: number | null;
  nombrePack: string;
  unidadesPack: number;
  descripcion?: string;
}

export interface ProductoProveedorDto {
  idProducto: number;
  nombre: string;
  stock: number;
  nombrePack: string;
  unidadesPack: number;
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
    descripcionOferta?: string;
  }[];
  ticket?: TicketDto | null;
}

export interface AhorroTicketDto {
  descripcion: string;
  importe: number;
}

export interface LineaTicketDto {
  nombre: string;
  cantidad: number;
  precio: number;
  subTotal: number;
  ahorro: AhorroTicketDto | null;
  nota: string;
}

export interface TicketDto {
  fecha: string;
  nroCaja: number | null;
  lineas: LineaTicketDto[];
  ahorros: AhorroTicketDto[];
  total: number;
  medios: { descripcion: string; importe: number }[];
  items: number;
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
  gananciaDelDia: number;
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
  deuda?: DeudaDto;
}

export interface DeudaDto {
  total: number;
  porCliente: { idCliente: number; nombre: string; saldo: number; cuentas: number }[];
}

export interface ClienteDto {
  id: number;
  nombre: string;
  telefono: string;
}

export interface CuentaListaDto {
  id: number;
  idCliente: number;
  cliente: string;
  telefono: string;
  fecha: string;
  importe: number;
  saldo: number;
  estado: 'abierta' | 'pagada' | 'incobrable';
  nombreUsuario: string;
}

export interface CuentaDto extends CuentaListaDto {
  nroCaja: number;
  fechaIncobrable: string | null;
  usuarioIncobrable: string;
  lineas: { id: number; idProducto: number | null; codigo: string; nombre: string; cantidad: number; precio: number; subTotal: number; descripcionOferta: string }[];
  pagos: { id: number; fecha: string; importe: number; idInformeVenta: number | null }[];
  ticket?: TicketDto | null;
}

