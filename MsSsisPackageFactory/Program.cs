using MsSsisPackageFactory.FactoryEngine;
using MsSsisPackageFactory.PackageFactoryConfiguration.UserConfiguration;
using MsSsisPackageFactory.PackageFactoryConfiguration.DbMetadata;
using MsSsisPackageFactory.Orchestration;
using MsSsisPackageFactory.PackageFactoryConfiguration.Validation;

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
            FactoryController fController = new();
            ValidationResult vResult = fController.Validator.Validate(fController.MetadataProvider.CurrentDbMetaData, fController.ConfigProvider.CurrentConfiguration);
            if (vResult.IsValid)
            {
                fController.PackageBuilder.Build(); 
            }
            else
            {
                Console.WriteLine($"Consistencyvalidation-Error: {string.Join(",", vResult.Errors)}");
                Console.ReadKey();
            }
        }
    }
}
