using System.Xml.Serialization;

namespace MsSsisPackageFactory.PackageFactoryConfiguration.UserConfiguration.Model
{
    internal class AnonymizationRule
    {
        [XmlElement("TableName")]
        internal string TableName { get; set; } = string.Empty;

        [XmlElement("ColumnName")]
        internal string ColumnName { get; set; } = string.Empty;

        [XmlElement("Method")]
        internal string Method { get; set; } = string.Empty; // "Mask", "Hash", etc.
    }
}
