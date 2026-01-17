using MsSsisPackageFactory.MetadataManagement.Configuration.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MsSsisPackageFactory.MetadataManagement.Configuration
{
    internal class ConfigdataManager : IConfigurationProvider
    {
        private readonly string _defaultFilePath;
        public UserConfiguration CurrentConfiguration { get; private set; }

        public ConfigdataManager(string configFileName = "FactoryConfig.xml")
        {
            _defaultFilePath = GetFQFN(configFileName);
            CurrentConfiguration = LoadConfiguration();
        }

        private string GetFQFN(string configFileName)
        {
            // 1. Verzeichnis der aktuell ausgeführten Assembly ermitteln
            string assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // 2. Fallback auf aktuelles Arbeitsverzeichnis, falls Assembly-Pfad null
            string baseDirectory = assemblyDirectory ?? Environment.CurrentDirectory;

            // 3. Vollqualifizierten Pfad zur Konfigurationsdatei erstellen
            return Path.Combine(baseDirectory, configFileName);
        }

        private UserConfiguration LoadConfiguration()
        {
            if (!File.Exists(_defaultFilePath))
            {
                throw new FileNotFoundException($"Konfigurationsdatei nicht gefunden: {_defaultFilePath}");
            }

            var serializer = new XmlSerializer(typeof(UserConfiguration));
            using var reader = new StreamReader(_defaultFilePath);
            return (UserConfiguration)serializer.Deserialize(reader);
        }
    }
}
