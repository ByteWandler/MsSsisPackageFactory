using System.Xml.Serialization;

namespace MsSsisPackageFactory.PackageFactoryConfiguration.UserConfiguration.Model
{
    [XmlRoot("FactoryConfiguration")]
    internal class UserConfigurationModel
    {
        [XmlElement("TemplateFileName")]
        internal string TemplateFileName { get; set; } = string.Empty;

        [XmlElement("OutputDirectory")]
        internal string OutputDirectory { get; set; } = string.Empty;

        [XmlArray("ExcludedTables")]
        [XmlArrayItem("Table")]
        internal List<string> ExcludedTables { get; set; } = new();

        [XmlArray("AnonymizationRules")]
        [XmlArrayItem("Rule")]
        internal List<AnonymizationRule> AnonymizationRules { get; set; } = new();

        [XmlElement("Database")]
        internal DatabaseSettings Database { get; set; } = new DatabaseSettings();
    }
}

