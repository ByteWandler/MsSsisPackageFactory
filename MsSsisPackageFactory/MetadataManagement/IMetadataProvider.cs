using MsSsisPackageFactory.MetadataManagement.Model;

namespace MsSsisPackageFactory.MetadataManagement
{
    public interface IMetadataProvider
    {
        DbMetaData GetSchema();
    }
}
