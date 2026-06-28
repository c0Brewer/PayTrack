import { expect, Page, test } from '@playwright/test';

import { e2eUsers, getPaymentRequestFlowUser } from '../fixtures/users';
import { PaymentRequestSubmissionPage } from '../pages/payment-request-submission.page';
import { disableNotificationChannels } from '../utils/api';
import { authenticatePage, requestE2EJwt } from '../utils/auth';

test('moves a payment request from submitted to paid for the assigned user', async ({
  browser,
  browserName,
  page,
  request,
}) => {
  const paymentRequestUser = getPaymentRequestFlowUser(browserName);
  const adminToken = await requestE2EJwt(request, e2eUsers.admin);
  await disableNotificationChannels(request, adminToken);

  const amount = '155.75';
  const purposeOfPayment = `E2E payment request status ${browserName}`;

  await authenticatePage(page, request, e2eUsers.admin);
  const submissionPage = new PaymentRequestSubmissionPage(page);
  await submissionPage.goto();
  await submissionPage.fillPaymentDetails({
    amount,
    dueDate: '2026-07-15',
    purposeOfPayment,
  });
  await submissionPage.selectAssignedUser(paymentRequestUser.email);
  await submissionPage.selectTeam('Chassis');
  await submissionPage.selectFirstAvailableBudget();
  await submissionPage.submit();

  await openAdminPaymentRequestFromOverview(page, purposeOfPayment);
  await expectPaymentRequestStatus(page, 'Submitted');

  const userContext = await browser.newContext();
  const userPage = await userContext.newPage();
  await authenticatePage(userPage, request, paymentRequestUser);
  await openUserPaymentRequestFromOverview(userPage, purposeOfPayment);
  await expectPaymentRequestStatus(userPage, 'Submitted');

  await markPaymentRequestAsPaid(page);
  await expectPaymentRequestStatus(page, 'Paid');

  await userPage.reload();
  await expectPaymentRequestStatus(userPage, 'Paid');

  await userContext.close();
});

async function openAdminPaymentRequestFromOverview(
  page: Page,
  purposeOfPayment: string,
): Promise<void> {
  await page.goto('/payment-requests-by-team');
  await expect(page.getByRole('heading', { name: 'Payment Requests' })).toBeVisible();
  await page.getByPlaceholder('Purpose of payment...').fill(purposeOfPayment);

  const row = page.locator('tbody tr').filter({ hasText: purposeOfPayment });
  await expect(row).toBeVisible();
  await expect(row).toContainText('Submitted');
  await row.getByRole('button', { name: 'View' }).click();
  await expect(page.getByRole('heading', { name: /Payment Request #/ })).toBeVisible();
  await expect(page.locator('.detail-shell__subtitle')).toContainText(purposeOfPayment);
}

async function openUserPaymentRequestFromOverview(
  page: Page,
  purposeOfPayment: string,
): Promise<void> {
  await page.goto('/');
  await expect(page.getByText('Your current invoice and payment-request overview.')).toBeVisible();
  await page
    .getByRole('complementary')
    .getByRole('link', { name: /My Payment Requests/ })
    .click();
  await expect(page.getByRole('heading', { name: 'My Payment Requests' })).toBeVisible();
  await page.getByPlaceholder('Purpose of payment...').fill(purposeOfPayment);

  const row = page.locator('tbody tr').filter({ hasText: purposeOfPayment });
  await expect(row).toBeVisible();
  await expect(row).toContainText('Submitted');
  await row.getByRole('button', { name: 'View' }).click();
  await expect(page.getByRole('heading', { name: /Payment Request #/ })).toBeVisible();
  await expect(page.locator('.detail-shell__subtitle')).toContainText(purposeOfPayment);
}

async function markPaymentRequestAsPaid(page: Page): Promise<void> {
  await page.getByRole('button', { name: 'Mark as Paid' }).click();

  const dialog = page.getByRole('dialog').filter({ hasText: 'Mark as Paid' });
  await expect(dialog).toBeVisible();
  await dialog.locator('textarea').fill('E2E payment request paid.');
  await dialog.getByRole('button', { name: 'Confirm' }).click();

  await expect(page.getByText('Payment marked as paid.')).toBeVisible();
}

async function expectPaymentRequestStatus(page: Page, status: string): Promise<void> {
  const statusRow = page.locator('.info-table tr').filter({
    has: page.locator('th').filter({ hasText: /^Status$/ }),
  });
  await expect(statusRow).toContainText(status);
}
