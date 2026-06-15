import type { components, paths } from './api-types';

// Error Message
export type ProblemDetails = components['schemas']['ProblemDetails'];

// Team
export type TeamDto = components['schemas']['TeamDto'];
export type TeamDtoPaginatedResponse = components['schemas']['TeamDtoPaginatedResponse'];
export type CreateTeamRequestDto = components['schemas']['CreateTeamRequestDto'];
export type CreateTeamBudgetEntryDto = components['schemas']['CreateTeamBudgetEntryDto'];
export type UpdateTeamDto = components['schemas']['UpdateTeamRequestDto'];
export type UpsertTeamBudgetEntryDto = components['schemas']['UpsertTeamBudgetEntryDto'];
export type DeleteTeamImpactDto = components['schemas']['DeleteTeamImpactDto'];

// User
export type UserDto = components['schemas']['UserDto'];
export type UserDtoPaginatedResponse = components['schemas']['UserDtoPaginatedResponse'];
export type UpdateUserDto = components['schemas']['UpdateUserDto'];

export type BankAccount = components['schemas']['BankAccountDto'];
// Team Path
export type GetTeamOptions = paths['/api/v1/team']['get']['parameters']['query'];
export type GetTeamByIdOptions = paths['/api/v1/team/{id}']['get']['parameters']['query'];

// User Path
export type GetUserOptions = paths['/api/v1/user']['get']['parameters']['query'];
export type GetUserByIdOptions = paths['/api/v1/user/{id}']['get']['parameters']['query'];

// Payment request by user
export type PaginatedPaymentRequestByUserDto =
  components['schemas']['PaymentRequestByUserDtoPaginatedResponse'];
export type PaymentRequestByUserDto = components['schemas']['PaymentRequestByUserDto'];
export type CreatePaymentRequestByUserDto = components['schemas']['CreatePaymentRequestByUserDto'];
export type UpdatePaymentRequestByUserDto = components['schemas']['UpdatePaymentRequestByUserDto'];
export type MarkPaymentRequestByUserAsPaidDto =
  components['schemas']['MarkPaymentRequestByUserAsPaidDto'];
export type ApprovePaymentRequestByUserDto =
  components['schemas']['ApprovePaymentRequestByUserDto'];
export type DeclinePaymentRequestByUserDto =
  components['schemas']['DeclinePaymentRequestByUserDto'];
export type RequestChangesPaymentRequestByUserDto =
  components['schemas']['RequestChangesPaymentRequestByUserDto'];
export type DuplicatePaymentRequestByUserDto =
  components['schemas']['DuplicatePaymentRequestByUserDto'];

export type GetPaymentRequestsByUserOptions =
  paths['/api/v1/transaction/user']['get']['parameters']['query'] & SortQueryOptions;
export type GetPaymentRequestsByUserByIdOptions =
  paths['/api/v1/transaction/user/{id}']['get']['parameters']['query'];
export type GetDuplicatePaymentRequestsByUserOptions =
  paths['/api/v1/transaction/user/duplicate']['get']['parameters']['query'];

export type SortDirection = 'Asc' | 'Desc';

export type SortQueryOptions = {
  SortBy?: string;
  SortDirection?: SortDirection;
};

export type PaymentRequestByTeamQueryExtras = {
  VisibleStatusesOnly?: boolean;
};

export enum PayoutType {
  User = 0,
  NotYetPaid = 1,
  AlreadyPaid = 2,
}

export enum BudgetType {
  Expense = 0,
  Income = 1,
}

export const PayoutTypeLabels: Record<PayoutType, string> = {
  [PayoutType.User]: 'Pay to User',
  [PayoutType.NotYetPaid]: 'Pay to Supplier',
  [PayoutType.AlreadyPaid]: 'Already Paid',
};

// Roles
export enum Role {
  REGULAR_USER = 0,
  TEAM_LEAD = 1,
  ADMIN = 2,
}
// TransactionStatus
export enum TransactionStatus {
  Submitted = 0,
  ChangesRequested = 1,
  Approved = 2,
  Paid = 3,
  Declined = 4,
  Review = 5,
}

export const TransactionStatusLabels: Record<TransactionStatus, string> = {
  [TransactionStatus.Submitted]: 'Submitted',
  [TransactionStatus.ChangesRequested]: 'Changes Requested',
  [TransactionStatus.Approved]: 'Approved',
  [TransactionStatus.Paid]: 'Paid',
  [TransactionStatus.Declined]: 'Declined',
  [TransactionStatus.Review]: 'Review',
};

export const TransactionStatusCssClass: Record<TransactionStatus, string> = {
  [TransactionStatus.Submitted]: 'status-submitted',
  [TransactionStatus.ChangesRequested]: 'status-changes-requested',
  [TransactionStatus.Approved]: 'status-approved',
  [TransactionStatus.Paid]: 'status-paid',
  [TransactionStatus.Declined]: 'status-declined',
  [TransactionStatus.Review]: 'status-review',
};

export const TEAM_REQUEST_ALLOWED_STATUSES: readonly TransactionStatus[] = [
  TransactionStatus.Submitted,
  TransactionStatus.Paid,
];

// Authentication
export type GoogleAuthCallbackDto = components['schemas']['GoogleAuthCallbackDto'];
export type GoogleAuthResponseDto = components['schemas']['GoogleAuthResponseDto'];

// Cost Centre
export type BudgetDto = components['schemas']['BudgetDto'];
export type BudgetDtoPaginatedResponse = components['schemas']['BudgetDtoPaginatedResponse'];
export type CreateBudgetRequestDto = components['schemas']['CreateBudgetRequestDto'];
export type UpdateBudgetRequestDto = components['schemas']['UpdateBudgetRequestDto'];
export type CostCentreDto = components['schemas']['CostCentreDto'];
export type CostCentreDtoPaginatedResponse =
  components['schemas']['CostCentreDtoPaginatedResponse'];
export type CreateBudgetEntryDto = components['schemas']['CreateCostCentreBudgetEntryDto'];
export type CreateCostCentreRequestDto = components['schemas']['CreateCostCentreRequestDto'];
export type UpdateCostCentreRequestDto = components['schemas']['UpdateCostCentreRequestDto'];
export type UpsertBudgetEntryDto = components['schemas']['UpsertCostCentreBudgetEntryDto'];
export type DeleteCostCentrePreviewDto = components['schemas']['DeleteCostCentrePreviewDto'];

// Cost Centre Paths
export type GetBudgetOptions = paths['/api/v1/budget']['get']['parameters']['query'];
export type GetCostCentreOptions = paths['/api/v1/cost-centre']['get']['parameters']['query'];

// Season
export type SeasonDto = components['schemas']['SeasonDto'];
export type CreateSeasonRequestDto = components['schemas']['CreateSeasonRequestDto'];
export type UpdateSeasonRequestDto = components['schemas']['UpdateSeasonRequestDto'];
// Payment request by team
export type CreatePaymentRequestByTeamDto = components['schemas']['CreatePaymentRequestByTeamDto'];
export type PaymentRequestByTeamDto = components['schemas']['PaymentRequestByTeamDto'];
export type MarkAsPaidPaymentRequestByTeamDto =
  components['schemas']['MarkAsPaidPaymentRequestByTeamDto'];
export type PaginatedPaymentRequestByTeamDto =
  components['schemas']['PaymentRequestByTeamDtoPaginatedResponse'];
export type GetPaymentRequestsByTeamOptions =
  paths['/api/v1/transaction/team']['get']['parameters']['query'] &
    SortQueryOptions &
    PaymentRequestByTeamQueryExtras;
export type GetPaymentRequestsByTeamByIdOptions =
  paths['/api/v1/transaction/team/{id}']['get']['parameters']['query'];

// Financial export
export enum FinancialExportFormat {
  Csv = 1,
  Pdf = 2,
}

export enum FinancialExportSource {
  All = 0,
  SubmittedInvoices = 1,
  PaymentRequests = 2,
}

export type GetFinancialExportOptions = NonNullable<
  paths['/api/v1/transaction/export']['get']['parameters']['query']
>;

export type FinancialExportQueryOptions = Omit<
  GetFinancialExportOptions,
  'Source' | 'InvoiceNumber' | 'PayoutType' | 'BankAccountId' | 'RequestById'
> &
  SortQueryOptions & {
    Source?: FinancialExportSource;
    InvoiceNumber?: string;
    PayoutType?: PayoutType;
    BankAccountId?: number;
    RequestById?: number;
    VisibleStatusesOnly?: boolean;
  };

// Bank Account
export type BankAccountDto = components['schemas']['BankAccountDto'];
export type CreateBankAccountRequestDto = components['schemas']['CreateBankAccountRequestDto'];
export type UpdateBankAccountRequestDto = components['schemas']['UpdateBankAccountRequestDto'];

// Bank Statement Matching
export type BankStatementEntryDto = components['schemas']['BankStatementEntryDto'];
export type BankStatementMatchedTransactionDto =
  components['schemas']['BankStatementMatchedTransactionDto'];
export type BankStatementMatchResponseDto = components['schemas']['BankStatementMatchResponseDto'];
export type BankStatementMatchResultDto = components['schemas']['BankStatementMatchResultDto'];
export type BankStatementUpdateRequestDto = components['schemas']['BankStatementUpdateRequestDto'];
export type TransactionDto = components['schemas']['TransactionDto'];
