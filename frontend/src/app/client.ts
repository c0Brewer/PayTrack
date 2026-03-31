import createClient from 'openapi-fetch';

import type { paths } from './types/api-types.ts';

const PUBLIC_ROUTES: (keyof paths)[] = ['/api/v1/auth/google'];

function isPublicRoute(url: string): boolean {
  return PUBLIC_ROUTES.some((route) => url.includes(route));
}

export const client = createClient<paths>({
  baseUrl: 'http://localhost:5154',
  headers: {
    'Content-Type': 'application/json',
  },
});

client.use({
  onRequest({ request }) {
    const token = localStorage.getItem('jwt');
    if (token) {
      request.headers.set('Authorization', `Bearer ${token}`);
    }

    return request;
  },
});

// Called once from app.config.ts — wires logout into the middleware
export function initClientInterceptors(logout: () => void): void {
  client.use({
    onRequest({ request }) {
      if (isPublicRoute(request.url)) return request; // skip check entirely

      const token = localStorage.getItem('jwt');
      if (!token) {
        logout();
        throw new Error('No token');
      }
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        if (payload.exp * 1000 < Date.now()) {
          logout();
          throw new Error('Token expired');
        }
      } catch {
        logout();
        throw new Error('Invalid token');
      }
      return request;
    },
    onResponse({ response }) {
      if (response.status === 401 && !isPublicRoute(response.url)) {
        logout();
      }
      return response;
    },
  });
}
