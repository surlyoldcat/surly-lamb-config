using System.Collections.Generic;

namespace Surly.LambConfig.ConfigProviders.ProviderModel
{
    public class AwsResourceComposite
    {
        public List<ResourceMapping> DynamoTables { get; set; } = new List<ResourceMapping>();
        public List<LambdaMapping> Lambdas { get; set; } = new List<LambdaMapping>();

        public List<ElasticSearchDomainConfigItem> ElasticSearchDomains { get; set; } =
            new List<ElasticSearchDomainConfigItem>();

        public List<ResourceMapping> SNSTopics { get; set; } = new List<ResourceMapping>();
        public List<ResourceMapping> APIs { get; set; } = new List<ResourceMapping>();
        public List<ResourceMapping> S3Buckets { get; set; } = new List<ResourceMapping>();
        public List<ResourceMapping> KinesisStreams { get; set; } = new List<ResourceMapping>();
        public List<ResourceMapping> SQSs { get; set; } = new List<ResourceMapping>();
    }
}