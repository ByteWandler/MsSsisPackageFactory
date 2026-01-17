using MsSsisPackageFactory.FactoryEngine;
using MsSsisPackageFactory.MetadataManagement;
using MsSsisPackageFactory.Orchestration;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

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
            var factoryController = new FactoryController(new DbMetadataManager(), new ConfigdataManager(), new ConsistencyValidator(), new DtsxFileFactory());
        }
    }
}
