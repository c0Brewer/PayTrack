# Project: PayTrack (QSE-08)

Payment request and transaction tracking app for teams and cost centres.

## Stack

- **Frontend:** Angular 21 (standalone components, signals), TypeScript, SCSS, Bootstrap 5.3.8
- **Backend:** .NET (C#), REST API at `/api/v1/`
- **Icons:** Google Material Symbols Outlined — `<span class="material-symbols-outlined">icon_name</span>`
- **No Tailwind in new code** — migrating toward component-scoped SCSS

## Frontend (`frontend/src/app/`)

| Path | Purpose |
|---|---|
| `components/` | Feature components grouped by domain (payment-requests, team, settings, bankaccount, cost-centre, user-management, navbar, home, general) |
| `components/general/` | Reusable UI primitives: box, stat-box, notification |
| `services/` | Angular services per domain |
| `types/api-types.ts` | OpenAPI-generated DTOs — source of truth for all data shapes |
| `types/exporter.ts` | Re-exports from api-types.ts — **always import DTOs from here** |
| `styles/_variables.scss` | All design tokens (colors, spacing, fonts) |
| `app.routes.ts` | All client-side routes |

Every component SCSS file starts with `@use 'variables' as v;`, uses BEM naming, and references all tokens via `v.$variable-name`. Never hardcode colors or pixel values that have a variable equivalent.

Design system (colors, spacing, components): `docs/frontend/design-system.md`
Angular patterns (signals, control flow, inputs): `docs/frontend/patterns.md`

## Backend (`backend/PayTrack/`)

| Path | Purpose |
|---|---|
| `Api/Handler/` | Minimal API endpoint handlers (static classes, not controllers) |
| `Application/Services/Model/` | Service interfaces (`IXxxService`) |
| `Application/Services/Implementation/` | Service implementations |
| `Application/Dto/` | Request/response DTOs, grouped by domain |
| `Application/Exceptions/` | `NotFoundException`, `InvalidStateException`, etc. |
| `Data/Entities/` | EF Core entities |
| `Data/Repositories/Model/` | Repository interfaces |
| `Data/Repositories/Implementation/` | Repository implementations |
| `Data/AppDbContext.cs` | EF Core DbContext |

**Auth in handlers** — inject `IAuthService`, call `authService.GetCurrentUser()` which returns `User?`. Throw `NotFoundException` if null — there is no `[Authorize]` attribute pattern.

## Key entities

`Transaction` is an **abstract base class** (TPH). Concrete types: `PaymentRequestByUser` (has `InvoiceNumber`, `PaymentDirection`), `PaymentRequestByTeam`, `PaymentManual` (admin-created).

Important fields: `Id`, `UserId`, `TeamId`, `Amount` (decimal), `PurposeOfPayment`, `PaymentReference`, `Status` (TransactionStatus enum), `PaidAt` (DateTime?), `StatusHistory` (ICollection).

**Enums:** `TransactionStatus`: 0=Submitted, 1=Approved, 2=Rejected, 3=Paid, 4=Reimbursed · `PaymentDirection`: `In`, `Out` · `Role`: 0=RegularUser, 1=TeamLead, 2=Admin

Other key entities: `Budget` (links Team + CostCentre + Season with a target amount and period), `Season` (groups budgets), `CostCentre`, `BankAccount` (per-user IBAN records), `SystemSetting` (key-value store for admin-configurable runtime settings).

Test patterns (xUnit, Moq, WebApplicationFactory): `docs/backend/testing.md`

## Agent skills

Issues: GitLab on reset.inso-world.com (`glab`) — see `docs/agents/issue-tracker.md`.
Triage labels: default five-role vocabulary — see `docs/agents/triage-labels.md`.
Domain docs: single-context (`CONTEXT.md` + `docs/adr/`) — see `docs/agents/domain.md`.
