# 2025-07-23
# Dimitri Khodak
# Pfad anlegen, Dateien uebertragen & Pfadberechtigungen festlegen

$sourcePath = "$currentWorkingPath\source"
$destinationPath = "F:\GuliverDbReplicator"

# Zu übertragende Dateien
$dtsxFile = "replicateDb.dtsx" 
$configFile = "replicateDb.dtsConfig" 
$dtexecBatch = "StartDbReplicator.bat"

$userName = "DOMAIN\USERNAME"


function CreatePath()
{
    if (Test-Path $destinationPath) 
    {    
        throw "Die Dateien konnten nicht kopiert werden. Der Pfad '$destinationPath' existiert bereits und muss zuerst geloescht werden."
        exit
    } 

    # Zielpfad anlegen
    New-Item -ItemType Directory -Path $destinationPath -Force

    PrintAction -str_Action_Descr "Der Pfad wurde erfolgreich angelegt." `
                -bool_AskToProceed $false `                -bool_clsBefore $false
}

function CopyFiles()
{    
    # Dateien uebertragen
    Copy-Item -Path ($sourcePath + [IO.Path]::DirectorySeparatorChar + $dtsxFile) -Destination ($destinationPath + [IO.Path]::DirectorySeparatorChar + $dtsxFile)
    Copy-Item -Path ($sourcePath + [IO.Path]::DirectorySeparatorChar + $configFile) -Destination ($destinationPath + [IO.Path]::DirectorySeparatorChar + $configFile)
    Copy-Item -Path ($sourcePath + [IO.Path]::DirectorySeparatorChar + $dtexecBatch) -Destination ($destinationPath + [IO.Path]::DirectorySeparatorChar + $dtexecBatch)
    
    PrintAction -str_Action_Descr "Die Dateien wurden erfolgreich uebertragen." `
                -bool_AskToProceed $false `                -bool_clsBefore $false
}

function SetPermissions()
{
    # Zugriffsberechtigungen setzen
    $acl = Get-Acl $destinationPath
    $identityReference = [System.Security.Principal.NTAccount]$userName
    $permission = "FullControl"
    $inheritanceFlags = [System.Security.AccessControl.InheritanceFlags]::ContainerInherit, [System.Security.AccessControl.InheritanceFlags]::ObjectInherit
    $propagationFlags = [System.Security.AccessControl.PropagationFlags]::None
    $accessControlType = [System.Security.AccessControl.AccessControlType]::Allow

    $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule($identityReference, $permission, $inheritanceFlags, $propagationFlags, $accessControlType)
    $acl.SetAccessRule($accessRule)
    Set-Acl -Path $destinationPath -AclObject $acl
    
    PrintAction -str_Action_Descr "Zugriffsberechtigungen wurden erfolgreich festgelegt." `
                -bool_AskToProceed $false `                -bool_clsBefore $false
}


try {
    PrintAction -str_Action_Descr "Start 'uebertrage quelldateien'." `
                -bool_AskToProceed $false `                -bool_clsBefore $true    

    CreatePath

    CopyFiles

    SetPermissions

    PrintAction -str_Action_Descr "File transfer succeeded." `
                -bool_AskToProceed $true `                -bool_clsBefore $false
    
    Start-Process explorer.exe $destinationPath
}
catch 
{
    PrintError -str_Error_Descr "Fehler: $_" -bool_AskToProceed $true
}
