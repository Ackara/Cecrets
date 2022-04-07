using Microsoft.Build.Framework;
using Newtonsoft.Json.Linq;
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

            var document = JObject.Parse(File.ReadAllText(filePath));
            var f = document.SelectToken(JPath, false);

            //if (document.SelectToken(JPath) is JProperty prop)
            //{
            //    prop.Value = Value;
            //    File.WriteAllText(filePath, document.ToString());
            //}
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