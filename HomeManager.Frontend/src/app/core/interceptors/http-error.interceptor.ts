import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const httpErrorInterceptor: HttpInterceptorFn = (request, next) =>
  next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      console.error('HTTP error', {
        url: request.url,
        method: request.method,
        status: error.status,
        message: error.message
      });

      return throwError(() => error);
    })
  );
