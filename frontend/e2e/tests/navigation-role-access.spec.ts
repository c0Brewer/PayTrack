import { expect, Page, test } from '@playwright/test';

import { e2eUsers, getInvoiceFlowUser } from '../fixtures/users';
import { authenticatePage } from '../utils/auth';

test('redirects a regular user from admin routes to the unauthorized page', async ({
  browserName,
  page,
  request,
}) => {
  await authenticatePage(page, request, getInvoiceFlowUser(browserName));

  await expectForbiddenRoute(page, '/requests');
  await expectForbiddenRoute(page, '/requests/1');
  await expectForbiddenRoute(page, '/bank-statement-upload');
  await expectForbiddenRoute(page, '/create-payment-request');
  await expectForbiddenRoute(page, '/payment-requests-by-team');
  await expectForbiddenRoute(page, '/payment-requests-by-team/1');
  await expectForbiddenRoute(page, '/user');
  await expectForbiddenRoute(page, '/team');
  await expectForbiddenRoute(page, '/team/1');
  await expectForbiddenRoute(page, '/cost-centre');
  await expectForbiddenRoute(page, '/cost-centre/1');
  await expectForbiddenRoute(page, '/season');
});

test('hides admin navigation for a regular user', async ({ browserName, page, request }) => {
  await authenticatePage(page, request, getInvoiceFlowUser(browserName));
  await page.goto('/');
  await expect(page.getByText('Your current invoice and payment-request overview.')).toBeVisible();

  await expectAdminNavigationHidden(page);
});

test('shows admin navigation only for an admin user', async ({ page, request }) => {
  await authenticatePage(page, request, e2eUsers.admin);
  await page.goto('/');
  await expect(page.getByText('Your current invoice and payment-request overview.')).toBeVisible();

  await expectAdminNavigationVisible(page);
});

test('hides admin navigation and forbids admin routes for a team lead', async ({
  page,
  request,
}) => {
  await authenticatePage(page, request, e2eUsers.teamLead);
  await page.goto('/');
  await expect(page.getByRole('link', { name: /Home/ })).toBeVisible();

  await expectAdminNavigationHidden(page);

  await expectForbiddenRoute(page, '/requests');
  await expectForbiddenRoute(page, '/payment-requests-by-team');
});

async function expectForbiddenRoute(page: Page, path: string): Promise<void> {
  await page.goto(path);
  await expect(page).toHaveURL(/\/unauthorized$/);
  await expect(page.getByRole('heading', { name: 'Access Denied' })).toBeVisible();
  await expect(page.getByText('You do not have permission to access this page.')).toBeVisible();
}

async function expectAdminNavigationHidden(page: Page): Promise<void> {
  const navigation = page.getByRole('complementary');

  await expect(navigation.getByRole('button', { name: /Management/ })).toHaveCount(0);
  await expect(navigation.getByRole('button', { name: /Requests/ })).toHaveCount(0);
  await expect(navigation.getByRole('link', { name: /User/ })).toHaveCount(0);
  await expect(navigation.getByRole('link', { name: /Team/ })).toHaveCount(0);
  await expect(navigation.getByRole('link', { name: /Cost Centre/ })).toHaveCount(0);
  await expect(navigation.getByRole('link', { name: /Seasons/ })).toHaveCount(0);
  await expect(navigation.getByRole('link', { name: /Create Payment Request/ })).toHaveCount(0);
  await expect(navigation.getByRole('link', { name: /View Payment Requests/ })).toHaveCount(0);
  await expect(navigation.getByRole('link', { name: /View Submitted Invoices/ })).toHaveCount(0);
}

async function expectAdminNavigationVisible(page: Page): Promise<void> {
  const navigation = page.getByRole('complementary');
  const managementMenu = navigation.getByRole('button', { name: /Management/ });
  const requestsMenu = navigation.getByRole('button', { name: /Requests/ });

  await expect(managementMenu).toBeVisible();
  await expect(requestsMenu).toBeVisible();

  if ((await managementMenu.getAttribute('aria-expanded')) !== 'true') {
    await managementMenu.click();
  }
  await expect(navigation.getByRole('link', { name: /User/ })).toBeVisible();
  await expect(navigation.getByRole('link', { name: /Team/ })).toBeVisible();
  await expect(navigation.getByRole('link', { name: /Cost Centre/ })).toBeVisible();
  await expect(navigation.getByRole('link', { name: /Seasons/ })).toBeVisible();

  if ((await requestsMenu.getAttribute('aria-expanded')) !== 'true') {
    await requestsMenu.click();
  }
  await expect(navigation.getByRole('link', { name: /Create Payment Request/ })).toBeVisible();
  await expect(navigation.getByRole('link', { name: /View Payment Requests/ })).toBeVisible();
  await expect(navigation.getByRole('link', { name: /View Submitted Invoices/ })).toBeVisible();
}
