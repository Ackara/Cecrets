using System.IO;
using System.Management.Automation;

namespace Acklann.Cecrets
{
    /// <summary>
    /// <para type="synopsis">Add a foo</para>
    /// <para type="description">Add a foo</para>
    /// </summary>
    /// <example>
    /// </example>
    /// <seealso cref="System.Management.Automation.Cmdlet" />
    [Cmdlet(VerbsCommon.Add, "UserSecret")]
    public class AddUserSecretCmdlet : Cmdlet
    {
        /// <summary>
        /// Gets or sets the key.
        /// <para type="description">A unique identifier.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// <para type="description">A value.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string Value { get; set; }

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
            WriteObject("hello world");
        }
    }
}