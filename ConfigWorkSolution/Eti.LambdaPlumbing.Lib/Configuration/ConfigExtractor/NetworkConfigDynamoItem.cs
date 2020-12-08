using Amazon.DynamoDBv2.DataModel;

namespace Eti.LambdaPlumbing.Configuration.ConfigExtractor
{

    internal class NetworkConfigDynamoItem
    {
        [DynamoDBHashKey("service")] public string ServiceName { get; set; }

        [DynamoDBRangeKey("version")] public string Version { get; set; }

        [DynamoDBProperty("config")] public string ConfigJson { get; set; }

        [DynamoDBProperty("hash_pepper")] public string HashPepper { get; set; }
    }

}