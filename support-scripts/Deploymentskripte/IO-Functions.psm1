# 2025-07-23
# Dimitri Khodak
# Container fuer Funktionen
# HINWEIS: Nur Funktionen, keine Befehle, definieren. Skript wird über Dot-Sourcing aufgerufen, um Funktionen zu importieren. Befehle würden ausgeführt werden.

function PrintAction($str_Action_Descr, $bool_AskToProceed, $bool_clsBefore)
{ 
    if($bool_clsBefore) { Clear-Host }

    Write-Host $str_Action_Descr
    Write-Output "`r`n"

    if($bool_AskToProceed) { askToProceed }
}

function PrintError($str_Error_Descr, $bool_AskToProceed)
{
    Write-Host $str_Error_Descr -ForegroundColor Red
    Write-Output "`r`n"

    if($bool_AskToProceed) { askToProceed }
}

function askToProceed()
{
    Write-Host "Press to continue ..."
    Write-Output "`r`n"
    Read-Host
}