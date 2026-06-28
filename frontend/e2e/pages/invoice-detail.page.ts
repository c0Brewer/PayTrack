import { expect, Page } from '@playwright/test';

export class InvoiceDetailPage {
  constructor(private readonly page: Page) {}

  async expectLoaded(invoiceNumber: string): Promise<void> {
    await expect(
      this.page.getByRole('heading', { name: `Invoice ${invoiceNumber}` }),
    ).toBeVisible();
  }

  async expectSubmittedWithoutComment(): Promise<void> {
    const commentRow = this.page.locator('tr').filter({ hasText: 'Comment' });
    await expect(commentRow).toContainText('—');
  }

  async expectPaidMyselfSelected(): Promise<void> {
    const payoutTypeRow = this.page.locator('tr').filter({ hasText: 'Payout Type' });
    await expect(payoutTypeRow).toContainText('Pay to User');
  }

  async expectReceiptPreviewVisible(): Promise<void> {
    await expect(this.page.getByRole('heading', { name: 'Receipt' })).toBeVisible();
    await expect(this.page.locator('iframe.receipt-frame[title="Receipt"]')).toBeVisible();
    await expect(this.page.getByRole('button', { name: 'Download Receipt' })).toBeVisible();
  }
}
