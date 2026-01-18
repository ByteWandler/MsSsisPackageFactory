using MsSsisPackageFactory.FactoryEngine;
using MsSsisPackageFactory.MetadataManagement;
using MsSsisPackageFactory.MetadataManagement.Configuration;
using MsSsisPackageFactory.MetadataManagement.DbMetadata;
using MsSsisPackageFactory.Orchestration;

namespace MsSsisPackageFactory
{
    /// <summary>
    /// Der strukturelle Umfang (Klassen, Interfaces, Namespaces) dieses Projekts dient der reinen Demonstration der Modularität gem. Architekturmodell
    /// und des potenziellen Aufbaus bei Erweiterung. Für rein funktionale Implementierung wird das KISS-Prinzip empfohlen (Keep It Simple and Stupid).
    /// Das ist der Kern von Übersichtlichkeit und Wartbarkeit.
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            IConfigurationProvider configurationProvider = new ConfigdataManager();
            IMetadataProvider metadataProvider = new DbMetadataManager(configurationProvider.CurrentConfiguration.Database.ConnectionString);
            IConsistencyValidator validator = new ConsistencyValidator();
            IPackageBuilder packageBuilder = new DtsxFileFactory(configurationProvider, metadataProvider);

            (new FactoryController(metadataProvider, configurationProvider, validator, packageBuilder)).Run();
        }
    }
}
