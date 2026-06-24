# PayTrack E2E Tests

This folder contains Playwright end-to-end tests for the Angular frontend.

## Structure

```text
e2e/
  tests/       Playwright spec files grouped by user workflow
  pages/       Page objects for repeated UI interactions
  fixtures/    Stable test data used by specs and helpers
  utils/       Shared helpers such as auth, API setup, and data reset utilities
```

## Running Tests

From the `frontend` folder:

```bash
ng e2e
```

or:

```bash
npm run e2e
```

The Angular e2e target starts the frontend dev server and then runs Playwright.

## Test Scope

Prefer E2E coverage for complete user workflows, especially:

- authentication and route access
- role-based navigation and permissions
- payment request submission and review
- admin workflows for users, teams, cost centres, and seasons
- smoke checks that prove the app can render

Keep small validation rules, formatting behavior, service branches, and isolated component behavior in unit or component tests.

## Naming

Use workflow-oriented spec names:

```text
smoke.spec.ts
auth.spec.ts
payment-request-user.spec.ts
payment-request-admin.spec.ts
admin-master-data.spec.ts
```

Use accessible locators such as `getByRole`, `getByLabel`, and `getByText` where possible. Add stable `data-testid` attributes only when the UI has no reliable accessible selector.
