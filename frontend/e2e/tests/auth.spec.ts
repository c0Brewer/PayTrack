import { expect, test } from '@playwright/test';

import { e2eUsers, getSkipBankInformationUser } from '../fixtures/users';
import { authenticatePage, decodeJwtPayload, requestE2EJwt } from '../utils/auth';

test('offers Google OAuth as the only login option', async ({ page }) => {
  await page.goto('/login');

  await expect(page.getByRole('heading', { name: 'Welcome to PayTrack' })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Login Via Google' })).toBeVisible();
  await expect(page.locator('input[type="email"]')).toHaveCount(0);
  await expect(page.locator('input[type="password"]')).toHaveCount(0);
  await expect(page.getByRole('button', { name: /login|sign in/i })).toHaveCount(1);
});

test('redirects unauthenticated users to login before protected pages load', async ({ page }) => {
  await page.goto('/my-invoices');

  await expect(page).toHaveURL(/\/login$/);
  await expect(page.getByRole('button', { name: 'Login Via Google' })).toBeVisible();
});

test('gets an admin JWT from the E2E login endpoint', async ({ request }) => {
  const token = await requestE2EJwt(request, e2eUsers.admin);
  const payload = decodeJwtPayload(token);

  expect(token.split('.')).toHaveLength(3);
  expect(payload.email).toBe(e2eUsers.admin.email);
  expect(payload.role).toBe('Admin');
  expect(payload.exp * 1000).toBeGreaterThan(Date.now());
});

test('shows bank information onboarding with skip option for first-login users', async ({
  page,
  request,
}) => {
  await authenticatePage(page, request, e2eUsers.firstLoginUser);
  await page.goto('/initial-setup');

  await expect(page.getByText('First login')).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Bank information' })).toBeVisible();
  await expect(page.getByText('Enter your payout account now, or skip this step')).toBeVisible();
  await expect(page.getByLabel('Account holder')).toBeVisible();
  await expect(page.getByLabel('IBAN')).toBeVisible();
  await expect(page.getByLabel('BIC')).toBeVisible();
  await expect(page.getByRole('button', { name: 'Skip' })).toBeVisible();
});

test('goes home after skipping bank information and shows settings warning', async ({
  browserName,
  page,
  request,
}) => {
  await authenticatePage(page, request, getSkipBankInformationUser(browserName));
  await page.goto('/initial-setup');

  await expect(page.getByRole('heading', { name: 'Bank information' })).toBeVisible();
  await page.getByRole('button', { name: 'Skip' }).click();

  await expect(page).toHaveURL(/\/$/);
  await expect(page.getByText('Your current invoice and payment-request overview.')).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Action required' })).toBeVisible();
  await expect(
    page.getByText(
      'Bank account details are missing. Add them before submitting payout-to-user invoices.',
    ),
  ).toBeVisible();
  const addBankAccountLink = page.getByRole('link', { name: 'Add Bank Account' });
  await expect(addBankAccountLink).toBeVisible();
  await expect(addBankAccountLink).toHaveAttribute('href', '/settings#bank-accounts');

  const settingsLink = page.locator('a.sidebar__settings');
  await expect(settingsLink).toHaveClass(/sidebar__settings--warning/);
  await expect(settingsLink).toHaveAttribute('data-tooltip', 'You have no Bank Account set');
  await expect(settingsLink.locator('.warning-badge-wrapper')).toBeVisible();

  await addBankAccountLink.click();

  await expect(page).toHaveURL(/\/settings#bank-accounts$/);
  await expect(
    page.locator('.settings-tabs__item--warning', { hasText: 'Bank Accounts' }),
  ).toBeVisible();
  await expect(page.getByRole('heading', { name: 'No bank accounts added yet.' })).toBeVisible();
});

test('does not show settings warning when the user has a bank account', async ({
  page,
  request,
}) => {
  await authenticatePage(page, request, e2eUsers.userWithBankAccount);
  await page.goto('/');

  const settingsLink = page.locator('a.sidebar__settings');
  await expect(settingsLink).not.toHaveClass(/sidebar__settings--warning/);
  await expect(settingsLink).not.toHaveAttribute('data-tooltip', 'You have no Bank Account set');
  await expect(settingsLink.locator('.warning-badge-wrapper')).toHaveCount(0);
});
