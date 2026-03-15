using System.Xml.Serialization;

namespace MsSsisPackageFactory.PackageFactoryConfiguration.UserConfiguration.Model
{
    internal class DatabaseSettings
    {
        [XmlElement("ConnectionString")]
        internal string ConnectionString { get; set; } = "Server=.;Database=Northwind;Trusted_Connection=True;";

        [XmlElement("Schema")]
        internal string Schema { get; set; } = "dbo";

        // Hilfsmethode für einfachen Zugriff
        internal string GetDefaultSchema() => string.IsNullOrEmpty(Schema) ? "dbo" : Schema;
    }
}
