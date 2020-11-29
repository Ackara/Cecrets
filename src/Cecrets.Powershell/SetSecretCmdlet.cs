using System.IO;
using System.Management.Automation;

namespace Acklann.Cecrets
{
    /// <summary>
    /// <para type="synopsis">Set json property to file.</para>
    /// <para type="description"></para>
    /// </summary>
    /// <example>
    /// </example>
    /// <seealso cref="System.Management.Automation.Cmdlet" />
    [Cmdlet(VerbsCommon.Set, "Secret")]
    public class SetSecretCmdlet : Cmdlet
    {
        /// <summary>
        /// Gets or sets the key.
        /// <para type="description">A unique identifier.</para>
        /// </summary>
        [Alias("k", "name")]
        [Parameter(Mandatory = true, Position = 1)]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// <para type="description">A value.</para>
        /// </summary>
        [Alias("v")]
        [Parameter(Mandatory = true, Position = 2)]
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        [Alias(nameof(FileInfo.FullName), "p", "file")]
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public string Path { get; set; }

        /// <summary>
        /// Processes the record.
        /// </summary>
        protected override void ProcessRecord()
        {
            JsonEditor.SetProperty(Path, Key, Value);
        }
    }
}