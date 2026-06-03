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
