using System;
using System.IO;
using System.Linq;

namespace Acklann.Cecrets
{
    public class TestData
    {
        static TestData()
        {
            Directory = Path.Combine(AppContext.BaseDirectory, "test-data");
        }

        public static readonly string Directory;

        public static string GetFile(string pattern)
        {
            return System.IO.Directory.EnumerateFiles(Directory, pattern, System.IO.SearchOption.AllDirectories).First();
        }
    }
}