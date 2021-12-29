using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Acklann.Cecrets
{
    public class JsonEditor
    {
        public static void CopyProperty(string sourceFile, string destinationFile, string jpath)
        {
            if (!File.Exists(sourceFile)) throw new FileNotFoundException($"Could not find file at '{sourceFile}'.");
            if (string.IsNullOrEmpty(destinationFile)) throw new ArgumentNullException(nameof(destinationFile));
            CreateJsonFileIfNotExists(destinationFile);

            Stream result;
            using (Stream input = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (Stream output = new FileStream(destinationFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                result = CopyProperty(input, output, jpath);
            }

            if (result != null)
                using (Stream output = new FileStream(destinationFile, FileMode.Open, FileAccess.Write, FileShare.Read))
                {
                    result.CopyTo(output);
                }
        }

        public static Stream CopyProperty(Stream inputStream, Stream outputStream, string jpath)
        {
            if (inputStream == null) throw new ArgumentNullException(nameof(inputStream));
            if (outputStream == null) throw new ArgumentNullException(nameof(outputStream));
            if (string.IsNullOrEmpty(jpath)) return null;

            using var sourceReader = new JsonTextReader(new StreamReader(inputStream));
            JObject source = JObject.Load(sourceReader);
            JToken[] sourceValues = source.SelectTokens(jpath).ToArray();
            if ((sourceValues?.Length ?? 0) == 0) return null;
            
            JObject destination;
            using var outputReader = new JsonTextReader(new StreamReader(outputStream));
            try { destination = JObject.Load(outputReader); } catch { destination = new JObject(); }

            JToken target; JProperty property;
            foreach (JToken token in sourceValues)
            {
                property = (JProperty)token.Parent;
                target = destination.SelectToken(property.Name);
                if (target == null) destination.Add(property);
                else (target.Parent as JProperty).Value = token;
            }

            return new MemoryStream(Encoding.UTF8.GetBytes(destination.ToString()));
        }

        public static string GetValue(Stream stream, string key)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (string.IsNullOrEmpty(key)) return null;

            using (var streamReader = new StreamReader(stream))
            using (var reader = new JsonTextReader(streamReader))
            {
                JObject document = JObject.Load(reader);
                JToken value = document?.SelectToken(key.Replace(':', '.'));
                return value?.ToString();
            }
        }

        public static string GetValue(string sourceFile, string key)
        {
            if (!File.Exists(sourceFile)) throw new FileNotFoundException($"Could not find file at '{sourceFile}'.");
            if (string.IsNullOrEmpty(key)) return null;

            using (var file = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return GetValue(file, key);
            }
        }

        public static void SetProperty(Stream inputStream, string key, object value)
        {
            if (inputStream == null) throw new ArgumentNullException(nameof(inputStream));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key), $"The {nameof(key)} cannot be null or whitespace.");

            // Parsing the document.
            JObject document;
            using var baseReader = new StreamReader(inputStream);
            using var reader = new JsonTextReader(baseReader);
            document = JObject.Load(reader);

            // Initializing variables.
            string[] segments = key.Split(new char[] { '.', ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0) return;

            var propertyNames = new Stack<string>(segments.Reverse());
            JProperty property = null;

            // Edit the document.
            setvalue();

            // Save the document.
            inputStream.Seek(0, SeekOrigin.Begin);
            using var writer = new JsonTextWriter(new StreamWriter(inputStream)) { Formatting = Formatting.Indented };
            document.WriteTo(writer);
            writer.Flush();

            // ===== DONE ===== //

            void setvalue()
            {
                string n = propertyNames.Pop();
                JProperty next = findOrCreate(n);
                System.Diagnostics.Debug.WriteLine($"current: ${next.Name}");

                if (propertyNames.Count == 0)
                {
                    next.Value = new JValue(value);
                    return;
                }
                else
                {
                    property = next;
                    setvalue();
                }
            }

            JProperty findOrCreate(string n)
            {
                JProperty result;

                // Trying to find the property
                if (property == null) result = document.Property(n, StringComparison.InvariantCultureIgnoreCase);
                else result = getChild(n);

                // I did not find an existing property so I am creating one.
                if (result == null)
                {
                    result = new JProperty(n, null);
                    if (property == null) document.Add(result);
                    else
                    {
                        if (property.Value.Type == JTokenType.Object)
                        {
                            var obj = (JObject)property.Value;
                            obj.Add(result);
                        }
                        else
                        {
                            var obj = new JObject(result);
                            property.Value = obj;
                        }
                    }
                }

                return result;
            }

            JProperty getChild(string name)
            {
                return (from token in property.Value.Children()
                        where token.Type == JTokenType.Property
                        let prop = (JProperty)token
                        where string.Equals(prop.Name, name, StringComparison.InvariantCultureIgnoreCase)
                        select prop
                       ).FirstOrDefault();
            }
        }

        public static void SetProperty(string sourceFile, string key, object value)
        {
            if (string.IsNullOrEmpty(sourceFile)) throw new ArgumentNullException(nameof(sourceFile));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            CreateJsonFileIfNotExists(sourceFile);

            using (Stream file = new FileStream(sourceFile, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
            {
                SetProperty(file, key, value);
            }
        }

        #region Backing Members

        private static void CreateJsonFileIfNotExists(string filePath)
        {
            if (!File.Exists(filePath))
            {
                string folder = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                File.WriteAllText(filePath, "{}", Encoding.UTF8);
            }
        }

        #endregion Backing Members
    }
}