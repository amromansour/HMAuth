import { CanActivateFn, Router } from '@angular/router';
import { Auth } from '../Services/Auth/auth'
import { inject } from '@angular/core';



export const authGuard: CanActivateFn = (route, state) => {

  const authService = inject(Auth);
  const router = inject(Router);
  const token = authService.getToken();

  // Back to login if no token found
  if (!token) {
    router.navigate(['/login']);
    return false;
  }
  try {
    // decode the JWT to check its expiration

    const payload = JSON.parse(atob(token.split('.')[1]));
    const exp = payload.exp * 1000;

    if (Date.now() > exp) {
      authService.logout();
      router.navigate(['/login']);
      return false;
    }

    return true;

  } catch (error) {
    // لو التوكن مش JWT صالح
    authService.logout();
    router.navigate(['/login']);
    return false;
  }
  return true;
};
