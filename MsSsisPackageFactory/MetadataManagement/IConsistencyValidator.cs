using MsSsisPackageFactory.MetadataManagement.Model;

namespace MsSsisPackageFactory.MetadataManagement
{
    public interface IConsistencyValidator
    {
        bool IsConsistent(DbMetaData dbMetaData, UserConfiguration userConfig);
    }
}
