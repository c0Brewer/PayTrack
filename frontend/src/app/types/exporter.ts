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
export type DuplicatePaymentRequestByUserDto =
  components['schemas']['DuplicatePaymentRequestByUserDto'];

export type GetPaymentRequestsByUserOptions =
  paths['/api/v1/transaction/user']['get']['parameters']['query'];
export type GetPaymentRequestsByUserByIdOptions =
  paths['/api/v1/transaction/user/{id}']['get']['parameters']['query'];
export type GetDuplicatePaymentRequestsByUserOptions =
  paths['/api/v1/transaction/user/duplicate']['get']['parameters']['query'];

export enum PayoutType {
  User = 0,
  External = 1,
}

export const PayoutTypeLabels: Record<PayoutType, string> = {
  [PayoutType.User]: 'Pay to User',
  [PayoutType.External]: 'Pay to Supplier',
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
}

export const TransactionStatusLabels: Record<TransactionStatus, string> = {
  [TransactionStatus.Submitted]: 'Submitted',
  [TransactionStatus.ChangesRequested]: 'Changes requested',
  [TransactionStatus.Approved]: 'Approved',
  [TransactionStatus.Paid]: 'Paid',
  [TransactionStatus.Declined]: 'Declined',
};

export const TransactionStatusCssClass: Record<TransactionStatus, string> = {
  [TransactionStatus.Submitted]: 'status-submitted',
  [TransactionStatus.ChangesRequested]: 'status-changes-requested',
  [TransactionStatus.Approved]: 'status-approved',
  [TransactionStatus.Paid]: 'status-paid',
  [TransactionStatus.Declined]: 'status-declined',
};

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
export type GetPaymentRequestsByTeamOptions =
  paths['/api/v1/transaction/team']['get']['parameters']['query'];

// Bank Account
export type BankAccountDto = components['schemas']['BankAccountDto'];
export type CreateBankAccountRequestDto = components['schemas']['CreateBankAccountRequestDto'];
export type UpdateBankAccountRequestDto = components['schemas']['UpdateBankAccountRequestDto'];
