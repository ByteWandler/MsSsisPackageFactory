using MsSsisPackageFactory.FactoryEngine;
using MsSsisPackageFactory.PackageFactoryConfiguration.UserConfiguration;
using MsSsisPackageFactory.PackageFactoryConfiguration.DbMetadata;
using MsSsisPackageFactory.PackageFactoryConfiguration.Validation;

namespace MsSsisPackageFactory.Orchestration
{
    /// <summary>
    /// Der zentrale Controller (Orchestrator) für die SSIS-Package-Factory.
    /// Koordiniert den Ablauf der Paketgenerierung: lädt Daten, validiert sie und startet die Erstellung.
    /// </summary>
    public class FactoryController
    {
        public IMetadataProvider MetadataProvider { get; private set; }
        public IConfigurationProvider ConfigProvider { get; private set; }
        public IConsistencyValidator Validator { get; private set; }
        public IPackageBuilder PackageBuilder { get; private set; }

        /// <summary>
        /// Initialisiert eine neue Instanz des FactoryControllers mit den erforderlichen Abhängigkeiten.
        /// </summary>
        /// <param name="metadataProvider">Liefert die Metadaten der Datenbank (Schema-Informationen).</param>
        /// <param name="configProvider">Lädt die Benutzerkonfiguration (z.B. aus einer JSON/XML-Datei).</param>
        /// <param name="validator">Prüft die Konsistenz zwischen Metadaten und Konfiguration.</param>
        /// <param name="packageBuilder">Hauptkomponente zur Generierung des SSIS-Pakets. (HINWEIS: Wird in Run() aktuell umgangen)</param>
        /// </summary>
        public FactoryController()
        {
            this.ConfigProvider = new ConfigdataManager();
            this.MetadataProvider = new DbMetadataManager(this.ConfigProvider.CurrentConfiguration.Database.ConnectionString);
            this.Validator = new ConsistencyValidator();
            this.PackageBuilder = new DtsxFileBuilder(this.ConfigProvider.CurrentConfiguration, this.MetadataProvider.CurrentDbMetaData);
        }
    }
}
