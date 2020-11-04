using System;
using System.Collections.Generic;
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


namespace Surly.LambConfig.ConfigProviders
{
    internal class DynamoDBProvider : IConfigProvider
    {
        private readonly LambConfigDocument _configDoc;
        
        public DynamoDBProvider(LambConfigDocument configDoc)
        {
            _configDoc = configDoc;
            ImportEnvironmentVariables();
        }
        
        public void UpdateConfig()
        {
            ValidateSettings();
            _configDoc.Settings[SettingsKeys.ConfigurationSource] = "dynamodb";
            var client = CreateDynamoClient();
            DynamoDBContext dynamo = new DynamoDBContext(client);
            
            var networkConfigTask = FetchNetworkConfig(dynamo);
            //var registryTask = FetchServiceRegistry(dynamo);
            //Task.WaitAll(networkConfigTask, registryTask);
            networkConfigTask.Wait();
            //_configDoc.ServiceRegistryEntries = registryTask.Result.Select(r => (ServiceRegistryEntry) r).ToList();
            
            string configJson = networkConfigTask.Result.ConfigJson;
            var parsedMappings = NetworkConfigTableParser.Parse(configJson);
            
            _configDoc.Lambdas = parsedMappings.Lambdas
                .ToDictionary(rm => rm.LogicalId, rm => rm);

            _configDoc.DynamoTables = parsedMappings.DynamoTables
                .ToDictionary(rm => rm.LogicalId, rm => rm);
            
            _configDoc.APIs = parsedMappings.APIs
                .ToDictionary(rm => rm.LogicalId, rm => rm);

            _configDoc.S3Buckets = parsedMappings.S3Buckets
                .ToDictionary(rm => rm.LogicalId, rm => rm);

            _configDoc.SNSTopics = parsedMappings.SNSTopics
                .ToDictionary(rm => rm.LogicalId, rm => rm);

            _configDoc.ElasticSearchDomains = parsedMappings.ElasticSearchDomains
                .ToDictionary(es => es.ServiceName, es => es);
            
            _configDoc.KinesisStreams = parsedMappings.KinesisStreams
                .ToDictionary(rm => rm.LogicalId, rm => rm);
            
            _configDoc.SQSs = parsedMappings.SQSs
                .ToDictionary(rm => rm.LogicalId, rm => rm);

        }

        private AmazonDynamoDBClient CreateDynamoClient()
        {
            var regEndpoint = RegionEndpoint.GetBySystemName(Region);
            if (_configDoc.Settings.ContainsKey(SettingsKeys.AwsProfile))
            {
                var credentials = GetCredentials(_configDoc.Settings[SettingsKeys.AwsProfile]);
                return new AmazonDynamoDBClient(credentials, regEndpoint);
            }
            return new AmazonDynamoDBClient(regEndpoint);
            
        }
        
        private void ImportEnvironmentVariables()
        {
            //we will NOT overwrite any settings already set!
            Action<string, string> addIfNotPresent = (key, val) =>
            {
                if (!_configDoc.Settings.ContainsKey(key))
                    _configDoc.Settings.Add(key, val);
            };
            addIfNotPresent(SettingsKeys.Region, Environment.GetEnvironmentVariable(EnvironmentKeys.AwsRegion));
            addIfNotPresent(SettingsKeys.ServiceName, Environment.GetEnvironmentVariable(EnvironmentKeys.ServiceName));
            addIfNotPresent(SettingsKeys.ServiceVersion, Environment.GetEnvironmentVariable(EnvironmentKeys.ServiceVersion));
            addIfNotPresent(SettingsKeys.ServiceRegistryTable, Environment.GetEnvironmentVariable(EnvironmentKeys.RegistryTable));
            addIfNotPresent(SettingsKeys.NetworkConfigTable, Environment.GetEnvironmentVariable(EnvironmentKeys.NetworkConfigTable));
            
        }

        private void ValidateSettings()
        {
            Action<string> validateKey = key =>
            {
                if (string.IsNullOrEmpty(_configDoc.Settings[key]))
                    throw new ApplicationException($"Missing DynamoDB setting: {key}");
            };

            validateKey(SettingsKeys.Region);
            
            validateKey(SettingsKeys.NetworkConfigTable);
            validateKey(SettingsKeys.ServiceRegistryTable);
            validateKey(SettingsKeys.ServiceName);
            validateKey(SettingsKeys.ServiceVersion);
        }
        
        
        private string Region
        {
            get { return _configDoc.Settings[SettingsKeys.Region] ; }
        }

        private string Table
        {
            get { return _configDoc.Settings[SettingsKeys.NetworkConfigTable]; }
        }
        
        private string ServiceName
        {
            get { return _configDoc.Settings[SettingsKeys.ServiceName]; }
        }
        
        private string ServiceVersion
        {
            get { return _configDoc.Settings[SettingsKeys.ServiceVersion]; }
        }

        private string ServiceRegistryTable
        {
            get { return _configDoc.Settings[SettingsKeys.ServiceRegistryTable]; }
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
            var config = new DynamoDBOperationConfig
            {
                OverrideTableName = HttpUtility.HtmlEncode(Table)
            };
            var entry = await context.LoadAsync<NetworkConfigDynamoItem>(ServiceName, ServiceVersion, config, CancellationToken.None);
            return entry;
        }

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