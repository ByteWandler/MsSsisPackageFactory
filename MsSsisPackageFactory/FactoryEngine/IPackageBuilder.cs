using MsSsisPackageFactory.MetadataManagement.Model;

namespace MsSsisPackageFactory.FactoryEngine
{
    public interface IPackageBuilder
    {
        void CreatePackage(DbMetaData dbMetaData, UserConfiguration userConfig, string templateFilename);
    }
}
