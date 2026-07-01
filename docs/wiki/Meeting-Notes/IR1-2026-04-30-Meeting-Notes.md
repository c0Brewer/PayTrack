Time: 15:00 - 16:30, 1h30min

# Participants:

* Jonas Tatzberger, Thimon Pelka, Jakob Jeschke, Jakob Schreiblehner, Christoph Bräuer, Michael Lohwasser, Felix Kainz, Victor Toumbev

# Absent:

* 

# Meeting Goals:

* Conduct Internal-Review 1 (IR1)

# Notes:

* Short introduction of the project
* Each member presents their features, then gets feedback/discusses (\~15min/Member)
  * Thimon:
    * no notes
  * Jakob S:
    * don't use native browser pop-ups, use angular modal-windows
  * Michael:
    * IBAN input: split display into for characters (easier to check for user)
    * show validation errors (e.g. missing fields) only after the the "submit"-button is pressed (not after the first field is entered!)
  * Jonas:
    * Create CostCentre doesnt work at the moment (maybe broken by a different merge?)
  * Christoph:
    * implement domain model-changes as soon as possible (e.g. add spent-amount in addition to target-amount! (to track spending on the budget!))
  * Jakob J:
    * no notes
* General remarks
  * For validation: do as much as possible in the dto layer! (e.g. length, null-values, etc.). Decide on one way to do it (e.g. bank-accounts (Jakob S) have some double-validation)
  * Error objects: define a standard (have a discussion about this)
    * Suggestion by Felix: https://www.rfc-editor.org/rfc/rfc9457.html
  * Think about technical architecture: what should we pre-define/standardize (communication frontend-backend, exception handling, where to validate what and how do we check it?)

# Decisions:

* As soon as MVP exists -\> meeting with racing-team

# Questions: