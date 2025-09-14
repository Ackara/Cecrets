using ApprovalTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Acklann.Cecrets.Tests
{
    [TestClass]
    public class JsonTest
    {
        [ClassInitialize]
        public static void Cleanup(TestContext _)
        {
            if (Directory.Exists(_currentWorkingDirectory)) Directory.Delete(_currentWorkingDirectory, recursive: true);
        }

        [TestMethod]
        [DynamicData(nameof(GetJPaths), DynamicDataSourceType.Method)]
        public void Can_copy_json_property(string jpath)
        {
            // Arrange
            string sourceFilePath = TestData.GetFile("config1.json");
            string resultFilePath = Path.Combine(_currentWorkingDirectory, $"cecret-copy-test.{jpath}.json".Replace('*', '_'));

            using var approver = ApprovalTests.Namers.ApprovalResults.ForScenario(jpath);

            // Act
            if (File.Exists(resultFilePath)) File.Delete(resultFilePath);
            JsonEditor.CopyProperty(sourceFilePath, resultFilePath, jpath);
            JsonEditor.CopyProperty(sourceFilePath, resultFilePath, jpath);

            // Assert
            File.Exists(resultFilePath).ShouldBeTrue();
            Approvals.Verify(File.ReadAllText(resultFilePath));
        }

        [TestMethod]
        public void Can_get_json_value()
        {
            // Arrange
            var sourceFile = TestData.GetFile("config1.json");

            // Act
            var result1 = JsonEditor.GetValue(sourceFile, "nugetKey");
            var result2 = JsonEditor.GetValue(sourceFile, "local:datastore:auth");
            var result3 = JsonEditor.GetValue(sourceFile, "local:datastore:main");
            var result4 = JsonEditor.GetValue(sourceFile, "invaild");

            // Assert
            result1.ShouldNotBeNullOrEmpty();
            result2.ShouldNotBeNullOrEmpty();
            result3.ShouldNotBeNullOrEmpty();
            result4.ShouldBeNullOrEmpty();
        }

        [TestMethod]
        [DynamicData(nameof(GetKey), DynamicDataSourceType.Method)]
        public void Can_set_json_properties(string[] keys)
        {
            // Arrange
            using var approver = ApprovalTests.Namers.ApprovalResults.ForScenario(string.Join("__", keys));
            var line = string.Concat(Enumerable.Repeat('-', 50));

            var sourceFile = Path.Combine(_currentWorkingDirectory, $"secrets-set-test({string.Join(" ", keys)}).json");

            // Act
            int index = 0;
            foreach (var item in keys)
            {
                JsonEditor.SetProperty(sourceFile, item, (index++ % 2 == 0 ? 123 : 456));

                System.Diagnostics.Debug.WriteLine($"arg: {item}");
                var result = File.ReadAllText(sourceFile);
                System.Diagnostics.Debug.WriteLine(result);
                System.Diagnostics.Debug.WriteLine(line);
                System.Diagnostics.Debug.WriteLine(string.Empty);
                System.Diagnostics.Debug.WriteLine(string.Empty);
            }

            // Assert
            Approvals.VerifyFile(sourceFile);
        }

        #region Backing Members

        private static readonly string _currentWorkingDirectory = Path.Combine(Path.GetTempPath(), nameof(Cecrets));

        private static IEnumerable<object[]> GetKey()
        {
            yield return new object[] { new string[] { "a" } };
            yield return new object[] { new string[] { "a", "b", "c" } };
            yield return new object[] { new string[] { "a", "a.b", "a.c" } };
            yield return new object[] { new string[] { "d", "d" } };
            yield return new object[] { new string[] { "a.b.c", "a.b.d", "a.b.e", "x.y.z", "x.y.w" } };
        }

        private static IEnumerable<object[]> GetJPaths()
        {
            yield return new object[] { "donetexist" };
            yield return new object[] { "nugetKey" };
            yield return new object[] { "local" };
            yield return new object[] { "local.datastore" };
            yield return new object[] { "local.*" };
            yield return new object[] { "$.*" };
        }

        #endregion Backing Members
    }
}