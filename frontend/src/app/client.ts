import createClient from 'openapi-fetch';

import type { paths } from './types/api-types.ts';

export const client = createClient<paths>({
  baseUrl: 'http://localhost:5154',
  headers: {
    'Content-Type': 'application/json',
  },
});
