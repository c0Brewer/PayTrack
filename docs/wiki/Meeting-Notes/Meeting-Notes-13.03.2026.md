---
title: 2026-03-13-Meeting-Notes
---
Zeit: 10:00-11:15, 1h15min

### Teilnehmer

Anwesend:

* Jonas, Jakob1, Jakob2, Michael, Thimon, Felix (Tutor)\
 
Abwesend:
* Christoph

### Agenda

* Vorstellungsrunde
* Organisatorisches
* Zoom-Meeting mit TUW Racing Team
* Diskussion

### Organisatorisches:

* 3 Management Reviews:
* Project Proposal (\~7min Projektvorstellung, 8 min Diskussion mit 2 anderen Tutoren, danch Assistenzzuordnung (i.e. Professor, welcher unser Projekt betreut))
* Management Review: Abstecken, was konkret gemacht wird (i.e. Leistungsgrundlage, Verpflichtung, was alles erreicht werden soll, darauf basiert die Bewertung)
* ca. 2 Wochen, bis Osterferien für Organisatorischen Teil
* SCRUM, ca. 4 Sprints mit 2 Wochen
  * nach 1. Sprint: internal review (\~2h); zum wöchentlichen Fixtermin
  * nach 2. Sprint: 2. Management review --\> hier muss Prototyp laufen/man muss etwas herzeigen können
  * nach 3. Sprint: internal review (\~2h); zum wöchentlichen Fixtermin
  * nach 4. Sprint: 3. Management review --\> Projekt abschließend vorstellen, etc. schon davor fertig sein und Testen (e.g. mit Racing Team) --\> gleich danach: Note
* Produkt gehört uns (kann man danach selbst ohne Einschränkung weiterführen wenn man will)
* Gedanken machen, was wir uns aus dem Projekt mitnehmen wollen
* Wöchentlicher Fixtermin: `Freitag: 9:00`, in Person, ca. 1h; Ausgenommen `17.04.2026` --\> für diesen Tag Zoom-Meeting unter der Woche abends
  * Wenn man keine Zeit hat --\> früh genug davor (\~24h) bescheid geben, dann kein Problem!
* Vorab schon realistische Vorstellung der gewünschten Note abgeben, je nachdem dann den Aufwand der einzelnen Gruppenmitglieder zuteilen (schon beim MR1)
* Mail an Felix, falls es Probleme im Team gibt (Zusammenarbeit, etc.) (dann kann man 1 on 1 Meeting machen um Lösung zu finden)
* Zeit die man in das Projekt invesitert laufend mittracken: Issues anlegen, time spent (/spend h:mm um Zeit zu tracken) --\> im Wiki genauer erklärt
  * Issue für Jour-Fix erstellen
  * Issue für Rolle pro Person
  * Issue für Projektplanung/Auftrag, etc.
  * Zeitbuchung immer auf Issues, nicht auf merge-requests
* AI-Contributions: Wenn ganze Frameworks (z.B. CRUD-funktionen) oder Methoden/Klassen generiert werden: inline einen kurzen Kommentar, dass es von AI ist, ansonsten (Autocomplete, debugging, etc.) muss nicht dokumentiert werden

### Implementierung:

* Relative frei (Tech-Stack etc.)
* GitLab Repo zur verfügung gestellt
* Nicht jeder muss überall mitprogrammieren (e.g. aufteilung Frontend/Backend, etc.)

### Projekt: TUW Racing Team

* Finance Team: Rechnungen sammeln, bearbeiten, analysen gestalten, verlauf nachvollziehen können (Rechnungseingang bis bezahlung)
* Project Proposal: (detailliertes Dokument auf TUWeL)
  * Features
  * Domain
  * Target Audience/Users
  * UML domain model
* Pastebin-Link wird von Felix gesendet, dort sind erste Requirements & Projekt-Idee geschildert
* Kontaktperson: Luzia (Kontaktinfo wird noch geteilt)
* min. 1 Teil/Feature der über Basis-CRUD hinausgeht
  * in diesem Projekt: wahrscheinlich irgendwas mit erweiterter Datenanalyse etc. (Luzia weiß mehr)
  * technischer Aufhängepunkt (z.B. Matching algorithmus, einbindung von Qualitätssichernden Frameworks, hohe Komplexität im tech-stack, reinforcement-learning, react-native, etc.)
  * Idee von Thimon: Daten direkt aus einer Rechnung auslesen, OCR/erkennung, muss mehr als nur library anschließen sein damit es zählt
  * Letzte Racing-Team Gruppe hatte einbindung von Caching --\> Ladezeiten optimieren
* Kommunikationsweg finden (Whatsapp/Discord?)

### QA:

* In the Asessement Criteria site: Is UI expert or AI expert correct?
  * Jede Person eine Hauptrolle, 4 Rollen "zwingend", die anderen beiden kann man Frei zuteilen
* GitLab: Projektspezifische DInge (commit template, messages, code-guidelines, branch protection)?
  * Kann man an Technik weiterleiten & die erstellen das dann.
  * GitLab Chef (Heimo Stranner) direkt taggen
* How are the meetings/management reviews organized? Fixed time-slots or individual for each group?
  * Wird für jede Gruppe individuell ausgemacht.

### ToDo:

* Email an Aris Mandolini: Discord-Invite schicken! (bzw. Nummern austauschen)
* Informieren: hat Racing Team infrastruktur, welche wir verwenden können, falls nicht --\> schauen ob die gegebenen Resources reichen (sind in GitLab-Wiki dokumentiert)
* Wir bekommen Demo-Version mit Mock Daten von aktuellem System um sich einen Überblick zu schaffen.
* Falls rückfragen/feedback nötig: Mail an Felix (e.g. für Project Proposal). Am besten dann Abends ein Zoom-meeting