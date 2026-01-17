using MsSsisPackageFactory.MetadataManagement.Configuration.Model;
using MsSsisPackageFactory.MetadataManagement.DbMetadata.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsSsisPackageFactory.MetadataManagement
{
    internal class ConsistencyValidator : IConsistencyValidator
    {
        public bool IsConsistent(DbMetaData dbMetaData, UserConfiguration userConfig)
        {
            throw new NotImplementedException();
        }
    }
}
