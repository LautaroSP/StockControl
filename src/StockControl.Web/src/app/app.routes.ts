import { Routes } from '@angular/router';
import { cajaGuard, localGuard, sesionGuard } from './guards/sesion.guard';
import { LoginComponent } from './paginas/login.component';
import { LocalesComponent } from './paginas/locales.component';
import { ElegirCajaComponent } from './paginas/elegir-caja.component';
import { CajaComponent } from './paginas/caja.component';
import { ProductosComponent } from './paginas/productos.component';
import { InformesComponent } from './paginas/informes.component';
import { CajasComponent } from './paginas/cajas.component';
import { GruposComponent } from './paginas/grupos.component';
import { ShellComponent } from './shell/shell.component';

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
      { path: 'grupos', component: GruposComponent },
      { path: 'informes', component: InformesComponent },
      { path: 'cajas', component: CajasComponent },
      { path: '', redirectTo: 'caja', pathMatch: 'full' }
    ]
  },
  { path: '**', redirectTo: 'login' }
];
