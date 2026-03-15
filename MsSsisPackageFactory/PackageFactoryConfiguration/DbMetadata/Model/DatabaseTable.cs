namespace MsSsisPackageFactory.PackageFactoryConfiguration.DbMetadata.Model
{
    internal class DatabaseTable
    {
        internal string SchemaName { get; set; } = string.Empty;
        internal string TableName { get; set; } = string.Empty;
        internal List<TableColumn> Columns { get; set; } = new();
        internal string FullName => $"{SchemaName}.{TableName}";
    }
}
