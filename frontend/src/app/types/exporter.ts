import type { components, paths } from './api-types';

// Error Message
export type ProblemDetails = components['schemas']['ProblemDetails'];

// Team
export type TeamDto = components['schemas']['TeamDto'];
export type TeamDtoPaginatedResponse = components['schemas']['TeamDtoPaginatedResponse'];

// User
export type UserDto = components['schemas']['UserDto'];
export type UserDtoPaginatedResponse = components['schemas']['UserDtoPaginatedResponse'];
export type UpdateUserDto = components['schemas']['UpdateUserDto'];

export type BankAccount = components['schemas']['BankAccountDto'];
// Team Path
export type GetTeamOptions = paths['/api/v1/team']['get']['parameters']['query'];

// User Path
export type GetUserOptions = paths['/api/v1/user']['get']['parameters']['query'];
export type GetUserByIdOptions = paths['/api/v1/user/{id}']['get']['parameters']['query'];

// Payment request by user
export type PaginatedPaymentRequestByUserDto =
  components['schemas']['PaymentRequestByUserDtoPaginatedResponse'];
export type PaymentRequestByUserDto = components['schemas']['PaymentRequestByUserDto'];
export type CreatePaymentRequestByUserDto = components['schemas']['CreatePaymentRequestByUserDto'];
export type UpdatePaymentRequestByUserDto = components['schemas']['UpdatePaymentRequestByUserDto'];

export type GetPaymentRequestsByUserOptions =
  paths['/api/v1/transaction/user']['get']['parameters']['query'];
export type GetPaymentRequestsByUserByIdOptions =
  paths['/api/v1/transaction/user/{id}']['get']['parameters']['query'];

export enum PayoutType {
  User = 0,
  External = 1,
}

// Roles
export enum Role {
  REGULAR_USER = 0,
  TEAM_LEAD = 1,
  ADMIN = 2,
}

// Authentication
export type GoogleAuthCallbackDto = components['schemas']['GoogleAuthCallbackDto'];
export type GoogleAuthResponseDto = components['schemas']['GoogleAuthResponseDto'];

// Bank Account
export type BankAccountDto = components['schemas']['BankAccountDto'];
export type CreateBankAccountRequestDto = components['schemas']['CreateBankAccountRequestDto'];
export type UpdateBankAccountRequestDto = components['schemas']['UpdateBankAccountRequestDto'];
