import { Routes } from '@angular/router';
import { cajaGuard, duenoGuard, localGuard, proveedoresGuard, sesionGuard } from './guards/sesion.guard';
import { LoginComponent } from './paginas/login.component';
import { LocalesComponent } from './paginas/locales.component';
import { ElegirCajaComponent } from './paginas/elegir-caja.component';
import { CajaComponent } from './paginas/caja.component';
import { ProductosComponent } from './paginas/productos.component';
import { EstadisticasComponent } from './paginas/estadisticas.component';
import { CartelesComponent } from './paginas/carteles.component';
import { InformesComponent } from './paginas/informes.component';
import { CajasComponent } from './paginas/cajas.component';
import { GruposComponent } from './paginas/grupos.component';
import { ShellComponent } from './shell/shell.component';
import { MediosPagoComponent } from './paginas/medios-pago.component';
import { UsuariosComponent } from './paginas/usuarios.component';
import { ConfiguracionComponent } from './paginas/configuracion.component';
import { ProveedoresComponent } from './paginas/proveedores.component';
import { ProveedorComponent } from './paginas/proveedor.component';
import { CatalogoProveedorComponent } from './paginas/catalogo-proveedor.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'locales', component: LocalesComponent, canActivate: [sesionGuard] },
  { path: 'elegir-caja', component: ElegirCajaComponent, canActivate: [localGuard] },
  {
    path: '',
    component: ShellComponent,
    canActivate: [cajaGuard],
    children: [
      { path: 'caja', component: CajaComponent },
      { path: 'productos', component: ProductosComponent },
      { path: 'estadisticas', component: EstadisticasComponent, canActivate: [duenoGuard] },
      { path: 'carteles', component: CartelesComponent, canActivate: [duenoGuard] },
      { path: 'proveedores', component: ProveedoresComponent, canActivate: [proveedoresGuard] },
      { path: 'proveedores/:id', component: ProveedorComponent, canActivate: [proveedoresGuard] },
      { path: 'proveedores/:id/catalogo', component: CatalogoProveedorComponent, canActivate: [proveedoresGuard] },
      { path: 'grupos', component: GruposComponent },
      { path: 'informes', component: InformesComponent },
      { path: 'cajas', component: CajasComponent },
      { path: 'medios-pago', component: MediosPagoComponent },
      { path: 'usuarios', component: UsuariosComponent },
      { path: 'configuracion', component: ConfiguracionComponent },
      { path: '', redirectTo: 'caja', pathMatch: 'full' }
    ]
  },
  { path: '**', redirectTo: 'login' }
];
