using ApprovalTests;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.IO;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

namespace Acklann.Cecrets.Tests
{
    [TestClass]
    public class MSBuildTest
    {
        [TestMethod]
        public void Can_invoke_CopyJsonProperty_task()
        {
            // Arrange
            var projectFile = Path.Combine(Path.GetTempPath(), "msbuild-cecrets.proj");
            var resultFile = Path.Combine(Path.GetTempPath(), "custom-appsettings.json");

            var mockEngine = Mock.Create<IBuildEngine>();
            Mock.Arrange(() => mockEngine.LogMessageEvent(Arg.IsAny<BuildMessageEventArgs>()))
                .OccursAtLeast(1);
            Mock.Arrange(() => mockEngine.ProjectFileOfTaskNode)
                .Returns(projectFile);

            var mockHost = Mock.Create<ITaskHost>();

            var sourceFile = Mock.Create<ITaskItem>();
            Mock.Arrange(() => sourceFile.GetMetadata("FullPath"))
                .Returns(TestData.GetFile("config1.json"))
                .OccursAtLeast(1);

            var destinationFile = Mock.Create<ITaskItem>();
            Mock.Arrange(() => destinationFile.GetMetadata("FullPath"))
                .Returns(resultFile);

            var sut = new MSBuild.CopyJsonProperties
            {
                BuildEngine = mockEngine,
                HostObject = mockHost,
                SourceFile = sourceFile,
                JPath = "local.*"
            };

            // Act
            var exitCode1 = sut.Execute();
            var result1 = File.ReadAllText(Path.Combine(Path.GetTempPath(), "appsettings.json"));

            sut.DestinationFile = destinationFile;
            var exitCode2 = sut.Execute();
            var result2 = File.ReadAllText(resultFile);

            // Assert
            result1.ShouldBe(result2);
            Approvals.Verify(result1);
            exitCode1.ShouldBeTrue();
            exitCode2.ShouldBeTrue();

            sourceFile.AssertAll();
            mockEngine.AssertAll();
        }

        [TestMethod]
        public void Can_invoke_GetJsonProperty_task()
        {
            // Arrange

            string filePath = TestData.GetFile("config2*");

            var sourceFile = Mock.Create<ITaskItem>();
            sourceFile.Arrange(x => x.GetMetadata(Arg.AnyString))
                .Returns(filePath);

            var sut = new MSBuild.GetJsonProperty
            {
                BuildEngine = Mock.Create<IBuildEngine>(),
                HostObject = Mock.Create<ITaskHost>(),
                SourceFile = sourceFile
            };

            // Act

            sut.JPath = "Serilog.MinimumLevel.Default";
            sut.Execute();
            string result1 = sut.Value;

            sut.JPath = "Serilog.WriteTo[0].Name";
            sut.Execute();
            string result2 = sut.Value;

            sut.JPath = "Serilog.WriteTo[0].Args.serverUrl";
            sut.Execute();
            string result3 = sut.Value;

            // Assert

            result1.ShouldBe("Information");
            result2.ShouldBe("Seq");
            result3.ShouldBe("http://localhost:5341");
        }

        //[TestMethod]
        public void Can_invoke_SetJsonProperty_task()
        {
            // Arrange

            string testFile = Path.Combine(Path.GetTempPath(), "set-prop-test.json");
            if (File.Exists(testFile)) File.Delete(testFile);
            File.WriteAllText(testFile, File.ReadAllText(TestData.GetFile("config2*")));

            var sourceFile = Mock.Create<ITaskItem>();
            Mock.Arrange(() => sourceFile.GetMetadata(Arg.AnyString))
                .Returns(testFile);

            var sut = new MSBuild.SetJsonProperty
            {
                SourceFile = sourceFile,
                JPath = "Serilog.WriteTo[0].Args",
                Value = "abc"
            };

            // Act

            var ok = sut.Execute();

            // Assert

            ok.ShouldBeTrue();
            Approvals.VerifyFile(testFile);
        }
    }
}