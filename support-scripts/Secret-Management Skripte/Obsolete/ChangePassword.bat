@echo off
setlocal

:: Pfad zur DTSX-Datei
set "dtsxFile=replicateDb.dtsx"
:: Pfad zur neuen DTSX-Datei
set "newDtsxFile=replicateDb.dtsx"

cd /d %~dp0
echo Lege aktuellen Pfad auf '%cd%' fest...

:: Benutzer nach dem alten Passwort fragen
set /p oldPassword="Geben Sie das alte Passwort ein: "

:: Benutzer nach dem neuen Passwort fragen
set /p newPassword="Geben Sie das neue Passwort ein: "

:: DTSX-Paket entschlüsseln und mit neuem Passwort verschlüsseln
dtutil /FILE "%dtsxFile%" /DECRYPT %oldPassword% /ENCRYPT FILE;%newDtsxFile%;3;%newPassword%

if %ERRORLEVEL% EQU 0 (
    echo Das Passwort wurde erfolgreich geaendert.
) else (
    echo Fehler beim Aendern des Passworts.
)
pause
endlocal
