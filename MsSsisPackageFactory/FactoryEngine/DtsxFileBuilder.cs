using Microsoft.Data.SqlClient;
using MsSsisPackageFactory.PackageFactoryConfiguration.UserConfiguration;
using MsSsisPackageFactory.PackageFactoryConfiguration.UserConfiguration.Model;
using MsSsisPackageFactory.PackageFactoryConfiguration.DbMetadata;
using MsSsisPackageFactory.PackageFactoryConfiguration.DbMetadata.Model;
using System.Text;
using System.Xml;

namespace MsSsisPackageFactory.FactoryEngine
{
    /// <summary>
    /// Der Aufbau der Hauptmethoden entspricht der Reihenfolge in dem dtsx-Template.
    /// </summary>
    internal class DtsxFileBuilder : IPackageBuilder
    {
        private readonly UserConfigurationModel _configurations;
        private readonly DbMetaData _metadata;
        private readonly XmlDocument _xmlDocument;
        private readonly XmlNamespaceManager _xmlNamespaceManager;

        public DtsxFileBuilder(UserConfigurationModel userConfiguration, DbMetaData dbMetaData)
        {
            this._xmlDocument = new XmlDocument();
            this._configurations = userConfiguration;
            this._metadata = dbMetaData;

            // XML-Namespace hinzufügen
            this._xmlNamespaceManager = new XmlNamespaceManager(this._xmlDocument.NameTable);
            this._xmlNamespaceManager.AddNamespace("DTS", "www.microsoft.com/SqlServer/Dts");
        }

        public void Build()
        {
            this._xmlDocument.Load(this._configurations.TemplateFileName);

            this.WriteCreationDate();
            this.WriteTransferStructureExec();
            this.WriteTransformAndTransferExec();
            this.WriteTransferSqlServerObjectsExec();
            
            this._xmlDocument.Save(CreateNewFileName(this._configurations.TemplateFileName));
        }

        private void WriteCreationDate()
        {
            XmlNode root = _xmlDocument.SelectSingleNode(@"//DTS:Executable[@DTS:refId='Package']", _xmlNamespaceManager);

            root.Attributes["DTS:CreationDate"].Value = DateTime.Now.ToString("M/d/yyyy h:mm:ss tt");
        }

        private string CreateNewFileName(string oldFileName)
        {
            // Get current DateTime with ten millionths of a second accurancy.
            string dateTime = DateTime.Now.ToString("yyyy-MM-dd HHmmssfffffff");

            // Hole Dateinamen.
            string newFilename = Path.GetFileName(oldFileName);

            // Entferne alte Extension.
            newFilename = Path.GetFileNameWithoutExtension(newFilename);

            // Entferne 'Template'.
            newFilename = newFilename.Replace("Template",string.Empty);

            // Füge TimeStamp und Extension an.
            newFilename = $"{newFilename} {dateTime}.dtsx";

            // Erstelle Directory.
            string newDir = $"{Directory.GetCurrentDirectory()}\\{this._configurations.OutputDirectory}";

            if (!Directory.Exists(newDir))
            {
                Directory.CreateDirectory(newDir);
            }

            return $"{newDir}\\{newFilename}";
        }

        private void CreateVariablesForTable(XmlNode variablesNode, string tableName, List<TableColumn> columns)
        {
            string ns = _xmlNamespaceManager.LookupNamespace("DTS");
            // _DestName Variable
            XmlElement destVar = _xmlDocument.CreateElement("DTS:Variable", ns);
            destVar.SetAttribute("CreationName", ns, "");
            destVar.SetAttribute("DTSID", ns, "{" + Guid.NewGuid().ToString().ToUpper() + "}");
            destVar.SetAttribute("EvaluateAsExpression", ns, "True");
            destVar.SetAttribute("Expression", ns, "\"[\" + @[User::DestinationDbName] + \"].dbo." + tableName + "\"");
            destVar.SetAttribute("IncludeInDebugDump", ns, "2345");
            destVar.SetAttribute("Namespace", ns, "User");
            destVar.SetAttribute("ObjectName", ns, tableName + "_DestName");

            XmlElement destValue = _xmlDocument.CreateElement("DTS:VariableValue");
            destValue.SetAttribute("DataType", ns, "8");
            destValue.InnerText = "[].dbo." + tableName;
            destVar.AppendChild(destValue);

            // _SelectCmd Variable
            XmlElement selectVar = _xmlDocument.CreateElement("DTS:Variable", ns);
            selectVar.SetAttribute("CreationName", ns, "");
            selectVar.SetAttribute("DTSID", ns, "{" + Guid.NewGuid().ToString().ToUpper() + "}");
            selectVar.SetAttribute("IncludeInDebugDump", ns, "2345");
            selectVar.SetAttribute("Namespace", ns, "User");
            selectVar.SetAttribute("ObjectName", ns, tableName + "_SelectCmd");

            string dbName = new SqlConnectionStringBuilder(this._configurations.Database.ConnectionString).InitialCatalog;
            string schema = this._configurations.Database.Schema;

            string selectCmd = "SELECT [" + string.Join("], [", columns) + "] FROM [" + dbName + "].[" + schema + "].[" + tableName + "]";
            XmlElement selectValue = _xmlDocument.CreateElement("DTS:VariableValue", ns);
            selectValue.SetAttribute("DataType", ns, "8");
            selectValue.InnerText = selectCmd;
            selectVar.AppendChild(selectValue);

            variablesNode.AppendChild(destVar);
            variablesNode.AppendChild(selectVar);
        }

        private void WriteTransferStructureExec()
        {
            XmlNode taskData = _xmlDocument.SelectSingleNode(@"//DTS:Executable[@DTS:refId='Package\Transfer structure']/DTS:ObjectData/TransferSqlServerObjectsTaskData", _xmlNamespaceManager);

            taskData.Attributes["TablesList"].Value = CreateDtsTablesList(this._metadata.Tables, ExclusionLevel.NoExcludedTables);
        }

        private void WriteTransformAndTransferExec()
        {
            XmlNode components = _xmlDocument.SelectSingleNode(@"//DTS:Executable[@DTS:refId='Package\Transform and transfer']/DTS:ObjectData/pipeline/components", _xmlNamespaceManager);
            XmlNode paths = _xmlDocument.SelectSingleNode(@"//DTS:Executable[@DTS:refId='Package\Transform and transfer']/DTS:ObjectData/pipeline/paths", _xmlNamespaceManager);

            // Entferne zunächst alle components
            components.RemoveAll();

            // Entferne zunächst alle paths
            paths.RemoveAll();

            // Variablen-Node finden (für dynamische Variablen pro Tabelle)
            XmlNode variablesNode = _xmlDocument.SelectSingleNode("DTS:Executable/DTS:Variables", _xmlNamespaceManager);

            RemoveVariables(variablesNode);

            int counter = 0;

            foreach (DatabaseTable table in this._metadata.Tables)
            {
                if(this._configurations.AnonymizationRules.Any(rule => string.Equals(rule.TableName, $"{table.SchemaName}.{table.TableName}",
                                           StringComparison.OrdinalIgnoreCase)))
                {
                    string sourceName = $"Quelle - {table.TableName}";
                    string destinationName = $"Ziel - {table.TableName}";

                    // Variablen für diese Tabelle erstellen (dynamisch)
                    CreateVariablesForTable(variablesNode, table.TableName, table.Columns);

                    // Quell-Komponente erstellen
                    XmlElement sourceComponent = CreateSourceComponent(table.TableName, table.Columns, sourceName);

                    // Ziel-Komponente erstellen
                    XmlElement destinationComponent = CreateTargetComponent(table.TableName, table.Columns, destinationName);

                    // Path-Komponente erstellen
                    XmlElement pathComponent = CreatePath(sourceName, destinationName, counter);

                    // Füge die Komponenten an die richtige Stelle im XML-Dokument ein
                    components.AppendChild(sourceComponent);
                    components.AppendChild(destinationComponent);
                    paths.AppendChild(pathComponent);
                    counter++;
                }
            }
        }

        #region WriteTransformAndTransferExec Helpers
        XmlElement CreateSourceComponent(string tableName, List<TableColumn> columns, string componentName)
        {
            // Komponente erstellen (basierend auf Template für "Quelle - Shippers")
            XmlElement component = _xmlDocument.CreateElement("component");
            component.SetAttribute("refId", $"Package\\Transform and transfer\\{componentName}");
            component.SetAttribute("componentClassID", "Microsoft.OLEDBSource");
            component.SetAttribute("contactInfo", "OLE DB-Quelle;Microsoft Corporation; Microsoft SQL Server; (C) Microsoft Corporation; Alle Rechte vorbehalten; http://www.microsoft.com/sql/support;4");
            component.SetAttribute("description", "OLE DB-Quelle");
            component.SetAttribute("name", componentName);
            component.SetAttribute("usesDispositions", "true");
            component.SetAttribute("validateExternalMetadata", "False");
            component.SetAttribute("version", "4");

            // <properties>
            XmlElement properties = _xmlDocument.CreateElement("properties");
            AddProperty(properties, "AccessMode", "3", "System.Int32", "Gibt den Modus zum Abrufen von Daten aus der Quelle an.");
            AddProperty(properties, "OpenRowset", "", "System.String", "Gibt den Namen des zum Öffnen eines Rowsets verwendeten Datenbankobjekts an.");
            AddProperty(properties, "OpenRowsetVariable", "", "System.String", "Gibt die Variable an, die den Namen des zum Öffnen eines Rowsets verwendeten Datenbankobjekts enthält.");
            AddProperty(properties, "SqlCommand", "", "System.String", "Der auszuführende SQL-Befehl.", "UITypeEditor", "Microsoft.DataTransformationServices.Controls.ModalMultilineStringEditor");
            AddProperty(properties, "SqlCommandVariable", $"User::{tableName}_SelectCmd", "System.String", "Gibt die Variable an, die den auszuführenden SQL-Befehl enthält.");
            AddProperty(properties, "ParameterMapping", "", "System.String", "Ordnet Variablen SQL-Parametern zu.");
            AddProperty(properties, "DefaultCodePage", "1252", "System.Int32", "Gibt die zu verwendende Spaltencodepage an, wenn keine Codepageinformationen von der Datenquelle verfügbar sind.");
            AddProperty(properties, "AlwaysUseDefaultCodePage", "false", "System.Boolean", "Erzwingt die Verwendung des DefaultCodePage-Eigenschaftswerts beim Beschreiben von Zeichendaten.");
            component.AppendChild(properties);

            // <connections>
            XmlElement connections = _xmlDocument.CreateElement("connections");
            XmlElement connection = _xmlDocument.CreateElement("connection");
            connection.SetAttribute("refId", $"Package\\Transform and transfer\\{componentName}.Connections[OleDbConnection]");
            connection.SetAttribute("connectionManagerID", "Package.ConnectionManagers[OledbSourceConnection]");
            connection.SetAttribute("connectionManagerRefId", "Package.ConnectionManagers[OledbSourceConnection]");
            connection.SetAttribute("description", "Die für den Zugriff auf die Datenquelle verwendete OLE DB-Laufzeitverbindung.");
            connection.SetAttribute("name", "OleDbConnection");
            connections.AppendChild(connection);
            component.AppendChild(connections);

            // <outputs>
            XmlElement outputs = _xmlDocument.CreateElement("outputs");

            // Erste Output: Ausgabe der OLE DB-Quelle
            XmlElement output1 = _xmlDocument.CreateElement("output");
            output1.SetAttribute("refId", $"Package\\Transform and transfer\\{componentName}.Outputs[Ausgabe der OLE DB-Quelle]");
            output1.SetAttribute("name", "Ausgabe der OLE DB-Quelle");
            XmlElement outputColumns1 = _xmlDocument.CreateElement("outputColumns");
            foreach (TableColumn col in columns)
            {
                AddOutputColumn(outputColumns1, col.ColumnName, DatatypeMapper.GetSsisDataType(col.DataType),
                        $"Package\\Transform and transfer\\{componentName}.Outputs[Ausgabe der OLE DB-Quelle].Columns[{col.ColumnName}]");
            }
            output1.AppendChild(outputColumns1);
            XmlElement externalMetadataColumns1 = _xmlDocument.CreateElement("externalMetadataColumns");
            externalMetadataColumns1.SetAttribute("isUsed", "True");
            foreach (TableColumn col in columns)
            {
                AddExternalMetadataColumn(externalMetadataColumns1, col.ColumnName, componentName, DatatypeMapper.GetSsisDataType(col.DataType));
            }
            output1.AppendChild(externalMetadataColumns1);
            outputs.AppendChild(output1);

            // Zweite Output: Fehlerausgabe
            XmlElement output2 = _xmlDocument.CreateElement("output");
            output2.SetAttribute("refId", $"Package\\Transform and transfer\\{componentName}.Outputs[Fehlerausgabe der OLE DB-Quelle]");
            output2.SetAttribute("isErrorOut", "true");
            output2.SetAttribute("name", "Fehlerausgabe der OLE DB-Quelle");
            XmlElement outputColumns2 = _xmlDocument.CreateElement("outputColumns");
            foreach (TableColumn col in columns)
            {
                AddOutputColumn(outputColumns2, col.ColumnName, DatatypeMapper.GetSsisDataType(col.DataType), $"Package\\Transform and transfer\\{componentName}.Outputs[Fehlerausgabe der OLE DB-Quelle].Columns[{col.ColumnName}]");
            }
            output2.AppendChild(outputColumns2);
            XmlElement externalMetadataColumns2 = _xmlDocument.CreateElement("externalMetadataColumns");
            output2.AppendChild(externalMetadataColumns2);
            outputs.AppendChild(output2);

            component.AppendChild(outputs);

            return component;
        }

        XmlElement CreateTargetComponent(string tableName, List<TableColumn> columns, string componentName)
        {
            // Komponente erstellen (basierend auf Template für "Ziel - Shippers")
            XmlElement component = _xmlDocument.CreateElement("component");
            component.SetAttribute("refId", $"Package\\Transform and transfer\\{componentName}");
            component.SetAttribute("componentClassID", "Microsoft.OLEDBDestination");
            component.SetAttribute("contactInfo", "OLE DB-Ziel;Microsoft Corporation; Microsoft SQL Server; (C) Microsoft Corporation; Alle Rechte vorbehalten; http://www.microsoft.com/sql/support;4");
            component.SetAttribute("description", "OLE DB-Ziel");
            component.SetAttribute("name", componentName);
            component.SetAttribute("usesDispositions", "true");
            component.SetAttribute("validateExternalMetadata", "False");
            component.SetAttribute("version", "4");

            // <properties>
            XmlElement properties = _xmlDocument.CreateElement("properties");
            AddProperty(properties, "CommandTimeout", "0", "System.Int32", "Die Anzahl der Sekunden für das Timeout eines Befehls. Der Wert \"0\" zeigt einen unendlichen Timeoutwert an.");
            AddProperty(properties, "OpenRowset", "", "System.String", "Gibt den Namen des zum Öffnen eines Rowsets verwendeten Datenbankobjekts an.");
            AddProperty(properties, "OpenRowsetVariable", $"User::{tableName}_DestName", "System.String", "Gibt die Variable an, die den Namen des zum Öffnen eines Rowsets verwendeten Datenbankobjekts enthält.");
            AddProperty(properties, "SqlCommand", "", "System.String", "Der auszuführende SQL-Befehl.", "UITypeEditor", "Microsoft.DataTransformationServices.Controls.ModalMultilineStringEditor");
            AddProperty(properties, "DefaultCodePage", "1252", "System.Int32", "Gibt die zu verwendende Spaltencodepage an, wenn keine Codepageinformationen von der Datenquelle verfügbar sind.");
            AddProperty(properties, "AlwaysUseDefaultCodePage", "false", "System.Boolean", "Erzwingt die Verwendung des DefaultCodePage-Eigenschaftswerts beim Beschreiben von Zeichendaten.");
            AddProperty(properties, "AccessMode", "4", "System.Int32", "Gibt den zum Zugreifen auf die Datenbank verwendeten Modus an.", "typeConverter", "AccessMode");
            AddProperty(properties, "FastLoadKeepIdentity", "true", "System.Boolean", "Zeigt an, ob die für Identitätsspalten übergebenen Werte zum Ziel kopiert werden. ...");
            AddProperty(properties, "FastLoadKeepNulls", "true", "System.Boolean", "Zeigt an, ob für Spalten, die NULL enthalten, NULL am Ziel eingefügt wird. ...");
            AddProperty(properties, "FastLoadOptions", "", "System.String", "Gibt die für die Option \"Schnelles Laden\" zu verwendenden Optionen an. ...");
            AddProperty(properties, "FastLoadMaxInsertCommitSize", "2147483647", "System.Int32", "Gibt an, wann beim Einfügen von Daten Commits ausgegeben werden. ...");
            component.AppendChild(properties);

            // <connections>
            XmlElement connections = _xmlDocument.CreateElement("connections");
            XmlElement connection = _xmlDocument.CreateElement("connection");
            connection.SetAttribute("refId", $"Package\\Transform and transfer\\{componentName}.Connections[OleDbConnection]");
            connection.SetAttribute("connectionManagerID", "Package.ConnectionManagers[OledbDestinationConnection]");
            connection.SetAttribute("connectionManagerRefId", "Package.ConnectionManagers[OledbDestinationConnection]");
            connection.SetAttribute("description", "Die für den Zugriff auf die Datenbank verwendete OLE DB-Laufzeitverbindung.");
            connection.SetAttribute("name", "OleDbConnection");
            connections.AppendChild(connection);
            component.AppendChild(connections);

            // <inputs>
            XmlElement inputs = _xmlDocument.CreateElement("inputs");
            XmlElement input = _xmlDocument.CreateElement("input");
            input.SetAttribute("refId", $"Package\\Transform and transfer\\{componentName}.Inputs[Eingabe des OLE DB-Ziels]");
            input.SetAttribute("errorOrTruncationOperation", "Einfügen");
            input.SetAttribute("errorRowDisposition", "FailComponent");
            input.SetAttribute("hasSideEffects", "true");
            input.SetAttribute("name", "Eingabe des OLE DB-Ziels");

            XmlElement inputColumns = _xmlDocument.CreateElement("inputColumns");
            foreach (TableColumn col in columns)
            {
                AddInputColumn(inputColumns, col.ColumnName, componentName, DatatypeMapper.GetSsisDataType(col.DataType),
                    $"Package\\Transform and transfer\\{componentName}.Inputs[Eingabe des OLE DB-Ziels].ExternalColumns[{col.ColumnName}]",
                    $"Package\\Transform and transfer\\Quelle - {tableName}.Outputs[Ausgabe der OLE DB-Quelle].Columns[{col.ColumnName}]"); // Anpassen, wenn Source-Name anders ist
            }
            input.AppendChild(inputColumns);

            XmlElement externalMetadataColumnsInput = _xmlDocument.CreateElement("externalMetadataColumns");
            externalMetadataColumnsInput.SetAttribute("isUsed", "True");
            foreach (TableColumn col in columns)
            {
                AddExternalMetadataColumn(externalMetadataColumnsInput, col.ColumnName, componentName, DatatypeMapper.GetSsisDataType(col.DataType));
            }
            input.AppendChild(externalMetadataColumnsInput);
            inputs.AppendChild(input);
            component.AppendChild(inputs);

            // <outputs>
            XmlElement outputs = _xmlDocument.CreateElement("outputs");
            XmlElement output = _xmlDocument.CreateElement("output");
            output.SetAttribute("refId", $"Package\\Transform and transfer\\{componentName}.Outputs[Fehlerausgabe des OLE DB-Ziels]");
            output.SetAttribute("exclusionGroup", "1");
            output.SetAttribute("isErrorOut", "true");
            output.SetAttribute("name", "Fehlerausgabe des OLE DB-Ziels");
            output.SetAttribute("synchronousInputId", $"Package\\Transform and transfer\\{componentName}.Inputs[Eingabe des OLE DB-Ziels]");

            XmlElement outputColumns = _xmlDocument.CreateElement("outputColumns");
            AddOutputColumn(outputColumns, "ErrorCode", "i4", $"Package\\Transform and transfer\\{componentName}.Outputs[Fehlerausgabe des OLE DB-Ziels].Columns[ErrorCode]", specialFlags: "1");
            AddOutputColumn(outputColumns, "ErrorColumn", "i4", $"Package\\Transform and transfer\\{componentName}.Outputs[Fehlerausgabe des OLE DB-Ziels].Columns[ErrorColumn]", specialFlags: "2");
            output.AppendChild(outputColumns);

            XmlElement externalMetadataColumnsOutput = _xmlDocument.CreateElement("externalMetadataColumns");
            output.AppendChild(externalMetadataColumnsOutput);
            outputs.AppendChild(output);
            component.AppendChild(outputs);

            return component;
        }

        XmlElement CreatePath(string sourceName, string destinationName, int counter)
        {
            XmlElement path = _xmlDocument.CreateElement("path");
            path.SetAttribute("refId", $"Package\\Transform and transfer.Paths[Ausgabe der OLE DB-Quelle{counter}]");
            path.SetAttribute("endId", $"Package\\Transform and transfer\\{destinationName}.Inputs[Eingabe des OLE DB-Ziels]");
            path.SetAttribute("name", "Ausgabe der OLE DB-Quelle");
            path.SetAttribute("startId", $"Package\\Transform and transfer\\{sourceName}.Outputs[Ausgabe der OLE DB-Quelle]");

            return path;
        }

        void RemoveVariables(XmlNode variablesNode)
        {
            List<XmlNode> removables = new List<XmlNode>();
            // Entferne Tabellenvariablen (Es existieren nur solche für zu anonymisierende Tabellen.
            foreach (XmlNode variableNode in variablesNode.ChildNodes)
            {
                bool isSelectCmdVariable = variableNode.Attributes["DTS:ObjectName"].Value.EndsWith("_SelectCmd");
                bool isDestNameVariable = variableNode.Attributes["DTS:ObjectName"].Value.EndsWith("_DestName");

                // Entferne Template-Variablen
                if (isSelectCmdVariable || isDestNameVariable)
                {
                    removables.Add(variableNode);
                    //variablesNode.RemoveChild(variableNode);
                }
            }

            foreach (XmlNode removable in removables)
            {
                variablesNode.RemoveChild(removable);
            }

        }
        #region WriteTransformAndTransferExec HelpersHelpers
        private void AddInputColumn(XmlElement inputColumns, string colName, string componentName, string dataType, string externalMetadataColumnId, string lineageId, int length=50)
        {
            XmlElement col = _xmlDocument.CreateElement("inputColumn");
            col.SetAttribute("refId", $"Package\\Transform and transfer\\{componentName}.Inputs[Eingabe des OLE DB-Ziels].Columns[{componentName}]"); // Dynamisch anpassen
            col.SetAttribute("cachedDataType", dataType);
            if (dataType.Equals("wstr")) col.SetAttribute("cachedLength", length.ToString());
            col.SetAttribute("cachedName", colName);
            col.SetAttribute("externalMetadataColumnId", externalMetadataColumnId);
            col.SetAttribute("lineageId", lineageId);
            inputColumns.AppendChild(col);
        }

        private void AddExternalMetadataColumn(XmlElement externalColumns, string colName, string componentName, string dataType, int length=50)
        {
            XmlElement col = _xmlDocument.CreateElement("externalMetadataColumn");
            col.SetAttribute("refId", $"Package\\Transform and transfer\\{componentName}.Inputs[Eingabe des OLE DB-Ziels].ExternalColumns[{colName}]"); // Dynamisch anpassen
            col.SetAttribute("dataType", dataType);
            if (dataType.Equals("wstr")) col.SetAttribute("length", length.ToString());
            col.SetAttribute("name", colName);
            externalColumns.AppendChild(col);
        }

        private void AddProperty(XmlElement properties, string name, string value, string dataType, string description, string uiTypeEditor = null, string typeConverter = null)
        {
            XmlElement prop = _xmlDocument.CreateElement("property");
            prop.SetAttribute("dataType", dataType);
            prop.SetAttribute("description", description);
            prop.SetAttribute("name", name);
            if (uiTypeEditor != null) prop.SetAttribute("UITypeEditor", uiTypeEditor);
            if (typeConverter != null) prop.SetAttribute("typeConverter", typeConverter);
            prop.InnerText = value;
            properties.AppendChild(prop);
        }

        private void AddOutputColumn(XmlElement outputColumns, string colName, string dataType, string lineageId, string specialFlags = null, int length = 50)
        {
            XmlElement col = _xmlDocument.CreateElement("outputColumn");
            col.SetAttribute("refId", lineageId); // Oder dynamisch anpassen
            col.SetAttribute("dataType", dataType);
            if (dataType.Equals("wstr")) col.SetAttribute("length", length.ToString());
            col.SetAttribute("lineageId", lineageId);
            col.SetAttribute("name", colName);
            if (specialFlags != null) col.SetAttribute("specialFlags", specialFlags);
            outputColumns.AppendChild(col);
        }
        #endregion WriteTransformAndTransferExec HelpersHelpers
        #endregion WriteTransformAndTransferExec Helpers

        void WriteTransferSqlServerObjectsExec()
        {
            XmlNode taskData = _xmlDocument.SelectSingleNode(@"//DTS:Executable[@DTS:refId='Package\Transfer SQL-Server objects']/DTS:ObjectData/TransferSqlServerObjectsTaskData", _xmlNamespaceManager);

            taskData.Attributes["TablesList"].Value = CreateDtsTablesList(this._metadata.Tables, ExclusionLevel.NoExcludedTablesAndAnonymized);
        }



        string CreateDtsTablesList(List<DatabaseTable> tables, ExclusionLevel exclusionLevel)
        {                
            var stringBuilder = new StringBuilder();
            foreach (DatabaseTable table in tables)
            {
                if (exclusionLevel == ExclusionLevel.NoExcludedTables)
                    if (this._configurations.ExcludedTables.Contains($"{table.SchemaName}.{table.TableName}")) continue;

                if (exclusionLevel == ExclusionLevel.NoExcludedTablesAndAnonymized)
                {
                    if (this._configurations.AnonymizationRules.Any(rule => string.Equals(rule.TableName, $"{table.SchemaName}.{table.TableName}",
                                           StringComparison.OrdinalIgnoreCase))) continue;
                }

                string entryName = $"[{table.SchemaName}].[{table.TableName}]";
                stringBuilder.Append($"{entryName.Length},{entryName},");
            }
            return $"{tables.Count},{stringBuilder.ToString()}";
        }

        /// <summary>
        /// Gibt an, welche Tabellen auszuschließen sind. Bsp.: Bei der Strukturübertragung nur solche, die als Excluded angegeben wurden, 
        /// nicht die die zu anonymisieren sind. Bei der Übertragung der SQL-Server Objekte auch solche, die zu anonymisieren sind, da
        /// diese durch einen anderen Executable übertragen werden.
        /// </summary>
        enum ExclusionLevel 
        {
            KeepAllTables = 0,
            NoExcludedTables = 1,
            NoExcludedTablesAndAnonymized = 2
        }
    }
}
