using ApprovalTests.Reporters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: UseReporter(typeof(DiffReporter))]
[assembly: ApprovalTests.Namers.UseApprovalSubdirectory("../approved-results")]

namespace Acklann.Cecrets
{
    [TestClass]
    public class Startup
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext _)
        {
        }
    }
}