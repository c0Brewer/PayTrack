import { expect, Locator, Page, test } from '@playwright/test';

import { e2eUsers, getInvoiceFlowUser } from '../fixtures/users';
import { authenticatePage } from '../utils/auth';

test('shows required-field validation on invoice submission', async ({
  browserName,
  page,
  request,
}) => {
  await authenticatePage(page, request, getInvoiceFlowUser(browserName));
  await page.goto('/submit');
  await expect(page.getByRole('heading', { name: 'Invoice Submission' })).toBeVisible();

  await page.getByRole('button', { name: 'Submit Invoice' }).click();

  await expectInvalid(page.locator('#amount'));
  await expectInvalid(page.locator('#paidAt'));
  await expectInvalid(page.locator('#invoiceNumber'));
  await expectInvalid(page.locator('#purposeOfPayment'));
  await expect(page.locator('.payout-type')).toHaveClass(/payout-type--invalid/);
  await expect(page.locator('.receipt-upload')).toHaveClass(/receipt-upload--invalid/);
  await expect(page.getByText('This field is required.')).toHaveCount(6);
});

test('shows required-field validation on payment request creation', async ({ page, request }) => {
  await authenticatePage(page, request, e2eUsers.admin);
  await page.goto('/create-payment-request');
  await expect(page.getByRole('heading', { name: 'Create Payment Request' })).toBeVisible();

  await page.getByRole('button', { name: 'Create Payment Request' }).click();

  await expectInvalid(page.locator('#amount'));
  await expectInvalid(page.locator('#dueDate'));
  await expectInvalid(page.locator('#purposeOfPayment'));
  await expectInvalid(page.locator('#teamId'));
  await expectInvalid(page.locator('#budgetId'));
  await expect(page.getByText('Please select a user.')).toBeVisible();
  await expect(page.getByText('This field is required.')).toHaveCount(5);
});

test('validates required cost centre budget team and season fields', async ({ page, request }) => {
  await authenticatePage(page, request, e2eUsers.admin);
  await openCreateCostCentreModal(page);

  const addBudgetForm = page.locator('.add-budget-form');
  await addBudgetForm.getByRole('button', { name: '+ Add Budget' }).click();

  await expect(page.getByText('Name is required.')).toBeVisible();
  await expect(page.getByText('Team is required.')).toBeVisible();
  await expect(page.getByText('Amount is required.')).toBeVisible();
  await expect(page.getByText('Season is required.')).toBeVisible();
  await expect(page.getByText('Period start is required.')).toBeVisible();
  await expect(page.getByText('Period end is required.')).toBeVisible();
});

test('validates invalid budget date ranges', async ({ page, request }) => {
  await authenticatePage(page, request, e2eUsers.admin);
  await openCreateCostCentreModal(page);

  const addBudgetForm = page.locator('.add-budget-form');
  await addBudgetForm.getByPlaceholder('Budget name').fill('E2E invalid date budget');
  await addBudgetForm.locator('select').nth(1).selectOption({ index: 1 });
  await addBudgetForm.locator('select').nth(2).selectOption({ index: 1 });
  await addBudgetForm.getByPlaceholder('0.00').fill('100');
  await addBudgetForm.locator('input[type="date"]').nth(0).fill('2026-07-15');
  await addBudgetForm.locator('input[type="date"]').nth(1).fill('2026-07-01');
  await addBudgetForm.getByRole('button', { name: '+ Add Budget' }).click();

  await expect(page.getByText('Period end must not be before period start.')).toBeVisible();
});

async function openCreateCostCentreModal(page: Page): Promise<void> {
  await page.goto('/cost-centre');
  await expect(page.getByRole('heading', { name: 'Cost Centre Management' })).toBeVisible();
  await page.getByRole('button', { name: /Create/ }).click();
  await expect(page.getByRole('dialog').filter({ hasText: 'Create Cost Centre' })).toBeVisible();
}

async function expectInvalid(locator: Locator): Promise<void> {
  await expect(locator).toHaveClass(/is-invalid/);
}
