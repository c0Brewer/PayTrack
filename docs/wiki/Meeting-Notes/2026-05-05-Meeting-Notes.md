---
title: 2026-05-05-Meeting-Notes
---
Time: 18:00 - 19:00, 1h

# Participants:

* Jonas Tatzberger, Thimon Pelka, Jakob Schreiblehner, Michael Lohwasser, Luzia Jeckel (TUW Racing), Jakob Jeschke (from 19:30), Christoph Bräuer (from 19:30)

# Absent:

* 

# Meeting Goals:

* Present the current state of the project to the sponsor (TUW Racing) and get feedback

## Notes :

Invoice submission:

* Default setting: user’s own (likely referring to preselection).
* Invoice number is **not required**.
* New payout-type: “Already paid” is only for documentation purposes.
* Change payout-type: “Not yet paid” requires the **company name** (mandatory).
* Cost centers should have a **parent category (overclass)**.

New Budget-logic: “Posten” (budget items / entries) are tied to cost-centres and teams. Invoices are now implicitly tied to cost-centres via these budget items (i.e. the finance team selects which budget item an invoice belongs to -\> this budget item is then tied to the corresponding cost-centre).\
Requirements fro budget-items:

* Name
* Description
* Assigned cost center
* Assigned module (team)
* Due date
* Target budget (planned amount)
* Assigned to a season

User Management:

* Users set to **inactive** can still log in, but **cannot submit** anything.
* There should be a way to **fully delete users** (hidden), but only after \~7 years and upon request.

Cost-Centres:

* Should have (optional) categories that they are assigned to

## Decisions

* Changes to:
  * Invoice Submission
  * Budget-logic
  * User Management
  * Cost-Centres