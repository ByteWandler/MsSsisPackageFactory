# 2025-07-23
# Dimitri Khodak
# Dieses Hauptskript führt alle erforderlichen Skripte aus

# Automatische Fehlerausgabe abschalten.
$ErrorActionPreference = "SilentlyContinue"

try
{
    # Aktuellen Arbeitspfad auf das des ausgefuehrten Skripts festlegen (Bei Admin-Nutzer ggf. nicht standardmäßg zutreffend)
    $currentWorkingPath = $PSScriptRoot
    Set-Location -Path $currentWorkingPath

    Write-Output ("CurrentWorkingPath set to: {0}" -f $currentWorkingPath)

    Import-Module -Name "$currentWorkingPath\IO-Functions.psm1"
    if (Get-Module IO-Functions)
    {
        PrintAction -str_Action_Descr "IO-Functions imported" -bool_AskToProceed $false -bool_clsBefore $false
    }

    Import-Module -Name "$currentWorkingPath\SqlServer" -Verbose
    if (Get-Module SqlServer)
    {
        PrintAction -str_Action_Descr "SqlServer-Modul imported" -bool_AskToProceed $false -bool_clsBefore $false
    }
        
    PrintAction -str_Action_Descr "Starting file transfer ..." -bool_AskToProceed $true -bool_clsBefore $false
     . '.\1. Übertrage Quelldateien.ps1'

     PrintAction -str_Action_Descr "Starting sql-server configuration ..." -bool_AskToProceed $true -bool_clsBefore $false    
     . '.\2. CreateSqlServerUser.ps1'

    PrintAction -str_Action_Descr "Start to create environment variable ..." -bool_AskToProceed $true -bool_clsBefore $false
     . '.\3. ErstelleUmgebungsvariable.ps1'

}
catch
{
    Write-Host $_ -ForegroundColor Red
    Write-Output "`r`n"
    Read-Host
}
