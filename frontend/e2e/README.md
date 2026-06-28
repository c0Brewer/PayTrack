# PayTrack E2E Tests

This folder contains Playwright end-to-end tests for the Angular frontend.

## Structure

```text
e2e/
  tests/          Playwright spec files grouped by user workflow
  pages/          Page objects for repeated UI interactions
  fixtures/
    files/        Upload fixtures used by import and receipt tests
    users.ts      Browser-safe E2E users and role definitions
  utils/          Shared API and authentication helpers
  scripts/        E2E backend/database startup scripts
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

The Angular e2e target starts the frontend dev server, and Playwright starts the backend in the `E2E` environment before running tests.

The backend startup command is configured in `playwright.config.ts` and delegates to:

```bash
sh ./e2e/scripts/start-e2e-backend.sh
```

That script starts the dedicated E2E Postgres database from `backend/docker-compose-e2e.yml`, starts the backend on `http://localhost:5154`, and removes the E2E database volume again when the backend process exits. The backend also resets, migrates, and seeds the `paytrack_e2e` database during `E2E` startup.

Docker must be running before executing the E2E tests.

The suite runs in Chromium, Firefox, and WebKit. Playwright runs files in
parallel locally, so tests that write data should use browser-specific fixture
users or browser-specific names.

To run a single spec during development:

```bash
ng e2e --files e2e/tests/invoice-flow.spec.ts --reporter=line
```

To run multiple specs, pass a comma-separated file list:

```bash
ng e2e --files e2e/tests/auth.spec.ts,e2e/tests/smoke.spec.ts --reporter=line
```

By default, tests call the backend at `http://localhost:5154`. Override the API URL used by test helpers with:

```bash
PLAYWRIGHT_API_BASE_URL=http://localhost:5154 ng e2e
```

Make sure no other backend process is already listening on port `5154` before running `ng e2e`.

## Test Scope

The current suite covers the main end-to-end workflows:

- smoke rendering and login page checks
- Google-login page behavior and E2E JWT login
- unauthenticated redirects and role-based route access
- first-login bank-information onboarding and missing-bank warnings
- user management role, team, and active-state changes
- home dashboard data, banners, and navigation buttons
- invoice submission with receipt extraction
- invoice duplicate warnings
- invoice status changes through review, approval, changes requested, and paid states
- payment request creation and user/admin visibility
- payment request status changes
- admin master data creation for seasons, teams, cost centres, and budgets
- critical form validation for important submit/create forms
- offline dashboard caching and offline invoice draft upload
- bank statement JSON import and matching
- payment request CSV bulk import
- admin invoice/payment-request overview filtering

Prefer additional E2E coverage for complete user workflows, especially where
multiple pages, permissions, backend state, file uploads, or browser storage are
involved.

Keep small validation rules, formatting behavior, service branches, and isolated component behavior in unit or component tests.

## Authentication

Do not automate real Google OAuth in E2E tests. Use `authenticatePage` from
`e2e/utils/auth.ts`, which calls the backend-only E2E login endpoint and installs
the JWT into local storage before the page loads.

The E2E login endpoint is intended only for the backend `E2E` environment. It
must not be enabled in development, staging, or production deployments.

## Data and Fixtures

Use `e2e/fixtures/users.ts` for users. When a test mutates user-owned data, prefer
browser-specific helpers such as `getInvoiceFlowUser(browserName)`,
`getPaymentRequestFlowUser(browserName)`, or `getHomeDashboardUser(browserName)`
so parallel browser projects do not edit the same records.

Use `e2e/utils/api.ts` to seed prerequisite backend state directly when the
workflow under test starts after that state already exists. For example, a test
that verifies filtering can create invoices through the API, then use the UI to
test the filters.

Commit upload fixtures under `e2e/fixtures/files/`. Current examples include:

- browser-specific bank statement JSON files
- a CSV file for payment request bulk import

## Database Behavior

The E2E backend uses a dedicated `paytrack_e2e` PostgreSQL database. Startup
resets, migrates, and seeds that database before tests run. The reset is guarded
so only an E2E-named database can be deleted.

Do not point E2E tests at the normal development database.

## Naming

Use workflow-oriented spec names:

```text
smoke.spec.ts
auth.spec.ts
invoice-flow.spec.ts
payment-request-flow.spec.ts
admin-master-data-flow.spec.ts
bank-statement-import.spec.ts
payment-request-csv-import.spec.ts
```

Use accessible locators such as `getByRole`, `getByLabel`, and `getByText` where possible. Add stable `data-testid` attributes only when the UI has no reliable accessible selector.
