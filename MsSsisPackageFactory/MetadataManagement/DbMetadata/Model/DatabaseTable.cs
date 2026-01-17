using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsSsisPackageFactory.MetadataManagement.DbMetadata.Model
{
    public class DatabaseTable
    {
        public string SchemaName { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public List<TableColumn> Columns { get; set; } = new();

        public string FullName => $"{SchemaName}.{TableName}";
    }
}
