# End-to-End Tests

This folder contains Cypress tests that run against the Angular application in a browser.

## Structure

- `smoke.cy.ts`: first stable checks that verify the app starts and anonymous users reach the login page.
- `helpers/`: shared test helpers, for example login helpers or API setup once the backend test setup is stable.
- `fixtures/`: reusable Cypress fixture data.

## Commands

- `npm run e2e`: starts the Angular dev server and runs Cypress headlessly.
- `npm run e2e:run`: runs Cypress headlessly against an already running app.
- `npm run e2e:open`: opens the Cypress app for local debugging.
- `npm run e2e:debug`: alias for opening Cypress in interactive mode.

## Conventions

Prefer stable selectors such as `data-testid` for important controls. Avoid selectors that depend on CSS classes, DOM nesting, or unfinished layout details.

Keep early tests focused on stable application behavior. As the frontend stabilizes, add full workflow tests for business-critical flows.
