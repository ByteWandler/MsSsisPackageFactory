New-StoredCredential -Target "ReplicateDbCred" -UserName "Dimitri\Dimitri" -Password "Passwort#1234"

#$credentials = Get-StoredCredential -Target "ReplicateDbCred"

#$username = $credentials.UserName
#$klartextpw = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($credentials.Password))


#Write-Host $username
#Write-Host $klartextpw
Read-Host