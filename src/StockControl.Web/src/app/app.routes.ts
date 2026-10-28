import { Routes } from '@angular/router';
import { adminGuard, cajaGuard, duenoGuard, localGuard, proveedoresGuard, sesionGuard, tiendaGuard } from './guards/sesion.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', loadComponent: () => import('./paginas/landing.component').then((m) => m.LandingComponent) },
  { path: 'login', loadComponent: () => import('./paginas/login.component').then((m) => m.LoginComponent) },
  { path: 'registro', loadComponent: () => import('./paginas/registro.component').then((m) => m.RegistroComponent) },
  { path: 'confirmar', loadComponent: () => import('./paginas/confirmar.component').then((m) => m.ConfirmarComponent) },
  { path: 'contacto', loadComponent: () => import('./paginas/contacto.component').then(m => m.ContactoComponent) },
  { path: 'admin', loadComponent: () => import('./paginas/admin.component').then(m => m.AdminComponent), canActivate: [adminGuard] },
  { path: 'vista-cliente', loadComponent: () => import('./paginas/vista-cliente.component').then((m) => m.VistaClienteComponent) },
  { path: 'tienda/:alias', loadComponent: () => import('./paginas/tienda.component').then((m) => m.TiendaComponent) },
  { path: 'locales', loadComponent: () => import('./paginas/locales.component').then((m) => m.LocalesComponent), canActivate: [sesionGuard] },
  { path: 'elegir-caja', loadComponent: () => import('./paginas/elegir-caja.component').then((m) => m.ElegirCajaComponent), canActivate: [localGuard] },
  {
    path: '',
    loadComponent: () => import('./shell/shell.component').then((m) => m.ShellComponent),
    canActivate: [cajaGuard],
    children: [
      { path: 'caja', loadComponent: () => import('./paginas/caja.component').then((m) => m.CajaComponent) },
      { path: 'productos', loadComponent: () => import('./paginas/productos.component').then((m) => m.ProductosComponent) },
      { path: 'estadisticas', loadComponent: () => import('./paginas/estadisticas.component').then((m) => m.EstadisticasComponent), canActivate: [duenoGuard] },
      { path: 'carteles', loadComponent: () => import('./paginas/carteles.component').then((m) => m.CartelesComponent), canActivate: [duenoGuard] },
      { path: 'cuentas', loadComponent: () => import('./paginas/cuentas.component').then((m) => m.CuentasComponent) },
      { path: 'ofertas', loadComponent: () => import('./paginas/ofertas.component').then((m) => m.OfertasComponent), canActivate: [duenoGuard] },
      { path: 'combos', loadComponent: () => import('./paginas/combos.component').then((m) => m.CombosComponent), canActivate: [duenoGuard] },
      { path: 'proveedores', loadComponent: () => import('./paginas/proveedores.component').then((m) => m.ProveedoresComponent), canActivate: [proveedoresGuard] },
      { path: 'proveedores/:id', loadComponent: () => import('./paginas/proveedor.component').then((m) => m.ProveedorComponent), canActivate: [proveedoresGuard] },
      { path: 'proveedores/:id/catalogo', loadComponent: () => import('./paginas/catalogo-proveedor.component').then((m) => m.CatalogoProveedorComponent), canActivate: [proveedoresGuard] },
      { path: 'grupos', loadComponent: () => import('./paginas/grupos.component').then((m) => m.GruposComponent) },
      { path: 'informes', loadComponent: () => import('./paginas/informes.component').then((m) => m.InformesComponent) },
      { path: 'cajas', loadComponent: () => import('./paginas/cajas.component').then((m) => m.CajasComponent) },
      { path: 'medios-pago', loadComponent: () => import('./paginas/medios-pago.component').then((m) => m.MediosPagoComponent) },
      { path: 'usuarios', loadComponent: () => import('./paginas/usuarios.component').then((m) => m.UsuariosComponent) },
      { path: 'configuracion', loadComponent: () => import('./paginas/configuracion.component').then((m) => m.ConfiguracionComponent) },
      {
        path: 'mi-tienda',
        canActivate: [tiendaGuard],
        children: [
          { path: '', pathMatch: 'full', loadComponent: () => import('./paginas/tienda-resumen.component').then((m) => m.TiendaResumenComponent) },
          { path: 'configuracion', loadComponent: () => import('./paginas/config-tienda.component').then((m) => m.ConfigTiendaComponent) },
          { path: 'imagenes', loadComponent: () => import('./paginas/imagenes-tienda.component').then((m) => m.ImagenesTiendaComponent) }
        ]
      },
      {
        path: 'activar',
        loadComponent: () => import('./paginas/activar.component').then((m) => m.ActivarComponent),
        canActivate: [proveedoresGuard]
      },
      { path: '', redirectTo: 'caja', pathMatch: 'full' }
    ]
  },
  { path: '**', loadComponent: () => import('./paginas/no-encontrada.component').then((m) => m.NoEncontradaComponent) }
];
