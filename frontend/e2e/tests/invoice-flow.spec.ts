import path from 'path';

import { expect, test } from '@playwright/test';

import { e2eUsers, getInvoiceFlowUser } from '../fixtures/users';
import { InvoiceDetailPage } from '../pages/invoice-detail.page';
import { InvoiceSubmissionPage } from '../pages/invoice-submission.page';
import { MyInvoicesPage } from '../pages/my-invoices.page';
import { disableNotificationChannels } from '../utils/api';
import { authenticatePage, requestE2EJwt } from '../utils/auth';

const receiptPath = path.resolve(
  process.cwd(),
  '../backend/PayTrack/uploads/presentation-invoices/test_invoice_paytrack_dummy.pdf',
);

test('submits an invoice with receipt extraction and verifies it in My Invoices', async ({
  browserName,
  page,
  request,
}) => {
  const invoiceFlowUser = getInvoiceFlowUser(browserName);
  const adminToken = await requestE2EJwt(request, e2eUsers.admin);
  await disableNotificationChannels(request, adminToken);
  await authenticatePage(page, request, invoiceFlowUser);

  const submissionPage = new InvoiceSubmissionPage(page);
  await submissionPage.goto();
  await submissionPage.uploadReceipt(receiptPath);
  const extractedValues = await submissionPage.expectReceiptExtractionFilledInvoiceFields();
  const submittedInvoiceNumber = `${extractedValues.invoiceNumber}-${browserName.toUpperCase()}`;

  const purpose = `E2E invoice upload ${browserName}`;
  await submissionPage.fillInvoiceNumber(submittedInvoiceNumber);
  await submissionPage.fillPurpose(purpose);
  await submissionPage.selectTeam('Chassis');
  await submissionPage.expectNoCommentProvided();
  await submissionPage.selectPaidMyselfWithFirstBankAccount();
  await submissionPage.submit();

  await expect(page).toHaveURL(/\/$/);

  const myInvoicesPage = new MyInvoicesPage(page);
  await myInvoicesPage.openFromNavbar();
  await myInvoicesPage.filterByAmount(extractedValues.amount);
  await myInvoicesPage.expectInvoiceVisible(
    submittedInvoiceNumber,
    euroAmountPattern(extractedValues.amount),
  );
  await myInvoicesPage.openInvoiceDetail(submittedInvoiceNumber);

  const detailPage = new InvoiceDetailPage(page);
  await detailPage.expectLoaded(submittedInvoiceNumber);
  await detailPage.expectPaidMyselfSelected();
  await detailPage.expectSubmittedWithoutComment();
  await detailPage.expectReceiptPreviewVisible();
});

function euroAmountPattern(amount: string): RegExp {
  const [euros, cents = '00'] = Number(amount).toFixed(2).split('.');
  const formattedEuros = euros.replace(/\B(?=(\d{3})+(?!\d))/g, '.');

  return new RegExp(`${formattedEuros},${cents}\\s*€`);
}
