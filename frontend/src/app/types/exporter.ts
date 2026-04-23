import type { components, paths } from '../types/api-types';

// Error Message
export type ProblemDetails = components['schemas']['ProblemDetails'];

// Team
export type TeamDto = components['schemas']['TeamDto'];

// User
export type UserDto = components['schemas']['UserDto'];
export type UserDtoPaginatedResponse = components['schemas']['UserDtoPaginatedResponse'];
export type UpdateUserDto = components['schemas']['UpdateUserDto'];

// User Path
export type GetUserOptions = paths['/api/v1/user']['get']['parameters']['query'];

// Roles
export enum Role {
  REGULAR_USER = 0,
  TEAM_LEAD = 1,
  ADMIN = 2,
}

// Authentication
export type GoogleAuthCallbackDto = components['schemas']['GoogleAuthCallbackDto'];
export type GoogleAuthResponseDto = components['schemas']['GoogleAuthResponseDto'];

// Cost Centre
export interface BudgetDto {
  id: number;
  teamId: number;
  costCentreId: number;
  targetAmount: number;
  periodStart: string;
  periodEnd: string;
}

export interface CostCentreDto {
  id: number;
  name: string;
  description: string | null;
  displayColor: string | null;
  budgets: BudgetDto[];
}

export interface CreateBudgetEntryDto {
  teamId: number;
  targetAmount: number;
  periodStart: string;
  periodEnd: string;
}

export interface CreateCostCentreRequestDto {
  name: string;
  description?: string;
  displayColor?: string;
  budgets?: CreateBudgetEntryDto[];
}

export interface UpsertBudgetEntryDto {
  id: number | null;
  teamId: number;
  targetAmount: number;
  periodStart: string;
  periodEnd: string;
}

export interface UpdateCostCentreRequestDto {
  name?: string;
  description?: string;
  displayColor?: string;
  budgetsToUpsert?: UpsertBudgetEntryDto[];
  budgetIdsToDelete?: number[];
}

export interface DeleteCostCentrePreviewDto {
  costCentreName: string;
  budgetCount: number;
  transactionCount: number;
  affectedUserCount: number;
  affectedTeamNames: string[];
}

export interface CostCentreSaveEvent {
  costCentre: CostCentreDto;
  budgetsToUpsert: UpsertBudgetEntryDto[];
  budgetIdsToDelete: number[];
}
