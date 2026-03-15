namespace MsSsisPackageFactory.PackageFactoryConfiguration.DbMetadata.Model
{
    internal class TableColumn
    {
        internal string ColumnName { get; set; } = string.Empty;
        internal string DataType { get; set; } = string.Empty;
        internal int MaxLength { get; set; }
        internal bool IsNullable { get; set; }

        internal string FullDescription
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
