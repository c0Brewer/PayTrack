---
title: 2026-04-22-Meeting-Notes
---
Time: 18:30 - 19:30, 1h

# Participants:

* Jonas Tatzberger, Jakob Jeschke, Jakob Schreiblehner, Christoph Bräuer, Michael Lohwasser, Thimon Pelka (from 19:15)

# Absent:

* 

# Meeting Goals:

* Discuss what went well/where problems occured so far

# Notes:

* Christoph: View all teams implementation about done, started with manage teams work items
  * TeamBudgetDto -\> how to do it?
* Jakob1: Manage Personal Information issue done -\> review by Thimon already happened -\> will rework until friday
  * Michael will base his issue (bank information on first login) on this
* Jakob2: Mock-up's: no real progress since last meeting; local project setup done
* Jonas: Manage all teams & view all teams issues submitted for review
* Michael: started bank-information on first login issue, based on Jakob1's branch
* Issues should probably define the roles better (e.g. Finance Team Member means Admin)

# Decisions:

Ideas for issue/branch handling:

* Define hierarchy of issues to determine which issues depend on what other issues.
  * Then do sprint-planning meetings
* Create dev-branches (where unfinished stuff can be commited aswell)
  * problem: can't forget to merge into master!
* Split up issues more fine grained (e.g. backend + frontend not in one issue)
* add linked/child items to issues in order to see dependencies
* Input by Thimon: can stay in one issue and do frontend + backend in the same branch

When something is ready to review: Write into the chat, so somebody feels responsible for it (also request review!)