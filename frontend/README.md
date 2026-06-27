# PayTrack

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 21.2.3.

## Development server

To start a local development server, run:

```bash
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), you can run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Vitest](https://vitest.dev/) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

End-to-end tests use [Playwright](https://playwright.dev/) and are wired into the Angular CLI.
From the `frontend` folder, run:

```bash
ng e2e
```

The command starts the Angular development server, starts the backend in the `E2E`
environment, and runs the Playwright specs against a dedicated Docker-backed
PostgreSQL database. Docker must be running before executing the command.

The E2E suite currently runs against Chromium, Firefox, and WebKit. It covers
authentication, route guards, user management, invoice and payment-request
workflows, offline behavior, import flows, admin master data, and overview
filtering.

To run one spec while developing:

```bash
ng e2e --files e2e/tests/invoice-flow.spec.ts --reporter=line
```

See `e2e/README.md` for the E2E folder structure, backend startup flow, fixture
files, and test authoring conventions.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
