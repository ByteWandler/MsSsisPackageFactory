$src = Get-ChildItem "\\servername\F$\DbReplicate\replicateDbNew.dtsx"
$dest = "\\servername\F$\DbReplicate\replicateDbNewNew.dtsx"

# Lese packagedatei
$dts = [xml](Get-Content -Path $src.FullName -Encoding UTF8)

$mng = [System.Xml.XmlNamespaceManager]($dts.NameTable)
$mng.AddNamespace("DTS", "www.microsoft.com/SqlServer/Dts")

# Passwort setzen
$dts.SelectSingleNode("/DTS:Executable/DTS:PackageParameters/DTS:PackageParameter[@DTS:ObjectName='Password']", $mng).ChildNodes[0].ChildNodes[0].InnerText = "NeuesPasswort"

$dts.Save($dest)

Write-Output "Passwort wurde erfolgreich geaendert"