using MsSsisPackageFactory.MetadataManagement.Configuration.Model;
using MsSsisPackageFactory.MetadataManagement.DbMetadata.Model;

namespace MsSsisPackageFactory.FactoryEngine
{
    /// <summary>
    /// Definiert den Vertrag für eine Komponente, die SSIS-Pakete generiert.
    /// Dieses Interface abstrahiert die konkrete Implementierung der Paketerstellung
    /// und ermöglicht so eine lose Kopplung innerhalb der Factory-Architektur.
    /// </summary>
    public interface IPackageBuilder
    {
        /// <summary>
        /// Erstellt ein SSIS-Paket basierend auf Datenbank-Metadaten und Benutzerkonfiguration.
        /// </summary>
        /// <param name="dbMetaData">
        /// Die Metadaten der Quelldatenbank (Schema, Tabellen, Spalten etc.),
        /// die die Struktur der zu übertragenden Daten definieren.
        /// </param>
        /// <param name="userConfig">
        /// Die Benutzerkonfiguration, die Steuerparameter wie auszuschließende Tabellen,
        /// Anonymisierungsregeln und Pfade zum Template enthält.
        /// </param>
        /// <remarks>
        /// Implementierungen dieser Methode sind für die konkrete Logik der
        /// Paketgenerierung verantwortlich, z.B. das Laden eines Templates und
        /// dessen Anpassung anhand der übergebenen Parameter.
        /// Die Methode wirft keine Exceptions für Validierungsfehler; diese sollten
        /// vor dem Aufruf durch den <see cref="IConsistencyValidator"/> geprüft werden.
        /// </remarks>
        void CreatePackage(DbMetaData dbMetaData, UserConfiguration userConfig);
    }
}
