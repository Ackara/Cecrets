using CommandLine;

namespace Tekcari.Cecrets
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<GetCommand, SetCommand>(args)
                .WithParsed<GetCommand>((x) => x.Execute())
                .WithParsed<SetCommand>((x) => x.Execute());
        }
    }
}