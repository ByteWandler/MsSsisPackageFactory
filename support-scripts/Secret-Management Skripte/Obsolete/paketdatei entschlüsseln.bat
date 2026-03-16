@echo off
setlocal

:: Pfad zur DTSX-Datei
set "dtsxFile=replicateDb.dtsx"

:: Pfad zur neuen DTSX-Datei
set "newDtsxFile=replicateDbNew.dtsx"

cd /d %~dp0
echo Lege aktuellen Pfad auf '%cd%' fest...

:: Passwort abfragen
set /p password="Geben Sie das Package-Passwort ein: "

:: DTSX-Paket entschlüsseln und speichern
dtutil /FILE "%dtsxFile%" /DECRYPT %password% /ENCRYPT FILE;%newDtsxFile%;0

if %ERRORLEVEL% EQU 0 (
    echo Die Datei wurde erfolgreich entschluesselt.
) else (
    echo Fehler beim Entschluesseln der Datei.
)
pause
endlocal
