using MsSsisPackageFactory.FactoryEngine;
using MsSsisPackageFactory.MetadataManagement;
using MsSsisPackageFactory.MetadataManagement.Configuration;
using MsSsisPackageFactory.MetadataManagement.DbMetadata;

namespace MsSsisPackageFactory.Orchestration
{
    /// <summary>
    /// Der zentrale Controller (Orchestrator) für die SSIS-Package-Factory.
    /// Koordiniert den Ablauf der Paketgenerierung: lädt Daten, validiert sie und startet die Erstellung.
    /// </summary>
    public class FactoryController
    {
        private readonly IMetadataProvider _metadataProvider;
        private readonly IConfigurationProvider _configProvider;
        private readonly IConsistencyValidator _validator;
        private readonly IPackageBuilder _packageBuilder;

        /// <summary>
        /// Initialisiert eine neue Instanz des FactoryControllers mit den erforderlichen Abhängigkeiten.
        /// </summary>
        /// <param name="metadataProvider">Liefert die Metadaten der Datenbank (Schema-Informationen).</param>
        /// <param name="configProvider">Lädt die Benutzerkonfiguration (z.B. aus einer JSON/XML-Datei).</param>
        /// <param name="validator">Prüft die Konsistenz zwischen Metadaten und Konfiguration.</param>
        /// <param name="packageBuilder">Hauptkomponente zur Generierung des SSIS-Pakets. (HINWEIS: Wird in Run() aktuell umgangen)</param>
        /// </summary>
        public FactoryController(IMetadataProvider metadataProvider, IConfigurationProvider configProvider, IConsistencyValidator validator, IPackageBuilder packageBuilder)
        {
            this._metadataProvider = metadataProvider;
            this._configProvider = configProvider;
            this._validator = validator;
            this._packageBuilder = packageBuilder;
        }

        /// <summary>
        /// Führt den Hauptablauf der Paketgenerierung aus(Einstiegspunkt nach der Initialisierung).
        /// 1. Lädt Metadaten und Konfiguration.
        /// 2. Validiert die Konsistenz der Eingabedaten.
        /// 3. Generiert das SSIS-Paket basierend auf einem statischen Template.
        /// </summary>
        public void Run()
        {
            if (!File.Exists(this._configProvider.CurrentConfiguration.TemplateFileName))
            {
                throw new FileNotFoundException("Die angegebene Templatedatei existiert nicht. Überprüfen Sie die Pfadangabe.");
            }
            
            this._packageBuilder.CreatePackage(this._metadataProvider.CurrentDbMetaData, this._configProvider.CurrentConfiguration);
        }
    }
}
