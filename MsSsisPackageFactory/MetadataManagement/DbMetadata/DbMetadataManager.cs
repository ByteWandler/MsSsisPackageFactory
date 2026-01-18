using Microsoft.Data.SqlClient;
using MsSsisPackageFactory.MetadataManagement.DbMetadata.Model;

namespace MsSsisPackageFactory.MetadataManagement.DbMetadata
{
    /// <summary>
    /// Implementiert den <see cref="IMetadataProvider"/> zur Verwaltung von Datenbank-Metadaten.
    /// Diese Komponente ist für das Abrufen und Bereitstellen von Schema-Informationen
    /// (Tabellen, Spalten, Beziehungen) aus einer SQL Server-Datenbank verantwortlich.
    /// </summary>
    /// <remarks>
    /// Im aktuellen Prototyp-Zustand stellt diese Klasse eine <b>Mock-Implementierung</b> dar,
    /// die fest kodierte Testdaten zurückgibt. In einer produktiven Implementierung würde sie
    /// eine aktive Datenbankverbindung nutzen, um Metadaten aus den Systemkatalogen (z.B. sys.tables)
    /// abzufragen.
    /// 
    /// Entwurfsentscheidung: Die Metadaten werden einmalig geladen und über die Property
    /// <see cref="CurrentDbMetaData"/> zur Verfügung gestellt, um Mehrfachabfragen zu vermeiden.
    /// </remarks>
    internal class DbMetadataManager : IMetadataProvider
    {
        private readonly string _connectionString;
        public DbMetaData CurrentDbMetaData { get; private set; }

        public DbMetadataManager(string connectionString)
        {
            _connectionString = connectionString;
            CurrentDbMetaData = LoadMetadata();
        }

        private DbMetaData LoadMetadata()
        {
            var dbMetaData = new DbMetaData();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            // Tabellen und Spalten abfragen
            string query = @"
                SELECT 
                    s.name AS SchemaName,
                    t.name AS TableName,
                    c.name AS ColumnName,
                    ty.name AS DataType,
                    c.max_length AS MaxLength,
                    c.is_nullable AS IsNullable
                FROM sys.tables t
                INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                INNER JOIN sys.columns c ON t.object_id = c.object_id
                INNER JOIN sys.types ty ON c.user_type_id = ty.user_type_id
                WHERE t.is_ms_shipped = 0
                ORDER BY s.name, t.name, c.column_id";

            using var command = new SqlCommand(query, connection);
            using var reader = command.ExecuteReader();

            string currentTableKey = string.Empty;
            DatabaseTable currentTable = null;

            while (reader.Read())
            {
                string schemaName = reader["SchemaName"].ToString();
                string tableName = reader["TableName"].ToString();
                string tableKey = $"{schemaName}.{tableName}";

                // Neue Tabelle?
                if (tableKey != currentTableKey)
                {
                    currentTable = new DatabaseTable
                    {
                        SchemaName = schemaName,
                        TableName = tableName
                    };
                    dbMetaData.Tables.Add(currentTable);
                    currentTableKey = tableKey;
                }

                // Spalte hinzufügen
                var column = new TableColumn
                {
                    ColumnName = reader["ColumnName"].ToString(),
                    DataType = reader["DataType"].ToString(),
                    MaxLength = Convert.ToInt32(reader["MaxLength"]),
                    IsNullable = Convert.ToBoolean(reader["IsNullable"])
                };
                currentTable.Columns.Add(column);
            }

            return dbMetaData;
        }

    }
}
