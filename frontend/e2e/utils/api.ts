import { APIRequestContext } from '@playwright/test';

const apiBaseUrl = process.env['PLAYWRIGHT_API_BASE_URL'] ?? 'http://localhost:5154';

interface PaginatedResponse<T> {
  items: T[];
}

export interface E2EApiUser {
  id: number;
  name: string;
  email: string;
}

export interface E2ETeam {
  id: number;
  name: string;
}

export interface E2EBankAccount {
  id: number;
  iban: string;
  bic: string;
  accountHolder: string;
}

export interface CreatedInvoice {
  id: number;
  invoiceNumber: string;
  purposeOfPayment: string;
}

export interface CreatedTeamPaymentRequest {
  id: number;
  purposeOfPayment: string;
}

interface CreateInvoiceOptions {
  token: string;
  teamId: number;
  invoiceNumber: string;
  amount: number;
  purposeOfPayment: string;
  paidAt: string;
  payoutType?: 'user' | 'supplier';
  bankAccountId?: number;
}

interface ResubmitInvoiceOptions extends CreateInvoiceOptions {
  invoiceId: number;
  comment: string;
}

interface CreateTeamPaymentRequestOptions {
  token: string;
  teamId: number;
  userToAssignToId: number;
  amount: number;
  purposeOfPayment: string;
  dueDate: string;
}

function authorizationHeaders(token: string): Record<string, string> {
  return {
    Authorization: `Bearer ${token}`,
  };
}

async function expectOk(response: Awaited<ReturnType<APIRequestContext['get']>>): Promise<void> {
  if (!response.ok()) {
    throw new Error(`API request failed with ${response.status()}: ${await response.text()}`);
  }
}

export async function getUserByEmail(
  request: APIRequestContext,
  token: string,
  email: string,
): Promise<E2EApiUser> {
  const response = await request.get(`${apiBaseUrl}/api/v1/user`, {
    headers: authorizationHeaders(token),
  });
  await expectOk(response);

  const body = (await response.json()) as PaginatedResponse<E2EApiUser>;
  const user = body.items.find((item) => item.email === email);
  if (!user) {
    throw new Error(`E2E user not found: ${email}`);
  }

  return user;
}

export async function getTeamByName(
  request: APIRequestContext,
  token: string,
  name: string,
): Promise<E2ETeam> {
  const response = await request.get(`${apiBaseUrl}/api/v1/team`, {
    headers: authorizationHeaders(token),
  });
  await expectOk(response);

  const body = (await response.json()) as PaginatedResponse<E2ETeam>;
  const team = body.items.find((item) => item.name === name);
  if (!team) {
    throw new Error(`E2E team not found: ${name}`);
  }

  return team;
}

export async function getFirstBankAccount(
  request: APIRequestContext,
  token: string,
): Promise<E2EBankAccount> {
  const response = await request.get(`${apiBaseUrl}/api/v1/bankaccount`, {
    headers: authorizationHeaders(token),
  });
  await expectOk(response);

  const body = (await response.json()) as E2EBankAccount[];
  const bankAccount = body[0];
  if (!bankAccount) {
    throw new Error('E2E bank account not found for current user');
  }

  return bankAccount;
}

export async function disableNotificationChannels(
  request: APIRequestContext,
  token: string,
): Promise<void> {
  const disabledChannel = {
    sendEmail: false,
    sendSlack: false,
    sendPush: false,
  };

  const response = await request.put(`${apiBaseUrl}/api/v1/admin/settings/notification-channels`, {
    headers: authorizationHeaders(token),
    data: {
      creation: disabledChannel,
      confirmation: disabledChannel,
      reminders: disabledChannel,
      deletion: disabledChannel,
      invoiceApproval: disabledChannel,
      invoiceRejection: disabledChannel,
      invoiceChangesRequested: disabledChannel,
      invoicePaymentCompleted: disabledChannel,
    },
  });
  await expectOk(response);
}

export async function createInvoice(
  request: APIRequestContext,
  options: CreateInvoiceOptions,
): Promise<CreatedInvoice> {
  const payoutType = options.payoutType ?? 'supplier';
  const isUserPayout = payoutType === 'user';
  const response = await request.post(`${apiBaseUrl}/api/v1/transaction/user`, {
    headers: authorizationHeaders(options.token),
    multipart: {
      receipt: {
        name: `${options.invoiceNumber}.txt`,
        mimeType: 'text/plain',
        buffer: Buffer.from(`Receipt for ${options.invoiceNumber}`),
      },
      invoiceNumber: options.invoiceNumber,
      comment: 'Created by the home dashboard E2E test.',
      payoutType: isUserPayout ? '0' : '1',
      ...(isUserPayout
        ? { bankAccountId: String(options.bankAccountId) }
        : {
            creditorName: 'E2E Supplier',
            dueDate: options.paidAt,
          }),
      'transaction.teamId': String(options.teamId),
      'transaction.amount': String(options.amount),
      'transaction.purposeOfPayment': options.purposeOfPayment,
      'transaction.paidAt': options.paidAt,
    },
  });
  await expectOk(response);

  return (await response.json()) as CreatedInvoice;
}

export async function resubmitInvoice(
  request: APIRequestContext,
  options: ResubmitInvoiceOptions,
): Promise<CreatedInvoice> {
  const response = await request.post(
    `${apiBaseUrl}/api/v1/transaction/user/${options.invoiceId}/resubmit`,
    {
      headers: authorizationHeaders(options.token),
      multipart: {
        'Transaction.TeamId': String(options.teamId),
        'Transaction.Amount': String(options.amount),
        'Transaction.PurposeOfPayment': options.purposeOfPayment,
        'Transaction.PaidAt': options.paidAt,
        InvoiceNumber: options.invoiceNumber,
        Comment: options.comment,
        PayoutType: '1',
        BankAccountId: '0',
      },
    },
  );
  await expectOk(response);

  return (await response.json()) as CreatedInvoice;
}

export async function createTeamPaymentRequest(
  request: APIRequestContext,
  options: CreateTeamPaymentRequestOptions,
): Promise<CreatedTeamPaymentRequest> {
  const response = await request.post(`${apiBaseUrl}/api/v1/transaction/team`, {
    headers: authorizationHeaders(options.token),
    data: {
      userToAssignToId: options.userToAssignToId,
      dueDate: options.dueDate,
      transaction: {
        teamId: options.teamId,
        amount: options.amount,
        purposeOfPayment: options.purposeOfPayment,
        paidAt: options.dueDate,
      },
    },
  });
  await expectOk(response);

  return (await response.json()) as CreatedTeamPaymentRequest;
}
