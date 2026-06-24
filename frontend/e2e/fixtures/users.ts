export enum E2ERole {
  RegularUser = 0,
  TeamLead = 1,
  Admin = 2,
}

export interface E2EUser {
  email: string;
  role: E2ERole;
}

export const e2eUsers = {
  admin: {
    email: 'admin@paytrack.local',
    role: E2ERole.Admin,
  },
  teamLead: {
    email: 'lead@paytrack.local',
    role: E2ERole.TeamLead,
  },
  regularUser: {
    email: 'chassis.member@paytrack.local',
    role: E2ERole.RegularUser,
  },
} satisfies Record<string, E2EUser>;
