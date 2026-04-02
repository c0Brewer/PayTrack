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
