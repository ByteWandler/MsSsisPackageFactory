namespace MsSsisPackageFactory.PackageFactoryConfiguration.DbMetadata
{
    /// <summary>
    /// Stellt eine statische Zuordnung von SQL Server-Datentypen zu SSIS-Datentyp-Monikern bereit.
    /// </summary>
    public class DatatypeMapper
    {
        private static readonly Dictionary<string, string> _typeMap = new()
        {
            // SQL Server Data Types -> SSIS Data Type Moniker (DTS)
            {"int", "i4"},
            {"smallint", "i2"},
            {"tinyint", "i1"},
            {"bigint", "i8"},
            {"char", "wstr"},
            {"varchar", "wstr"},
            {"nvarchar", "wstr"},
            {"nchar", "wstr"},
            {"text", "wstr"},
            {"ntext", "wstr"},
            {"datetime", "date"},
            {"smalldatetime", "date"},
            {"date", "date"},
            {"time", "date"}, 
            {"bit", "boolean"},
            {"decimal", "numeric"},
            {"numeric", "numeric"},
            {"money", "numeric"},
            {"smallmoney", "numeric"},
            {"float", "r8"},
            {"real", "r4"},
            {"uniqueidentifier", "guid"},
            {"varbinary", "byte[]"},
            {"image", "byte[]"}
        };

        /// <summary>
        /// Ermittelt den passenden SSIS-Datentyp-Moniker für einen gegebenen SQL Server-Datentyp.
        /// </summary>
        /// <param name="sqlDataType">Der SQL Server-Datentyp, z.B. "nvarchar(50)" oder "int".</param>
        /// <returns>Den zugehörigen SSIS-Datentyp-Moniker (z.B. "wstr"). Bei unbekannten Typen wird "wstr" als Fallback zurückgegeben.</returns>
        public static string GetSsisDataType(string sqlDataType)
        {
            // Normalisiere: entferne Längenangabe wie '(50)' und konvertiere zu Lowercase
            string normalizedType = sqlDataType.ToLowerInvariant().Split('(')[0].Trim();

            if (_typeMap.TryGetValue(normalizedType, out string ssisType))
            {
                return ssisType;
            }

            // Fallback für unbekannte Typen
            return "wstr";
        }
    }
}
