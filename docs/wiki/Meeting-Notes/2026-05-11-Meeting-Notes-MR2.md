---
title: 'MR2 2026-05-11-Meeting-Notes '
---
Time: 18:30 - 20:00, 1h 30min

# Participants:

* Jonas Tatzberger, Thimon Pelka, Jakob Schreiblehner, Michael Lohwasser, Jakob Jeschke, Christoph Bräuer, Felix Kainz (Tutor) (until 19:00), Peter Frühwirt

# Absent:

* 

# Meeting Goals:

* Discuss progress and get feedback

## Notes:

* Present Project overview & internal reflection (positives & negatives)
* Thimon:
  * Login + Invoice submission
    * invoice subission: on "not-yet-paid" option --\> have input to specify the deadline of the payment
  * Project Setup
* Jonas:
  * Cost-Centre Management
  * Invoice/Request overview + detail page
    * Currency/Formatting bei invoice view
* Michael:
  * Deployment
  * Bank account on first login
  * Invoice-Review process:
    * discuss with TUW Racing if we want to have an undo button for certain actions in the lifecycle! (e.g. requets-changes, etc.)
    * Error-message when the reason for a lifecycle change is too short should describe what is wrong (currently Unknown error)
    * Purpose when switching to Paid is probably not needed
* Jakob S
  * Bank Account Management
  * Invoice Duplicates
    * Use different criteria to detect duplicates (e.g. amount + day + user or team, only invoice ID)
    * When duplicate detected --\> give better information on what invoice is duplicated (e.g. amount + day + user)
    * differentiate between soft + hard
    * have view for admins where they get some visual indicator that it might be a duplicate with another invoice!
    * discuss with TUW Racing to find a solution that is appropriate for them
* Christoph
  * Team Management
* Jakob J
  * Mockups
  * Frontend
    * make design more consistent: buttons with/without icons, etc.
    * make colors consistent (e.g. in list views --\> background colors are slightly different)
  * PWA
* AI Usage:
  * Create architecture.md or something similar to have uniform structure for AI generated code (formatting rules/etc.)
  * use skills (e.g. grill-me)
  * when writing features --\> take a clean context for test-file generation (else you get tests that are exactly like the code!)
* Notes:
  * Should season really be defined by the user?
    * Discuss with TUW Racing on how this is best handled (automatically set? finance-team adjusts it?)
  * Privacy:
    * Data has to be stored for 7 years in case of a financial audit
    * After these 7 years the personal data HAS to be deleted
    * Since this is not in the project contract -\> don't implement it unless we get PAID
  * validation is inconsistent (UI in general!) (backend is on the top right, frontend is directly in the modals --\> according to Jakob J this is already implemented in a brancht that is to be merged)
* Questions:
  * What is the best way to define admins on product launch
    * First use is fine --\> if somebody is faster --\> just reset
    * Can also be done via terminal, etc. --\> don't overthink it!

## Decisions:

* Date MR3 25.06/26.06 not possible --\> write suggestions to Felix, he will then forward it!