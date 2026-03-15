using Microsoft.Data.SqlClient;
using MsSsisPackageFactory.PackageFactoryConfiguration.DbMetadata;
using MsSsisPackageFactory.PackageFactoryConfiguration.DbMetadata.Model;
using MsSsisPackageFactory.PackageFactoryConfiguration.UserConfiguration.Model;
using System.Runtime.CompilerServices;
using System.Xml;

namespace MsSsisPackageFactory.FactoryEngine
{
    internal class DtsElementCreator
    {
        private readonly UserConfigurationModel _configurations;
        private readonly DbMetaData _metadata;
        private readonly XmlNamespaceManager _xmlNamespaceManager;

        internal DtsElementCreator(UserConfigurationModel userConfiguration, DbMetaData dbMetaData, XmlNamespaceManager xmlNamespaceManager)
        {
            this._configurations = userConfiguration;
            this._metadata = dbMetaData;
            this._xmlNamespaceManager = xmlNamespaceManager;
        }

        internal XmlNode Create_TransformAndTransferNode(XmlNode pipelineTemplateNode)
        {
            XmlNode pipelineNodeClone = pipelineTemplateNode.Clone();

            // Erstelle Klon für neuen Hauptknoten
            // Erstelle Verweise auf Klonschnipsel (Klone, die durch schleifen neu erzeugt werden müssen.
            // In der Schleife, erzeuge den Klon aus dem Verweis auf den Teil im Template
            // Innerhalb der Methode, in der die neuen Komponenten erstellt werden sollen, werden am Anfang Verweise auf die Templateschnipsle erzeugt,
            // vor der Manipulation werden sie geklont, d. h. z. B. in der Schleife dann.

            // Entferne alle Kindknoten.
            pipelineNodeClone.SelectSingleNode(@"//components", _xmlNamespaceManager).RemoveAll();
            pipelineNodeClone.SelectSingleNode(@"//paths", _xmlNamespaceManager).RemoveAll();

            int counter = 0;

            foreach (DatabaseTable table in this._metadata.Tables)
            {
                // Wenn Tabelle anonymisiert werden soll.
                if (this._configurations.AnonymizationRules.Any(rule => string.Equals(rule.TableName, $"{table.SchemaName}.{table.TableName}",
                                           StringComparison.OrdinalIgnoreCase)))
                {
                    string sourceName = $"Quelle - {table.TableName}";
                    string destinationName = $"Ziel - {table.TableName}";

                    // Quell-Komponente erstellen
                    XmlNode sourceComponent = CreateSourceComponent(pipelineTemplateNode["components"], table.TableName, table.Columns, sourceName);

                    // Ziel-Komponente erstellen
                    XmlNode destinationComponent = CreateTargetComponent(pipelineTemplateNode["components"], table.TableName, table.Columns, destinationName, sourceName);

                    //// Path-Komponente erstellen
                    XmlNode pathComponent = CreatePath(pipelineTemplateNode["paths"], sourceName, destinationName, counter);

                    // Füge die Komponenten an die richtige Stelle im XML-Dokument ein
                    pipelineNodeClone["components"].AppendChild(sourceComponent);
                    pipelineNodeClone["components"].AppendChild(destinationComponent);
                    pipelineNodeClone["paths"].AppendChild(pathComponent);
                    counter++;
                }
            }

            return pipelineNodeClone;
        }

        internal XmlNode Create_VariablesNode(XmlNode variablesTemplateNode)
        {
            // Remove template-variables.
            List<XmlNode> removables = new();

            foreach (XmlNode variableNode in variablesTemplateNode.ChildNodes)
                if (variableNode.Attributes["DTS:ObjectName"].Value.StartsWith("Template"))
                    removables.Add(variableNode);

            foreach (XmlNode removable in removables)
                variablesTemplateNode.RemoveChild(removable);


            // Create and append new variables for each table.
            foreach (DatabaseTable table in this._metadata.Tables)
            {
                bool anonymizeTable = this._configurations.AnonymizationRules.Any(rule => string.Equals(rule.TableName, $"{table.SchemaName}.{table.TableName}",
                                           StringComparison.OrdinalIgnoreCase));

                // Wenn Tabelle anonymisiert werden soll.
                if (anonymizeTable)
                {
                    // Variablen für diese Tabelle erstellen (dynamisch)
                    AppendNewVariablesForTable(variablesTemplateNode, table.TableName, table.Columns);
                }
            }

            return variablesTemplateNode.Clone();
        }

        #region WriteTransformAndTransferExec Helpers
        // <component refId="Package\Transform and transfer\Quelle1"/>
        private XmlNode CreateSourceComponent(XmlNode componentsTemplateRef, string tableName, List<TableColumn> columns, string componentName)
        {
            // Placeholder
            string pipelineSrcComponentPlaceholder = "Quelle1";
            string strColNamePlaceholder = "ColWstr";
            string nonStrColNamePlaceholder = "ColNonWstr";

            // Erstelle Verweis auf Komponentenschnipsel. ('Quelle1' ist Node-Template-Bezeichnung)
            XmlNode sourceComponentClone = componentsTemplateRef.SelectSingleNode($@"component[@refId='Package\Transform and transfer\Quelle1']", this._xmlNamespaceManager).Clone();

            // <component>-Attributes
            sourceComponentClone.Attributes["refId"].Value = $"Package\\Transform and transfer\\{componentName}";
            sourceComponentClone.Attributes["name"].Value = componentName;

            // <properties>
            sourceComponentClone["properties"].SelectSingleNode($@"property[@name='SqlCommandVariable']", this._xmlNamespaceManager).InnerText = $"User::{tableName}_SelectCmd";

            // <connections>
            sourceComponentClone["connections"]["connection"].Attributes["refId"].Value = $"Package\\Transform and transfer\\{componentName}.Connections[OleDbConnection]";

            // <outputs>
            // Erste Output: Ausgabe der OLE DB-Quelle
            sourceComponentClone["outputs"].ChildNodes[0].Attributes["refId"].Value = $"Package\\Transform and transfer\\{componentName}.Outputs[Ausgabe der OLE DB-Quelle]";

            // Geht nur wenn keine Attribute vorhanden. RemoveAll() entfernt auch alle Attribute.
            sourceComponentClone["outputs"].ChildNodes[0]["outputColumns"].RemoveAll();

            // Hole Referenzen auf Column-Knoten eines String- und Nicht-String-Datentyps aus dem Template. (Stringtyp-Spalte hat ein Attribut mehr: length)
            XmlNode outputColumnsTemplateRef = componentsTemplateRef.SelectSingleNode($@"component[@refId='Package\Transform and transfer\Quelle1']", this._xmlNamespaceManager)
                                            .SelectSingleNode("outputs", this._xmlNamespaceManager)
                                            .SelectSingleNode($@"output[@name='Ausgabe der OLE DB-Quelle']", this._xmlNamespaceManager)["outputColumns"];

            foreach (TableColumn col in columns)
            {
                XmlNode outputColumnStringTypeClone = outputColumnsTemplateRef.SelectSingleNode("outputColumn[@dataType='wstr']", this._xmlNamespaceManager).Clone();
                XmlNode outputColumnIntegerTypeClone = outputColumnsTemplateRef.SelectSingleNode("outputColumn[@dataType='i4']", this._xmlNamespaceManager).Clone();

                string ssisDataType = DatatypeMapper.GetSsisDataType(col.DataType);

                if (ssisDataType.Equals("wstr"))
                {
                    outputColumnStringTypeClone.Attributes["refId"].Value = outputColumnStringTypeClone.Attributes["refId"].Value.Replace(pipelineSrcComponentPlaceholder, componentName).Replace(strColNamePlaceholder, col.ColumnName);
                    outputColumnStringTypeClone.Attributes["dataType"].Value = ssisDataType;
                    outputColumnStringTypeClone.Attributes["externalMetadataColumnId"].Value = outputColumnStringTypeClone.Attributes["externalMetadataColumnId"].Value.Replace(pipelineSrcComponentPlaceholder, componentName).Replace(strColNamePlaceholder, col.ColumnName);
                    outputColumnStringTypeClone.Attributes["length"].Value = "50";
                    outputColumnStringTypeClone.Attributes["lineageId"].Value = outputColumnStringTypeClone.Attributes["lineageId"].Value.Replace(pipelineSrcComponentPlaceholder, componentName).Replace(strColNamePlaceholder, col.ColumnName);
                    outputColumnStringTypeClone.Attributes["name"].Value = col.ColumnName;
                    sourceComponentClone["outputs"].ChildNodes[0]["outputColumns"].AppendChild(outputColumnStringTypeClone);
                }
                else
                {
                    outputColumnIntegerTypeClone.Attributes["refId"].Value = outputColumnIntegerTypeClone.Attributes["refId"].Value.Replace(pipelineSrcComponentPlaceholder, componentName).Replace(nonStrColNamePlaceholder, col.ColumnName);
                    outputColumnIntegerTypeClone.Attributes["dataType"].Value = ssisDataType;
                    outputColumnIntegerTypeClone.Attributes["externalMetadataColumnId"].Value = outputColumnIntegerTypeClone.Attributes["externalMetadataColumnId"].Value.Replace(pipelineSrcComponentPlaceholder, componentName).Replace(nonStrColNamePlaceholder, col.ColumnName);
                    outputColumnIntegerTypeClone.Attributes["lineageId"].Value = outputColumnIntegerTypeClone.Attributes["lineageId"].Value.Replace(pipelineSrcComponentPlaceholder, componentName).Replace(nonStrColNamePlaceholder, col.ColumnName);
                    outputColumnIntegerTypeClone.Attributes["name"].Value = col.ColumnName;
                    sourceComponentClone["outputs"].ChildNodes[0]["outputColumns"].AppendChild(outputColumnIntegerTypeClone);
                }
            }

            // Entferne alle Kindknoten, ohne Attributentfernung.
            while (sourceComponentClone["outputs"].ChildNodes[0]["externalMetadataColumns"].HasChildNodes)
            {
                sourceComponentClone["outputs"].ChildNodes[0]["externalMetadataColumns"].RemoveChild(sourceComponentClone["outputs"].ChildNodes[0]["externalMetadataColumns"].FirstChild);
            }

            // Hole Referenzen auf Column-Knoten eines String- und Nicht-String-Datentyps aus dem Template. (Stringtyp-Spalte hat ein Attribut mehr: length)
            XmlNode externalMetadataColumnsTemplateRef = componentsTemplateRef.SelectSingleNode($@"component[@refId='Package\Transform and transfer\Quelle1']", this._xmlNamespaceManager)
                                            .SelectSingleNode("outputs", this._xmlNamespaceManager)
                                            .SelectSingleNode($@"output[@name='Ausgabe der OLE DB-Quelle']", this._xmlNamespaceManager)["externalMetadataColumns"];

            foreach (TableColumn col in columns)
            {
                // ToDo: Output/Input dynamisch setzen.
                XmlNode outputColumnIntegerTypeClone = externalMetadataColumnsTemplateRef.SelectSingleNode("externalMetadataColumn[@dataType='i4']", this._xmlNamespaceManager).Clone();
                XmlNode outputColumnStringTypeClone = externalMetadataColumnsTemplateRef.SelectSingleNode("externalMetadataColumn[@dataType='wstr']", this._xmlNamespaceManager).Clone();

                string ssisDataType = DatatypeMapper.GetSsisDataType(col.DataType);

                if (ssisDataType.Equals("wstr"))
                {
                    outputColumnStringTypeClone.Attributes["refId"].Value = outputColumnStringTypeClone.Attributes["refId"].Value.Replace(pipelineSrcComponentPlaceholder, componentName).Replace(strColNamePlaceholder, col.ColumnName);
                    outputColumnStringTypeClone.Attributes["dataType"].Value = ssisDataType;
                    outputColumnStringTypeClone.Attributes["length"].Value = "50";
                    outputColumnStringTypeClone.Attributes["name"].Value = col.ColumnName;

                    sourceComponentClone["outputs"].ChildNodes[0]["externalMetadataColumns"].AppendChild(outputColumnStringTypeClone);
                }
                else
                {
                    outputColumnIntegerTypeClone.Attributes["refId"].Value = outputColumnIntegerTypeClone.Attributes["refId"].Value.Replace(pipelineSrcComponentPlaceholder, componentName).Replace(nonStrColNamePlaceholder, col.ColumnName);
                    outputColumnIntegerTypeClone.Attributes["dataType"].Value = ssisDataType;
                    outputColumnIntegerTypeClone.Attributes["name"].Value = col.ColumnName;

                    sourceComponentClone["outputs"].ChildNodes[0]["externalMetadataColumns"].AppendChild(outputColumnIntegerTypeClone);
                }
            }

            // Zweite Output: Fehlerausgabe
            sourceComponentClone["outputs"].ChildNodes[1].Attributes["refId"].Value = sourceComponentClone["outputs"].ChildNodes[1].Attributes["refId"].Value.Replace(pipelineSrcComponentPlaceholder, componentName);

            // Geht nur wenn keine Attribute vorhanden. RemoveAll() entfernt auch alle Attribute.
            sourceComponentClone["outputs"].ChildNodes[1]["outputColumns"].RemoveAll();

            // Hole Referenzen auf Column-Knoten eines String- und Nicht-String-Datentyps aus dem Template. (Stringtyp-Spalte hat ein Attribut mehr: length)
            XmlNode outputColumns2TemplateRef = componentsTemplateRef.SelectSingleNode($@"component[@refId='Package\Transform and transfer\Quelle1']", this._xmlNamespaceManager)
                                            .SelectSingleNode("outputs", this._xmlNamespaceManager)
                                            .SelectSingleNode($@"output[@name='Fehlerausgabe der OLE DB-Quelle']", this._xmlNamespaceManager)["outputColumns"];

            // <outputColumns>
            foreach (TableColumn col in columns)
            {
                XmlNode outputColumnIntegerTypeClone = outputColumns2TemplateRef.SelectSingleNode("outputColumn[@dataType='i4']", this._xmlNamespaceManager).Clone();
                XmlNode outputColumnStringTypeClone = outputColumns2TemplateRef.SelectSingleNode("outputColumn[@dataType='wstr']", this._xmlNamespaceManager).Clone();

                string ssisDataType = DatatypeMapper.GetSsisDataType(col.DataType);

                if (ssisDataType.Equals("wstr"))
                {
                    outputColumnStringTypeClone.Attributes["refId"].Value = outputColumnStringTypeClone.Attributes["refId"].Value.Replace(pipelineSrcComponentPlaceholder, componentName).Replace(strColNamePlaceholder, col.ColumnName);
                    outputColumnStringTypeClone.Attributes["dataType"].Value = ssisDataType;
                    outputColumnStringTypeClone.Attributes["length"].Value = "50";
                    outputColumnStringTypeClone.Attributes["lineageId"].Value = outputColumnStringTypeClone.Attributes["lineageId"].Value.Replace(pipelineSrcComponentPlaceholder, componentName).Replace(strColNamePlaceholder, col.ColumnName);
                    outputColumnStringTypeClone.Attributes["name"].Value = col.ColumnName;

                    sourceComponentClone["outputs"].ChildNodes[1]["outputColumns"].AppendChild(outputColumnStringTypeClone);
                }
                else
                {
                    outputColumnIntegerTypeClone.Attributes["refId"].Value = outputColumnIntegerTypeClone.Attributes["refId"].Value.Replace(pipelineSrcComponentPlaceholder, componentName).Replace(nonStrColNamePlaceholder, col.ColumnName);
                    outputColumnIntegerTypeClone.Attributes["dataType"].Value = ssisDataType;
                    outputColumnIntegerTypeClone.Attributes["lineageId"].Value = outputColumnIntegerTypeClone.Attributes["lineageId"].Value.Replace(pipelineSrcComponentPlaceholder, componentName).Replace(nonStrColNamePlaceholder, col.ColumnName);
                    outputColumnIntegerTypeClone.Attributes["name"].Value = col.ColumnName;

                    sourceComponentClone["outputs"].ChildNodes[1]["outputColumns"].AppendChild(outputColumnIntegerTypeClone);
                }
            }

            return sourceComponentClone;
        }

        private XmlNode CreateTargetComponent(XmlNode componentsTemplateRef, string tableName, List<TableColumn> columns, string componentName, string srcCompName)
        {
            // Placeholder
            string pipelineTargetComponentPlaceHolder = "Ziel1";
            string pipelineSrcComponentPlaceholder = "Quelle1";
            string strColNamePlaceholder = "ColWstr";
            string nonStrColNamePlaceholder = "ColNonWstr";

            // Erstelle Verweis auf Komponentenschnipsel. ('Quelle1' ist Node-Template-Bezeichnung)
            XmlNode targetComponentClone = componentsTemplateRef.SelectSingleNode($@"component[@refId='Package\Transform and transfer\Ziel1']", this._xmlNamespaceManager).Clone();

            // <component>-Attribute
            targetComponentClone.Attributes["refId"].Value = $"Package\\Transform and transfer\\{componentName}";
            targetComponentClone.Attributes["name"].Value = componentName;

            // <properties>
            targetComponentClone["properties"].SelectSingleNode($@"property[@name='OpenRowsetVariable']", this._xmlNamespaceManager).InnerText = $"User::{tableName}_DestName";

            // <connections>
            targetComponentClone["connections"]["connection"].Attributes["refId"].Value = $"Package\\Transform and transfer\\{componentName}.Connections[OleDbConnection]";

            // <inputs>
            // <input>-Attributes
            XmlNode inputNode = targetComponentClone.SelectSingleNode($@"inputs/input[@name='Eingabe des OLE DB-Ziels']");
            inputNode.Attributes["refId"].Value = inputNode.Attributes["refId"].Value.Replace(pipelineTargetComponentPlaceHolder, componentName);

            // <inputColumns>
            string xPathInputColumns = "inputs/input[@name='Eingabe des OLE DB-Ziels']/inputColumns";
            targetComponentClone.SelectSingleNode(xPathInputColumns, this._xmlNamespaceManager).RemoveAll(); // Entfernt auch alle Attribute. Nur bei Elementen ohne Attribute anwenden.

            // Hole Referenzen auf Column-Knoten eines String- und Nicht-String-Datentyps aus dem Template. (Stringtyp-Spalte hat ein Attribut mehr: length)
            XmlNode inputColumnsTemplateRef = componentsTemplateRef
                .SelectSingleNode($@"component[@refId='Package\Transform and transfer\{pipelineTargetComponentPlaceHolder}']/inputs/input[@name='Eingabe des OLE DB-Ziels']/inputColumns",
                this._xmlNamespaceManager);

            foreach (TableColumn col in columns)
            {
                // TODO:    Column korrigieren
                XmlNode inputColumnStrTypeClone = inputColumnsTemplateRef.SelectSingleNode("inputColumn[@cachedDataType='wstr']", this._xmlNamespaceManager).Clone();
                XmlNode inputColumnNonStrTypeClone = inputColumnsTemplateRef.SelectSingleNode("inputColumn[@cachedDataType='i4']", this._xmlNamespaceManager).Clone();

                string ssisDataType = DatatypeMapper.GetSsisDataType(col.DataType);

                if (ssisDataType.Equals("wstr"))
                {
                    inputColumnStrTypeClone.Attributes["refId"].Value = inputColumnStrTypeClone.Attributes["refId"].Value
                        .Replace(pipelineTargetComponentPlaceHolder, componentName).Replace(strColNamePlaceholder, col.ColumnName);
                    inputColumnStrTypeClone.Attributes["cachedDataType"].Value = ssisDataType;
                    inputColumnStrTypeClone.Attributes["cachedLength"].Value = "50";
                    inputColumnStrTypeClone.Attributes["cachedName"].Value = col.ColumnName;
                    inputColumnStrTypeClone.Attributes["externalMetadataColumnId"].Value = inputColumnStrTypeClone.Attributes["externalMetadataColumnId"].Value
                        .Replace(pipelineTargetComponentPlaceHolder, componentName).Replace(strColNamePlaceholder, col.ColumnName);
                    inputColumnStrTypeClone.Attributes["lineageId"].Value = inputColumnStrTypeClone.Attributes["lineageId"].Value
                        .Replace(pipelineSrcComponentPlaceholder, srcCompName).Replace(strColNamePlaceholder, col.ColumnName);
                    targetComponentClone.SelectSingleNode(xPathInputColumns, this._xmlNamespaceManager).AppendChild(inputColumnStrTypeClone);
                }
                else
                {
                    inputColumnNonStrTypeClone.Attributes["refId"].Value = inputColumnNonStrTypeClone.Attributes["refId"].Value
                        .Replace(pipelineTargetComponentPlaceHolder, componentName).Replace(nonStrColNamePlaceholder, col.ColumnName);
                    inputColumnNonStrTypeClone.Attributes["cachedDataType"].Value = ssisDataType;
                    inputColumnNonStrTypeClone.Attributes["cachedName"].Value = col.ColumnName;
                    inputColumnNonStrTypeClone.Attributes["externalMetadataColumnId"].Value = inputColumnNonStrTypeClone.Attributes["externalMetadataColumnId"].Value
                        .Replace(pipelineTargetComponentPlaceHolder, componentName).Replace(nonStrColNamePlaceholder, col.ColumnName);
                    inputColumnNonStrTypeClone.Attributes["lineageId"].Value = inputColumnNonStrTypeClone.Attributes["lineageId"].Value
                        .Replace(pipelineSrcComponentPlaceholder, srcCompName).Replace(nonStrColNamePlaceholder, col.ColumnName);
                    targetComponentClone.SelectSingleNode(xPathInputColumns, this._xmlNamespaceManager).AppendChild(inputColumnNonStrTypeClone);
                }
            }

            // <externalMetadataColumns>
            // Entferne alle Kindknoten, ohne Attributentfernung.
            string xPathExternalMetadataColumns = "inputs/input[@name='Eingabe des OLE DB-Ziels']/externalMetadataColumns";
            XmlNode externalMetadataColumns = targetComponentClone.SelectSingleNode(xPathExternalMetadataColumns);

            while (externalMetadataColumns.HasChildNodes)
            {
                externalMetadataColumns.RemoveChild(externalMetadataColumns.FirstChild);
            }

            // Hole Referenzen auf Column-Knoten eines String- und Nicht-String-Datentyps aus dem Template. (Stringtyp-Spalte hat ein Attribut mehr: length)
            XmlNode externalMetadataColsTemplateRef = componentsTemplateRef
                .SelectSingleNode($@"component[@refId='Package\Transform and transfer\{pipelineTargetComponentPlaceHolder}']/inputs/input[@name='Eingabe des OLE DB-Ziels']/externalMetadataColumns",
                this._xmlNamespaceManager);

            foreach (TableColumn col in columns)
            {
                XmlNode externalMetadataColStrTypeClone = externalMetadataColsTemplateRef.SelectSingleNode("externalMetadataColumn[@dataType='wstr']", this._xmlNamespaceManager).Clone();
                XmlNode externalMetadataColNonStrTypeClone = externalMetadataColsTemplateRef.SelectSingleNode("externalMetadataColumn[@dataType='i4']", this._xmlNamespaceManager).Clone();

                string ssisDataType = DatatypeMapper.GetSsisDataType(col.DataType);

                if (ssisDataType.Equals("wstr"))
                {
                    externalMetadataColStrTypeClone.Attributes["refId"].Value = externalMetadataColStrTypeClone.Attributes["refId"].Value
                        .Replace(pipelineTargetComponentPlaceHolder, componentName).Replace(strColNamePlaceholder, col.ColumnName);
                    externalMetadataColStrTypeClone.Attributes["dataType"].Value = ssisDataType;
                    externalMetadataColStrTypeClone.Attributes["length"].Value = "50";
                    externalMetadataColStrTypeClone.Attributes["name"].Value = col.ColumnName;
                    targetComponentClone.SelectSingleNode(xPathExternalMetadataColumns).AppendChild(externalMetadataColStrTypeClone);
                }
                else
                {
                    externalMetadataColNonStrTypeClone.Attributes["refId"].Value = externalMetadataColNonStrTypeClone.Attributes["refId"].Value
                        .Replace(pipelineTargetComponentPlaceHolder, componentName).Replace(nonStrColNamePlaceholder, col.ColumnName);
                    externalMetadataColNonStrTypeClone.Attributes["dataType"].Value = ssisDataType;
                    externalMetadataColNonStrTypeClone.Attributes["name"].Value = col.ColumnName;
                    targetComponentClone.SelectSingleNode(xPathExternalMetadataColumns).AppendChild(externalMetadataColNonStrTypeClone);
                }
            }

            // <outputs>
            // <input>-Attributes
            XmlNode outputNode = targetComponentClone.SelectSingleNode($@"outputs/output[@name='Fehlerausgabe des OLE DB-Ziels']");
            outputNode.Attributes["refId"].Value = outputNode.Attributes["refId"].Value.Replace(pipelineTargetComponentPlaceHolder, componentName);
            outputNode.Attributes["synchronousInputId"].Value = outputNode.Attributes["synchronousInputId"].Value.Replace(pipelineTargetComponentPlaceHolder, componentName);

            // <outputColumns>
            XmlNode outputCols = targetComponentClone.SelectSingleNode($@"outputs/output[@name='Fehlerausgabe des OLE DB-Ziels']/outputColumns");
            
            // Erstes
            outputCols.ChildNodes[0].Attributes["refId"].Value = outputCols.ChildNodes[0].Attributes["refId"].Value.Replace(pipelineTargetComponentPlaceHolder, componentName);
            outputCols.ChildNodes[0].Attributes["lineageId"].Value = outputCols.ChildNodes[0].Attributes["lineageId"].Value.Replace(pipelineTargetComponentPlaceHolder, componentName);

            // Zweites
            outputCols.ChildNodes[1].Attributes["refId"].Value = outputCols.ChildNodes[1].Attributes["refId"].Value.Replace(pipelineTargetComponentPlaceHolder, componentName);
            outputCols.ChildNodes[1].Attributes["lineageId"].Value = outputCols.ChildNodes[1].Attributes["lineageId"].Value.Replace(pipelineTargetComponentPlaceHolder, componentName);

            return targetComponentClone;
        }

        private XmlNode CreatePath(XmlNode pathsTemplateRef, string sourceComponentName, string targetComponentName, int pathCount)
        {
            // Erstelle Verweis auf Komponentenschnipsel. ('Quelle1' ist Node-Template-Bezeichnung)
            XmlNode pathClone = pathsTemplateRef["path"].Clone();
            string pathRefIdNo = pathCount<1 ? string.Empty : pathCount.ToString();

            // <component>-Attribute
            pathClone.Attributes["refId"].Value = pathClone.Attributes["refId"].Value.Replace("NoX", pathRefIdNo);
            pathClone.Attributes["endId"].Value = pathClone.Attributes["endId"].Value.Replace("Ziel1", targetComponentName);
            pathClone.Attributes["startId"].Value = pathClone.Attributes["startId"].Value.Replace("Quelle1", sourceComponentName);

            return pathClone;
        }

        private void AppendNewVariablesForTable(XmlNode variablesNode, string tableName, List<TableColumn> columns)
        {
            string ns = _xmlNamespaceManager.LookupNamespace("DTS");
            string dbSchema = this._configurations.Database.Schema;

            // _DestName Variable
            XmlElement destVar = variablesNode.OwnerDocument.CreateElement("DTS:Variable", ns);
            destVar.SetAttribute("CreationName", ns, "");
            destVar.SetAttribute("DTSID", ns, "{" + Guid.NewGuid().ToString().ToUpper() + "}");
            destVar.SetAttribute("EvaluateAsExpression", ns, "True");
            // Bsp. => "[" + @[User::Destination] + "].[dbo].[Tabelle]"
            destVar.SetAttribute("Expression", ns, $"\"[\" + @[User::DestinationDbName] + \"].[{dbSchema}].[{tableName}]\"");
            destVar.SetAttribute("IncludeInDebugDump", ns, "2345");
            destVar.SetAttribute("Namespace", ns, "User");
            destVar.SetAttribute("ObjectName", ns, tableName + "_DestName");

            XmlElement destValue = variablesNode.OwnerDocument.CreateElement("DTS:VariableValue");
            destValue.SetAttribute("DataType", ns, "8");
            destValue.InnerText = $"[].[{dbSchema}].[{tableName}]";
            destVar.AppendChild(destValue);

            // _SelectCmd Variable
            XmlElement selectVar = variablesNode.OwnerDocument.CreateElement("DTS:Variable", ns);
            selectVar.SetAttribute("CreationName", ns, "");
            selectVar.SetAttribute("DTSID", ns, "{" + Guid.NewGuid().ToString().ToUpper() + "}");
            selectVar.SetAttribute("IncludeInDebugDump", ns, "2345");
            selectVar.SetAttribute("Namespace", ns, "User");
            selectVar.SetAttribute("ObjectName", ns, tableName + "_SelectCmd");

            string selectCmd = CreateSelectCmd(tableName, columns);
            XmlElement selectValue = variablesNode.OwnerDocument.CreateElement("DTS:VariableValue", ns);
            selectValue.SetAttribute("DataType", ns, "8");
            selectValue.InnerText = selectCmd;
            selectVar.AppendChild(selectValue);

            variablesNode.AppendChild(destVar);
            variablesNode.AppendChild(selectVar);
        }

        private string CreateSelectCmd(string tableName, List<TableColumn> columns)
        {
            string schema = this._configurations.Database.Schema;
            string dbName = new SqlConnectionStringBuilder(this._configurations.Database.ConnectionString).InitialCatalog;
            List<string> newColumnsNames = new List<string>();

            foreach (TableColumn column in columns)
            {
                bool shouldAnonymize = this._configurations.AnonymizationRules.Any(
                    rule => string.Equals(rule.ColumnName, column.ColumnName, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(rule.TableName, $"{schema}.{tableName}", StringComparison.OrdinalIgnoreCase));

                newColumnsNames.Add(shouldAnonymize ? $"'**********' {column.ColumnName}" : $"{column.ColumnName}");
            }

            return $"SELECT {string.Join(",", newColumnsNames)} FROM [{dbName}].[{schema}].[{tableName}]";
        }

        #endregion WriteTransformAndTransferExec Helpers

    }
}
