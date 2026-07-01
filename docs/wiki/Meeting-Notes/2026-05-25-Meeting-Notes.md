---
title: 2026-05-25-Meeting-Notes
---
Time: 9:10 - 10:05, 55min

# Participants:

* Jonas Tatzberger, Thimon Pelka, Jakob Schreiblehner, Christoph Bräuer, Felix Kainz (Tutor)

# Absent:

* Michael Lohwasser, Jakob Jeschke

# Meeting Goals:

## Notes:

* Should the "Submitted"-State be renamed to "Open" in the frontend for payment-requests by team
* Navbar sould be updated --\> group related tabs/move tabs into the settings?
* Find better names for the tabs in the navbar
* Suggestion: MR3 on the evening of 29.06.
  * In MR3: Present final application
  * Short discussion between tutor and prof
  * then you directly know your grade
  * grade is received after ALL MR3's are done (\~ beginning of July)
  * for Mr. Frühwirt --\> he will especially look at the parts where he gave feedback in MR2
* Invoice number:
  * Talk again with Luzia: Do all (most?) invoices they get have an invoice number? If not --\> how should we handle this case?ye
* Seasons:
  * Should seasons be able to be "completed" (i.e. you can lock it after the annual financial statement is done)
* Cost Center-Management
  * Remove budget-editing from cost-centre management, should only be for teams
* Payment requests by team:
  * remove payment-reference
  * if email/slack fails --\> throw custom error
  * send email after payment-request is created -\> after creating also give information that an email was sent!
  * admin view should be sorted -\> open/closed + due date descending
  * suggestion: one week before MR3 --\> feature freeze so documentation, user study (with racing team-members), polishing, presentation content, etc. can be done
* Add sorting functionality for the different list-views!
* Ask Luzia: Should Google-Drive backups also have a structure that resembles the seasons?
* Duplicates:
  * Improve modal (UI/UX) to be understandable more easily
  * Admins should be able to confirm duplicates (=delete one of the invoices) or deny them (= remove warning!)
* Amounts should be displayed uniformly (issue is already created)

## Decisions:

* 