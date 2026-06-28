import { expect, test } from '@playwright/test';

import { e2eUsers, getHomeDashboardUser } from '../fixtures/users';
import { HomePage } from '../pages/home.page';
import {
  createInvoice,
  createTeamPaymentRequest,
  disableNotificationChannels,
  getTeamByName,
  getUserByEmail,
} from '../utils/api';
import { authenticatePage, requestE2EJwt } from '../utils/auth';

test('shows created invoices and payment requests on the home dashboard', async ({
  browserName,
  page,
  request,
}) => {
  const dashboardUser = getHomeDashboardUser(browserName);
  const adminToken = await requestE2EJwt(request, e2eUsers.admin);
  const dashboardUserToken = await requestE2EJwt(request, dashboardUser);
  await disableNotificationChannels(request, adminToken);

  const dashboardApiUser = await getUserByEmail(request, adminToken, dashboardUser.email);
  const chassisTeam = await getTeamByName(request, adminToken, 'Chassis');
  const runPrefix = `E2E-HOME-${browserName.toUpperCase()}`;

  const invoices = await Promise.all([
    createInvoice(request, {
      token: dashboardUserToken,
      teamId: chassisTeam.id,
      invoiceNumber: `${runPrefix}-INV-1`,
      amount: 110,
      purposeOfPayment: `${runPrefix} invoice one`,
      paidAt: '2026-06-10T00:00:00Z',
    }),
    createInvoice(request, {
      token: dashboardUserToken,
      teamId: chassisTeam.id,
      invoiceNumber: `${runPrefix}-INV-2`,
      amount: 290,
      purposeOfPayment: `${runPrefix} invoice two`,
      paidAt: '2026-06-11T00:00:00Z',
    }),
  ]);

  const paymentRequests = await Promise.all([
    createTeamPaymentRequest(request, {
      token: adminToken,
      teamId: chassisTeam.id,
      userToAssignToId: dashboardApiUser.id,
      amount: 30,
      purposeOfPayment: `${runPrefix} team request one`,
      dueDate: '2026-07-12T00:00:00Z',
    }),
    createTeamPaymentRequest(request, {
      token: adminToken,
      teamId: chassisTeam.id,
      userToAssignToId: dashboardApiUser.id,
      amount: 40,
      purposeOfPayment: `${runPrefix} team request two`,
      dueDate: '2026-07-13T00:00:00Z',
    }),
    createTeamPaymentRequest(request, {
      token: adminToken,
      teamId: chassisTeam.id,
      userToAssignToId: dashboardApiUser.id,
      amount: 50,
      purposeOfPayment: `${runPrefix} team request three`,
      dueDate: '2026-07-14T00:00:00Z',
    }),
  ]);

  await authenticatePage(page, request, dashboardUser);
  const homePage = new HomePage(page);
  await homePage.goto();

  await homePage.expectLoaded();
  await homePage.expectStats({
    openInvoiceAmount: /400,00\s*€/,
    openInvoices: 2,
    openRequests: 3,
    needsAttention: 0,
  });

  await homePage.expectInvoiceSummary(2, 0);
  for (const invoice of invoices) {
    await homePage.expectInvoiceShown(invoice.invoiceNumber);
  }

  await homePage.expectPaymentRequestSummary(3, 0);
  for (const paymentRequest of paymentRequests) {
    await homePage.expectPaymentRequestShown(paymentRequest.purposeOfPayment);
  }

  await homePage.expectNoActionRequiredWarnings();

  await homePage.goToInvoiceDetail(invoices[0].invoiceNumber);
  await expect(page).toHaveURL(new RegExp(`/my-invoices/${invoices[0].id}$`));

  await homePage.goto();
  await homePage.goToPaymentRequestDetail(paymentRequests[0].purposeOfPayment);
  await expect(page).toHaveURL(new RegExp(`/my-team-requests/${paymentRequests[0].id}$`));

  await homePage.goto();
  await homePage.goToMyInvoices();
  await expect(page).toHaveURL(/\/my-invoices$/);

  await homePage.goto();
  await homePage.goToMyPaymentRequests();
  await expect(page).toHaveURL(/\/my-team-requests$/);

  await homePage.goto();
  await homePage.goToSubmitInvoice();
  await expect(page).toHaveURL(/\/submit$/);

  await homePage.goto();
  await homePage.goToReviewCorrespondingInvoices();
  await expect(page).toHaveURL(/\/my-invoices\?status=1$/);
});
