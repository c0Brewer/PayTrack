# Project: PayTrack (QSE-08)

Payment request and transaction tracking app for teams and cost centres.

## Stack

- **Frontend:** Angular 17+ (standalone components, signals), TypeScript, SCSS, Bootstrap 5.3.8
- **Backend:** .NET (C#), REST API at `/api/v1/`
- **Icons:** Google Material Symbols Outlined — `<span class="material-symbols-outlined">icon_name</span>`
- **No Tailwind in new code** — the project is migrating away from Tailwind toward component-scoped SCSS

## Frontend structure (`frontend/src/app/`)

| Path | Purpose |
|---|---|
| `components/` | Feature components grouped by domain (payment-requests, team, settings, home, general) |
| `components/general/` | Reusable UI primitives: box, stat-box, notification |
| `services/` | Angular services per domain |
| `types/api-types.ts` | OpenAPI-generated DTOs — source of truth for all data shapes |
| `types/exporter.ts` | Re-exports from api-types.ts — import from here in components |
| `styles/_variables.scss` | All design tokens (colors, spacing, fonts) |
| `styles.scss` | Global Bootstrap overrides and shared utility classes |
| `app.routes.ts` | All client-side routes |

## Design system

### Brand colors (`styles/_variables.scss`)

| Variable | Hex | Use |
|---|---|---|
| `$TUR-primary-orange` | `#EB6A00` | Primary buttons, accents, hover states |
| `$TUR-secondary-orange` | `#FBE7D6FF` | Subtle orange tint backgrounds |
| `$TUR-primary-black` | `#141415` | Dark backgrounds (navbar) |
| `$TUR-border-color` | `#DBDADA` | Card/input borders |
| `$TUR-secondary-background` | `#F6F6F6` | Page background, panel fills |
| `$text-color` | `#111827` | Body text |
| `$text-muted` | `#6b7280` | Secondary/placeholder text |
| `$success-color` | `#16a34a` | Success states |
| `$warning-color` | `#f59e0b` | Warning states |
| `$border-color` | `#d1d5db` | Subtle inner borders |
| `$shadow-color` | `rgba(0,0,0,0.1)` | Card shadows |

### Spacing (`$size-*`)
`xxxs`=4px · `xxs`=8px · `xs`=12px · `s`=16px · `m`=24px · `l`=32px · `xl`=48px

### Font sizes (`$font-*`)
`xxs`=12px · `xs`=14px · `s`=16px (body baseline) · `m`=18px · `l`=20px · `xl`=24px

### Font weights
`$font-medium`=500 · `$font-semibold`=600 · `$font-bold`=700

### Buttons
Always use Bootstrap global classes — never write custom button styles:
- `.btn.btn-primary` — orange fill, white text
- `.btn.btn-secondary` — white fill, orange border/text; inverts on hover

### Cards
All cards: `background: white; border: 1px solid $TUR-border-color; border-radius: 12px; box-shadow: 0 1px 2px $shadow-color`

Reusable wrappers:
- `<app-box-component title="..." subtitle="..." size="small|medium|large">` — general content card
- `<app-stat-box-component header="..." [content]="..." icon="..." iconColor="..." iconBackgroundColor="..." size="small">` — KPI stat card

### Tables
Use `.TUR-table` class. Collapses to card layout on mobile via `data-label` attributes on `<td>`.

### Status badges
`.status-btn` — pill shape, `border-radius: 999px`. Modifiers: `.active` (green), `.inactive` (gray).

### Page layout
Top-level route components wrap content in `<div class="main-page-container">` — applies `margin: 4rem` desktop / `1.5rem` mobile.

## Writing component styles

Every component SCSS file:
1. Starts with `@use 'variables' as v;`
2. Uses BEM naming: `.block__element--modifier`
3. References all colors/spacing/fonts via `v.$variable-name`

Never hardcode colors or pixel values that have a variable equivalent.

## Angular patterns used in this project

**Standalone components with signals:**
```typescript
@Component({ selector: '...', imports: [OtherComponent], templateUrl: '...', styleUrl: '...' })
export class MyComponent {
  myValue = signal<string | null>(null);
  myList = signal<MyType[]>([]);
  count = computed(() => this.myList().filter(x => x.active).length);

  // Immutable update pattern
  this.myList.update(list =>
    list.map(item => item.id === id ? { ...item, flag: !item.flag } : item)
  );
}
```

**Template control flow:**
```html
@if (condition) { ... } @else { ... }
@for (item of list(); track item.id) { ... }
```

**Class bindings:**
```html
[class.modifier]="condition"
[class]="methodReturningClassString()"
```

**Component inputs** use `input()` function, not `@Input()` decorator.

**Notifications:** inject `NotificationService`, call `.showSuccess(msg)` / `.showError(msg)`.

Always import DTOs from `types/exporter.ts`.

## Backend structure (`backend/PayTrack/`)

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

**Minimal API handler pattern** — handlers are static classes, methods are registered in `Program.cs`:
```csharp
public static class MyHandler {
    public static async Task<Results<Ok<Dto>, BadRequest<ProblemDetails>, ProblemHttpResult>>
        DoThing([FromBody] RequestDto body, IMyService service) { ... }
}
```

**Auth in handlers** — inject `IAuthService`, call `authService.GetCurrentUser()` which returns `User?`. Throw `NotFoundException` if null — there is no `[Authorize]` attribute pattern.

## Key entities

`Transaction` is an **abstract base class**. Concrete types:
- `PaymentRequestByUser` — has `InvoiceNumber`, `PaymentDirection`
- `PaymentRequestByTeam`

Important fields on `Transaction`: `Id`, `UserId`, `TeamId`, `Amount` (decimal), `PurposeOfPayment`, `PaymentReference`, `Status` (TransactionStatus enum), `PaidAt` (DateTime?), `StatusHistory` (ICollection).

**Enums:**
- `TransactionStatus`: 0=Submitted, 1=Approved, 2=Rejected, 3=Paid, 4=Reimbursed
- `PaymentDirection`: `In`, `Out`

## Backend test patterns

**Project:** `backend/PayTrack.Tests/` — xUnit + Moq + FluentAssertions + EF InMemory

**Service unit test** (mock repository, test business logic):
```csharp
public class MyServiceTests {
    private readonly Mock<IMyRepository> repoMock = new();
    private readonly MyService service;
    public MyServiceTests() => service = new MyService(repoMock.Object);

    [Fact] public async Task Method_Condition_ExpectedResult() {
        repoMock.Setup(r => r.GetAsync(...)).ReturnsAsync(...);
        var result = await service.MethodAsync(...);
        result.Should().NotBeNull();
    }

    [Theory, InlineData(...)] public async Task ... { }
}
```

**Endpoint integration test** (WebApplicationFactory, mock services):
```csharp
public class MyEndpointsTests(MyApiFactory factory) : IClassFixture<MyApiFactory> { ... }

public class MyApiFactory : WebApplicationFactory<Program> {
    public Mock<IAuthService> AuthServiceMock { get; } = new();
    public Mock<IMyService> MyServiceMock { get; } = new();
    protected override void ConfigureWebHost(IWebHostBuilder builder) {
        builder.UseEnvironment("Test");
        builder.ConfigureServices(services => {
            // Replace DB with InMemory
            // Register TestAuthHandler for "Test" scheme
            // Replace service registrations with mocks via services.AddSingleton(mock.Object)
        });
    }
}
```
The `TestAuthHandler` (in `Helper/`) always authenticates successfully — tests don't need real tokens. Add `client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test")` to every authenticated request.

**Positional records** — DTOs like `BankStatementUpdateRequestDto` use positional constructor syntax:
```csharp
new BankStatementUpdateRequestDto("entry-0", MatchedTransactionId: 5, Skipped: false)
// NOT: new() { EntryId = ..., ... }
```

**Repository mock for ITransactionRepository:**
```csharp
repoMock.Setup(r => r.GetAllAsync(It.IsAny<GetTransactionQuery>()))
    .ReturnsAsync((new List<Transaction> { tx }, totalCount));
repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<GetTransactionQueryById>()))
    .ReturnsAsync(transaction);
repoMock.Setup(r => r.UpdateAsync(It.IsAny<Transaction>()))
    .ReturnsAsync((Transaction t) => t);
```
