# Frontend Design Notes

This is a short reference for the shared frontend styling.

## Source files

- `frontend/src/styles/_variables.scss`
- `frontend/src/styles.scss`
- `frontend/src/styles/_badges.scss`

## Main colors

| Variable | Value | Typical use |
| --- | --- | --- |
| `$TUR-primary-orange` | `#EB6A00` | Primary actions, highlights |
| `$TUR-primary-black` | `#141415` | Dark backgrounds |
| `$TUR-secondary-background` | `#F6F6F6` | Inputs, soft surfaces |
| `$TUR-border-color` | `#DBDADA` | Borders on cards and fields |
| `$text-color` | `#111827` | Main text |
| `$text-muted` | `#6b7280` | Secondary text |
| `$success-color` | `#16a34a` | Success states |
| `$warning-color` | `#f59e0b` | Warning states |
| `$info-color` | `#0ea5e9` | Informational states |
| `$accent-color` | `#ef4444` | Error and destructive states |
| `$teal-color` | `#14b8a6` | Paid status |
| `$purple-color` | `#8b5cf6` | Review status |

## Spacing and text

- Spacing scale: `$size-xxxs` to `$size-xxxl` (`4px` to `96px`)
- Base font size: `$font-s` = `16px`
- Common weights: `$font-medium` = `500`, `$font-semibold` = `600`, `$font-bold` = `700`

## Common global classes

- `.main-page-container`: default page wrapper with responsive outer margin
- `.background-tur-black`: sets the background to the brand black
- `.toolbar`: white card-style wrapper for filters and action bars
- `.btn.btn-primary`: orange primary button
- `.btn.btn-secondary`: white outlined button with orange hover state

## Forms

Inputs, selects, and textareas are styled globally:

- background uses `$TUR-secondary-background`
- border uses `$TUR-border-color`
- focus state uses the orange brand color
- default field height is `2.6rem`

## Status classes

Use `.status` as the base badge class and add one modifier:

- `.status-submitted`
- `.status-changes-requested`
- `.status-approved`
- `.status-paid`
- `.status-declined`
- `.status-review`

## Reusable UI components

- `app-box-component`: standard content card with title and subtitle
- `app-stat-box-component`: stat or KPI card
- `app-pagination-component`: shared pagination footer
- `app-modal-component`: shared modal shell

## Simple rules

- Prefer shared variables from `_variables.scss` over inline hex values
- Prefer Bootstrap button classes plus existing overrides over custom button CSS
- Prefer shared components for cards, modals, and stat sections
- Keep page content inside `.main-page-container`
