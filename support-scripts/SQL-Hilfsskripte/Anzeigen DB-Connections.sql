/*
Beschreibung: Anzeigen aller bestehenden Verbindungen zur angegebenen Datenbank.
Autor: Dimitri Khodak
Erstellungsdatum: 12.12.2024
Datum letzter Änderung: 12.12.2024
*/

USE master
GO
DECLARE @dbname SYSNAME = 'DekaImmoBranchWartung'
DECLARE @spid SMALLINT
DECLARE @username varchar(max) = 'b025889'

-- Auswählen der kleinsten Server-Process-ID (spid) mit Bezug auf spezifizierte Datenbank.
SELECT 
	@spid = MIN(spid) 
FROM 
	master.dbo.sysprocesses
WHERE 
	dbid = DB_ID(@dbname)
AND
	nt_username = @username

-- Anzeigen Process mit der spid und wähle nächsthöhere spid aus.
-- Wiederhole solange spid vorhanden.
WHILE @spid IS NOT NULL
	BEGIN
		PRINT 'spid: ' + CONVERT(VARCHAR(MAX), @spid);
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

--SELECT * FROM master.dbo.sysprocesses where dbid = db_id('DekaImmoBranchWartung')