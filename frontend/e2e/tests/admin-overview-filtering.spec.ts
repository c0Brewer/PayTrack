import { expect, Locator, Page, test } from '@playwright/test';

import { e2eUsers, getInvoiceFlowUser, getPaymentRequestFlowUser } from '../fixtures/users';
import {
  createInvoice,
  createTeamPaymentRequest,
  disableNotificationChannels,
  getTeamByName,
  getUserByEmail,
} from '../utils/api';
import { authenticatePage, requestE2EJwt } from '../utils/auth';

test('filters admin invoice and payment request overviews and keeps rows actionable', async ({
  browserName,
  page,
  request,
}) => {
  const suffix = browserName.toUpperCase();
  const invoiceUser = getInvoiceFlowUser(browserName);
  const paymentRequestUser = getPaymentRequestFlowUser(browserName);

  const adminToken = await requestE2EJwt(request, e2eUsers.admin);
  const invoiceUserToken = await requestE2EJwt(request, invoiceUser);
  await disableNotificationChannels(request, adminToken);

  const chassisTeam = await getTeamByName(request, adminToken, 'Chassis');
  const assignedUser = await getUserByEmail(request, adminToken, paymentRequestUser.email);

  const primaryInvoice = await createInvoice(request, {
    token: invoiceUserToken,
    teamId: chassisTeam.id,
    invoiceNumber: `E2E-FILTER-${suffix}-A`,
    amount: 301.23,
    purposeOfPayment: `E2E invoice filter primary ${browserName}`,
    paidAt: '2026-06-11T00:00:00Z',
  });
  await createInvoice(request, {
    token: invoiceUserToken,
    teamId: chassisTeam.id,
    invoiceNumber: `E2E-FILTER-${suffix}-B`,
    amount: 999.99,
    purposeOfPayment: `E2E invoice filter secondary ${browserName}`,
    paidAt: '2026-06-12T00:00:00Z',
  });

  const primaryPaymentPurpose = `E2E payment filter primary ${browserName}`;
  const secondaryPaymentPurpose = `E2E payment filter secondary ${browserName}`;
  const primaryPaymentRequest = await createTeamPaymentRequest(request, {
    token: adminToken,
    teamId: chassisTeam.id,
    userToAssignToId: assignedUser.id,
    amount: 52.4,
    purposeOfPayment: primaryPaymentPurpose,
    dueDate: '2026-07-15T00:00:00Z',
  });
  await createTeamPaymentRequest(request, {
    token: adminToken,
    teamId: chassisTeam.id,
    userToAssignToId: assignedUser.id,
    amount: 88.8,
    purposeOfPayment: secondaryPaymentPurpose,
    dueDate: '2026-07-16T00:00:00Z',
  });

  await authenticatePage(page, request, e2eUsers.admin);

  await page.goto('/requests');
  await expect(page.getByRole('heading', { name: 'Invoices' })).toBeVisible();
  await expect(invoiceRow(page, primaryInvoice.invoiceNumber)).toBeVisible();
  await expect(invoiceRow(page, `E2E-FILTER-${suffix}-B`)).toBeVisible();

  await page.getByPlaceholder('Invoice number...').fill(primaryInvoice.invoiceNumber);
  await expectOnlyMatchingInvoice(page, primaryInvoice.invoiceNumber, `E2E-FILTER-${suffix}-B`);

  await page.getByPlaceholder('Min amount...').fill('300');
  await page.getByPlaceholder('Max amount...').fill('302');
  await expectOnlyMatchingInvoice(page, primaryInvoice.invoiceNumber, `E2E-FILTER-${suffix}-B`);

  await page.locator('app-admin-invoice-filter-component select').nth(0).selectOption('0');
  await expectOnlyMatchingInvoice(page, primaryInvoice.invoiceNumber, `E2E-FILTER-${suffix}-B`);
  await expect(invoiceRow(page, primaryInvoice.invoiceNumber)).toContainText('Submitted');
  await invoiceRow(page, primaryInvoice.invoiceNumber)
    .getByRole('button', { name: /View/ })
    .click();
  await expect(
    page.getByRole('heading', { name: `Invoice ${primaryInvoice.invoiceNumber}` }),
  ).toBeVisible();

  await page.goto('/payment-requests-by-team');
  await expect(page.getByRole('heading', { name: 'Payment Requests' })).toBeVisible();
  await expect(paymentRequestRow(page, primaryPaymentPurpose)).toBeVisible();
  await expect(paymentRequestRow(page, secondaryPaymentPurpose)).toBeVisible();

  await page.getByPlaceholder('Purpose of payment...').fill(primaryPaymentPurpose);
  await expectOnlyMatchingPaymentRequest(page, primaryPaymentPurpose, secondaryPaymentPurpose);

  await page
    .locator('app-team-request-admin-filter-component select.status-select')
    .selectOption('0');
  await expectOnlyMatchingPaymentRequest(page, primaryPaymentPurpose, secondaryPaymentPurpose);
  await expect(paymentRequestRow(page, primaryPaymentPurpose)).toContainText('Submitted');
  await paymentRequestRow(page, primaryPaymentPurpose)
    .getByRole('button', { name: /View/ })
    .click();
  await expect(page).toHaveURL(
    new RegExp(`/payment-requests-by-team/${primaryPaymentRequest.id}$`),
  );
  await expect(page.getByRole('heading', { name: /Payment Request #/ })).toBeVisible();
  await expect(page.locator('.detail-shell__subtitle')).toContainText(primaryPaymentPurpose);
});

async function expectOnlyMatchingInvoice(
  page: Page,
  visibleInvoiceNumber: string,
  hiddenInvoiceNumber: string,
): Promise<void> {
  await expect(invoiceRow(page, visibleInvoiceNumber)).toBeVisible();
  await expect(invoiceRow(page, hiddenInvoiceNumber)).toHaveCount(0);
}

async function expectOnlyMatchingPaymentRequest(
  page: Page,
  visiblePurpose: string,
  hiddenPurpose: string,
): Promise<void> {
  await expect(paymentRequestRow(page, visiblePurpose)).toBeVisible();
  await expect(paymentRequestRow(page, hiddenPurpose)).toHaveCount(0);
}

function invoiceRow(page: Page, invoiceNumber: string): Locator {
  return page.locator('table.invoice-table tbody tr').filter({ hasText: invoiceNumber });
}

function paymentRequestRow(page: Page, purposeOfPayment: string): Locator {
  return page.locator('table.team-request-table tbody tr').filter({ hasText: purposeOfPayment });
}
