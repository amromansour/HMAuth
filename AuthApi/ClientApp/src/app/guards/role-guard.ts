import { CanActivateFn, Router } from '@angular/router';
import { Auth } from '../Services/Auth/auth'
import { inject } from '@angular/core';

export const roleGuard: CanActivateFn = (route, state) => {

  const authService = inject(Auth);
  const router = inject(Router);
  const token = authService.getToken();
  const expectedRoles = route.data['roles'] as string[];
  const userRole = authService.getRole();

  if (userRole && expectedRoles.includes(userRole)) {
    return true;
  }

  router.navigate(['/unauthorized']);
  return false;

};
