import { expect, Page, test } from '@playwright/test';

import { e2eUsers, getInvoiceFlowUser } from '../fixtures/users';
import {
  createInvoice,
  disableNotificationChannels,
  getTeamByName,
  resubmitInvoice,
} from '../utils/api';
import { authenticatePage, requestE2EJwt } from '../utils/auth';

test('moves an invoice through changes requested, review, approved, and paid', async ({
  browser,
  browserName,
  page,
  request,
}) => {
  const invoiceUser = getInvoiceFlowUser(browserName);
  const adminToken = await requestE2EJwt(request, e2eUsers.admin);
  const invoiceUserToken = await requestE2EJwt(request, invoiceUser);
  await disableNotificationChannels(request, adminToken);

  const chassisTeam = await getTeamByName(request, adminToken, 'Chassis');
  const invoiceNumber = `E2E-STATUS-${browserName.toUpperCase()}`;
  const updatedPurpose = `E2E status workflow updated ${browserName}`;
  const changeRequestReason = 'Please update the invoice purpose for review.';

  const invoice = await createInvoice(request, {
    token: invoiceUserToken,
    teamId: chassisTeam.id,
    invoiceNumber,
    amount: 177.42,
    purposeOfPayment: `E2E status workflow ${browserName}`,
    paidAt: '2026-06-10T00:00:00Z',
  });

  await authenticatePage(page, request, e2eUsers.admin);
  await openAdminInvoiceFromOverview(page, invoiceNumber);
  await expectInvoiceStatus(page, 'Submitted');

  await requestInvoiceChanges(page, changeRequestReason);
  await expectInvoiceStatus(page, 'Changes Requested');

  const userContext = await browser.newContext();
  const userPage = await userContext.newPage();
  await authenticatePage(userPage, request, invoiceUser);
  await userPage.goto(`/my-invoices/${invoice.id}`);
  await expect(userPage.getByRole('heading', { name: `Invoice ${invoiceNumber}` })).toBeVisible();
  await expect(userPage.getByRole('heading', { name: 'Changes requested' })).toBeVisible();
  await expect(userPage.locator('.change-request-message')).toContainText(changeRequestReason);

  await userPage.getByRole('button', { name: 'Edit Invoice' }).click();
  await expect(userPage.getByRole('heading', { name: 'Edit Invoice' })).toBeVisible();
  await userPage.locator('#purposeOfPayment').fill(updatedPurpose);
  await userPage.locator('#creditorName').fill('E2E Supplier Updated');
  await userPage.locator('#dueDate').fill('2026-06-10');
  await resubmitInvoice(request, {
    token: invoiceUserToken,
    invoiceId: invoice.id,
    teamId: chassisTeam.id,
    invoiceNumber,
    amount: 177.42,
    purposeOfPayment: updatedPurpose,
    paidAt: '2026-06-10T00:00:00Z',
    comment: 'Updated after requested changes.',
  });

  await page.goto(`/requests/${invoice.id}`);
  await expect(page.getByRole('heading', { name: `Invoice ${invoiceNumber}` })).toBeVisible();
  await expectInvoiceStatus(page, 'Review');

  await approveInvoice(page);
  await expectInvoiceStatus(page, 'Approved');

  await markInvoiceAsPaid(page);
  await expectInvoiceStatus(page, 'Paid');

  await userContext.close();
});

async function openAdminInvoiceFromOverview(page: Page, invoiceNumber: string): Promise<void> {
  await page.goto('/requests');
  await expect(page.getByRole('heading', { name: 'Invoices' })).toBeVisible();
  await page.getByPlaceholder('Invoice number...').fill(invoiceNumber);

  const row = page.locator('tbody tr').filter({ hasText: invoiceNumber });
  await expect(row).toBeVisible();
  await expect(row).toContainText('Submitted');
  await row.getByRole('button', { name: 'View' }).click();
  await expect(page.getByRole('heading', { name: `Invoice ${invoiceNumber}` })).toBeVisible();
}

async function requestInvoiceChanges(page: Page, reason: string): Promise<void> {
  await page.getByRole('button', { name: 'Request Changes' }).click();
  const dialog = page.getByRole('dialog').filter({ hasText: 'Request Changes' });
  await expect(dialog).toBeVisible();
  await dialog.locator('textarea[name="changeRequestReason"]').fill(reason);
  await dialog.getByRole('button', { name: 'Request Changes' }).click();
  await expectInvoiceStatus(page, 'Changes Requested');

  const notificationDialog = page
    .getByRole('dialog')
    .filter({ hasText: 'Send Email Notification' });
  if (await notificationDialog.isVisible()) {
    await notificationDialog.getByRole('button', { name: 'Cancel' }).click();
  }
}

async function approveInvoice(page: Page): Promise<void> {
  await page.getByRole('button', { name: 'Approve' }).click();
  const dialog = page.getByRole('dialog').filter({ hasText: 'Approve Invoice' });
  await expect(dialog).toBeVisible();
  await dialog.locator('select[name="approvalBudgetId"]').selectOption({ index: 1 });
  await dialog.getByRole('button', { name: 'Approve' }).click();
  await expect(page.getByText('Invoice approved')).toBeVisible();
}

async function markInvoiceAsPaid(page: Page): Promise<void> {
  await page.getByRole('button', { name: 'Mark as Paid' }).click();
  const dialog = page.getByRole('dialog').filter({ hasText: 'Mark as Paid' });
  await expect(dialog).toBeVisible();
  await dialog.locator('input[name="paymentReference"]').fill('E2E-PAID-REFERENCE');
  await dialog.locator('input[name="paymentDate"]').fill('2026-06-26');
  await dialog.locator('textarea[name="paymentPurpose"]').fill('E2E payment completed');
  await dialog.getByRole('button', { name: 'Mark as Paid' }).click();
  await expect(page.getByText('Invoice marked as paid')).toBeVisible();
}

async function expectInvoiceStatus(page: Page, status: string): Promise<void> {
  const statusRow = page.locator('.invoice-info-card tr').filter({
    has: page.locator('th').filter({ hasText: /^Status$/ }),
  });
  await expect(statusRow).toContainText(status);
}
