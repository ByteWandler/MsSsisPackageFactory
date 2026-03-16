$folgeSkript = "RunAsSvcUser.bat"
$credentials = Get-StoredCredential -Target "ReplicateDbCred"

if ($credentials -eq $null) {
    Write-Host "Anmeldeinformationen nicht gefunden: $credentialName"
    exit 1
}

$username = $credentials.UserName
$password = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($credentials.Password))

Write-Host "Username: $username"
Write-Host "Password: $password"
Read-Host

Start-Process -FilePath $folgeSkript -ArgumentList $username, $password
