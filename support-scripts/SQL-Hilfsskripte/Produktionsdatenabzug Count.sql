/*
Beschreibung: Anzeigen aller bestehenden Verbindungen zur angegebenen Datenbank.
Autor: Dimitri Khodak
Erstellungsdatum: 12.12.2024
Datum letzter Änderung: 01.01.2025
*/

USE DekaImmoBranchWartung

SELECT 
--(SELECT COUNT(*) FROM Gutachten) AS Gutachten,
--(SELECT COUNT(*) FROM MietfreieZeit) AS MietfreieZeit,
(SELECT COUNT(*) FROM RegelmaessigeZahlung) AS RegelmaessigeZahlung,
(SELECT COUNT(*) FROM Mietvertrag) AS Mietvertrag,
(SELECT COUNT(*) FROM Mietflaeche) AS Mietflaeche