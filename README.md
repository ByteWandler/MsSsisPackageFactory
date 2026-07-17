# Microsoft SSIS-Package-Factory
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![SSIS](https://img.shields.io/badge/SSIS-Compatible-F96302?logo=microsoftsqlserver&logoColor=white)](https://learn.microsoft.com/sql/integration-services/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-Standard%20%7C%20Developer-0078D4?logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server/)
[![On-Premises](https://img.shields.io/badge/On--Premises-Ready-00A4EF?logo=microsoft&logoColor=white)](https://learn.microsoft.com/sql/integration-services/)

*Eine metadatengesteuerte Pipeline-Factory zur hochdynamischen ETL-Automatisierung, Paketgenerierung und datenschutzkonformen Geschäftsdatenbereitstellung im Microsoft-Ökosystem.*

## 📑 Inhalt
[Business-Value & Projekt-Überblick](#-business-value--projekt-überblick)

[Technische Projektbeschreibung](#-technische-projektbeschreibung)

&nbsp;

## 💼 Business-Value & Projekt-Überblick
### 💡 Kernvorteile für User:
* **Zeitersparnis:** Automatisierte Generierung statt händischer Erstellung oder Anpassung von SSIS-Paketen.
* **Erpart manuelle Routineaufgaben:** Manuelle Anpassungen der Datenpipelines bei Änderungen an der Datenquelle entfallen durch die Automatisierung.
* **Reduziertes Fehlerrisiko:** Jede manuelle Code-Anpassung erhöht das Risiko Fehler einzubauen.
* **Compliance & Datenschutz:** Integrierte konfigurierbare Mechanismen zur Anonymisierung sensibler Produktionsdaten für den sicheren Einsatz in Nicht-Produktivumgebungen.
* **Dynamisches Schema-Management:** Automatische Anpassung an Quell- und Zielstrukturen ohne manuellen Refactoring-Aufwand der Pipeline.
* **Vollautomatisierte Nutzung:** Durch die mögliche Integration in die Pipeline-Umgebung, ist das Tool vollautomatisiert nutzbar.
* **Einfache Nutzung als Enduser:** Die Nutzung erfordert keine Einrichtgung von Entwicklungsumgebungen oder der Kenntnis gesonderter Sprachen, wie es bei BIML der Fall ist. Die Steuerung erfolgt durch eine einfache Konfigurationsdatei.

&nbsp;

### 🛠️ Allgemeine Funktionsweise

Das Tool agiert als dynamische Fabrik (*Factory*), die Metadaten der Datenbank einliest und daraus SSIS-Pakete erzeugt.

1. **Input 1:** Die datenbank-spezifischen Metadaten werden von der spezifizierten Datenbank abgerufen, z. B. vorhandene Tabellen, Spalten oder Datentypen.
2. **Input 2:** In der Konfigurationsdatei werden Mapping-Regeln und Anonymisierungsvorgaben festgelegt, z. B. Tabelle X Spalte Y auslassen oder Inhalt maskieren.
3. **Input 3:** Eine generische Template-Datei der SSIS-Package-Datei enthält die Basislogik für die zu erzeugenden SSIS-Pakete.
4. **Verarbeitung:** Die *Factory-Engine* generiert die generischen Teile des Templates auf Basis der Metadaten.
5. **Output:** Ein SSIS-Paket (Datei), das mit der gegebenen Datenbank vollständig kompatibel ist.

___

&nbsp;

## 🛠️ Technische Projektbeschreibung
### 📖 Beschreibung:
Dieses Projekt ist eine Basisversion einer SSIS-Package-Factory für die Übertragung und individuellen Anonymisierung von relationalen MS SQL-Server Datenbanken. Ihr Ziel ist es eine einfache Form der Dynamik gewährleisten, indem bei strukturellen Änderungen der Quell-DB, anstelle einer manuellen Anpassung des SSIS-Packages, ein neues Package generiert werden kann, passend zu der jeweiligen Datenbankstruktur. Die Factory funktioniert Template-basiert. Auf Basis von Benutzerkonfiguration über eine Konfigurationsdatei und den DB-Metadaten wird ein neues SSIS-Packages generiert. Die Factory baut zur Laufzeit somit eine Verbindung zur Zieldatenbank auf.

Es stellt eine Alternative zu BIML und Microsoft Fabric, unter Beschränkung auf die Übertragung relationaler MS SQL-Server DBs. Aufgrund dieser Beschränkung ist die Nutzung einfacher als bei BIML und es ist für OnPremisses-Verwendung geeignet im Gegensatz zu Microsoft Fabric.

> [!NOTE]
> Die Anonymisierung ist derzeit auf Maskierung beschränkt. Weitere Verfahren (z. B. Hashing, Pseudonymisierung) sind in Planung.

&nbsp;

### 🗂️ Projektstruktur:
Das Projekt enthält die zwei Teilprojekte _MsSsisPackageFactory_ und _MsSsisPackageFactoryTemplate_. 
Ersteres enthält das eigentliche Projekt. Zweites enthält ein SSIS-Projekt als Tool zur Unterstützung der Entwicklung.
*support-scripts* enthält nützliche Code-Snippets für die Entwicklung.

&nbsp;

### 📦 Installation:
Für die Ausführung des Tools ist .NET 9.0 als Zielframework nötig.

#### Für die Ausführung in der Entwicklungsumgebung
Erforderliche Tools: 
- Visual Studio
- SSDT-Erweiterung
- Ms SQL-Server (Mindestens _Standard-Edition_. Auch _Standard-Developer_. Nicht in _Express_, _Express With Advanced Services_ oder _Web_)

#### Für die Ausführung außerhalb der Entwicklungsumgebung
Erforderliche Tools: 
- Ms SQL-Server (Mindestens _Standard-Edition_. Auch _Standard-Developer_. Nicht in _Express_, _Express With Advanced Services_ oder _Web_)

&nbsp;

### ▶️ Ausführung:
Für die Verwendung der Factory ist es erforderlich den Pfad zur Template Datei _'TemplateFileName'_ und den Outputfad _'OutputDirectory'_ in der Konfigurationsdatei _'FactoryConfig.xml'_ festzulegen. Ebenso muss der ConnectionString und das Datenbankschema für die DB festgelegt werden, die als Datenquelle fungiert. 

Optional können einzelne Tabellen festgelegt werden, die durch das Package von der Übertragung ausgeschlossen werden sollen mit _'<Table\>'_ unter '_<ExcludedTables\>_'. Zudem können auch Anonymisierungsregeln festgelegt werden, falls Spalten anonymisiert werden sollen. (Siehe Beispiele in _FactoryConfig.xml_)

Die Anwendung erfolgt durch Ausführung der Executable-Datei (.exe), z. B. durch Doppelklick. Es handelt sich hierbei um eine .NET-Konsolenanwendung. Für die Ausführung muss eine Verbindung zur Quelldatenbank aufgebaut werden können. Das generierte SSIS-Package wird im festgelegten _OutputPath_ abgelegt.

Die Verwendung des generierten Packages selbst erfolgt entsprechend mit der SSIS-Engine.

&nbsp;

### ✅ Tests:
Das Tool _MsSsisPackageFactory_ wurde mit Microsoft Visual Studio Community 2022 (64-Bit) Version 17.14.26 und .NET 9.0 getestet.

Das generierte SSIS-Package wurde zusätzlich mit SSDT (SQL-Server Data Tools) Version 2.1.2 (Preview) in Visual Studio und SQL Server-Paketausführungsprogramm (dtexec.exe) Version 17.0.1000.7 for 64-bit (unter Microsoft SQL Server Standard - Developer Edition (64-bit) Version 17.0.1050.2) erfolgreich getestet. Für den Test wurde die [NorthWind-Database](https://github.com/microsoft/sql-server-samples/blob/master/samples/databases/northwind-pubs/readme.md) von Microsoft verwendet.

&nbsp;

### 📋 Projektstatus & ToDos:
> [!NOTE]
> Status: Minimal Viable Project (MVP)

Ausstehend sind noch u. a. Unittests, Logging und umfänglichere Fehlerbehandlung. 

DB-Tabellenschemata müssen korrigiert werden. Sie sollen nicht in der Konfiguration festgelegt werden, sondern aus den DB-Metadaten abgerufen werden.

#### ToDos
- Erweiterung der individuellen Anonymisierung. (Aktuell nur masking)
- Adapter/Connection-Manager für Nicht-MS SQL-Server-Datenbanken

&nbsp;

### 📄 Lizenz
Dieses Projekt ist unter der **GNU General Public License v3.0** lizenziert – siehe die [LICENSE](LICENSE) Datei für Details.

