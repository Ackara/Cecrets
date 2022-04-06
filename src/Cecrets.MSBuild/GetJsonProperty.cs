using Microsoft.Build.Framework;
using System.IO;

namespace Acklann.Cecrets.MSBuild
{
    public class GetJsonProperty : ITask
    {
        [Required]
        public ITaskItem SourceFile { get; set; }

        [Required]
        public string JPath { get; set; }

        [Output]
        public string Value { get; set; }

        public bool Execute()
        {
            string filePath = SourceFile.GetMetadata("FullPath");
            Value = JsonEditor.GetValue(filePath, JPath);
            BuildEngine?.LogMessageEvent(new BuildMessageEventArgs(
                    $"Set '{JPath}' in {Path.GetFileName(filePath)}.", null, nameof(SetJsonProperty), MessageImportance.Normal));

            return true;
        }

        #region Backing Members

        public ITaskHost HostObject { get; set; }

        public IBuildEngine BuildEngine { get; set; }

        #endregion Backing Members
    }
}