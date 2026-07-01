# Test Plan
## Purpose
This test plan defines the current testing approach for the PayTrack backend and establishes a baseline for further quality assurance activities in the ASE project.

## Test Scope
The following components are covered by automated tests:
- Backend services (business logic)
- Repository layer (data access)
- API endpoints
- Middleware components
- Mapping logic
- Frontend services
- Frontend route guards
- Frontend UI components

Out of scope:
- End-to-end browser testing
- Performance and load testing
- Security testing beyond basic validation

## Test Levels
- Unit Tests: test individual classes (services, mappers, helpers, frontend services, guards)
- Component Tests: test combined components (e.g. repositories with in-memory DB, Angular components with TestBed)
- Integration Tests: test API endpoints with real application host
- System Tests: currently performed manually for key user flows

## Test strategy
We use manual code reviews for every merge request as a quality assurance measure. Each request is reviewed before merging. During review, the team checks code quality, correctness, maintainability, possible bugs, and compliance with project conventions. This acts as a lightweight static analysis measure and helps detect issues that are not covered by automated tests alone.

The project currently uses automated tests on backend and frontend level with a strong focus on unit tests and lightweight integration-style API tests.
Tests types currently implemented:
- Unit tests for services, repositories, mappers, and middleware.
- API tests using real ASP.NET application host with mocked services and in-memory database.
- Frontend unit tests for Angular services, guards, and standalone components.
- Frontend component tests using Angular `TestBed` with mocked dependencies and Vitest spies.

Key strategy decisions already visible in the codebase:
- database-dependent tests use `EntityFrameworkCore.InMemory`
- enpoint tests run in a dedicated Test environment
- authentication is bypassed with custom test authentication handlers
- mocking is used to isolate service dependencies
- frontend tests use Angular `TestBed` for dependency injection and component setup
- frontend tests use `Vitest` globals and `vi` spies/mocks
- code coverage is collected automatically during test execution

## Test Execution
- During development: unit and component tests are written alongside features
- Before merging: all tests must pass in the merge request
- Before milestones: manual testing of critical workflows is performed

## Test Frameworks and Tools
The backend currently uses:
- `XUnit` for the test execution
- `Microsoft.NET.Test.Sdk`
- `Moq` for mocking
- `FluentAssertions` for readable assertions
- `Microsoft.AspNetCore.Mvc.Testing` for endpoint/API tests
- `Microsoft.EntityFrameworkCore.inMemory` for in-memory persistence tests
- `coverlet.collector` and `coverlet.msbuild` for code coverage
- `Microsoft.Testing.Platform` as test runner platform

The frontend currently uses:
- Angular CLI test runner via `ng test`
- `@angular/build:unit-test` as test builder
- `Vitest` globals and mocking utilities
- Angular `TestBed` for service and component tests
- `Playwright` browser installation for running frontend tests in CI
- `lcov` and HTML coverage reports for code coverage

## Coverage Baseline
Coverage is currently configured with:
- backend coverage collection enabled
- backend output format: `lcov`
- backend threshold: **90%** total line coverage
- frontend coverage collection enabled
- frontend output formats: `lcov` and `html`
- frontend threshold: **80%** for statements, branches, functions, and lines

## Responsibilities
Test lead (@12223394):
- maintains this test plan
- tracks coverage and test progress
- coordinates missing test areas with the team

Development team:
- adds and maintains automated tests for newly implemented features
- ensures all relevant tests pass before merging changes (e.g. via merge requests)
- supports defect reproduction and regression prevention
- prevents regressions by adding tests for previously identified defects
- follows agreed testing standards and conventions (e.g. naming, coverage expectations)
