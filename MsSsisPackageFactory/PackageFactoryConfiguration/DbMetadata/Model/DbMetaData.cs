namespace MsSsisPackageFactory.PackageFactoryConfiguration.DbMetadata.Model
{
    internal class DbMetaData
    {
        internal List<DatabaseTable> Tables { get; set; } = new();

        // Hilfsmethoden für einfachen Zugriff
        internal DatabaseTable GetTable(string schemaName, string tableName)
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


        internal bool TableExists(string schemaName, string tableName)
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
