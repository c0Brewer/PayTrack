import { expect, test } from '@playwright/test';

import { e2eUsers, getPaymentRequestFlowUser } from '../fixtures/users';
import { MyPaymentRequestsPage } from '../pages/my-payment-requests.page';
import { PaymentRequestDetailPage } from '../pages/payment-request-detail.page';
import { PaymentRequestSubmissionPage } from '../pages/payment-request-submission.page';
import { disableNotificationChannels } from '../utils/api';
import { authenticatePage, requestE2EJwt } from '../utils/auth';

test('creates a payment request and verifies it in My Payment Requests', async ({
  browserName,
  page,
  request,
}) => {
  const paymentRequestUser = getPaymentRequestFlowUser(browserName);
  const adminToken = await requestE2EJwt(request, e2eUsers.admin);
  await disableNotificationChannels(request, adminToken);
  await authenticatePage(page, request, e2eUsers.admin);

  const amount = '123.45';
  const purposeOfPayment = `E2E payment request ${browserName}`;

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

  const userPage = await page.context().newPage();
  await authenticatePage(userPage, request, paymentRequestUser);
  await userPage.goto('/');
  await expect(
    userPage.getByText('Your current invoice and payment-request overview.'),
  ).toBeVisible();

  const myPaymentRequestsPage = new MyPaymentRequestsPage(userPage);
  await myPaymentRequestsPage.openFromNavbar();
  await myPaymentRequestsPage.filterByAmount(amount);
  await myPaymentRequestsPage.expectPaymentRequestVisible(
    purposeOfPayment,
    euroAmountPattern(amount),
  );
  await myPaymentRequestsPage.openPaymentRequestDetail(purposeOfPayment);

  const detailPage = new PaymentRequestDetailPage(userPage);
  await detailPage.expectLoaded(purposeOfPayment);
  await detailPage.expectDetails({
    amount: euroAmountPattern(amount),
    purposeOfPayment,
  });
});

function euroAmountPattern(amount: string): RegExp {
  const [euros, cents = '00'] = Number(amount).toFixed(2).split('.');
  const formattedEuros = euros.replace(/\B(?=(\d{3})+(?!\d))/g, '.');

  return new RegExp(`${formattedEuros},${cents}\\s*€`);
}
