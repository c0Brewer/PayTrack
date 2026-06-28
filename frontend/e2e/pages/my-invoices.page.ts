import { expect, Locator, Page } from '@playwright/test';

export class MyInvoicesPage {
  constructor(private readonly page: Page) {}

  async goto(): Promise<void> {
    await this.page.goto('/my-invoices');
    await expect(this.page.getByRole('heading', { name: /My Invoices/i })).toBeVisible();
  }

  async openFromNavbar(): Promise<void> {
    await this.page
      .getByRole('complementary')
      .getByRole('link', { name: /My Invoices/ })
      .click();
    await expect(this.page.getByRole('heading', { name: /My Invoices/i })).toBeVisible();
  }

  async filterByAmount(amount: string): Promise<void> {
    await this.page.getByPlaceholder('Min amount...').fill(amount);
    await this.page.getByPlaceholder('Max amount...').fill(amount);
  }

  async expectInvoiceVisible(invoiceNumber: string, amount: RegExp): Promise<Locator> {
    const row = this.page.locator('tbody tr').filter({ hasText: invoiceNumber });
    await expect(row).toBeVisible();
    await expect(row).toContainText(amount);
    return row;
  }

  async openInvoiceDetail(invoiceNumber: string): Promise<void> {
    const row = this.page.locator('tbody tr').filter({ hasText: invoiceNumber });
    await row.getByRole('button', { name: 'View' }).click();
  }
}
