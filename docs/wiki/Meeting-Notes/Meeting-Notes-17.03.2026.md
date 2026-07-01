---
title: 2026-03-17-Meeting Notes
---
Time: 17:00-19:00, 2h00min

# Participants:

* Attending: Jonas, Thimon, Michael, Jakob1, Jakob2 (until 18:30), Christoph (from 18;00), Luzia (TUW-Racing) (until 18:30)
* Missing: -

# Agenda:

* Go through feature list and define in more detail.
* Talk about the further process with the project team

# Notes:

## Discussion with Luzia

* Question from Luzia: Did access to all provided resources work? \\rightarrow Yes!
* Email, Name, Payment purpose, Invoice date, Submission date, Amount, IBAN, Payment to person or company (drop-down or similar for selection), Module (e.g. Electronics, Suspension, Finance; essentially department), Invoice upload, Comment
* approx. 800 incoming and 800 outgoing transactions per year
* Duplicate invoices: show a warning if similar invoices exist, do not automatically reject
* Cost centers are line items within a module
* Finance team adds cost center when confirming an invoice (automatic suggestions might be possible, but difficult → maybe prompt: which cost center does this invoice belong to? → output as a suggestion)
* Make modules and cost centers configurable (cost centers are not fixed to modules!)
* Order lists: enter in lists who wants what (linked to Google account → add new outstanding amount to that profile) → example of an order list needed! Matching via names stored in Google accounts
* Internal communication: important via Slack; less important via email
* Payment reminders via button (first overview of people with long-overdue invoices, then button to automatically send messages to all who are over X days)
* If status is changed to Rejected → comment required!; not necessary in “In review” status; normal lifecycle: Submitted → In progress (= clarification needed) → Paid/Rejected
* If status is changed to Paid → payment reference is entered and automatically stored with image in Google Drive! One folder per month per year; invoices are stored based on PAYMENT DATE, not submission date
* We receive a (Google) account with access to TUW Racing resources!
* PDF/CSV export (similar to bank statement) → overview of all incoming/outgoing invoices in a specific time period
* Corporate design document (access via internal document once we have access), (logo, color scheme, etc.)
* Provide website as browser bookmark and as Progressive Web App (PWA); possibly allow taking pictures directly via phone camera and uploading (if feasible)
* Short overview of current process/history of the tool:
  * New project/tool is simplification and risk reduction; everyone pays themselves, goes e.g. to Obi and buys what they need
    * Previously send invoice via email --\> missing data
    * Main idea: Google Forms with required fields (current version)
    * From that --\> idea to expand into a full system
* Tech stack as we prefer; suggestion from Aris: Java/Spring Boot backend, Angular frontend
* for hosting the website: talk to Aris (all their sites are on .tuwr.at adresses, we would most likely use the same)

## Internal Discussion

* Which feature is going beyond CRUD:
  * either extract information from invoices using OCR
  * banking integration (need to look into it concerning security/if we get access to it) --\> try it out using the Sandbox environment; their account is from ErsteBank/Sparkasse
  * fuzzy maching algorithm to detect duplicate invoices
  * Progressive Web App?
  * Store invoices on Google Docs
  * Automatic Notifications?
* Who does what:
  * Jonas: Presentation for Friday; 2.1, 2.2, 2.3
  * Thimon: 2.5, 2.6
  * Michael: 2.5
  * Christoph: 1.4, 1.5, 1.6
  * Jakob1: 2.4
  * Jakob2: 2.7

# Open Questions:

\--

## Discuss with tutor:

* Which feature is going beyond CRUD (what should we choose/what makes sense)?
* Scope of the project