import { expect, Locator, Page } from '@playwright/test';

interface HomeDashboardStats {
  openInvoiceAmount: RegExp | string;
  openInvoices: number;
  openRequests: number;
  needsAttention: number;
}

export class HomePage {
  constructor(private readonly page: Page) {}

  private statBox(label: string): Locator {
    return this.page.locator('section.stat-box').filter({ hasText: label });
  }

  private box(title: string): Locator {
    return this.page.locator('section.box').filter({ hasText: title });
  }

  private get invoiceSection(): Locator {
    return this.box('Invoices');
  }

  private get paymentRequestSection(): Locator {
    return this.box('Payment Requests');
  }

  private get actionRequiredSection(): Locator {
    return this.box('Action required');
  }

  async goto(): Promise<void> {
    await this.page.goto('/');
  }

  async expectLoaded(): Promise<void> {
    await expect(
      this.page.getByText('Your current invoice and payment-request overview.'),
    ).toBeVisible();
  }

  async expectStats(stats: HomeDashboardStats): Promise<void> {
    await expect(this.statBox('Open Invoice Amount')).toContainText(stats.openInvoiceAmount);
    await expect(this.statBox('Open Invoices')).toContainText(String(stats.openInvoices));
    await expect(this.statBox('Open Requests')).toContainText(String(stats.openRequests));
    await expect(this.statBox('Needs Attention')).toContainText(String(stats.needsAttention));
  }

  async expectInvoiceSummary(submittedCount: number, paidCount: number): Promise<void> {
    await expect(this.invoiceSection.getByText(`Submitted: ${submittedCount}`)).toBeVisible();
    await expect(this.invoiceSection.getByText(`Paid: ${paidCount}`)).toBeVisible();
  }

  async expectInvoiceShown(invoiceNumber: string): Promise<void> {
    await expect(this.invoiceSection.getByText(invoiceNumber)).toBeVisible();
  }

  async expectPaymentRequestSummary(submittedCount: number, paidCount: number): Promise<void> {
    await expect(
      this.paymentRequestSection.getByText(`Submitted: ${submittedCount}`),
    ).toBeVisible();
    await expect(this.paymentRequestSection.getByText(`Paid: ${paidCount}`)).toBeVisible();
  }

  async expectPaymentRequestShown(purposeOfPayment: string): Promise<void> {
    await expect(this.paymentRequestSection.getByText(purposeOfPayment)).toBeVisible();
  }

  async expectNoActionRequiredWarnings(): Promise<void> {
    await expect(this.actionRequiredSection.getByText('Nothing urgent right now.')).toBeVisible();
    await expect(
      this.actionRequiredSection.getByText('Bank account details are missing'),
    ).toHaveCount(0);
    await expect(
      this.actionRequiredSection.getByText('need attention because changes were requested'),
    ).toHaveCount(0);
  }

  async goToInvoiceDetail(invoiceNumber: string): Promise<void> {
    await this.invoiceSection.getByRole('link', { name: new RegExp(invoiceNumber) }).click();
  }

  async goToPaymentRequestDetail(purposeOfPayment: string): Promise<void> {
    await this.paymentRequestSection
      .getByRole('link', { name: new RegExp(purposeOfPayment) })
      .click();
  }

  async goToMyInvoices(): Promise<void> {
    await this.page.getByRole('link', { name: /View My Invoices/ }).click();
  }

  async goToMyPaymentRequests(): Promise<void> {
    await this.page.getByRole('link', { name: /View My Payment Requests/ }).click();
  }

  async goToSubmitInvoice(): Promise<void> {
    await this.page.getByRole('link', { name: /Submit an Invoice/ }).click();
  }

  async goToReviewCorrespondingInvoices(): Promise<void> {
    await this.page.getByRole('link', { name: /Review Corresponding Invoices/ }).click();
  }
}
