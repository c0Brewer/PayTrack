---
title: 2026-03-24-Meeting-Notes
---
Time: 20:30 - 21:15, 45min

# Participants:

* Jonas Tatzberger, Thimon Pelka, Jakob Jeschke, Jakob Schreiblehner, Michael Lohwasser, Felix Kainz (Tutor)

# Absent:

* Christoph Bräuer (no time)

# Meeting Goals:

* Get feedback on the project contract draft
* Answer questions that occured during the project contract creation

# Decisions/Questions answered:

* Does the tutor want to use Discord for communication aswell or stick with E-Mail?
  * We will stay with E-Mail.
* Does "Special-features point" include our technical highlight, or is this for other constraints only?
  * Does not have anything to do with the technical highlight --\> only with external conditions that might affect us. --\> TUW-Racing will host this, we should probably add this here.
* For section 6 - how detailed should it be?
  * Should be overview of where we use existing solutions - what are the risks when using them, etc.
  * Look through existing solution, how much effort will it be to integrate them? How well are they documented? What resources does it need?
  * Focus on 2-3 points that are critical and consider them in more detail!
  * Gap and Risk analysis is basically: If we need to write API ourselves -\> big risk, if it exists -\> smaller risk, etc.
    * especially for third party services --\> What if they are down? What if API changes? What if pricing gets changed?
* For section 7 - cost estimation \~750h in total for implementation is ok?
  * Yes!
  * User-stories are ok for this --\> Iceberg list is not needed
  * For MR1: think about who wants to do what!
* For section 8
  * Non-function requirements that we list MUST be verifiable --\> keep this in mind!
    * e.g. specify which browsers and which version are supported (in order to be testable!), what screen-sizes are supported --\> be very specific with this (i.e. tested on device x, with resolution y and version z)
  * be VERY specific with these requirements
* For section 10 - what exactly is meant with horizontal responsibilities (e.g. that the roles mean they are not the only ones doing this, or something else?)
  * It is the description that is already there, ok like this.
* For section 11
  * Here it is more important to specify points that should be not included (e.g. somebody reads the rest of the contract and would not know if these things are in or out of scope)
* For section 12 - how detailed should it be?
  * Can just be taken from TUWEL and be pretty simple.
* For section 14 - what other documentation is needed (should we just re-add the stuff that is in section 12 already?
  * No, what is there already is ok.
* In general, when is the final project contract submitted?
  * After MR1: Feedback is included --\> send to Tutor --\> confirms with assistant
  * this workflow is continued throughout the project --\> project contract is a living document!
* Jour-Fixe on Friday