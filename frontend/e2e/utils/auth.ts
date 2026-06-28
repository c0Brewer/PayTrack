import { APIRequestContext, Page } from '@playwright/test';

import { E2EUser } from '../fixtures/users';

const apiBaseUrl = process.env['PLAYWRIGHT_API_BASE_URL'] ?? 'http://localhost:5154';

interface E2ELoginResponse {
  jwtToken: string;
}

export interface JwtPayload {
  email: string;
  role: string;
  exp: number;
}

const emailClaim = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress';
const roleClaim = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

export async function requestE2EJwt(request: APIRequestContext, user: E2EUser): Promise<string> {
  const response = await request.post(`${apiBaseUrl}/api/v1/auth/e2e-login`, {
    data: {
      email: user.email,
      role: user.role,
    },
  });

  if (!response.ok()) {
    throw new Error(`E2E login failed with ${response.status()}: ${await response.text()}`);
  }

  const body = (await response.json()) as E2ELoginResponse;
  return body.jwtToken;
}

export function decodeJwtPayload(token: string): JwtPayload {
  const [, payload] = token.split('.');
  const decodedPayload = JSON.parse(Buffer.from(payload, 'base64url').toString('utf-8')) as Record<
    string,
    unknown
  >;

  return {
    email: String(decodedPayload[emailClaim]),
    role: String(decodedPayload[roleClaim]),
    exp: Number(decodedPayload['exp']),
  };
}

export async function authenticatePage(
  page: Page,
  request: APIRequestContext,
  user: E2EUser,
): Promise<void> {
  const token = await requestE2EJwt(request, user);

  await page.addInitScript((jwt) => {
    window.localStorage.setItem('jwt', jwt);
  }, token);
}
