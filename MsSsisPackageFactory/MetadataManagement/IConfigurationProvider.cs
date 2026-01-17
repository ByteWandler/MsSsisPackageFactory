using MsSsisPackageFactory.MetadataManagement.Model;

namespace MsSsisPackageFactory.MetadataManagement
{
    public interface IConfigurationProvider
    {
        UserConfiguration LoadConfiguration(string configFilePath);
    }
}
