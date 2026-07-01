---
title: 2026-06-25-User-Study-Meeting
---
# User Study & Sponsor Meeting — 2026-06-25

**Date:** 25 June 2026  
**Participants:** Paytrack project team and representatives of the TUW Racing Team  
**Purpose:** Validate the nearly feature-complete Paytrack prototype through four representative workflows, collect usability feedback, and identify remaining improvements before Milestone Review 3 (MR3).

## Summary

The study was very positive overall. The TUW Racing Team completed all demonstrated workflows without major issues and described the application as intuitive. The feedback primarily concerns a small number of usability issues, role-dependent data visibility, offline/PWA behaviour, and follow-up improvements.

## Workflow Results

### Workflow 1

**Result:** Positive.  
The workflow was completed successfully and no issues were reported.

### Workflow 2 — Role Changes and Invoice Visibility

**Finding:** When a user's role is changed from regular user to administrator, the user interface shows additional invoices under **My Invoices**. This behaviour was unexpected during the study.

**Follow-up:** Review the role-based filtering and the intended definition of *My Invoices*. Ensure that changing a user to an administrator does not unintentionally broaden this view beyond invoices submitted by that user, unless this is explicitly intended and clearly communicated in the UI.

### Workflow 3 — Budgets, Teams, and Payment Requests

**Finding 1:** Creating a team with a budget was perceived as unintuitive. Users need to add the budget before they can save/create the team, which can initially look like an input mistake.

**Follow-up:** Revisit the create-team flow and improve its guidance. Possible improvements include clearer wording, an explanatory hint, a more explicit “Add budget entry” action, or allowing a team to be created before optional budget entries are added.

**Finding 2:** A team without an income budget cannot be selected when creating a payment request.

**Follow-up:** Verify whether this restriction is intentional. If payment requests are meant to represent incoming payments, evaluate whether selecting a team should remain possible without an existing income-budget entry, or whether the UI should explain why the team is unavailable.

### Workflow 4 — Offline Use and PWA Caching

**Finding 1:** An error message should not be shown when the application is offline.

**Follow-up:** Replace generic offline errors with a clear, non-blocking offline-state indicator and user-friendly messaging where live data cannot be refreshed.

**Finding 2:** Offline caching only works after some pages have already been visited. Installing/downloading the application, signing in, closing it, and then going offline is not sufficient to access essential views.

**Follow-up:** Review service-worker precaching and first-install behaviour. Define and test which core assets and views must be available directly after installation and initial sign-in, without requiring prior navigation to each page.

**Finding 3:** The TUW Racing Team asked whether Paytrack could support multiple currencies.

**Scope decision:** Multi-currency accounting remains **out of scope** for the current project. The project contract explicitly excludes advanced financial-management features such as multi-currency accounting. The request should be documented as a potential future extension rather than implemented within the remaining project scope.

## General Feedback and Follow-Up Items

| Topic | Feedback / Decision | Follow-up |
| --- | --- | --- |
| Duplicate matching | When duplicate matching leads to deletion of an invoice, the responsible user should be notified. | Define the responsible role and implement or document an appropriate notification/audit mechanism. Ensure deletion decisions remain traceable. |
| Deployment review | Lucia will review the deployed version over the weekend and provide additional feedback if she identifies further issues. | Monitor and record any follow-up feedback received. |
| MR3 content | The TUW Racing Team will provide a short text for presentation on Monday at MR3. | Integrate the provided text into the MR3 presentation once received. |
| Overall usability | The team reported that the workflows were completed without issues and that the application looked intuitive. | Preserve the current core flows while addressing the identified edge cases and usability improvements. |

## Outcome

The user study provides evidence that the main Paytrack workflows are usable and understandable for the intended stakeholders. Remaining work should focus on the identified role-based visibility issue, clearer budget/team interaction, more reliable initial offline availability, and communication around automated duplicate-handling decisions.
