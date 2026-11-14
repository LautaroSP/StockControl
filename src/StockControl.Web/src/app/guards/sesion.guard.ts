import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../servicios/api.service';
import { SesionService } from '../servicios/sesion.service';

export const terminosGuard: CanActivateFn = async () => {
  const sesion = inject(SesionService);
  const api = inject(ApiService);
  const router = inject(Router);
  if (!sesion.hayToken()) return router.parseUrl('/login');
  if (sesion.rol() !== 'dueno' && sesion.rol() !== 'socio') return true;
  if (sesion.terminosVigentes() === true) return true;
  if (sesion.terminosVigentes() === false) return router.parseUrl('/aceptar');
  try {
    const r = await firstValueFrom(api.terminosSesion());
    sesion.terminosVigentes.set(r.vigente);
    return r.vigente ? true : router.parseUrl('/aceptar');
  } catch {
    return router.parseUrl('/login');
  }
};

export const sesionGuard: CanActivateFn = () => {
  const sesion = inject(SesionService);
  const router = inject(Router);
  return sesion.hayToken() ? true : router.parseUrl('/login');
};

export const adminGuard: CanActivateFn = () => {
  const sesion = inject(SesionService);
  const router = inject(Router);
  if (!sesion.hayToken()) return router.parseUrl('/login');
  return sesion.rol() === 'admin' ? true : router.parseUrl('/locales');
};

export const localGuard: CanActivateFn = () => {
  const sesion = inject(SesionService);
  const router = inject(Router);
  if (!sesion.hayToken()) return router.parseUrl('/login');
  if (!sesion.idLocal()) return router.parseUrl('/locales');
  return true;
};

export const cajaGuard: CanActivateFn = () => {
  const sesion = inject(SesionService);
  const router = inject(Router);
  if (!sesion.hayToken()) return router.parseUrl('/login');
  if (!sesion.idLocal()) return router.parseUrl('/locales');
  if (!sesion.hayCaja()) return router.parseUrl('/elegir-caja');
  return true;
};

export const duenoGuard: CanActivateFn = () => {
  const sesion = inject(SesionService);
  const router = inject(Router);
  if (!sesion.hayToken()) return router.parseUrl('/login');
  if (!sesion.idLocal()) return router.parseUrl('/locales');
  if (!sesion.esDueno()) return router.parseUrl('/caja');
  return true;
};

export const tiendaGuard: CanActivateFn = () => {
  const sesion = inject(SesionService);
  const router = inject(Router);
  if (!sesion.hayToken()) return router.parseUrl('/login');
  if (!sesion.idLocal()) return router.parseUrl('/locales');
  if (!sesion.puedeConfigurarLocal()) return router.parseUrl('/caja');
  return true;
};

export const proveedoresGuard: CanActivateFn = () => {
  const sesion = inject(SesionService);
  const router = inject(Router);
  if (!sesion.hayToken()) return router.parseUrl('/login');
  if (!sesion.idLocal()) return router.parseUrl('/locales');
  if (!sesion.puedeGestionarProveedores()) return router.parseUrl('/caja');
  return true;
};

