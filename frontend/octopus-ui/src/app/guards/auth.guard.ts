import { inject } from '@angular/core';
import { CanActivateFn, Router, UrlTree } from '@angular/router';
import { AuthService, UserRole } from '../services/auth.service';

export const authGuard: CanActivateFn = (route): boolean | UrlTree => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return router.createUrlTree(['/login']);
  }

  const allowedRoles = route.data?.['roles'] as UserRole[] | undefined;
  if (allowedRoles && !allowedRoles.includes(authService.currentRole!)) {
    const fallback = authService.currentRole === 'scheduler' ? '/scheduler' : '/operator';
    return router.createUrlTree([fallback]);
  }

  return true;
};
