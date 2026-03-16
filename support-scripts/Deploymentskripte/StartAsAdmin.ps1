# 2025-05-21
# Dimitri Khodak
try{
    $mainScript = "0. Hauptskript.ps1"
    $rootDir = $PSScriptRoot
    $fullMainscriptPath = ($rootDir + [IO.Path]::DirectorySeparatorChar + $mainScript)
    
    Write-Output "`r`n"
    Write-Output "Execution Variables:"
    Write-Output "mainSscript: `t`t`"$mainScript`""
    Write-Output "rootDir: `t`t`"$rootDir`""
    Write-Output "fullMainscriptPath: `t`"$fullMainscriptPath`""
    Write-Output "`r`n"

    # Funktion zum Starten eines Skripts als Administrator
    function Start-ScriptAsAdmin {
        param ([string]$scriptPath)

        # Erstellen des StartInfo-Objekts
        $startInfo = New-Object System.Diagnostics.ProcessStartInfo
        $startInfo.FileName = "powershell.exe"
        $startInfo.Arguments = "-File `"$scriptPath`""
        $startInfo.Verb = "RunAs"
        $startInfo.UseShellExecute = $true
        
        Write-Output ("Starting Process: `"$scriptPath`" ...")

        # Starten des Prozesses
        [System.Diagnostics.Process]::Start($startInfo)
    }


    # Falls User des aktuell ausgefuehrten Skripts nicht Administrator ist, dann starte das Skript als Admin und beende das Aktuelle
    $CurrentWindowsPrincipal = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent())
    $IsAdmin = $CurrentWindowsPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

    Write-Output "Checking Admin-rights ..."
    Write-Output "User has Admin-Rights: $IsAdmin"
    Write-Output "`r`n"

    if (-not $IsAdmin) {
        Write-Output "Restart with admin-rights ..."
        Write-Output "Press to conitnue ..."
        Read-Host
        Start-ScriptAsAdmin -scriptPath $PSCommandPath

        exit
    }

    # Ansonsten, starte das Hauptskript
    Write-Output "Starting `"$fullMainscriptPath`" ..."
    Write-Output "Press to conitnue ..."
    Read-Host
    Start-ScriptAsAdmin -scriptPath $fullMainscriptPath
}
catch {
    $scriptName = $MyInvocation.MyCommand.Name
    Write-Output "`r`n"
    Write-Output "FEHLER in => `"$scriptName`" "
    Write-Output "`r`n"
    Write-Host $Error[0].Exception
    Write-Output "`r`n"

    # Schliesse Konsole nur nach Eingabe
    Write-Output "`r`n"
    Write-Output "`r`n"
    Write-Host "Drücken Sie eine beliebige Taste, um die Konsole zu schließen..."
    Read-Host
}