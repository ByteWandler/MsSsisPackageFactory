# Microsoft SSIS-Package-Factory

## Features
- **Dynamische Package-Generierung bei Schema-Änderungen**
    - **Für Datenbankmigration relationaler MS SQL-Server-DBs**
    - **Z. B. Synchronisation der Datenbank zwischen Prod- & Non-Prod-Umgebung**
- **Individuell konfigurierbare Anonymisierung (Datenschutz)**
- **Individuell konfigurierbare Übertragungsinhalte (Performanz + Datenschutz)**
- **On-Prem / SSIS**

## Beschreibung
Dieses Projekt ist eine Basisversion einer SSIS-Package-Factory für die Übertragung und individuellen Anonymisierung von relationalen MS SQL-Server Datenbanken. Ihr Ziel ist es eine einfache Form der Dynamik gewährleisten, indem bei strukturellen Änderungen der Quell-DB, anstelle einer manuellen Anpassung des SSIS-Packages, ein neues Package generiert werden kann, passend zu der jeweiligen Datenbankstruktur. Die Factory funktioniert Template-basiert. Auf Basis von Benutzerkonfiguration über eine Konfigurationsdatei und den DB-Metadaten wird ein neues SSIS-Packages generiert. Die Factory baut zur Laufzeit somit eine Verbindung zur Zieldatenbank auf.

Es stellt eine Alternative zu BIML und Microsoft Fabric, unter Beschränkung auf die Übertragung relationaler MS SQL-Server DBs. Aufgrund dieser Beschränkung ist die Nutzung einfacher als bei BIML und es ist für OnPremisses-Verwendung geeignet im Gegensatz zu Microsoft Fabric.

Anonymisierung: Aktuell ist die Anonymisierung nur als Maskierung möglich.

## Projektaufbau
Das Projekt enthält die zwei Teilprojekte _MsSsisPackageFactory_ und _MsSsisPackageFactoryTemplate_. Ersteres enthält das eigentliche Projekt. Zweites enthält ein SSIS-Projekt als Tool zur Unterstützung der Entwicklung.

## Installation
Für die Ausführung des Tools ist .NET 9.0 als Zielframework nötig.

### Für die Ausführung in der Entwicklungsumgebung
Erforderliche Tools: 
- Visual Studio
- SSDT-Erweiterung
- Ms SQL-Server (Mindestens _Standard-Edition_. Auch _Standard-Developer_. Nicht in _Express_, _Express With Advanced Services_ oder _Web_)

### Für die Ausführung außerhalb der Entwicklungsumgebung
Erforderliche Tools: 
- Ms SQL-Server (Mindestens _Standard-Edition_. Auch _Standard-Developer_. Nicht in _Express_, _Express With Advanced Services_ oder _Web_)

## Anwendung
Für die Verwendung der Factory ist es erforderlich den Pfad zur Template Datei _'TemplateFileName'_ und den Outputfad _'OutputDirectory'_ in der Konfigurationsdatei _'FactoryConfig.xml'_ festzulegen. Ebenso muss der ConnectionString und das Datenbankschema für die DB festgelegt werden, die als Datenquelle fungiert. 

Optional können einzelne Tabellen festgelegt werden, die durch das Package von der Übertragung ausgeschlossen werden sollen mit _'<Table\>'_ unter '_<ExcludedTables\>_'. Zudem können auch Anonymisierungsregeln festgelegt werden, falls Spalten anonymisiert werden sollen. (Siehe Beispiele in _FactoryConfig.xml_)

Die Anwendung erfolgt durch Ausführung der Executable-Datei (.exe), z. B. durch Doppelklick. Es handelt sich hierbei um eine .NET-Konsolenanwendung. Für die Ausführung muss eine Verbindung zur Quelldatenbank aufgebaut werden können. Das generierte SSIS-Package wird im festgelegten _OutputPath_ abgelegt.

Die Verwendung des generierten Packages selbst erfolgt entsprechend mit der SSIS-Engine.

## Tests
Das Tool _MsSsisPackageFactory_ wurde mit Microsoft Visual Studio Community 2022 (64-Bit) Version 17.14.26 und .NET 9.0 getestet.

Das generierte SSIS-Package wurde zusätzlich mit SSDT (SQL-Server Data Tools) Version 2.1.2 (Preview) in Visual Studio und SQL Server-Paketausführungsprogramm (dtexec.exe) Version 17.0.1000.7 for 64-bit (unter Microsoft SQL Server Standard - Developer Edition (64-bit) Version 17.0.1050.2) erfolgreich getestet. Für den Test wurde die [NorthWind-Database](https://github.com/microsoft/sql-server-samples/blob/master/samples/databases/northwind-pubs/readme.md) von Microsoft verwendet.

## Project status
Ausstehend sind noch u. a. Unittests, Logging und umfänglichere Fehlerbehandlung. 

DB-Tabellenschemata müssen korrigiert werden. Sie sollen nicht in der Konfiguration festgelegt werden, sondern aus den DB-Metadaten abgerufen werden.
