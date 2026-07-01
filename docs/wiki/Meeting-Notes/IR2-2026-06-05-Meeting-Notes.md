Time: 9:00 - 10:30, 1h30min

# Participants:

* Jonas Tatzberger, Thimon Pelka, Jakob Jeschke, Jakob Schreiblehner, Christoph Bräuer, Michael Lohwasser, Felix Kainz, Victor Toumbev

# Absent:

* 

# Meeting Goals:

* Conduct Internal-Review 2 (IR2)

# Notes:

* Each member presents their features, then gets feedback/discusses (\~15min/Member)
  * Thimon:
    * Sort the entries in the bank-statement import (for better UX)
    * collapse the "no match" entries
    * add an additional modal to show an overview of the changes that will be made -\> the user has to confirm the changes there!
    * Should the scoring function be configurable by the paytrack admins? --\> in the config file (not the admin settings)
      * test and calibrate with real data for good, consistent results!
  * Jakob S:
    * when comparing duplicates --\> show info of both invoices that it concerns
    * test and calibrate duplicate detection with real data for good, consistent results!
    * for both matching-algorithms: scoring should use the same value ranges/algorithim for the heuristics
    * UI in the duplicates modal should be more intuitive (e.g. the orange text in the bottom of the modal, etc.)
    * for csv/pdf export: make sure all filters that are applied in the ui are also applied for the export (i.e. only what you see on the screen is exported!)
  * Michael:
    * re-work UI for changing lifecycle status (only have the buttons -\> open a modal where reasons/comments, etc. are entered and confirmed)
    * send emails for ALL status changes (even when a status change is undone!)
  * Jonas:
    * Payment requests
      * csv-import:
        * look into what happens when 2 users have the same name
        * allow manual editing of all users in the csv import modal
      * allow deletion of payment request (only when still on submitted status)
    * General:
      * change appsettings --\> use environment variables for secret values (passwords, slack-bot token, e-mail, etc.)
  * Christoph:
    * What happens when two invoices have the same invoice number?
      * date of submission is also added to the file-name (down to the second)
      * both are still stored (not overwritten) -\> should not be a problem
    * General:
      * change appsettings --\> use environment variables for secret values (passwords, slack-bot token, e-mail, etc.)
  * Jakob J:
    * Design is sehr clean.
* General remarks

# Decisions:

* For every developer: Look into the individual feedback and implement it!
* Discuss with racing team: what should be sent via Email? What should be sent via slack?
* Configs that might need to be changed (secrets, etc.) should be well documented so people which are not familiar with the system can update/change them!
* Possible date for MR3: Mo. 29.06. from \~17:00

Feedback from Victor:

* clean UI
* very good progress (it is rare to have such a clean and close to finished application 3 weeks ahead of the MR3)

Suggestion from Felix:

* do a user study (try to finish the project 1 week ahead of the MR3) --\> give it to racing team for feedback, etc.
* prepare the presentation for MR3 ahead of time -\> think about a story/how to present it

# Questions: