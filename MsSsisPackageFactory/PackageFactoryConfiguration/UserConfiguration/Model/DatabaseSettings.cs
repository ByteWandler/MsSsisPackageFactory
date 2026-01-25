using System.Xml.Serialization;

namespace MsSsisPackageFactory.PackageFactoryConfiguration.UserConfiguration.Model
{
    public class DatabaseSettings
    {
        [XmlElement("ConnectionString")]
        public string ConnectionString { get; set; } = "Server=.;Database=Northwind;Trusted_Connection=True;";

        [XmlElement("Schema")]
        public string Schema { get; set; } = "dbo";

        // Hilfsmethode für einfachen Zugriff
        public string GetDefaultSchema() => string.IsNullOrEmpty(Schema) ? "dbo" : Schema;
    }
}
