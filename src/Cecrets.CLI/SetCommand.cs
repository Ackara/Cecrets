using Acklann.Cecrets;
using CommandLine;
using System.IO;

namespace Tekcari.Cecrets
{
    [Verb("set", HelpText = "")]
    public class SetCommand : ICommand
    {
        [Option('k', "key", Required = true)]
        public string Key { get; set; }

        [Option('v', "value", Required = true)]
        public string Value { get; set; }

        [Option('p', "path")]
        public string SourceFile { get; set; }

        public int Execute()
        {
            if (!File.Exists(SourceFile)) File.WriteAllText(SourceFile, "{}");

            JsonEditor.SetProperty(SourceFile, Key, Value);
            return 0;
        }
    }
}