using MsSsisPackageFactory.PackageFactoryConfiguration.UserConfiguration.Model;

namespace MsSsisPackageFactory.PackageFactoryConfiguration.UserConfiguration
{
    /// <summary>
    /// Definiert den Vertrag für einen Konfigurationsanbieter innerhalb der SSIS-Package-Factory-Architektur.
    /// </summary>
    /// <remarks>
    /// Dieses Interface folgt dem Prinzip der <b>abstrakten Abhängigkeit</b> und ermöglicht so eine
    /// lose Kopplung zwischen der Geschäftslogik (z. B. <see cref="FactoryController"/>) und der konkreten
    /// Implementierung der Konfigurationsverwaltung (z. B. <see cref="ConfigDataManager"/>).
    /// Eine Implementierung ist dafür verantwortlich, die zur Laufzeit gültige Benutzerkonfiguration
    /// einmalig zu laden und über diese schreibgeschützte Eigenschaft verfügbar zu machen.
    /// </remarks>
    public interface IConfigurationProvider
    {
        // <summary>
        /// Ruft die aktuell gültige Benutzerkonfiguration ab.
        /// </summary>
        /// <value>
        /// Eine vollständig initialisierte Instanz von <see cref="UserConfiguration"/>, die alle
        /// benutzerdefinierten Einstellungen (z. B. Template-Pfade, auszuschließende Tabellen,
        /// Anonymisierungsregeln) enthält. Der Wert ist nach der Instanziierung des Anbieters
        /// garantiert nicht <see langword="null"/>.
        /// </value>
        UserConfigurationModel CurrentConfiguration { get; }
    }
}
