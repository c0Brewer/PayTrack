import type { components } from '../types/api-types';

// Team
export type TeamDto = components['schemas']['TeamDto'];

// User
export type UserDto = components['schemas']['UserDto'];

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
