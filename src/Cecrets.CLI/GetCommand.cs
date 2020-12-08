using Acklann.Cecrets;
using CommandLine;

namespace Tekcari.Cecrets
{
    [Verb("get", HelpText = "")]
    public class GetCommand : ICommand
    {
        [Option('k', "key", Required = true)]
        public string Key { get; set; }

        [Option('p', "path", Required = true)]
        public string Path { get; set; }

        public int Execute()
        {
            string result = JsonEditor.GetValue(Path, Key);
            System.Console.Write(result);
            return 0;
        }
    }
}