import { expect, Page } from '@playwright/test';

export class PaymentRequestDetailPage {
  constructor(private readonly page: Page) {}

  async expectLoaded(purposeOfPayment: string): Promise<void> {
    await expect(this.page.getByRole('heading', { name: /Payment Request #/ })).toBeVisible();
    await expect(this.page.locator('.detail-shell__subtitle')).toContainText(purposeOfPayment);
  }

  async expectDetails(options: { amount: RegExp; purposeOfPayment: string }): Promise<void> {
    const statusRow = this.page.locator('tr').filter({ hasText: 'Status' });
    const amountRow = this.page.locator('tr').filter({ hasText: 'Amount' });
    const purposeRow = this.page.locator('tr').filter({ hasText: 'Purpose' });

    await expect(statusRow).toContainText('Submitted');
    await expect(amountRow).toContainText(options.amount);
    await expect(purposeRow).toContainText(options.purposeOfPayment);
  }
}
