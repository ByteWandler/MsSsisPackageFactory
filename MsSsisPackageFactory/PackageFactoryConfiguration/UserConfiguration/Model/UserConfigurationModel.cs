using System.Xml.Serialization;

namespace MsSsisPackageFactory.PackageFactoryConfiguration.UserConfiguration.Model
{
    [XmlRoot("FactoryConfiguration")]
    public class UserConfigurationModel
    {
        [XmlElement("TemplateFileName")]
        public string TemplateFileName { get; set; } = string.Empty;

        [XmlElement("OutputDirectory")]
        public string OutputDirectory { get; set; } = string.Empty;

        [XmlArray("ExcludedTables")]
        [XmlArrayItem("Table")]
        public List<string> ExcludedTables { get; set; } = new();

        [XmlArray("AnonymizationRules")]
        [XmlArrayItem("Rule")]
        public List<AnonymizationRule> AnonymizationRules { get; set; } = new();

        [XmlElement("Database")]
        public DatabaseSettings Database { get; set; } = new DatabaseSettings();
    }
}

