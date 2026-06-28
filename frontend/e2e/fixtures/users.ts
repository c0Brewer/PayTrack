export enum E2ERole {
  RegularUser = 0,
  TeamLead = 1,
  Admin = 2,
}

export interface E2EUser {
  email: string;
  role: E2ERole;
}

const skipBankInformationUsers: Record<string, E2EUser> = {
  chromium: {
    email: 'e2e.skip-bank-information-chromium@paytrack.local',
    role: E2ERole.RegularUser,
  },
  firefox: {
    email: 'e2e.skip-bank-information-firefox@paytrack.local',
    role: E2ERole.RegularUser,
  },
  webkit: {
    email: 'e2e.skip-bank-information-webkit@paytrack.local',
    role: E2ERole.RegularUser,
  },
};

const homeDashboardUsers: Record<string, E2EUser> = {
  chromium: {
    email: 'e2e.home-chromium@paytrack.local',
    role: E2ERole.RegularUser,
  },
  firefox: {
    email: 'e2e.home-firefox@paytrack.local',
    role: E2ERole.RegularUser,
  },
  webkit: {
    email: 'e2e.home-webkit@paytrack.local',
    role: E2ERole.RegularUser,
  },
};

const invoiceFlowUsers: Record<string, E2EUser> = {
  chromium: {
    email: 'e2e.invoice-flow-chromium@paytrack.local',
    role: E2ERole.RegularUser,
  },
  firefox: {
    email: 'e2e.invoice-flow-firefox@paytrack.local',
    role: E2ERole.RegularUser,
  },
  webkit: {
    email: 'e2e.invoice-flow-webkit@paytrack.local',
    role: E2ERole.RegularUser,
  },
};

const paymentRequestFlowUsers: Record<string, E2EUser> = {
  chromium: {
    email: 'e2e.payment-request-flow-chromium@paytrack.local',
    role: E2ERole.RegularUser,
  },
  firefox: {
    email: 'e2e.payment-request-flow-firefox@paytrack.local',
    role: E2ERole.RegularUser,
  },
  webkit: {
    email: 'e2e.payment-request-flow-webkit@paytrack.local',
    role: E2ERole.RegularUser,
  },
};

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
  firstLoginUser: {
    email: 'e2e.first-login@paytrack.local',
    role: E2ERole.RegularUser,
  },
  userWithBankAccount: {
    email: 'chassis.member@paytrack.local',
    role: E2ERole.RegularUser,
  },
} satisfies Record<string, E2EUser>;

export function getSkipBankInformationUser(browserName: string): E2EUser {
  return skipBankInformationUsers[browserName] ?? skipBankInformationUsers['chromium'];
}

export function getHomeDashboardUser(browserName: string): E2EUser {
  return homeDashboardUsers[browserName] ?? homeDashboardUsers['chromium'];
}

export function getInvoiceFlowUser(browserName: string): E2EUser {
  return invoiceFlowUsers[browserName] ?? invoiceFlowUsers['chromium'];
}

export function getPaymentRequestFlowUser(browserName: string): E2EUser {
  return paymentRequestFlowUsers[browserName] ?? paymentRequestFlowUsers['chromium'];
}
