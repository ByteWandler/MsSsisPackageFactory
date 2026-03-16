# 2025-07-23
# Dimitri Khodak
# SQL-Server: Login erstellen, DB-User & Log-DB-User erstellen & berechtigen

$serverInstance = "servername, 2513" # "servername, port"
$databaseName = "DATABASENAME" # Quell-DB
$loginName = "DOMAIN\USERNAME"


function CreateServerLogin()
{
    $createLoginQuery = "CREATE LOGIN [$loginName] FROM WINDOWS"

    Invoke-Sqlcmd -ServerInstance $serverInstance -Query $createLoginQuery -TrustServerCertificate
    PrintAction -str_Action_Descr "Server Login erstellt." -bool_AskToProceed $false -bool_clsBefore $false
}

function CreateDbUserAndRights()
{
    $createUserQuery = "USE [$databaseName];" +
                       "CREATE USER [$loginName] FOR LOGIN [$loginName];" +
                       "ALTER ROLE [db_datareader] ADD MEMBER [$loginName];"

    Invoke-Sqlcmd -ServerInstance $serverInstance -Query $createUserQuery -TrustServerCertificate
    PrintAction -str_Action_Descr "DB-Nutzer für Primaer-DB erstellt." -bool_AskToProceed $false -bool_clsBefore $false
}

# Benutzer für die Log-Datenbank erstellen und Rechte zuweisen
function CreateLogDbUserAndRights()
{
    $logdatabaseName = "DekaImmo_Logs"
    
    $createLogUserQuery = "USE [$logdatabaseName]; " +
                          "CREATE USER [$loginName] FOR LOGIN [$loginName]; " +
                          "ALTER ROLE [db_datawriter] ADD MEMBER [$loginName]; "

    Invoke-Sqlcmd -ServerInstance $serverInstance -Query $createLogUserQuery -TrustServerCertificate
    PrintAction -str_Action_Descr ("DB-Nutzer für Log-DB erstellt.") -bool_AskToProceed $false -bool_clsBefore $false
}

try
{
    PrintAction -str_Action_Descr "Start 'createSqlServerUser'." `
                -bool_AskToProceed $false `                -bool_clsBefore $true    

    CreateServerLogin

    CreateDbUserAndRights

    CreateLogDbUserAndRights

    printAction "SqlServer configuration succeeded." $true $false

}
catch
{
    PrintError "Fehler: $_" $false
    Write-Host $_.ScriptStackTrace
    Write-Host $_.Exception
    Write-Host $_.
        Read-host
}



