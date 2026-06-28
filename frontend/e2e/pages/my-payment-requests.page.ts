import { expect, Locator, Page } from '@playwright/test';

export class MyPaymentRequestsPage {
  constructor(private readonly page: Page) {}

  async openFromNavbar(): Promise<void> {
    await this.page
      .getByRole('complementary')
      .getByRole('link', { name: /My Payment Requests/ })
      .click();
    await expect(this.page.getByRole('heading', { name: /My Payment Requests/i })).toBeVisible();
  }

  async filterByAmount(amount: string): Promise<void> {
    await this.page.getByPlaceholder('Min amount...').fill(amount);
    await this.page.getByPlaceholder('Max amount...').fill(amount);
  }

  async expectPaymentRequestVisible(purposeOfPayment: string, amount: RegExp): Promise<Locator> {
    const row = this.page.locator('tbody tr').filter({ hasText: purposeOfPayment });
    await expect(row).toBeVisible();
    await expect(row).toContainText(amount);
    await expect(row).toContainText('Submitted');
    return row;
  }

  async openPaymentRequestDetail(purposeOfPayment: string): Promise<void> {
    const row = this.page.locator('tbody tr').filter({ hasText: purposeOfPayment });
    await row.getByRole('button', { name: 'View' }).click();
  }
}
