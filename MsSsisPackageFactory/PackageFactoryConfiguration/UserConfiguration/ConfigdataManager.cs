using MsSsisPackageFactory.PackageFactoryConfiguration.UserConfiguration.Model;
using System.Reflection;
using System.Xml.Serialization;

namespace MsSsisPackageFactory.PackageFactoryConfiguration.UserConfiguration
{
    internal class ConfigdataManager : IConfigurationProvider
    {
        private readonly string _defaultFilePath;
        public UserConfigurationModel CurrentConfiguration { get; private set; }

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

        private UserConfigurationModel LoadConfiguration()
        {
            if (!File.Exists(_defaultFilePath))
            {
                throw new FileNotFoundException($"Konfigurationsdatei nicht gefunden: {_defaultFilePath}");
            }

            var serializer = new XmlSerializer(typeof(UserConfigurationModel));
            using var reader = new StreamReader(_defaultFilePath);
            object desXml = serializer.Deserialize(reader);
            return (UserConfigurationModel)desXml;
        }
    }
}
