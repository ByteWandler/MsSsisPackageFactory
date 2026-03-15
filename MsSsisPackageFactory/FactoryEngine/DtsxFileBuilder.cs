using MsSsisPackageFactory.PackageFactoryConfiguration.DbMetadata.Model;
using MsSsisPackageFactory.PackageFactoryConfiguration.UserConfiguration.Model;
using System.Globalization;
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
        private readonly DtsElementCreator _dtsElementCreator;

        public DtsxFileBuilder(UserConfigurationModel userConfiguration, DbMetaData dbMetaData)
        {
            this._xmlDocument = new XmlDocument();
            this._configurations = userConfiguration;
            this._metadata = dbMetaData;

            // XML-Namespace hinzufügen
            this._xmlNamespaceManager = new XmlNamespaceManager(this._xmlDocument.NameTable);
            this._xmlNamespaceManager.AddNamespace("DTS", "www.microsoft.com/SqlServer/Dts");

            this._dtsElementCreator = new DtsElementCreator(userConfiguration, dbMetaData, this._xmlNamespaceManager);
        }

        public void Build()
        {
            this._xmlDocument.Load(this._configurations.TemplateFileName);

            this.SetDtsCreationDate();
            this.RewriteTransferStructureExecutable();
            this.RewriteTransformAndTransferExecutable();
            this.RewriteTransferSqlServerObjectsExecutable();
            this.RewriteVariablesElement();

            this._xmlDocument.Save(CreateNewFileName(this._configurations.TemplateFileName));
        }

        private void SetDtsCreationDate()
        {// TODO: Korrigieren (Done)
            string newDatetime = DateTime.Now.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
            
            XmlNode root = _xmlDocument.SelectSingleNode(@"//DTS:Executable[@DTS:refId='Package']", _xmlNamespaceManager);

            root.Attributes["DTS:CreationDate"].Value = newDatetime;
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
            string newDir = string.Empty;

            if (Path.IsPathFullyQualified(this._configurations.OutputDirectory))
            {
                newDir = this._configurations.OutputDirectory;
            }
            else
            {
                newDir = $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}{this._configurations.OutputDirectory}";
            }                

            if (!Directory.Exists(newDir))
            {
                Directory.CreateDirectory(newDir);
            }

            return $"{newDir}\\{newFilename}";
        }

        private void RewriteTransferStructureExecutable()
        {
            // Hier nur Tabellenliste einfügen, um Komplexität gering zu halten.
            XmlNode taskDataTemplate = _xmlDocument.SelectSingleNode(@"//DTS:Executable[@DTS:refId='Package\Transfer structure']/DTS:ObjectData/TransferSqlServerObjectsTaskData", _xmlNamespaceManager);
            
            taskDataTemplate.Attributes["TablesList"].Value = CreateDtsTablesList(this._metadata.Tables, ExclusionLevel.NoExcludedTables);
        }

        private void RewriteTransformAndTransferExecutable()
        {
            // Hole den zu manipulierenden Template-Teil: <pipeline>-XML-Element.
            XmlNode pipelineTemplateNode = _xmlDocument.SelectSingleNode(@"//DTS:Executable[@DTS:refId='Package\Transform and transfer']/DTS:ObjectData/pipeline", _xmlNamespaceManager);

            // Erstelle funktionales <pipeline>-XML-Element auf Basis des Templates.
            XmlNode createdPipelineNode = this._dtsElementCreator.Create_TransformAndTransferNode(pipelineTemplateNode);

            // Erstetze das Template-Node im Dokument mit dem funktionalen.
            _xmlDocument.SelectSingleNode(@"//DTS:Executable[@DTS:refId='Package\Transform and transfer']/DTS:ObjectData", _xmlNamespaceManager).ReplaceChild(createdPipelineNode, pipelineTemplateNode);

        }

        private void RewriteTransferSqlServerObjectsExecutable()
        {
            // Hier nur Tabellenliste einfügen, um Komplexität gering zu halten.
            XmlNode taskDataTemplate = _xmlDocument.SelectSingleNode(@"//DTS:Executable[@DTS:refId='Package\Transfer SQL-Server objects']/DTS:ObjectData/TransferSqlServerObjectsTaskData", _xmlNamespaceManager);

            taskDataTemplate.Attributes["TablesList"].Value = CreateDtsTablesList(this._metadata.Tables, ExclusionLevel.NoExcludedTablesAndAnonymized);
        }

        private void RewriteVariablesElement()
        {
            // VariablenTemplate-Node finden
            XmlNode variablesTemplateNode = _xmlDocument.SelectSingleNode("DTS:Executable/DTS:Variables", _xmlNamespaceManager);

            // Erstelle funktionales Variablen-XML-Element auf Basis des Templates.
            XmlNode variablesNode = this._dtsElementCreator.Create_VariablesNode(variablesTemplateNode);

            // Erstetze das Template-Node im Dokument mit dem funktionalen.
            _xmlDocument["DTS:Executable"].ReplaceChild(variablesNode, variablesTemplateNode);
        }

        string CreateDtsTablesList(List<DatabaseTable> tables, ExclusionLevel exclusionLevel)
        {                
            var listTableEntries = new List<string>();
                    
            foreach (DatabaseTable table in tables)
            {
                bool isExcludedTable = this._configurations.ExcludedTables.Contains($"{table.SchemaName}.{table.TableName}");
                bool isAnonymizedTable = this._configurations.AnonymizationRules.Any(rule => string.Equals(rule.TableName, $"{table.SchemaName}.{table.TableName}",
                                           StringComparison.OrdinalIgnoreCase));

                if (exclusionLevel == ExclusionLevel.NoExcludedTables)
                    if (isExcludedTable) continue;

                if (exclusionLevel == ExclusionLevel.NoExcludedTablesAndAnonymized)
                {
                    if (isAnonymizedTable || isExcludedTable) continue;
                }

                string entryName = $"[{table.SchemaName}].[{table.TableName}]";
                listTableEntries.Add($"{entryName.Length},{entryName}");
            }
            return $"{listTableEntries.Count},{string.Join(",", listTableEntries)}";
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
