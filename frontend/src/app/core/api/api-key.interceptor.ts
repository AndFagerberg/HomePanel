import { HttpInterceptorFn } from '@angular/common/http';
import { API_KEY } from './api-key';

// Attaches the shared-secret API key to same-origin /api requests.
export const apiKeyInterceptor: HttpInterceptorFn = (req, next) => {
  if (!API_KEY || !req.url.startsWith('/api')) {
    return next(req);
  }

  return next(req.clone({ setHeaders: { 'X-Api-Key': API_KEY } }));
};
