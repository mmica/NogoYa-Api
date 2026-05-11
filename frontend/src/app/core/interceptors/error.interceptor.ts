import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snack = inject(MatSnackBar);
  return next(req).pipe(
    catchError(err => {
      const message =
        err?.error?.error ?? err?.error?.title ?? err?.message ??
        'Ocurrió un error inesperado. Intente nuevamente.';
      snack.open(message, 'Cerrar', { duration: 4000, panelClass: 'bg-red-600 text-white' });
      return throwError(() => err);
    })
  );
};
