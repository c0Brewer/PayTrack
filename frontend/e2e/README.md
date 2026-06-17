# End-to-End Tests

This folder contains Playwright tests that run against the Angular application in a browser.

## Structure

- `smoke.spec.ts`: first stable checks that verify the app starts and anonymous users reach the login page.
- `helpers/`: shared test helpers, for example login helpers or API setup once the backend test setup is stable.
- `fixtures/`: reusable test data or Playwright fixtures.

## Commands

- `npm run e2e`: runs the tests headlessly.
- `npm run e2e:ui`: opens the Playwright UI runner for local debugging.
- `npm run e2e:debug`: runs tests in debug mode.
- `npm run e2e:install`: installs Playwright browser binaries if they are missing.

## Conventions

Prefer stable selectors such as `data-testid` for important controls. Avoid selectors that depend on CSS classes, DOM nesting, or unfinished layout details.

Keep early tests focused on stable application behavior. As the frontend stabilizes, add full workflow tests for business-critical flows.
