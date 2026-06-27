import path from 'path';

import { expect, Page, test } from '@playwright/test';

import { e2eUsers, getInvoiceFlowUser } from '../fixtures/users';
import { InvoiceSubmissionPage } from '../pages/invoice-submission.page';
import { MyInvoicesPage } from '../pages/my-invoices.page';
import { disableNotificationChannels } from '../utils/api';
import { authenticatePage, requestE2EJwt } from '../utils/auth';

const receiptPath = path.resolve(
  process.cwd(),
  '../backend/PayTrack/uploads/presentation-invoices/test_invoice_paytrack_dummy.pdf',
);

test('warns about duplicate invoices and allows submitting regardless', async ({
  browserName,
  page,
  request,
}) => {
  const invoiceUser = getInvoiceFlowUser(browserName);
  const adminToken = await requestE2EJwt(request, e2eUsers.admin);
  await disableNotificationChannels(request, adminToken);
  await authenticatePage(page, request, invoiceUser);

  const invoiceNumber = `E2E-DUPLICATE-${browserName.toUpperCase()}`;
  const amount = '222.22';
  const paidAt = '2026-06-10';
  const firstPurpose = `E2E duplicate original ${browserName}`;
  const duplicatePurpose = `E2E duplicate resubmitted ${browserName}`;

  await submitInvoiceWithDuplicateFields(page, {
    invoiceNumber,
    amount,
    paidAt,
    purposeOfPayment: firstPurpose,
  });
  await expect(page).toHaveURL(/\/$/);

  const duplicateSubmissionPage = await submitInvoiceWithDuplicateFields(page, {
    invoiceNumber,
    amount,
    paidAt,
    purposeOfPayment: duplicatePurpose,
  });
  await duplicateSubmissionPage.expectDuplicateWarning(invoiceNumber);
  await duplicateSubmissionPage.submitDuplicateRegardless();
  await expect(page).toHaveURL(/\/$/);

  const myInvoicesPage = new MyInvoicesPage(page);
  await myInvoicesPage.openFromNavbar();
  await page.getByPlaceholder('Invoice number...').fill(invoiceNumber);

  const rows = page.locator('tbody tr').filter({ hasText: invoiceNumber });
  await expect(rows).toHaveCount(2);
  await expect(rows.filter({ hasText: firstPurpose })).toBeVisible();
  await expect(rows.filter({ hasText: duplicatePurpose })).toBeVisible();
});

async function submitInvoiceWithDuplicateFields(
  page: Page,
  options: {
    invoiceNumber: string;
    amount: string;
    paidAt: string;
    purposeOfPayment: string;
  },
): Promise<InvoiceSubmissionPage> {
  const submissionPage = new InvoiceSubmissionPage(page);
  await submissionPage.goto();
  await submissionPage.uploadReceipt(receiptPath);
  await submissionPage.fillInvoiceNumber(options.invoiceNumber);
  await submissionPage.fillAmount(options.amount);
  await submissionPage.fillPaidAt(options.paidAt);
  await submissionPage.fillPurpose(options.purposeOfPayment);
  await submissionPage.selectTeam('Chassis');
  await submissionPage.selectPaidMyselfWithFirstBankAccount();
  await submissionPage.submit();

  return submissionPage;
}
