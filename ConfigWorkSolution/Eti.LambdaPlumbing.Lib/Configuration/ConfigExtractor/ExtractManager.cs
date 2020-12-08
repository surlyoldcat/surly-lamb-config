using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Eti.LambdaPlumbing.Configuration.Core;
using Newtonsoft.Json;

namespace Eti.LambdaPlumbing.Configuration.ConfigExtractor
{
    /// <summary>
    /// Helper class used for extracting a flat set of key-value pairs from
    /// the NetworkConfig DynamoDB table for this service (a.k.a. "scope")
    /// </summary>
    public static class ExtractManager
    {
        public static async Task<IEnumerable<CloudResourceMapping>> ExtractNetworkConfig(NetworkConfigExtractParameters p)
        {
            var dynamo = DynamoUtils.GetDynamoClient(p.AwsRegion, p.AwsProfile);
            var extractor = new NetworkConfigExtractor(dynamo, p.TableName, p.ServiceName, p.ServiceVersion);
            var mappings = await extractor.Extract();
            return mappings;
        }

        /// <summary>
        /// Reads from the NetworkConfig table (which is in environment variables)
        /// </summary>
        /// <param name="p"></param>
        /// <param name="outputFile"></param>
        /// <returns></returns>
        public static async Task ExtractNetworkConfigToFile(NetworkConfigExtractParameters p, string outputFile)
        {
            var mappings = await ExtractNetworkConfig(p);
            string json = JsonConvert.SerializeObject(mappings);
            File.WriteAllText(outputFile, json);
        }

        /// <summary>
        /// Write to the new 'cache' table
        /// </summary>
        /// <param name="p"></param>
        /// <param name="mappings"></param>
        /// <returns></returns>
        public static async Task UpdateMappingsTable(ExtractManagerParameters p, IEnumerable<CloudResourceMapping> mappings)
        {
            var dynamo = DynamoUtils.GetDynamoClient(p.AwsRegion, p.AwsProfile);
            var writer = new MappingWriter(dynamo, p.TableName);
            await writer.WriteAsync(mappings);
        }

        /// <summary>
        /// Using a json file, writes to the 'cache' table.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="jsonFile"></param>
        /// <returns></returns>
        public static async Task ImportMappingsTable(ExtractManagerParameters p, string jsonFile)
        {
            List<CloudResourceMapping> mappings = null;
            using (var f = File.OpenText(jsonFile))
            using (var rdr = new JsonTextReader(f))
            {
                var serializer = new JsonSerializer();
                mappings = serializer.Deserialize<List<CloudResourceMapping>>(rdr);
            }

            if (null != mappings)
            {
                await UpdateMappingsTable(p, mappings);
            }
        }

        public class ExtractManagerParameters
        {
            public string AwsRegion { get; set; }
            public string AwsProfile { get; set; }
            public string TableName { get; set; }

        }
        
        public class NetworkConfigExtractParameters : ExtractManagerParameters
        {
            public string ServiceName { get; set; }
            public string ServiceVersion { get; set; }
        }
        
    }
}