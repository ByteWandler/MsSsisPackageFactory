using MsSsisPackageFactory.PackageFactoryConfiguration.DbMetadata.Model;

namespace MsSsisPackageFactory.PackageFactoryConfiguration.DbMetadata
{
    /// <summary>
    /// Definiert den Vertrag für eine Komponente, die Metadaten einer Datenbank bereitstellt.
    /// </summary>
    /// <remarks>
    /// Implementierungen dieser Schnittstelle sind für das Abrufen von Strukturinformationen
    /// (Schema) der Ziel-Datenbank verantwortlich, wie z. B. Tabellen, Spalten und Datentypen.
    /// Im Prototyp kann dies durch eine Mock-Implementierung mit statischen Testdaten realisiert werden.
    /// </remarks>
    internal interface IMetadataProvider
    {
        /// <summary>
        /// Ruft die aktuell geladenen Datenbank-Metadaten ab.
        /// </summary>
        /// <value>
        /// Ein <see cref="DbMetaData"/>-Objekt, das das Schema der Datenbank repräsentiert.
        /// Der Wert wird typischerweise im Konstruktor der implementierenden Klasse initialisiert.
        /// </value>
        DbMetaData CurrentDbMetaData { get; }
    }
}
