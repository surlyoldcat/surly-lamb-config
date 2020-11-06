using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;


namespace Surly.LambConfig.ConfigProviders
{
    internal class LocalJsonFileProvider : IConfigProvider
    {
        private readonly string _fileName;

        public LocalJsonFileProvider(string jsonFile)
        {
            _fileName = jsonFile;
        }
        
        public LambConfigDocument LoadConfig()
        {
            using (var rdr = OpenFile(_fileName))
            using (var jrdr = new JsonTextReader(rdr))
            {
                JsonSerializer serializer = new JsonSerializer();
                LambConfigDocument doc = serializer.Deserialize<LambConfigDocument>(jrdr);
                return doc;
            }
        }

        private static TextReader OpenFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new ApplicationException($"Config file not found: {filePath}");

            return File.OpenText(filePath);

        }

        public static void WriteConfigToFile(LambConfigDocument configDoc, string filename)
        {
            using (var file = File.CreateText(filename))
            using (var jwriter = new JsonTextWriter(file))
            {
                jwriter.Formatting = Formatting.Indented;
                var serializer = new JsonSerializer();
                serializer.Serialize(jwriter, configDoc);
            }
        }
    }
}