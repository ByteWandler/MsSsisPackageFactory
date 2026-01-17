using MsSsisPackageFactory.MetadataManagement.Model;
using MsSsisPackageFactory.FactoryEngine;
using MsSsisPackageFactory.MetadataManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MsSsisPackageFactory.Orchestration
{
    public class FactoryController
    {
        private readonly IMetadataProvider _metadataProvider;
        private readonly IConfigurationProvider _configProvider;
        private readonly IConsistencyValidator _validator;
        private readonly IPackageBuilder _packageBuilder;

        public FactoryController(IMetadataProvider metadataProvider, IConfigurationProvider configProvider, IConsistencyValidator validator, IPackageBuilder packageBuilder)
        {
            this._metadataProvider = metadataProvider;
            this._configProvider = configProvider;
            this._validator = validator;
            this._packageBuilder = packageBuilder;
        }

        public void Run()
        {
            string templateFileName = "replicateDb.dtsx";
            string templateDirectory = @"..\..\..\..\MsSsisPackageFactoryTemplate";

            DtsxFileFactory dtsxFileFactory = new DtsxFileFactory();

            Directory.SetCurrentDirectory(templateDirectory);

            dtsxFileFactory.CreatePackage(this._metadataProvider.GetSchema(), this._configProvider.LoadConfiguration(""), templateFileName);
        }
    }
}
