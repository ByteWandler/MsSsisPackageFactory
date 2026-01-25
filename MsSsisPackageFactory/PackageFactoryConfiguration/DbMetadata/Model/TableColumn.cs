namespace MsSsisPackageFactory.PackageFactoryConfiguration.DbMetadata.Model
{
    public class TableColumn
    {
        public string ColumnName { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public int MaxLength { get; set; }
        public bool IsNullable { get; set; }

        public string FullDescription
        {
            get
            {
                return $"{ColumnName} ({DataType}" +
                (MaxLength > 0 ? $"({MaxLength})" : "") +
                (IsNullable ? ", NULL" : ", NOT NULL") + ")";
            }
        }

        public override string ToString()
        {
            return this.ColumnName;
        }
    }
}
