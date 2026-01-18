using MsSsisPackageFactory.MetadataManagement.Configuration.Model;
using MsSsisPackageFactory.MetadataManagement.DbMetadata.Model;

namespace MsSsisPackageFactory.MetadataManagement
{
    /// <summary>
    /// Definiert den Vertrag für eine Komponente zur Konsistenzprüfung zwischen Datenbank-Metadaten und Benutzerkonfiguration.
    /// </summary>
    /// <remarks>
    /// Implementierungen dieser Schnittstelle stellen sicher, dass die vom Benutzer spezifizierten
    /// Verarbeitungsregeln (z.B. auszuschließende Tabellen oder zu anonymisierende Spalten)
    /// mit der tatsächlichen Struktur der Zieldatenbank kompatibel sind.
    /// Dies ist ein wesentlicher Schritt, um Laufzeitfehler aufgrund inkonsistenter Konfigurationen zu vermeiden.
    /// </remarks>
    public interface IConsistencyValidator
    {
        /// <summary>
        /// Prüft, ob die angegebene Benutzerkonfiguration mit den gegebenen Datenbank-Metadaten konsistent ist.
        /// </summary>
        /// <param name="dbMetaData">Die Metadaten der Datenbank (Schema-Informationen, Tabellen, Spalten).</param>
        /// <param name="userConfig">Die vom Benutzer bereitgestellte Konfiguration (Filter, Anonymisierungsregeln).</param>
        /// <returns>
        /// <c>true</c>, wenn alle in der Konfiguration referenzierten Datenbankobjekte in den Metadaten existieren
        /// und die spezifizierten Operationen auf sie anwendbar sind; andernfalls <c>false</c>.
        /// </returns>
        /// <example>
        /// Eine Implementierung könnte prüfen, ob alle in <paramref name="userConfig"/> aufgeführten Tabellen
        /// tatsächlich in <paramref name="dbMetaData"/> vorhanden sind.
        /// </example>
        ValidationResult Validate(DbMetaData dbMetaData, UserConfiguration userConfig);
    }
}
