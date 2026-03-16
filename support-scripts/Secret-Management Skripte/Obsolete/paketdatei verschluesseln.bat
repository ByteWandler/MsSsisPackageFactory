@echo off
setlocal

:: Pfad zur DTSX-Datei
set "dtsxFile=replicateDbNewNew.dtsx"

:: Pfad zur neuen DTSX-Datei
set "newDtsxFile=replicateDbMitPWVerschl.dtsx"

cd /d %~dp0
echo Lege aktuellen Pfad auf '%cd%' fest...

:: Passwort abfragen
set /p newpassword="Geben Sie das neue Package-Passwort ein: "

:: DTSX-Paket verschlüsseln und speichern
dtutil /FILE "%dtsxFile%" /ENCRYPT FILE;%newDtsxFile%;2;%newPassword%

if %ERRORLEVEL% EQU 0 (
    echo Die Datei wurde erfolgreich entschluesselt.
) else (
    echo Fehler beim Entschluesseln der Datei.
)
pause
endlocal
