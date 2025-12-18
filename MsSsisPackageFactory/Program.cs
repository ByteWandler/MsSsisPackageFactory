using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Dts.Runtime;

namespace MsSsisPackageFactory
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Application application = new Application();
            Package package = new Package();
            application.SaveToXml("C:\\Temp\\DbReplicator.dtsx", package, null);
        }
    }
}
