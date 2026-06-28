import path from 'path';

import { expect, Page, test } from '@playwright/test';

import { e2eUsers, getInvoiceFlowUser } from '../fixtures/users';
import {
  createInvoice,
  disableNotificationChannels,
  getFirstBankAccount,
  getTeamByName,
} from '../utils/api';
import { authenticatePage, requestE2EJwt } from '../utils/auth';

test('imports a bank statement JSON file and marks a matched invoice as paid', async ({
  browserName,
  page,
  request,
}) => {
  const invoiceUser = getInvoiceFlowUser(browserName);
  const invoiceNumber = `E2E-BANK-${browserName.toUpperCase()}`;
  const purposeOfPayment = `${invoiceNumber} bank statement reconciliation`;
  const statementPath = path.resolve(
    process.cwd(),
    `e2e/fixtures/files/bank-statement-${browserName}.json`,
  );

  const adminToken = await requestE2EJwt(request, e2eUsers.admin);
  const invoiceUserToken = await requestE2EJwt(request, invoiceUser);
  await disableNotificationChannels(request, adminToken);

  const chassisTeam = await getTeamByName(request, adminToken, 'Chassis');
  const bankAccount = await getFirstBankAccount(request, invoiceUserToken);
  const invoice = await createInvoice(request, {
    token: invoiceUserToken,
    teamId: chassisTeam.id,
    invoiceNumber,
    amount: 246.8,
    purposeOfPayment,
    paidAt: '2026-06-10T00:00:00Z',
    payoutType: 'user',
    bankAccountId: bankAccount.id,
  });

  await authenticatePage(page, request, e2eUsers.admin);
  await approveInvoice(page, invoice.id, invoiceNumber);

  await page.goto('/bank-statement-upload');
  await expect(page.getByRole('heading', { name: 'Bank Statement Import' })).toBeVisible();
  await page.locator('input[type="file"][accept=".json"]').setInputFiles(statementPath);

  await expect(page.getByText(`bank-statement-${browserName}.json`)).toBeVisible();
  await expect(page.getByText('1 entry ready to analyse')).toBeVisible();

  await page.getByRole('button', { name: /Find Matches/ }).click();
  await expect(page.getByText('Matches').first()).toBeVisible();
  await expect(page.locator('.result-item').filter({ hasText: invoiceNumber })).toBeVisible();
  await expect(page.locator('.result-item').filter({ hasText: invoiceNumber })).toContainText(
    'High match',
  );
  await expect(page.locator('.result-item').filter({ hasText: invoiceNumber })).toContainText(
    '246.80 EUR',
  );

  await page.getByRole('button', { name: 'Confirm 1 Update' }).click();
  await expect(page.getByRole('dialog').filter({ hasText: 'Confirm Updates' })).toBeVisible();
  await page.getByRole('button', { name: 'Confirm & Submit' }).click();
  await expect(
    page.getByText('Bank statement import successful. Updated transactions: 1'),
  ).toBeVisible();

  await page.goto(`/requests/${invoice.id}`);
  await expectInvoiceStatus(page, 'Paid');
});

async function approveInvoice(page: Page, invoiceId: number, invoiceNumber: string): Promise<void> {
  await page.goto(`/requests/${invoiceId}`);
  await expect(page.getByRole('heading', { name: `Invoice ${invoiceNumber}` })).toBeVisible();
  await expectInvoiceStatus(page, 'Submitted');

  await page.getByRole('button', { name: 'Approve' }).click();
  const dialog = page.getByRole('dialog').filter({ hasText: 'Approve Invoice' });
  await expect(dialog).toBeVisible();
  await dialog.locator('select[name="approvalBudgetId"]').selectOption({ index: 1 });
  await dialog.getByRole('button', { name: 'Approve' }).click();
  await expect(page.getByText('Invoice approved')).toBeVisible();
  await expectInvoiceStatus(page, 'Approved');
}

async function expectInvoiceStatus(page: Page, status: string): Promise<void> {
  const statusRow = page.locator('.invoice-info-card tr').filter({
    has: page.locator('th').filter({ hasText: /^Status$/ }),
  });
  await expect(statusRow).toContainText(status);
}
