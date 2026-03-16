echo "This is dtexec.bat"
:loop
echo Drücken Sie [Q] und dann [Enter], um die Batch-Datei zu schließen.
set /p input=Eingabe: 
if /i "%input%" neq "Q" goto loop

set /p input=Eingabe: