/*
Beschreibung: Löschen aller bestehenden Verbindungen zur angegebenen Datenbank.
Autor: Dimitri Khodak
Erstellungsdatum: 12.12.2024
Datum letzter Änderung: 12.12.2024
*/

USE master
GO
DECLARE @dbname SYSNAME = 'DekaImmoBranchWartungX'
DECLARE @spid SMALLINT
DECLARE @username varchar(max) = 'b025889X'

-- Auswählen der kleinsten Server-Process-ID (spid) mit Bezug auf spezifizierte Datenbank.
SELECT 
	@spid = MIN(spid) 
FROM 
	master.dbo.sysprocesses
WHERE 
	dbid = DB_ID(@dbname)
AND
	nt_username = @username

-- Lösche Process mit der spid und wähle nächsthöhere spid aus.
-- Wiederhole solange spid vorhanden.
WHILE @spid IS NOT NULL
	BEGIN
		Execute ('Kill ' + @spid)
		SELECT 
			@spid = min(spid) 
		FROM 
			master.dbo.sysprocesses
		WHERE 
			dbid = db_id(@dbname) 
		AND 
			spid > @spid
		AND
			nt_username = @username
	END