# Ziele / Funktionen
## Login & Mitgliederportal
- Login-System für jedes Mitglied (User-Accounts; bitte über Google-Account) 👍
**nur über google? also kein custom anmeldung sondern alle user-anmeldungen laufen über google?**

- Mitglieder können eigene Einreichungen sehen (Historie) 👍
- Statusübersicht pro Rechnung (z. B. eingereicht / in Prüfung / freigegeben / bezahlt) 👍
kommentar schreiben; änderung anfordern -> dann weiter
abgelehnt status

## Rechnung einreichen (Rückerstattung)
- Online-Formular zum Einreichen von Ausgaben 👍
- Upload von Rechnungen / Bildern / Belegen 👍
- Pflichtfelder (z.B. Betrag, Datum, Kategorie, Beschreibung, Zahlungsinfo etc.)
**👍 bitte ganz genau definieren was ihr alles für felder haben wollt!!** 
felder von jetziger implementierung
modul auswählen
email, name, zahlungszweck (warum), datum der rechnung, datum der einreichung, betrag, iban, haben sie es bereits gezahlt oder geht es direkt an das unternehmen, modul (electronics, suspensions, finance, etc.), rechnung hochladen, kommentar feld für zusätzliches
module anlegen funktion einbauen
user können für andere module einreichen

finanz-team muss beim bestätigen auch kostenstelle angeben

kostenstellen gehören nicht zu modul; kostenstellen müssen editierbar sein, etc

- Gespeichter Daten/Auto-fill (wichtige Infos wie IBAN im Profil hinterlegen und dann direkt vorausgefüllt – aber auch anderes möglich)
**Welche infos genau? Bitte spezifizieren**

- Erstellung Automatischer QR mit richtigem Verwendungszweck und Betrag 
**👍 (Keine direkte Anbindung an Zahlungsanbieter oder? Wäre ein deutlicher Mehraufwand; Andere Features müssten dafür weggehen und wir bräuchten die dementsprechende Infrastruktur bei euch)**
relativ unnötig eigene Anbindung
erste bank sparkasse

- Ähnliche/doppelte Rechnungen erkennen (Fuzzy Matching) Z. B.: Anhand von Betrag, Datum, Händlername, OCR Text, Bildhash
**Joa, beim Erstellen der Rechnung? Fuzzy matching oder wenn wirklich alles gleich ist? Weil wenn z.b. nur Datum abweicht werden jährliche Mitgliedsbeiträge da aufscheinen o.ä.**
**Wenn eindeutige rechnungsnr o.ä. im system dann throwen wir error, aber sonst schwer zu definieren bzw. gebt uns gerne eine definition*
**Evtl wenn ähnliche Rechnungen vorhanden dann anzeigen und extra bestätigung fordern**

warnung wenn ähnlich; sowohl bei submit als auch bei überprüfung

- OCR Text erkennung - system erkennt automatisch wichtige Infomationen (?)
**👍 Joa. Sind die Rechnungen immer einheitlich? (Vermutlich nicht). Wir können höchsten probieren die Informationen herauszuziehen aber es muss auf jeden Fall immer überprüft werden und gibt keine Garantien.
Das ist nicht so einfach zum implmentieren**
**Wir bäuchten test-rechnungen, bzw beispiele**

- Automatische Erkennung der Kostenstelle (?)
**👍 Kostenstelle im Sinne von interne Kostenstellen? Woran kann man das erkennen? Dafür bräuchten wir klare Kriterien wann etwas zu welcher Kostenstelle gehört**
**Automatisch erkennen wird vermutlich nicht möglich. Ist aber möglich dass der user beim erstellen der Rechnungen verpflichtend die Kostenstelle angeben muss**
**Dann entweder fix kostenstellen vorher definieren oder möglichkeit in SW zum erstellen/managen der kostenstellen**

## Verein stellt Mitgliedern etwas in Rechnung
- Mitglieder sehen offene Beträge, z. B. Mitgliedsbeitrag, Merch, Poster etc. 
**👍 Ja aber es müssen dann immer regelmäßig händisch (bzw per upload?) die Banküberweisungen hochgeladen werden. Außer es ist bei eurer Bank möglich dass man die per API abfragt (sehr unwahrscheinlich!).
Sonst stehen die Beiträge ewig auf offen**
**Es gibt theoretisch API schnittstellen bei den meisten Banken, die sind aber meistens nur für lizenzierte Anbieter gedacht. API Tokens müssen typischerweise alle 90 Tage oder so erneuert werden.**
**Welche Bank?**

- Automatisierung der Eintragung von Bestelliste in das System bzw die Profile
**Ist damit gemeint von einer vorhanden Excel/Google Docs Liste importieren? genau nachfragen**
liste folgt einem gewissen format (siehe bsp von luzia); import und automatische ansicht von allen transaktionen. Dann bestätigen lassen von finanz-user im frontend
gerade nur namen; matching anhand von namen?

(optional - aber schon wahrscheinlich)

- Übersicht: bestellt / offen / bezahlt 👍

## Automatische Benachrichtigungen & Reminder
- Automatische Info bei Statusänderungen per Email (z. B. „bezahlt“) 👍
Wichtig auf Slack, Unwichtig auf Email
Email => notification für statusänderung, bestätigung für einreichung
Slack => bitte zahl jetzt

- Benachrichtigung an Mitglied ( optional ans Finanzteam) 👍
- Automatische Erinnerungen bei offenen Zahlungen nach z.B. 10 Tagen („Inkasso“) Versand idealerweise über Slack, sonst Email
**👍 Ja, aber da müsst ihr auch regelmäßig die Banktransaktionen dann in die Software laden weil sonst die Mitglieder Benachrichtigungen bekommen obwohl sie schon bezahlt haben**
**Man könnte reminder auch schicken beim upload von neuen bankdaten; wenn dann zahlung noch offen und > X tage her ist. So könnte man "falsche notification" umgehen.**

auf knopfdruck benachrichtigung. Nicht automatisch aber evtl übersicht über länger ausstehende bezahlungen (rotes icon oder so)

**Bitte nochmal genau sagen ob lieber slack oder email. Ist bisschen inkonstant in den Fragen. Ging auch beides wenn ihr wollts**

## Kinda obvious: Finanzteam-Workflow / Admin-Ansicht
- Übersicht aller Einreichungen sowie aller offenen Beträge von Mitgliedern inkl Search Funktion mit Suchen nach Händler, Betrag, Datum etc. 👍
- Bearbeitung / Prüfung / Freigabe / Auszahlung bestätigen 👍
- Status ändern und nachvollziehbar dokumentieren 
**👍 Wollt ihr da dass man eine Begründung oder sowas bei Statusänderung schreiben soll? Oder reicht einfach wenn getracked wird wanns geändert wurde.**
**change-tracking. Wichtig für domain-model**

bei ablehnung Begründung; in prüfung status braucht es nicht; in Bearbeitung bei rückfragen; gezahlt oder abgelehnt; 
von team an user: offen/bezahlt (keine extra kommunikationsmöglichkeit weil selten genutzt. Sollen direkt auf slack einfach schreiben wenn probleme; kleines info feld dazu?)

- Möglichkeit für Kommentare oder Rückfragen
**Ist damit gemeint Kommentare von User zu Finanzteam oder Finanzteam zu User? Was soll dann genau passieren? Soll eine E-Mail bzw. Slack-Nachricht kommen? Soll in der Software ein 
Kommunikationsweg (z.b. Chat) sein? Das wäre ein deutlicher Mehraufwand**

- Tatsächlicher Überweisungszweck nach Auszahlung erfassen/Eintragen für Ablage
**Bin mir nicht ganz sicher was damit gemeint ist. Pls explain :)**

wenn status auf bezahlt -> dann muss genauer überweisungszweck eingetragen werden. -> das bild der rechnung wird mit dem passenden überweisungszweck auf drive hochgeladen
(filtern auf paytrack geht aber auf jeden fall doppelt sichern auf z.b. drive)

wir brauchen noch google account für paytrack generell

- Export als pdf, excel idk für Reporting, Analysen etc. 👍
letzten monate, letztes jahr, etc alle rechnung exportieren zum überprüfen (export button in übersicht)

## Automatische Ablage / Archivierung:
- nach Bestätigung wird die Rechnung automatisch mit korrektem Überweisungszweck gespeichert/abgelegt
**Welche Bestätigung? Vom User dass er bezahlt hat? Vom Finanzteam dass die Rechnung bezahlt wurde?**

- strukturierte, nachvollziehbare Archivierung (z. B. nach Jahr/Monat/Name/Status) 👍
rechnung sind nach foldern unterteilt also z.b. 2026/März

- aktuell passiert das in Google Drive → Ziel wäre, dies beizubehalten
**Was soll genau wie gespeichert werden? Nur die Rechnungen oder noch etwas dazu?**

- Abgleich mit Bankexport ob alle Ausgangs- und Eingangsrechnungen richtig abgelegt/vorhanden sind (Fuzzy matching)
**Wollt ihr nicht evtl überhaupt die Bsetätigung einfach über Bankexporte machen? Oder ist damit gemeint dass man ein "Verifizierungs"-Option hat bei der man nochmal alles was eh schon gespeichert ist abgleichen kann?**

## App oder Mobile Ansicht
- Möglichkeit Payback direkt am Handy zu öffnen und mit der Kamera in der App Fotos zu machen und hochzuladen.
**Reicht es wenn man die Webseite übers Handy aufruft? Eine eigene App nochmal dazu wäre ein deutlicher Mehraufwand**

auf jeden fall mobile-friendly
*optional*:
webseite als browser-lesezeichen als app (Progressive Web App) - unique feature?

am handy auf bild machen klicken anschauen; auf jeden fall bild upload von handy

## Verknüpfung mit Budget (?)
- Automatische Übertragung ins Budget
**Ist theoretisch möglich. Ist aber vermutlich relativ fehleranfällig. Wir könnten einfach in unsere Anwendung eine Finanzübersicht einbauen und daraus könnten ihr dann die Werte übertragen. Ehrlicherweise: Wie viele Werte sind das? Weil wenns nur nach Monat oder so aufgeteilt ist dann wäre das nicht der Riesenaufwand. Ich würde generell ungern direkt in euer Budget Excel schreiben da evtl. irgendwo bei uns ein Fehler sein könnte und wir dann dadurch euer Budget manipulieren.**
**(nicht dass ich denke dass es passiert aber safety-first)**

- Hervorhebung von Rechnungen die laut Projektplan schon fällig gewesen wären
**Wie ist das genau gemeint? Wie definiert man dass etwas lt Projektplan schon fällig gewesen sein sollte? Gibts hier einen unterschied zu "Übersicht aller offenen Beiträge/Rechnungen?"**
**Höchsten due date und anzeigen ob abgelaufen.**

- Abweichungen Plan/Ist
**Lesen wir das aus dem Excel aus? Das würde alles generell viel mehr in eine Richtung Finanztool dann noch extra gehen. Wenn wir so etwas machen sollten wäre es sinnvoller einfach generell vom Google Sheets/Excel wegzugehen und das sauber in einer Applikation zu implementieren.**

- Bereich für Leiter, um das Budget einzusehen (eingeplant, noch offen) und Änderungen anzufragen (?)
**same here. Es wäre keine besonders saubere Architektur wenn die Hälfte unserer "Datenbank" euer Excel sheet ist.**

## Implementierung KPIs (falls wer Lust hat sich mit sowas zu beschäftigen)
- Analyse von KPIs anhand von den gezahlten Rechnungen und bekommenen Zahlungen und Budget inkl. Dashboard mit Übersicht
**Wir könnten theoretisch Rechnungen, etc. kategorieren und dann ein Dashboard basteln bei dem ihr die Ein/Ausgaben pro Kategorie seht nur dann müsst ihr halt auch bei jeder Rechnung, etc. angeben zu welcher Kategorie es gehört**

paytrack.tuwr.at
