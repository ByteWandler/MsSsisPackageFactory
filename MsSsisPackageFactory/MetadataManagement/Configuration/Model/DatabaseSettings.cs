using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MsSsisPackageFactory.MetadataManagement.Configuration.Model
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
