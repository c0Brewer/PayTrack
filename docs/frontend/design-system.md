# Design System

## Brand colors (`styles/_variables.scss`)

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

## Spacing (`$size-*`)

`xxxs`=4px · `xxs`=8px · `xs`=12px · `s`=16px · `m`=24px · `l`=32px · `xl`=48px

## Font sizes (`$font-*`)

`xxs`=12px · `xs`=14px · `s`=16px (body baseline) · `m`=18px · `l`=20px · `xl`=24px

## Font weights

`$font-medium`=500 · `$font-semibold`=600 · `$font-bold`=700

## Buttons

Always use Bootstrap global classes — never write custom button styles:
- `.btn.btn-primary` — orange fill, white text
- `.btn.btn-secondary` — white fill, orange border/text; inverts on hover

## Cards

All cards: `background: white; border: 1px solid $TUR-border-color; border-radius: 12px; box-shadow: 0 1px 2px $shadow-color`

Reusable wrappers:
- `<app-box-component title="..." subtitle="..." size="small|medium|large">` — general content card
- `<app-stat-box-component header="..." [content]="..." icon="..." iconColor="..." iconBackgroundColor="..." size="small">` — KPI stat card

## Tables

Use `.TUR-table` class. Collapses to card layout on mobile via `data-label` attributes on `<td>`.

## Status badges

`.status-btn` — pill shape, `border-radius: 999px`. Modifiers: `.active` (green), `.inactive` (gray).

## Page layout

Top-level route components wrap content in `<div class="main-page-container">` — applies `margin: 4rem` desktop / `1.5rem` mobile.
