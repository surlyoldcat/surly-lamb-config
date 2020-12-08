using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using LazyCache;

namespace Eti.LambdaPlumbing.Configuration.Core
{
    internal class CloudResourceMapService
    {
        private static readonly IAppCache _cache = new CachingService();
        private static Lazy<AmazonDynamoDBClient> _dynamoLazy;

        private readonly ConfigManagerOptions _options;
        private readonly string _tableName;
        private readonly string _scope;
        
        public CloudResourceMapService(IDictionary<string, string> settings, ConfigManagerOptions options)
        {
            _options = options;
            string profile = settings[ConfigKeys.AwsProfile];
            string region = settings[ConfigKeys.AwsRegion];
            _dynamoLazy = new Lazy<AmazonDynamoDBClient>(() => DynamoUtils.GetDynamoClient(region, profile));
            _tableName = settings[ConfigKeys.ResourcesTable];
            _scope = settings[ConfigKeys.AppScope];
        }

        public async Task<string> GetPhysicalIdAsync(string logicalId)
        {
            ResourceMappingDynamoItem item = _options.UseCache
                ? await CacheFetchAsync(logicalId)
                : await DbFetchAsync(logicalId);
            return item.Value;
        }

        private async Task<ResourceMappingDynamoItem> CacheFetchAsync(string logicalId)
        {
            ResourceMappingDynamoItem item = await _cache.GetOrAddAsync(logicalId, () =>
                {
                    return DbFetchAsync(logicalId);
                }
                , DateTimeOffset.Now.AddMinutes(_options.CacheExpirationMinutes)
            );
            return item;
        }
        
            
        private async Task<ResourceMappingDynamoItem> DbFetchAsync(string logicalId)
        {
            var context = new DynamoDBContext(_dynamoLazy.Value);
            var dynConfig = new DynamoDBOperationConfig
            {
                OverrideTableName = _tableName
            };
            ResourceMappingDynamoItem mapping = await context.LoadAsync<ResourceMappingDynamoItem>(logicalId, _scope);
            return mapping;
        }
        
        private class ResourceMappingDynamoItem
        {
            [DynamoDBRangeKey("scope")]
            public string Scope { get; set; }
        
            [DynamoDBHashKey("key")]
            public string Key { get; set; }
        
            [DynamoDBProperty("value")]
            public string Value { get; set; }

            // public CloudResourceMapping ToModel()
            // {
            //     return new CloudResourceMapping
            //     {
            //         LogicalId = Key,
            //         Scope = Scope,
            //         PhysicalId = Value
            //     };
            // }

        }
        
    }
}