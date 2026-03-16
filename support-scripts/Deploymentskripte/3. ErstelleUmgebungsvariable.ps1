# 2025-07-23
# Dimitri Khodak
# Erstellen der Umgebungsvariable

# Automatische Fehlerausgabe abschalten.
#$ErrorActionPreference = "SilentlyContinue"

$variableName = "SSIS_DbReplicatorConfigFilePath"
$variableValue = "F:\DbReplicator\replicateDb.dtsConfig"

try{
    PrintAction -str_Action_Descr "Start 'ErstelleUmgebungsvariable'." `
                -bool_AskToProceed $false `                -bool_clsBefore $true    

    # Pfad zur System-Umgebungsvariablen in der Registrierung
    $regPath = "HKLM:\System\CurrentControlSet\Control\Session Manager\Environment"

    Set-ItemProperty -Path $regPath -Name $variableName -Value $variableValue

    [System.Environment]::SetEnvironmentVariable($variableName, $variableValue, [System.EnvironmentVariableTarget]::Machine)

    printAction -str_Action_Descr ("System-Umgebungsvariable für Konfigurationsdateipfad erstellt oder aktualisiert`r`nName: {0} `r`nWert: {1}" -f $variableName, $variableValue) `                -bool_AskToProceed $true `                -bool_clsBefore $false
}
catch 
{
    printError -str_Error_Descr "Fehler: $_" -bool_AskToProceed $true
}