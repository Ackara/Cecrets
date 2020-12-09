using Microsoft.Build.Framework;
using Newtonsoft.Json;
using System.IO;

namespace Acklann.Cecrets.MSBuild
{
    public class CopyJsonProperties : ITask
    {
        [Required]
        public string JPath { get; set; }

        [Required]
        public ITaskItem SourceFile { get; set; }

        public ITaskItem DestinationFile { get; set; }

        public bool Execute()
        {
            const string fullPath = "FullPath";
            string src = SourceFile.GetMetadata(fullPath);
            string dest = (DestinationFile?.GetMetadata(fullPath) ?? Path.Combine(Path.GetDirectoryName(BuildEngine.ProjectFileOfTaskNode), "appsettings.json"));

            foreach (string path in JPath.Split(new char[] { ';', ',' }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                JsonEditor.CopyProperty(src, dest, path);
                BuildEngine.LogMessageEvent(new BuildMessageEventArgs(
                    $"Copied  '{Path.GetFileName(src)}':'{path}' property to '{Path.GetFileName(dest)}'", null, nameof(CopyJsonProperties), MessageImportance.Normal));
            }

            return true;
        }

        #region Backing Members

        public ITaskHost HostObject { get; set; }

        public IBuildEngine BuildEngine { get; set; }

        #endregion Backing Members
    }
}