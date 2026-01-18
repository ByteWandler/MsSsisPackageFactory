namespace MsSsisPackageFactory.MetadataManagement.DbMetadata.Model
{
    public class DbMetaData
    {
        public List<DatabaseTable> Tables { get; set; } = new();

        // Hilfsmethoden für einfachen Zugriff
        public DatabaseTable GetTable(string schemaName, string tableName)
        {
            foreach (DatabaseTable table in Tables)
            {
                if (table.SchemaName.ToLower().Equals(schemaName.ToLower()) &&
                    table.TableName.ToLower().Equals(tableName.ToLower()))
                {
                    return table;
                }
            }
            return null;
        }
            

        public bool TableExists(string schemaName, string tableName)
        {
            foreach(DatabaseTable table in Tables)
            {
                if (table.SchemaName.ToLower().Equals(schemaName.ToLower()) &&
                    table.TableName.ToLower().Equals(tableName.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
