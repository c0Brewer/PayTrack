import path from 'path';

import { expect, test } from '@playwright/test';

import { e2eUsers } from '../fixtures/users';
import { disableNotificationChannels } from '../utils/api';
import { authenticatePage, requestE2EJwt } from '../utils/auth';

const csvFixturePath = path.resolve(
  process.cwd(),
  'e2e/fixtures/files/payment-request-bulk-import.csv',
);

test('imports payment requests from a CSV file and creates one request per extracted row', async ({
  browserName,
  page,
  request,
}) => {
  const adminToken = await requestE2EJwt(request, e2eUsers.admin);
  await disableNotificationChannels(request, adminToken);
  await authenticatePage(page, request, e2eUsers.admin);

  const purposeOfPayment = `E2E CSV payment requests ${browserName}`;

  await page.goto('/create-payment-request');
  await expect(page.getByRole('heading', { name: 'Create Payment Request' })).toBeVisible();
  await expect(page.getByText('Bulk CSV Import')).toBeVisible();

  await page.locator('.csv-file-input-hidden').setInputFiles(csvFixturePath);

  const dialog = page.getByRole('dialog').filter({ hasText: 'Bulk CSV Import' });
  await expect(dialog).toBeVisible();
  await expect(dialog.getByText('payment-request-bulk-import.csv')).toBeVisible();
  await expect(dialog.getByText('2 valid row(s) found')).toBeVisible();

  await dialog.locator('#csv-teamId').selectOption({ label: 'Chassis' });
  await expect(dialog.locator('#csv-budgetId option').nth(1)).toBeAttached();
  await dialog.locator('#csv-budgetId').selectOption({ index: 1 });
  await dialog.locator('#csv-purposeOfPayment').fill(purposeOfPayment);
  await dialog.locator('#csv-dueDate').fill('2026-07-15');
  await dialog.getByRole('button', { name: 'Next: Preview (2)' }).click();

  await expect(dialog.getByRole('columnheader', { name: 'CSV Name' })).toBeVisible();
  await expect(dialog.locator('tbody tr').filter({ hasText: 'Chassis Member' })).toContainText(
    /42,50\s*€/,
  );
  await expect(dialog.locator('tbody tr').filter({ hasText: 'Electronics Member' })).toContainText(
    /117,25\s*€/,
  );
  await expect(
    dialog.locator('tbody tr').filter({ hasText: 'Chassis Member' }).getByRole('textbox'),
  ).toHaveValue('Chassis Member (chassis.member@paytrack.local)');
  await expect(
    dialog.locator('tbody tr').filter({ hasText: 'Electronics Member' }).getByRole('textbox'),
  ).toHaveValue('Electronics Member (electronics.member@paytrack.local)');

  await dialog.getByRole('button', { name: 'Confirm & Submit All (2)' }).click();
  await expect(dialog.getByText('2 succeeded')).toBeVisible();
  await expect(dialog.getByText('0 failed')).toBeVisible();

  await dialog.getByRole('button', { name: 'Close', exact: true }).click();
  await expect(dialog).toBeHidden();

  await page.goto('/payment-requests-by-team');
  await expect(page.getByRole('heading', { name: 'Payment Requests' })).toBeVisible();
  await page.getByPlaceholder('Purpose of payment...').fill(purposeOfPayment);

  const createdRows = page.locator('tbody tr').filter({ hasText: purposeOfPayment });
  await expect(createdRows).toHaveCount(2);
  await expect(createdRows.filter({ hasText: /42,50\s*€/ })).toBeVisible();
  await expect(createdRows.filter({ hasText: /117,25\s*€/ })).toBeVisible();
});
