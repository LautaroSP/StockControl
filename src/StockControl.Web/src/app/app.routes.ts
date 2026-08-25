import { Routes } from '@angular/router';
import { localGuard, sesionGuard } from './guards/sesion.guard';
import { LoginComponent } from './paginas/login.component';
import { LocalesComponent } from './paginas/locales.component';
import { CajaComponent } from './paginas/caja.component';
import { ProductosComponent } from './paginas/productos.component';
import { ShellComponent } from './shell/shell.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'locales', component: LocalesComponent, canActivate: [sesionGuard] },
  {
    path: '',
    component: ShellComponent,
    canActivate: [localGuard],
    children: [
      { path: 'caja', component: CajaComponent },
      { path: 'productos', component: ProductosComponent },
      { path: '', redirectTo: 'caja', pathMatch: 'full' }
    ]
  },
  { path: '**', redirectTo: 'login' }
];
