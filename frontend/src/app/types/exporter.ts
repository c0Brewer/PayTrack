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
export type BudgetDto = components['schemas']['BudgetDto'];
export type CostCentreDto = components['schemas']['CostCentreDto'];
export type CostCentreDtoPaginatedResponse =
  components['schemas']['CostCentreDtoPaginatedResponse'];
export type CreateBudgetEntryDto = components['schemas']['CreateBudgetEntryDto'];
export type CreateCostCentreRequestDto = components['schemas']['CreateCostCentreRequestDto'];
export type UpdateCostCentreRequestDto = components['schemas']['UpdateCostCentreRequestDto'];
export type UpsertBudgetEntryDto = components['schemas']['UpsertBudgetEntryDto'];
export type DeleteCostCentrePreviewDto = components['schemas']['DeleteCostCentrePreviewDto'];

// Cost Centre Paths
export type GetCostCentreOptions = paths['/api/v1/cost-centre']['get']['parameters']['query'];

export interface CostCentreSaveEvent {
  costCentre: CostCentreDto;
  budgetsToUpsert: UpsertBudgetEntryDto[];
  budgetIdsToDelete: number[];
}
