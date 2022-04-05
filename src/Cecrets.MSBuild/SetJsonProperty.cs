using Microsoft.Build.Framework;
using System.IO;

namespace Acklann.Cecrets.MSBuild
{
    public class SetJsonProperty : ITask
    {
        [Required]
        public ITaskItem SourceFile { get; set; }

        [Required]
        public string JPath { get; set; }

        [Required]
        public string Value { get; set; }

        public bool Execute()
        {
            string filePath = SourceFile.GetMetadata("FullPath");
            JsonEditor.SetProperty(filePath, JPath, Value);
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