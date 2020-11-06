using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Surly.LambConfig.ConfigProviders.ProviderModel;

namespace Surly.LambConfig
{
    /// <summary>
    /// This is the official configuration object. The data that is specific to our Lambda/AWS environment
    /// is in the DynamoTables, ServiceRegistryEntries, Lambdas etc dictionaries. The Settings
    /// dictionary contains things like environment variables and other basic config settings.
    /// </summary>
   public class LambConfigDocument
    {
        public ImmutableDictionary<string, string> Settings { get; set; }
        
        public ImmutableDictionary<string, ResourceMapping> DynamoTables { get; set; }
        public ImmutableDictionary<string, LambdaMapping> Lambdas { get; set; }
        public ImmutableDictionary<string, ElasticSearchDomainConfigItem> ElasticSearchDomains { get; set; }
        public ImmutableDictionary<string, ResourceMapping> SNSTopics { get; set; }
        public ImmutableDictionary<string, ResourceMapping> APIs { get; set; }
        public ImmutableDictionary<string, ResourceMapping> S3Buckets { get; set; }
        public ImmutableDictionary<string, ResourceMapping> KinesisStreams { get; set; }
        public ImmutableDictionary<string, ResourceMapping> SQSs { get; set; }

        
    }
}