import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '@env/environment';

export const apiInterceptor: HttpInterceptorFn = (req, next) => {
  const isAbsolute = /^https?:\/\//i.test(req.url);
  if (isAbsolute) return next(req);
  const url = `${environment.apiBaseUrl.replace(/\/$/, '')}/${req.url.replace(/^\//, '')}`;
  return next(req.clone({ url }));
};
