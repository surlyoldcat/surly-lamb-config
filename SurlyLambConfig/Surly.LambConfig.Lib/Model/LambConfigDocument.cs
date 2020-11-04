using System.Collections.Generic;
using System.Linq;

namespace Surly.LambConfig
{
    /// <summary>
    /// This is the official configuration object. The data that is specific to our Lambda/AWS environment
    /// is in the DynamoTables, ServiceRegistryEntries, Lambdas etc dictionaries. The Settings
    /// dictionary contains things like environment variables and other basic config settings.
    /// </summary>
   public class LambConfigDocument
    {
        public Dictionary<string, string> Settings { get; set; }
        
        public Dictionary<string, ResourceMapping> DynamoTables { get; set; }
        public Dictionary<string, LambdaMapping> Lambdas { get; set; }
        public Dictionary<string, ElasticSearchDomainConfigItem> ElasticSearchDomains { get; set; }
        public Dictionary<string, ResourceMapping> SNSTopics { get; set; }
        public Dictionary<string, ResourceMapping> APIs { get; set; }
        public Dictionary<string, ResourceMapping> S3Buckets { get; set; }
        public Dictionary<string, ResourceMapping> KinesisStreams { get; set; }
        public Dictionary<string, ResourceMapping> SQSs { get; set; }

        // ServiceRegistry is a lot of data that seems to not be used. will
        // implement a leaner solution if it turns out to be needed.
        //public List<ServiceRegistryEntry> ServiceRegistryEntries { get; set; }
        public LambConfigDocument()
        {
            Settings = new Dictionary<string, string>();
            DynamoTables = new Dictionary<string, ResourceMapping>();
            Lambdas = new Dictionary<string, LambdaMapping>();
            ElasticSearchDomains = new Dictionary<string, ElasticSearchDomainConfigItem>();
            SNSTopics = new Dictionary<string, ResourceMapping>();
            APIs = new Dictionary<string, ResourceMapping>();
            S3Buckets = new Dictionary<string, ResourceMapping>();
            KinesisStreams = new Dictionary<string, ResourceMapping>();
            SQSs = new Dictionary<string, ResourceMapping>();
        }
    }
}