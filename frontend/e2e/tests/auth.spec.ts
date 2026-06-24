import { expect, test } from '@playwright/test';

import { e2eUsers } from '../fixtures/users';
import { decodeJwtPayload, requestE2EJwt } from '../utils/auth';

test('gets an admin JWT from the E2E login endpoint', async ({ request }) => {
  const token = await requestE2EJwt(request, e2eUsers.admin);
  const payload = decodeJwtPayload(token);

  expect(token.split('.')).toHaveLength(3);
  expect(payload.email).toBe(e2eUsers.admin.email);
  expect(payload.role).toBe('Admin');
  expect(payload.exp * 1000).toBeGreaterThan(Date.now());
});
