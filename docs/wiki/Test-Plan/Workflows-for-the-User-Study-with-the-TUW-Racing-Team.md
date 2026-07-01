---
title: Workflows for the User Study with the TUW Racing Team
---
# Workflows for the User Study with the TUW Racing Team
This wiki entry covers the workflows for a user study with the TUW Racing Team. It marks one of the last steps before the final project presentation. The listed workflows cover real-world scenarios in which our project will be used by the Racing Team. Each workflow describes a different task, which a TUW Racing Team member should perform without having seen the project before. The result should help us analyze whether our work is intuitive and easy to use.

## Overview

We designed four different workflows, which are described in the following sections in more detail. For a quick first glance, the workflows are the following:

* **Workflow 1**: *New Member Submits And Tracks A Reimbursement* <br>
This workflow covers the real-world scenario that a team member bought some parts for the racing team and wants to get reimbursed afterwards.

* **Workflow 2**: *Finance Team Reviews Invoices, Marks Payments, And Uploads A Bank Statement* <br>
This workflow covers the real-world scenario that the finance team receives submitted invoices, processes them correctly, and uses a bank statement upload to reconcile payments.

* **Workflow 3**: *Finance Creates A Payment Request And Maintains Finance Structure* <br>
This workflow covers the real-world scenario that a member owes the racing team money, for example for an event fee, team clothing, or shared travel costs. The finance team also checks whether the organisational data needed for the request is correct, including seasons, teams, cost centres, and budgets.

* **Workflow 4**: *Member Uses PayTrack As A PWA With Offline Behaviour* <br>
This workflow covers the real-world scenario that a team member uses PayTrack on a mobile device, loses internet connection, still accesses cached information, and saves an invoice draft offline for later synchronization.

## Workflow 1: New Member Submits And Tracks A Reimbursement

#### Scenario

A team member bought parts for the racing team and wants to get reimbursed. <br>

#### Role

Regular team member
#### Task for participants

1. Log in to PayTrack.
2. Check whether your bank account information is complete.
3. Add or edit a bank account if needed.
4. Submit an invoice for a self-paid purchase.
5. Upload a receipt or invoice file.
6. Select the correct team.
7. Choose the correct payout type.
8. Find the submitted invoice again in `My Invoices`.
9. Use filters to locate it by status, team, amount, or purpose.
10. Open the invoice detail page and explain what the current status means.

#### Covered Features

* Google login / authenticated access
* Mandatory bank account expectation
* Settings
* Bank account creation/editing
* Invoice submission
* Receipt upload
* Potentially the automatic invoice data extraction
* Payout type: `Paid Myself`
* Bank account selection
* Team selection
* My Invoices list
* Filtering
* Invoice detail/status tracking
* Duplicate-warning behavior, if test data triggers it

#### What to observe

* Do users understand that bank accounts are managed in settings?
* Do they understand when a bank account is required?
* Are the different payout types clear?
* Do they know what information belongs in purpose/comment?
* Was the data extraction feature seen and understood?
* Can they recover after validation errors?
* Can they find their invoice afterwards?
* Do the statuses make sense without explanation?

#### Success criteria

* Participant submits the invoice without moderator guidance.
* Participant can explain whether the invoice is waiting, approved, paid, declined, or requires changes.
* Participant can find the invoice again within about 1 minute.

## Workflow 2: Finance Team Reviews Invoices, Marks Payments, And Uploads A Bank Statement

#### Scenario

The finance team receives submitted invoices, processes them correctly, and later uploads a bank statement to reconcile payments with PayTrack transactions.

#### Role

Finance/admin user

#### Task for participant

1. Open the list of submitted invoices.
2. Find a specific invoice using filters.
3. Open the invoice detail page.
4. Inspect the uploaded receipt.
5. Decide that one invoice needs correction and request changes with a reason.
6. Choose whether to notify the user by email/Slack, if available.
7. Open another invoice that is correct.
8. Assign the correct budget/cost centre.
9. Approve the invoice.
10. Mark the approved invoice as paid with payment reference, payment date, and purpose.
11. Check the status history afterwards.
12. Open the bank statement upload/import area.
13. Upload a prepared bank statement file.
14. Let PayTrack find matching transactions.
15. Review matched, skipped, and unmatched entries.
16. Sort or inspect match confidence where possible.
17. Apply the matching updates and check whether the result is understandable.

#### Covered Features

* Admin navigation
* View Submitted Invoices
* Admin invoice filters
* Invoice detail view
* Receipt viewing/downloading
* Request changes
* Decline/approve decision path
* Email/Slack notification modal
* Budget assignment before approval
* Mark as paid
* Payment reference
* Status history
* Undo last status change, if relevant
* Bank statement upload/import
* Automatic transaction matching
* Match review
* Matched/skipped/unmatched statement entries
* Applying bank statement updates

#### What to observe

* Can finance users find the admin invoice area?
* Are filters understandable and useful?
* Is it clear that a budget must be assigned before approval?
* Do they understand the difference between request changes, decline, approve, and mark paid?
* Is the status history useful and readable?
* Is the notification flow understandable?
* Do users trust that the payment state changed correctly?
* Can users find the bank statement upload/import feature?
* Is the expected bank statement file format clear?
* Do users understand the difference between matched, skipped, and unmatched entries?
* Do users understand what will happen before applying the bank statement updates?

#### Success criteria

* Participant finds and processes the correct invoice.
* Participant does not approve without understanding budget assignment.
* Participant can explain the resulting status history.
* Participant can distinguish `approved` from `paid`.
* Participant can upload a bank statement and review the matching results.
* Participant can explain which entries were matched and which still need manual attention.

## Workflow 3: Finance Creates A Payment Request And Maintains Finance Structure

#### Scenario

A member owes the racing team money, for example for an event fee, team clothing, or shared travel costs. The finance team also checks whether the organisational data needed for the request is correct.

#### Role

Finance/admin user, then regular member for verification

#### Task for participant

1. Open Season Management.
2. Check whether the current season exists.
3. Create a new season if the test setup requires it.
4. In Team Management, check whether the relevant team exists and is active.
5. In Cost Centre Management, check whether the relevant cost centre exists.
6. Check whether the relevant budget exists and is assigned to the correct season and team.
7. Create or edit a cost centre/budget if the test setup requires it.
8. In User Management, find the target member.
9. Check or update the member’s team assignment and active status.
10. Create a payment request for that member.
11. Assign amount, due date, purpose, team, and cost centre.
12. Open the payment request list and find the created request using filters.
13. Open the payment request detail page.
14. Send a reminder via email/Slack, if available.
15. As the regular member, open `My Payment Requests` and verify that the request is visible and understandable.

#### Covered Features

* Season Management
* Season creation
* Team Management
* Cost Centre Management
* Budget entries
* Season/budget relationship
* User Management
* User filtering
* Team assignment
* Active/inactive users
* Create Payment Request
* Assign user via typeahead search
* Assign team and cost centre
* View Payment Requests admin list
* Payment request filters
* Payment request detail
* Email/Slack notification
* My Payment Requests user view
* Status/due date visibility

#### What to observe

* Can finance users find Season Management?
* Do users understand why a season is needed for budgets?
* Can finance users understand the relationship between season, team, cost centre, and budget?
* Is it clear where structural data is managed?
* Is the user search/typeahead discoverable?
* Is the payment request form understandable?
* Can the member understand what they owe, why, and by when?
* Are admin and member perspectives consistent?

#### Success criteria

* Finance participant creates or verifies the correct season.
* Finance participant creates or verifies the required team, cost centre, and budget setup.
* Finance participant creates the correct payment request without needing database/admin help.
* Participant can find the payment request again in the admin list.
* Member participant can find it in `My Payment Requests` and explain what action is expected.

## Workflow 4: Member Uses PayTrack As A PWA With Offline Behaviour

#### Scenario

A team member wants to use PayTrack on a mobile device during team work, where internet connection may be unstable. The user installs PayTrack as a PWA, opens previously loaded pages while offline, and saves an invoice draft locally until the connection is restored.

#### Role

Regular team member

#### Task for participants

1. Open PayTrack on a mobile device or mobile browser.
2. Install PayTrack as a PWA, if the browser/device offers the install option.
3. Open the installed PayTrack app.
4. Log in while online.
5. Visit `My Invoices`, `My Payment Requests`, and `Settings` once while online.
6. Open `Submit An Invoice` once while online.
7. Disconnect the device from the internet.
8. Reopen or refresh PayTrack and check whether the app shell still loads.
9. Check whether an offline banner or offline message is shown.
10. Open a previously visited page and check whether cached data is still visible.
11. Try an online-only action, for example approving, marking paid, or importing a bank statement, if available for the role/test user.
12. Observe whether PayTrack prevents the action or explains that it is unavailable offline.
13. Go to `Submit An Invoice` while offline.
14. Fill in invoice data and attach a receipt.
15. Save the invoice offline for later synchronization.
16. Reconnect to the internet.
17. Return to `Submit An Invoice`.
18. Restore or submit the locally saved invoice draft.
19. Verify that the invoice appears in `My Invoices`.

#### Covered Features

* PWA installation
* Mobile usage
* Service worker caching
* Cached app shell
* Cached previously loaded pages/data
* Offline status banner
* Offline read behavior
* Disabled online-only write actions
* Offline invoice submission draft
* Local offline queue
* Restoring/submitting an offline invoice draft after reconnecting
* User feedback during offline/online transitions

#### What to observe

* Can users discover that PayTrack can be installed as an app?
* Does the installed PWA feel like the normal web app?
* Is it clear when the app is offline?
* Do users understand which data is cached and which data may be outdated?
* Are online-only actions clearly disabled or explained?
* Can users save an invoice while offline without confusion?
* Can users find and restore the locally saved invoice draft after reconnecting?
* Do users trust that no invoice data was lost?

#### Success criteria

* Participant can install or open PayTrack as a PWA.
* Participant understands the offline banner/message.
* Participant can access at least one previously loaded page while offline.
* Participant understands that most write actions are unavailable offline.
* Participant can save an invoice draft offline.
* Participant can restore or submit the offline invoice draft after reconnecting.