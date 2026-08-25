import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { SesionService } from '../servicios/sesion.service';

export const sesionGuard: CanActivateFn = () => {
  const sesion = inject(SesionService);
  const router = inject(Router);
  return sesion.hayToken() ? true : router.parseUrl('/login');
};

export const localGuard: CanActivateFn = () => {
  const sesion = inject(SesionService);
  const router = inject(Router);
  if (!sesion.hayToken()) return router.parseUrl('/login');
  if (!sesion.idLocal()) return router.parseUrl('/locales');
  return true;
};
