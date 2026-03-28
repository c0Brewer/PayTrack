import createClient from 'openapi-fetch';

import type { paths } from './types/api-types.ts';

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
