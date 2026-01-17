using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace MsSsisPackageFactory.MetadataManagement.DbMetadata.Model
{
    public class DbMetaData
    {
        public List<DatabaseTable> Tables { get; set; } = new();

        // Hilfsmethoden für einfachen Zugriff
        public DatabaseTable GetTable(string schemaName, string tableName) =>
            Tables.FirstOrDefault(t =>
                t.SchemaName == schemaName &&
                t.TableName == tableName);

        public bool TableExists(string schemaName, string tableName)
        {
            foreach(DatabaseTable table in Tables)
            {
                if(table.SchemaName == schemaName &&
                    table.TableName == tableName)
                {
                    return true;
                }
            }
            return false;
        }
            
    }
}
