import { expect, Locator, Page } from '@playwright/test';

export interface ExtractedInvoiceValues {
  amount: string;
  invoiceNumber: string;
  paidAt: string;
}

export class InvoiceSubmissionPage {
  private readonly amountInput: Locator;
  private readonly paidAtInput: Locator;
  private readonly invoiceNumberInput: Locator;
  private readonly purposeInput: Locator;
  private readonly teamSelect: Locator;
  private readonly commentInput: Locator;
  private readonly receiptInput: Locator;
  private readonly bankAccountSelect: Locator;

  constructor(private readonly page: Page) {
    this.amountInput = this.page.locator('#amount');
    this.paidAtInput = this.page.locator('#paidAt');
    this.invoiceNumberInput = this.page.locator('#invoiceNumber');
    this.purposeInput = this.page.locator('#purposeOfPayment');
    this.teamSelect = this.page.locator('#teamId');
    this.commentInput = this.page.locator('#comment');
    this.receiptInput = this.page.locator('#receiptFile');
    this.bankAccountSelect = this.page.locator('#bankAccountId');
  }

  async goto(): Promise<void> {
    await this.page.goto('/submit');
    await expect(this.page.getByRole('heading', { name: 'Invoice Submission' })).toBeVisible();
  }

  async uploadReceipt(receiptPath: string): Promise<void> {
    await this.receiptInput.setInputFiles(receiptPath);
    await expect(this.page.getByText(/Pre-filled \d+ fields from the receipt/)).toBeVisible();
  }

  async expectReceiptExtractionFilledInvoiceFields(): Promise<ExtractedInvoiceValues> {
    await expect(this.amountInput).not.toHaveValue('');
    await expect(this.paidAtInput).not.toHaveValue('');
    await expect(this.invoiceNumberInput).not.toHaveValue('');

    return {
      amount: await this.amountInput.inputValue(),
      invoiceNumber: await this.invoiceNumberInput.inputValue(),
      paidAt: await this.paidAtInput.inputValue(),
    };
  }

  async fillPurpose(purpose: string): Promise<void> {
    await this.purposeInput.fill(purpose);
  }

  async fillInvoiceNumber(invoiceNumber: string): Promise<void> {
    await this.invoiceNumberInput.fill(invoiceNumber);
  }

  async selectTeam(teamName: string): Promise<void> {
    await this.teamSelect.selectOption({ label: teamName });
  }

  async selectPaidMyselfWithFirstBankAccount(): Promise<void> {
    await this.page.getByText('Paid Myself').click();
    await expect(this.bankAccountSelect).toBeVisible();
    await this.bankAccountSelect.selectOption({ index: 1 });
  }

  async expectNoCommentProvided(): Promise<void> {
    await expect(this.commentInput).toHaveValue('');
  }

  async submit(): Promise<void> {
    await this.page.getByRole('button', { name: 'Submit Invoice' }).click();
  }
}
