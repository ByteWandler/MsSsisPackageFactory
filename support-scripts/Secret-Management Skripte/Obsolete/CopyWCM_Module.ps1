$source = "\\servername\F$\servernameDbReplicate\CredentialManager"
$destination = "C:\Program Files\WindowsPowerShell\Modules\CredentialManager"

if (Test-Path -Path $destination) {
    Write-Host "WindowsCredentialManager Powershell-Modul existiert bereits."
} else {
    Copy-Item -Path $source -Destination $destination -Recurse
    Write-Host "WindowsCredentialManager erfolgreich kopiert."
}
Read-Host