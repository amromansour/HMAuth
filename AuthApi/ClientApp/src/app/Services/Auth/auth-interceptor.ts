import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Auth } from './auth';
import { catchError, switchMap, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(Auth);
  const token = authService.getToken();

  let clonedRequest = req;

  // Add the token to the request headers if available
  if (token) {
    clonedRequest = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
  }

  return next(clonedRequest).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        // Try to refresh the token
        const refreshToken = authService.refreshToken();
        if (refreshToken) {
          return refreshToken.pipe(
            switchMap((res) => {
              if (res._ResponseCode === 200 && res.data?.accessToken) {
                authService.saveToken(res.data.accessToken);
                authService.saveRefreshToken(res.data.refreshToken);
                const newRequest = req.clone({
                  setHeaders: {
                    Authorization: `Bearer ${res.data.accessToken}`,
                  },
                });
                return next(newRequest);
              } else {
                authService.logout();
                location.href = '/login';
                return throwError(() => error);
              }
            }),
            catchError(() => {
              authService.logout();
              location.href = '/login';
              return throwError(() => error);
            })
          );
        } else {
          authService.logout();
          location.href = '/login';
          return throwError(() => error);
        }
      }

      return throwError(() => error);
    })
  );
};
