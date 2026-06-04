# Angular Patterns

## Standalone components with signals

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

## Template control flow

```html
@if (condition) { ... } @else { ... }
@for (item of list(); track item.id) { ... }
```

## Class bindings

```html
[class.modifier]="condition"
[class]="methodReturningClassString()"
```

## Component inputs

Use `input()` function, not `@Input()` decorator.

## Notifications

Inject `NotificationService`, call `.showSuccess(msg)` / `.showError(msg)`.

## DTOs

Always import from `types/exporter.ts` (re-exports from `types/api-types.ts`).

## Currency display

Use `| euro` (from `app/pipes/euro.pipe.ts`) for all monetary values in templates. Outputs German format (`1.234,56 €`). Returns `—` for `null`/`undefined`. The `de-DE` locale is registered globally in `app.config.ts`.

```html
{{ transaction.amount | euro }}
{{ budget.targetAmount | euro }}
```

Import `EuroPipe` in the component's `imports` array. Never use `| currency`, `| number`, raw amount bindings, or `formatBudgetAmount()` methods.
