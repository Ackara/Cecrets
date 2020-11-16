using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace Acklann.Cecrets
{
    public class JsonEditor
    {
        public static void CopyProperty(string sourceFile, string destinationFile, string jpath)
        {
            if (!File.Exists(sourceFile)) throw new FileNotFoundException($"Could not find file at '{sourceFile}'.");
            if (string.IsNullOrEmpty(destinationFile)) throw new ArgumentNullException(nameof(destinationFile));
            if (string.IsNullOrEmpty(jpath)) return;

            JProperty sourceProperty;
            using (var stream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new JsonTextReader(new StreamReader(stream)))
            {
                JObject source = JObject.Load(reader);
                sourceProperty = (JProperty)source.SelectToken(jpath).Parent;
            }

            if (!File.Exists(destinationFile))
            {
                string folder = Path.GetDirectoryName(destinationFile);
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                File.WriteAllText(destinationFile, "{}", System.Text.Encoding.UTF8);
            }

            JObject destination;
            using (var stream = new FileStream(destinationFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new JsonTextReader(new StreamReader(stream)))
            {
                destination = JObject.Load(reader);

                int index = jpath.LastIndexOf('.') + 1;
                string p = jpath.Substring(index, jpath.Length - index);
                JToken target = destination.SelectToken(p);
                if (target == null)
                {
                    destination.Add(sourceProperty);
                }
                else
                {
                    (target.Parent as JProperty).Value = sourceProperty.Value;
                }
            }

            using (var stream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(destination.ToString(Formatting.Indented));
                writer.Flush();
            }
        }

        public static void AddProperty(string sourceFile, string key, string value)
        {
            if (!File.Exists(sourceFile)) throw new FileNotFoundException($"Could not find file at '{sourceFile}'.");
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            throw new System.NotImplementedException();
        }

        public static string GetValue(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            throw new System.NotImplementedException();
        }
    }
}