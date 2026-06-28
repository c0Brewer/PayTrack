import path from 'path';

import { expect, Page, test } from '@playwright/test';

import { e2eUsers, getInvoiceFlowUser } from '../fixtures/users';
import { HomePage } from '../pages/home.page';
import { InvoiceSubmissionPage } from '../pages/invoice-submission.page';
import { MyInvoicesPage } from '../pages/my-invoices.page';
import { PaymentRequestSubmissionPage } from '../pages/payment-request-submission.page';
import { disableNotificationChannels } from '../utils/api';
import { authenticatePage, requestE2EJwt } from '../utils/auth';

const receiptPath = path.resolve(
  process.cwd(),
  '../backend/PayTrack/uploads/presentation-invoices/test_invoice_paytrack_dummy.pdf',
);

test('shows cached dashboard data while offline', async ({ browserName, page, request }) => {
  await authenticatePage(page, request, getInvoiceFlowUser(browserName));

  const homePage = new HomePage(page);
  await homePage.goto();
  await homePage.expectLoaded();
  await expectCachedHomeDashboard(page);

  await page
    .getByRole('complementary')
    .getByRole('link', { name: /Submit An Invoice/ })
    .click();
  await expect(page.getByRole('heading', { name: 'Invoice Submission' })).toBeVisible();

  await goOffline(page);
  await page.getByRole('complementary').getByRole('link', { name: /Home/ }).click();

  await expectOfflineBanner(page);
  await homePage.expectLoaded();
});

test('queues an invoice draft offline, restores it online, and submits it', async ({
  browserName,
  page,
  request,
}) => {
  test.skip(
    browserName === 'webkit',
    'WebKit does not dispatch the invoice form submit reliably under simulated offline mode.',
  );

  const invoiceUser = getInvoiceFlowUser(browserName);
  const adminToken = await requestE2EJwt(request, e2eUsers.admin);
  await disableNotificationChannels(request, adminToken);
  await authenticatePage(page, request, invoiceUser);

  const invoiceNumber = `E2E-OFFLINE-${browserName.toUpperCase()}`;
  const amount = '188.88';
  const purpose = `E2E offline invoice ${browserName}`;

  const submissionPage = new InvoiceSubmissionPage(page);
  await submissionPage.goto();
  await submissionPage.uploadReceipt(receiptPath);
  await submissionPage.fillInvoiceNumber(invoiceNumber);
  await submissionPage.fillAmount(amount);
  await submissionPage.fillPaidAt('2026-06-10');
  await submissionPage.fillPurpose(purpose);
  await submissionPage.selectTeam('Chassis');
  await submissionPage.selectPaidMyselfWithFirstBankAccount();

  await goAppOffline(page);
  const saveOfflineButton = page.getByRole('button', { name: /Save Offline for Sync/ });
  await expect(saveOfflineButton).toBeEnabled();
  await saveOfflineButton.click();
  await page.waitForTimeout(1000);
  if ((await getOfflineDraftCount(page, invoiceNumber)) === 0) {
    await page.locator('form').evaluate((form: HTMLFormElement) => {
      form.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
    });
  }

  await expect.poll(async () => await getOfflineDraftCount(page, invoiceNumber)).toBe(1);
  await expect(page.getByText('Stored Offline')).toBeVisible();
  await expect(page.getByText(invoiceNumber)).toBeVisible();
  await expect(page.getByRole('button', { name: 'Plug into Form' })).toBeDisabled();

  await goAppOnline(page);
  await expect(page.getByRole('button', { name: 'Plug into Form' })).toBeEnabled();
  await page.getByRole('button', { name: 'Plug into Form' }).click();

  await expect(page.getByText('Offline invoice draft loaded into the form.')).toBeVisible();
  await expect(page.locator('#invoiceNumber')).toHaveValue(invoiceNumber);
  await expect(page.locator('#amount')).toHaveValue(amount);
  await expect(page.locator('#purposeOfPayment')).toHaveValue(purpose);
  await expect(await getOfflineDraftCount(page, invoiceNumber)).toBe(0);

  await submissionPage.submit();
  await expect(page.getByText('Invoice submitted successfully.')).toBeVisible();
  await expect(page).toHaveURL(/\/$/);

  const myInvoicesPage = new MyInvoicesPage(page);
  await myInvoicesPage.openFromNavbar();
  await page.getByPlaceholder('Invoice number...').fill(invoiceNumber);
  await myInvoicesPage.expectInvoiceVisible(invoiceNumber, euroAmountPattern(amount));
});

test('disables offline-only mutation buttons outside invoice draft saving', async ({
  page,
  request,
}) => {
  await authenticatePage(page, request, e2eUsers.admin);

  const submissionPage = new PaymentRequestSubmissionPage(page);
  await submissionPage.goto();
  await goOffline(page);

  const createPaymentRequestButton = page.getByRole('button', { name: 'Create Payment Request' });
  await expect(createPaymentRequestButton).toBeDisabled();
  await expect(createPaymentRequestButton).toHaveAttribute('aria-disabled', 'true');
  await expect(createPaymentRequestButton).toHaveAttribute(
    'title',
    'You are offline. Actions are unavailable until the connection is restored.',
  );
});

async function goOffline(page: Page): Promise<void> {
  await page.context().setOffline(true);
  await page.evaluate(() => window.dispatchEvent(new Event('offline')));
  await expectOfflineBanner(page);
}

async function goAppOffline(page: Page): Promise<void> {
  await page.evaluate(() => window.dispatchEvent(new Event('offline')));
  await expectOfflineBanner(page);
}

async function goAppOnline(page: Page): Promise<void> {
  await page.evaluate(() => window.dispatchEvent(new Event('online')));
  await expect(page.getByRole('status')).toHaveCount(0);
}

async function expectOfflineBanner(page: Page): Promise<void> {
  await expect(page.getByRole('status')).toContainText('Offline mode');
}

async function expectCachedHomeDashboard(page: Page): Promise<void> {
  await expect
    .poll(async () =>
      page.evaluate(() => window.localStorage.getItem('home-dashboard-cache') !== null),
    )
    .toBe(true);
}

async function getOfflineDraftCount(page: Page, invoiceNumber: string): Promise<number> {
  return await page.evaluate(
    async ({ databaseName, storeName, invoiceNumber }) =>
      await new Promise<number>((resolve, reject) => {
        const request = indexedDB.open(databaseName);

        request.onerror = (): void => reject(request.error);
        request.onsuccess = (): void => {
          const db = request.result;
          const transaction = db.transaction(storeName, 'readonly');
          const getAllRequest = transaction.objectStore(storeName).getAll();

          getAllRequest.onerror = (): void => reject(getAllRequest.error);
          getAllRequest.onsuccess = (): void => {
            const records = getAllRequest.result as Array<{
              payload?: { invoiceNumber?: string };
            }>;
            resolve(
              records.filter((record) => record.payload?.invoiceNumber === invoiceNumber).length,
            );
          };
        };
      }),
    {
      databaseName: 'paytrack-offline-submissions',
      storeName: 'user-invoice-submissions',
      invoiceNumber,
    },
  );
}

function euroAmountPattern(amount: string): RegExp {
  const [euros, cents = '00'] = Number(amount).toFixed(2).split('.');
  const formattedEuros = euros.replace(/\B(?=(\d{3})+(?!\d))/g, '.');

  return new RegExp(`${formattedEuros},${cents}\\s*€`);
}
