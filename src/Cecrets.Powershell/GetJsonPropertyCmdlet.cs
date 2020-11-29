using System.Management.Automation;

namespace Acklann.Cecrets
{
    /// <summary>
    /// <para type="synopsis"></para>
    /// <para type="description"></para>
    /// </summary>
    /// <seealso cref="System.Management.Automation.Cmdlet" />
    [Cmdlet(VerbsCommon.Get, "Secret")]
    public class GetJsonPropertyCmdlet : Cmdlet
    {
        [Alias("k", "name")]
        [Parameter(Mandatory = true, Position = 1)]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        [Alias("p", "file", nameof(System.IO.FileInfo.FullName))]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 2)]
        public string Path { get; set; }

        /// <summary>
        /// Processes the record.
        /// </summary>
        protected override void ProcessRecord()
        {
            string value = JsonEditor.GetValue(Path, Key);
            WriteObject(value ?? System.Environment.GetEnvironmentVariable(Key.Replace(":", "_")));
        }
    }
}