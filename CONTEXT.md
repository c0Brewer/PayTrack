# PayTrack – Domain Glossary

## Budget

A spending or income allocation assigned to a **Team** for a given period, linked to a **Cost Centre** and a **Season**. Budgets are typed as either an **Expense Budget** or an **Income Budget**.

## Expense Budget

A `Budget` with `Type = Expense`. Tracks outgoing money (invoices, reimbursements) against a `TargetAmount`. Utilisation is measured as paid and approved amounts against the target; exceeding the target is a warning state ("over budget").

## Income Budget

A `Budget` with `Type = Income`. Tracks incoming money (e.g. users who owe the organisation money, merchandise revenue). Has no `TargetAmount`. Exceeding the collected amount carries no warning semantics.

## BudgetType

Enum distinguishing the two budget kinds: `Expense = 0` (default), `Income = 1`.

## Transaction

Abstract base for a payment event. Concrete subtypes: `PaymentRequestByUser` (has `InvoiceNumber`, `PaymentDirection`) and `PaymentRequestByTeam`. Linked optionally to a `Budget` via `BudgetId`.

## PaymentDirection

Enum on `PaymentRequestByUser` indicating money flow: `In` (incoming) or `Out` (outgoing). The mapper uses direction to calculate `PaidAmount` and `ApprovedAmount` per budget type: for **Expense Budgets**, `Out - In`; for **Income Budgets**, `In - Out`.

## TransactionStatus

Lifecycle state of a Transaction: `Submitted → ChangesRequested ↔ Review → Approved → Paid` (primary happy path). Also: `Declined` (terminal). A `PaymentRequestByUser` submitted with `PayoutType = AlreadyPaid` initialises directly to `Paid`, bypassing the lifecycle.

## PayoutType

Enum on `PaymentRequestByUser` classifying how the payment is disbursed: `User = 0` (reimbursement to the submitting user — requires a bank account), `NotYetPaid = 1` (payment to an external creditor — requires CreditorName), `AlreadyPaid = 2` (documentation of a payment already made outside the system — status initialises to `Paid` at creation).

## CreditorName

The name of the external company to which a `NotYetPaid` payment must be made. Informal string; used for internal documentation and filtering. Required when `PayoutType = NotYetPaid`, absent for all other payout types.

## Team

Organisational unit that owns Budgets and has Members. Identified by name and optional display colour.

## Cost Centre

Financial classification entity. Budgets are linked to a Cost Centre; transactions inherit that classification via their Budget.

## Season

Time-bounded planning period. Budgets belong to a Season.

## Deactivated User

A **User** with `IsActive = false`. A Deactivated User may still log in and read their own transaction history, but may not submit new invoices or exercise any admin capabilities regardless of their assigned role. Deactivation is enforced per-request (not just at login), so it takes effect immediately without requiring the user to re-authenticate.
