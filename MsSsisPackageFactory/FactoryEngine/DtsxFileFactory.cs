using MsSsisPackageFactory.MetadataManagement.Configuration;
using MsSsisPackageFactory.MetadataManagement.Configuration.Model;
using MsSsisPackageFactory.MetadataManagement.DbMetadata;
using MsSsisPackageFactory.MetadataManagement.DbMetadata.Model;
using System.Text;
using System.Xml;

namespace MsSsisPackageFactory.FactoryEngine
{
    /// <summary>
    /// Der Aufbau der Hauptmethoden entspricht der Reihenfolge in dem dtsx-Template.
    /// </summary>
    internal class DtsxFileFactory : IPackageBuilder
    {
        private readonly IConfigurationProvider _configurations;
        private readonly IMetadataProvider _metadata;
        private readonly XmlDocument _xmlDocument;
        private readonly XmlNamespaceManager _xmlNamespaceManager;

        public DtsxFileFactory(IConfigurationProvider configurationProvider, IMetadataProvider metadataProvider)
        {
            this._xmlDocument = new XmlDocument();
            this._configurations = configurationProvider;
            this._metadata = metadataProvider;

            // XML-Namespace hinzufügen
            this._xmlNamespaceManager = new XmlNamespaceManager(this._xmlDocument.NameTable);
            this._xmlNamespaceManager.AddNamespace("DTS", "www.microsoft.com/SqlServer/Dts");
        }

        public void CreatePackage(DbMetaData dbMetaData, UserConfiguration userConfig)
        {
            this._xmlDocument.Load(userConfig.TemplateFileName);

            this.WriteTransferStructureExec();
            this.WriteTransformAndTransferExec();
            this.WriteTransferSqlServerObjectsExec();
            
            this._xmlDocument.Save(CreateNewFileName(userConfig.TemplateFileName));
        }

        private string CreateNewFileName(string oldFileName)
        {
            // Get current DateTime with ten millionths of a second accurancy.
            string dateTime = DateTime.Now.ToString("yyyy-MM-dd HHmmssfffffff");

            // Create destinationDbName
            return $"{oldFileName} {dateTime}.dtsx";
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

            string selectCmd = "SELECT [" + string.Join("], [", columns) + "] FROM [NorthWind].[dbo].[" + tableName + "]"; // Passe [NorthWind] an deine Source-DB an
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

            taskData.Attributes["TablesList"].Value = CreateDtsTablesList(this._metadata.CurrentDbMetaData.Tables);
        }

        private void WriteTransformAndTransferExec()
        {
            XmlNode components = _xmlDocument.SelectSingleNode(@"//DTS:Executable[@DTS:refId='Package\Transform and transfer']/DTS:ObjectData/pipeline/components", _xmlNamespaceManager);
            XmlNode paths = _xmlDocument.SelectSingleNode(@"//DTS:Executable[@DTS:refId='Package\Transform and transfer']/DTS:ObjectData/pipeline/paths", _xmlNamespaceManager);

            // Variablen-Node finden (für dynamische Variablen pro Tabelle)
            XmlNode variablesNode = _xmlDocument.SelectSingleNode("DTS:Executable/DTS:Variables", _xmlNamespaceManager);

            int index = 0; // Für eindeutige Namen, z.B. "Quelle 0 - Shippers", "Quelle 1 - Orders"
            foreach (DatabaseTable table in this._metadata.CurrentDbMetaData.Tables)
            {
                string sourceName = $"Quelle {index} - {table.TableName}";
                string destinationName = $"Ziel {index} - {table.TableName}";

                // Variablen für diese Tabelle erstellen (dynamisch)
                CreateVariablesForTable(variablesNode, table.TableName, table.Columns);

                // Quell-Komponente erstellen
                XmlElement sourceComponent = CreateSourceComponent(table.TableName, table.Columns, sourceName);

                // Ziel-Komponente erstellen
                XmlElement destinationComponent = CreateTargetComponent(table.TableName, table.Columns, destinationName);

                // Füge die Komponenten an die richtige Stelle im XML-Dokument ein
                components.AppendChild(sourceComponent);
                components.AppendChild(destinationComponent);
            }
        }

        #region WriteTransformAndTransferExec Helpers
        XmlElement CreateSourceComponent(string tableName, List<TableColumn> columns, string componentName)
        {
            // Komponente erstellen (basierend auf Template für "Quelle 0 - Shippers")
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
                AddOutputColumn(outputColumns1, col.ColumnName, GetDataTypeForColumn(col.ColumnName), GetLengthForColumn(col.ColumnName), $"Package\\Transform and transfer\\{componentName}.Outputs[Ausgabe der OLE DB-Quelle].Columns[{col.ColumnName}]");
            }
            output1.AppendChild(outputColumns1);
            XmlElement externalMetadataColumns1 = _xmlDocument.CreateElement("externalMetadataColumns");
            externalMetadataColumns1.SetAttribute("isUsed", "True");
            foreach (TableColumn col in columns)
            {
                AddExternalMetadataColumn(externalMetadataColumns1, col.ColumnName, componentName, GetDataTypeForColumn(col.ColumnName), GetLengthForColumn(col.ColumnName));
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
                AddOutputColumn(outputColumns2, col.ColumnName, GetDataTypeForColumn(col.ColumnName), GetLengthForColumn(col.ColumnName), $"Package\\Transform and transfer\\{componentName}.Outputs[Fehlerausgabe der OLE DB-Quelle].Columns[{col.ColumnName}]");
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
            // Komponente erstellen (basierend auf Template für "Ziel 0 - Shippers")
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
                AddInputColumn(inputColumns, col.ColumnName, componentName, GetDataTypeForColumn(col.ColumnName), GetLengthForColumn(col.ColumnName), $"Package\\Transform and transfer\\{componentName}.Inputs[Eingabe des OLE DB-Ziels].ExternalColumns[{col.ColumnName}]", $"Package\\Transform and transfer\\Quelle 0 - {tableName}.Outputs[Ausgabe der OLE DB-Quelle].Columns[{col.ColumnName}]"); // Anpassen, wenn Source-Name anders ist
            }
            input.AppendChild(inputColumns);

            XmlElement externalMetadataColumnsInput = _xmlDocument.CreateElement("externalMetadataColumns");
            externalMetadataColumnsInput.SetAttribute("isUsed", "True");
            foreach (TableColumn col in columns)
            {
                AddExternalMetadataColumn(externalMetadataColumnsInput, col.ColumnName, componentName, GetDataTypeForColumn(col.ColumnName), GetLengthForColumn(col.ColumnName));
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
            AddOutputColumn(outputColumns, "ErrorCode", "i4", 0, $"Package\\Transform and transfer\\{componentName}.Outputs[Fehlerausgabe des OLE DB-Ziels].Columns[ErrorCode]", specialFlags: "1");
            AddOutputColumn(outputColumns, "ErrorColumn", "i4", 0, $"Package\\Transform and transfer\\{componentName}.Outputs[Fehlerausgabe des OLE DB-Ziels].Columns[ErrorColumn]", specialFlags: "2");
            output.AppendChild(outputColumns);

            XmlElement externalMetadataColumnsOutput = _xmlDocument.CreateElement("externalMetadataColumns");
            output.AppendChild(externalMetadataColumnsOutput);
            outputs.AppendChild(output);
            component.AppendChild(outputs);

            return component;
        }

        #region WriteTransformAndTransferExec HelpersHelpers
        private void AddInputColumn(XmlElement inputColumns, string colName, string componentName, string dataType, int length, string externalMetadataColumnId, string lineageId)
        {
            XmlElement col = _xmlDocument.CreateElement("inputColumn");
            col.SetAttribute("refId", $"Package\\Transform and transfer\\{componentName}.Inputs[Eingabe des OLE DB-Ziels].Columns[{componentName}]"); // Dynamisch anpassen
            col.SetAttribute("cachedDataType", dataType);
            if (length > 0) col.SetAttribute("cachedLength", length.ToString());
            col.SetAttribute("cachedName", colName);
            col.SetAttribute("externalMetadataColumnId", externalMetadataColumnId);
            col.SetAttribute("lineageId", lineageId);
            inputColumns.AppendChild(col);
        }

        private void AddExternalMetadataColumn(XmlElement externalColumns, string colName, string componentName, string dataType, int length)
        {
            XmlElement col = _xmlDocument.CreateElement("externalMetadataColumn");
            col.SetAttribute("refId", $"Package\\Transform and transfer\\{componentName}.Inputs[Eingabe des OLE DB-Ziels].ExternalColumns[{colName}]"); // Dynamisch anpassen
            col.SetAttribute("dataType", dataType);
            if (length > 0) col.SetAttribute("length", length.ToString());
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

        private void AddOutputColumn(XmlElement outputColumns, string colName, string dataType, int length, string lineageId, string specialFlags = null)
        {
            XmlElement col = _xmlDocument.CreateElement("outputColumn");
            col.SetAttribute("refId", lineageId); // Oder dynamisch anpassen
            col.SetAttribute("dataType", dataType);
            if (length > 0) col.SetAttribute("length", length.ToString());
            col.SetAttribute("lineageId", lineageId);
            col.SetAttribute("name", colName);
            if (specialFlags != null) col.SetAttribute("specialFlags", specialFlags);
            outputColumns.AppendChild(col);
        }

        // Platzhalter für Datentypen (basierend auf Template; erweitere mit Metadaten)
        private string GetDataTypeForColumn(string colName)
        {
            if (colName.EndsWith("ID")) return "i4"; // Integer für IDs
            return "wstr"; // String für andere
        }

        private int GetLengthForColumn(string colName)
        {
            if (colName == "CompanyName") return 40;
            if (colName == "Phone") return 24;
            return 0; // Für non-string oder unbekannt
        }
        #endregion WriteTransformAndTransferExec HelpersHelpers
        #endregion WriteTransformAndTransferExec Helpers


        void WriteTransferSqlServerObjectsExec()
        {
            XmlNode taskData = _xmlDocument.SelectSingleNode(@"//DTS:Executable[@DTS:refId='Package\Transfer SQL-Server objects']/DTS:ObjectData/TransferSqlServerObjectsTaskData", _xmlNamespaceManager);

            taskData.Attributes["TablesList"].Value = CreateDtsTablesList(this._metadata.CurrentDbMetaData.Tables);
            taskData.Attributes["ViewsList"].Value = CreateDtsViewsList();
        }

        string CreateDtsTablesList(List<DatabaseTable> tables)
        {
            var stringBuilder = new StringBuilder();
            foreach (DatabaseTable table in tables)
            {
                string entryName = $"[{table.SchemaName}].[{table.TableName}]";
                stringBuilder.Append($"{entryName.Length},{entryName},");
            }
            return $"{tables.Count},{stringBuilder.ToString()}";
        }

        string CreateDtsViewsList()
        {
            return "16,37,[dbo].[Alphabetical list of products],31,[dbo].[Category Sales for 1997],28,[dbo].[Current Product List],38,[dbo].[Customer and Suppliers by City],16,[dbo].[Invoices],30,[dbo].[Order Details Extended],23,[dbo].[Order Subtotals],18,[dbo].[Orders Qry],30,[dbo].[Product Sales for 1997],36,[dbo].[Products Above Average Price],28,[dbo].[Products by Category],24,[dbo].[Quarterly Orders],25,[dbo].[Sales by Category],30,[dbo].[Sales Totals by Amount],35,[dbo].[Summary of Sales by Quarter],32,[dbo].[Summary of Sales by Year],";
        }

    }
}
