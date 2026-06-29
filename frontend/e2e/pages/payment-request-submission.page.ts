import { expect, Page } from '@playwright/test';

export class PaymentRequestSubmissionPage {
  constructor(private readonly page: Page) {}

  async goto(): Promise<void> {
    const usersLoaded = this.page.waitForResponse(
      (response) =>
        response.request().method() === 'GET' &&
        response.url().includes('/api/v1/user') &&
        response.ok(),
    );

    await this.page.goto('/create-payment-request');
    await expect(this.page.getByRole('heading', { name: 'Create Payment Request' })).toBeVisible();
    await usersLoaded;
  }

  async fillPaymentDetails(options: {
    amount: string;
    dueDate: string;
    purposeOfPayment: string;
  }): Promise<void> {
    await this.page.locator('#amount').fill(options.amount);
    await this.page.locator('#dueDate').fill(options.dueDate);
    await this.page.locator('#purposeOfPayment').fill(options.purposeOfPayment);
  }

  async selectAssignedUser(email: string): Promise<void> {
    await this.page.getByRole('textbox', { name: 'Search by name…' }).fill(email);
    const option = this.page.locator('.typeahead__option').filter({ hasText: email });
    await expect(option).toBeVisible();
    await option.click();
  }

  async selectTeam(teamName: string): Promise<void> {
    await this.page.locator('#teamId').selectOption({ label: teamName });
  }

  async selectFirstAvailableBudget(): Promise<void> {
    await expect(this.page.locator('#budgetId option').nth(1)).toBeAttached();
    await this.page.locator('#budgetId').selectOption({ index: 1 });
  }

  async submit(): Promise<void> {
    await this.page.getByRole('button', { name: 'Create Payment Request' }).click();
    await expect(this.page.getByText('Payment request created.')).toBeVisible();
  }
}
