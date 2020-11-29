using ApprovalTests.Reporters;
using Microsoft.VisualStudio.TestTools.UnitTesting;



namespace Acklann.Cecrets
{
    [TestClass]
    public class Startup
    {
        [AssemblyCleanup]
        public static void Cleanup()
        {
            ApprovalTests.Maintenance.ApprovalMaintenance.CleanUpAbandonedFiles();
        }


    }
}