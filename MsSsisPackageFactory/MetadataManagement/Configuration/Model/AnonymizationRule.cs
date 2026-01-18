using System.Xml.Serialization;

namespace MsSsisPackageFactory.MetadataManagement.Configuration.Model
{
    public class AnonymizationRule
    {
        [XmlElement("TableName")]
        public string TableName { get; set; } = string.Empty;

        [XmlElement("ColumnName")]
        public string ColumnName { get; set; } = string.Empty;

        [XmlElement("Method")]
        public string Method { get; set; } = string.Empty; // "Mask", "Hash", etc.
    }
}
