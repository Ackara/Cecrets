using ApprovalTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace Acklann.Cecrets.Tests
{
    [TestClass]
    public class JsonTest
    {
        [DataTestMethod]
        [DynamicData(nameof(GetJPaths), DynamicDataSourceType.Method)]
        public void Can_copy_json_property(string label, string jpath)
        {
            // Arrange
            string sourceFilePath = TestData.GetFile("config1.json");
            string resultFilePath = Path.Combine(Path.GetTempPath(), $"cecret-copy-test_{label}.json");
            
            using var approver = ApprovalTests.Namers.ApprovalResults.ForScenario(label);

            // Act
            JsonEditor.CopyProperty(sourceFilePath, resultFilePath, jpath);
            JsonEditor.CopyProperty(sourceFilePath, resultFilePath, jpath);

            // Assert
            Approvals.VerifyFile(resultFilePath);
        }

        [TestMethod]
        public void Can_add_json_property()
        {
            throw new System.NotImplementedException();
        }

        [TestMethod]
        public void Can_get_json_value()
        {
            throw new System.NotImplementedException();
        }

        #region Backing Members

        private static IEnumerable<object[]> GetJPaths()
        {
            yield return new object[] { "value", "nugetKey" };
            yield return new object[] { "object1", "local" };
            yield return new object[] { "object2", "local.datastore" };
        }

        #endregion Backing Members
    }
}