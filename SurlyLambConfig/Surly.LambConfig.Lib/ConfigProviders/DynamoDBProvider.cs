using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Surly.LambConfig.ConfigProviders.ProviderModel;


namespace Surly.LambConfig.ConfigProviders
{
    internal class DynamoDBProvider : IConfigProvider
    {

        private readonly Dictionary<string, string> _enVars;
        
        public DynamoDBProvider(Dictionary<string, string> environmentVariables)
        {
            _enVars = environmentVariables;
        }

        public  LambConfigDocument LoadConfig()
        {
            ValidateEnvironmentVariables();
            var client = CreateDynamoClient();
            DynamoDBContext dynamo = new DynamoDBContext(client);
            
            var networkConfigTask = FetchNetworkConfig(dynamo);
            //var registryTask = FetchServiceRegistry(dynamo);
            //Task.WaitAll(networkConfigTask, registryTask);
            networkConfigTask.Wait();
            //_configDoc.ServiceRegistryEntries = registryTask.Result.Select(r => (ServiceRegistryEntry) r).ToList();
            
            string configJson = networkConfigTask.Result.ConfigJson;
            var resources = NetworkConfigTableParser.Parse(configJson);
            //todo what is the correct key for Elastic Search Domain? Domain or ServiceName?
            return new LambConfigDocument
            {
                DynamoTables = resources.DynamoTables.ToImmutableDictionary(r => r.LogicalId, r => r),
                APIs = resources.APIs.ToImmutableDictionary(r => r.LogicalId, r => r),
                Lambdas = resources.Lambdas.ToImmutableDictionary(r => r.LogicalId, r => r),
                ElasticSearchDomains = resources.ElasticSearchDomains.ToImmutableDictionary(r => r.ServiceName, r => r),
                KinesisStreams = resources.KinesisStreams.ToImmutableDictionary(r => r.LogicalId, r => r),
                S3Buckets = resources.S3Buckets.ToImmutableDictionary(r => r.LogicalId, r => r),
                SQSs = resources.SQSs.ToImmutableDictionary(r => r.LogicalId, r => r),
                SNSTopics = resources.SNSTopics.ToImmutableDictionary(r => r.LogicalId, r => r),
                Settings = _enVars.ToImmutableDictionary()
            };
            
        }

        private AmazonDynamoDBClient CreateDynamoClient()
        {
            var regEndpoint = RegionEndpoint.GetBySystemName(_enVars[ConfigKeys.AwsRegion]);
            if (_enVars.ContainsKey(ConfigKeys.AwsProfile))
            {
                var credentials = GetCredentials(_enVars[ConfigKeys.AwsProfile]);
                return new AmazonDynamoDBClient(credentials, regEndpoint);
            }
            return new AmazonDynamoDBClient(regEndpoint);
            
        }
        
        

        private void ValidateEnvironmentVariables()
        {
            Action<string> validateKey = key =>
            {
                if (string.IsNullOrEmpty(_enVars[key]))
                    throw new ApplicationException($"Missing DynamoDB environment variable: {key}");
            };

            validateKey(ConfigKeys.AwsRegion);
            validateKey(ConfigKeys.NetworkConfigTable);
            validateKey(ConfigKeys.RegistryTable);
            validateKey(ConfigKeys.ServiceName);
            validateKey(ConfigKeys.ServiceVersion);
        }
        
        
       
      

        private static AWSCredentials GetCredentials(string profile)
        {
            var chain = new CredentialProfileStoreChain();
            AWSCredentials credentials;
            if (!chain.TryGetAWSCredentials(profile, out credentials))
                throw new ApplicationException($"Failed to get credentials for profile: {profile}");

            return credentials;
        }
        
        private async Task<NetworkConfigDynamoItem> FetchNetworkConfig(DynamoDBContext context)
        {
            string tableName = _enVars[ConfigKeys.NetworkConfigTable];
            string serviceName = _enVars[ConfigKeys.ServiceName];
            string version = _enVars[ConfigKeys.ServiceVersion];
            var config = new DynamoDBOperationConfig
            {
                OverrideTableName = HttpUtility.HtmlEncode(tableName)
            };
            var entry = await context.LoadAsync<NetworkConfigDynamoItem>(serviceName, version, config, CancellationToken.None);
            return entry;
        }

        /*
        private async Task<IEnumerable<ServiceRegistryDynamoItem>> FetchServiceRegistry(DynamoDBContext context)
        {
            //TODO shouldn't this be filtered? do we always need the entire table?
            var config = new DynamoDBOperationConfig
            {
                OverrideTableName = HttpUtility.HtmlEncode(ServiceRegistryTable)
            };
            ScanCondition[] conditions = new ScanCondition[0];
            var items = await context.ScanAsync<ServiceRegistryDynamoItem>(conditions, config)
                .GetRemainingAsync();
            return items;
        }
        */
        internal class NetworkConfigDynamoItem
        {
            [DynamoDBHashKey("service")] public string ServiceName { get; set; }

            [DynamoDBRangeKey("version")] public string Version { get; set; }

            [DynamoDBProperty("config")] public string ConfigJson { get; set; }

            [DynamoDBProperty("hash_pepper")] public string HashPepper { get; set; }
        }
        
        internal class ServiceRegistryDynamoItem : ServiceRegistryEntry
        {
            [DynamoDBHashKey("service")] public override string Service { get; set; }

            [DynamoDBRangeKey("version")] public override string Version { get; set; }

            [DynamoDBProperty("status")] public override string Status { get; set; }

            [DynamoDBProperty("url")] public override string Url { get; set; }

            [DynamoDBProperty("port")] public override string Port { get; set; }

            [DynamoDBProperty("stage")] public override string Stage { get; set; }

            [DynamoDBProperty("host")] public override string Host { get; set; }
        }
    }
}