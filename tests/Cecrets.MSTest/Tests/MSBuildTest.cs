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

            var sut = new MSBuild.CopyJsonProperty
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
    }
}