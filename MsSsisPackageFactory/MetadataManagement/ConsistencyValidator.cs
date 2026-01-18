using MsSsisPackageFactory.MetadataManagement.Configuration.Model;
using MsSsisPackageFactory.MetadataManagement.DbMetadata.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsSsisPackageFactory.MetadataManagement
{
    internal class ConsistencyValidator : IConsistencyValidator
    {
        public ValidationResult Validate(DbMetaData dbMetaData, UserConfiguration userConfig)
        {
            var result = new ValidationResult();

            // 1. Prüfe ausgeschlossene Tabellen
            ValidateExcludedTables(dbMetaData, userConfig, result);

            // 2. Prüfe Anonymisierungsregeln
            ValidateAnonymizationRules(dbMetaData, userConfig, result);

            // 3. Prüfe Schema-Konsistenz
            ValidateSchemaConsistency(userConfig, result);

            result.IsValid = !result.Errors.Any();
            return result;
        }

        private void ValidateExcludedTables(DbMetaData dbMetaData, UserConfiguration userConfig, ValidationResult result)
        {
            foreach (var tableName in userConfig.ExcludedTables)
            {
                // Versuche, Tabellennamen zu parsen (kann "Table" oder "Schema.Table" sein)
                var (schema, table) = ParseTableName(tableName, userConfig.Database.Schema);

                if (!dbMetaData.TableExists(schema, table))
                {
                    result.AddError($"Ausgeschlossene Tabelle existiert nicht: {schema}.{table}");
                }
            }
        }

        private void ValidateAnonymizationRules(DbMetaData dbMetaData, UserConfiguration userConfig, ValidationResult result)
        {
            foreach (var rule in userConfig.AnonymizationRules)
            {
                // Tabellennamen parsen
                var (schema, table) = ParseTableName(rule.TableName, userConfig.Database.Schema);

                // 1. Prüfe, ob Tabelle existiert
                if (!dbMetaData.TableExists(schema, table))
                {
                    result.AddError($"Anonymisierungsregel: Tabelle existiert nicht: {schema}.{table}");
                    continue;
                }

                var dbTable = dbMetaData.GetTable(schema, table);

                // 2. Prüfe, ob Spalte existiert
                if (dbTable.Columns.All(c => c.ColumnName != rule.ColumnName))
                {
                    result.AddError($"Anonymisierungsregel: Spalte '{rule.ColumnName}' existiert nicht in Tabelle {schema}.{table}");
                }

                // 3. Prüfe, ob Methode gültig ist (optional)
                if (!IsValidAnonymizationMethod(rule.Method))
                {
                    result.AddWarning($"Anonymisierungsregel: Unbekannte Methode '{rule.Method}' für {schema}.{table}.{rule.ColumnName}");
                }
            }
        }

        private void ValidateSchemaConsistency(UserConfiguration userConfig, ValidationResult result)
        {
            // Prüfe, ob ConnectionString gesetzt ist
            if (string.IsNullOrWhiteSpace(userConfig.Database.ConnectionString))
            {
                result.AddError("ConnectionString ist nicht konfiguriert");
            }

            // Prüfe, ob Schema gesetzt ist
            if (string.IsNullOrWhiteSpace(userConfig.Database.Schema))
            {
                result.AddWarning("Schema ist nicht explizit konfiguriert, verwende 'dbo' als Standard");
            }
        }

        private (string Schema, string Table) ParseTableName(string tableName, string defaultSchema)
        {
            // Unterstützt sowohl "Table" als auch "Schema.Table"
            var parts = tableName.Split('.');

            if (parts.Length == 1)
            {
                return (defaultSchema, parts[0].Trim());
            }
            else if (parts.Length == 2)
            {
                return (parts[0].Trim(), parts[1].Trim());
            }

            // Fallback
            return (defaultSchema, tableName.Trim());
        }

        private bool IsValidAnonymizationMethod(string method)
        {
            var validMethods = new[] { "Mask", "Hash", "Scramble", "Nullify", "Pseudonymize" };
            return validMethods.Contains(method, StringComparer.OrdinalIgnoreCase);
        }
    }
}

