using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using LazyCache;

namespace Eti.LambdaPlumbing.Configuration.Core
{
    internal class ServiceRegistryService
    {
        private static readonly IAppCache _cache = new CachingService();
        private static Lazy<AmazonDynamoDBClient> _dynamoLazy;
        private readonly string _tableName;
        private readonly ConfigManagerOptions _options;
        
        public ServiceRegistryService(IDictionary<string, string> settings, ConfigManagerOptions options)
        {
            _options = options;
            string profile = settings[ConfigKeys.AwsProfile];
            string region = settings[ConfigKeys.AwsRegion];
            _dynamoLazy = new Lazy<AmazonDynamoDBClient>(() => DynamoUtils.GetDynamoClient(region, profile));
            _tableName = settings[ConfigKeys.ServiceRegistryTable];
        }

        public async Task<RegisteredService> GetService(string name)
        {
            ServiceRegistryItem svc = _options.UseCache
                ? await CacheFetchAsync(name)
                : await DbFetchAsync(name);
            return svc.ToModel();
        }
        
        private async Task<ServiceRegistryItem> CacheFetchAsync(string name)
        {
            ServiceRegistryItem item = await _cache.GetOrAddAsync(name, () =>
                {
                    return DbFetchAsync(name);
                }
                , DateTimeOffset.Now.AddMinutes(_options.CacheExpirationMinutes)
            );
            return item;
        }
        
            
        private async Task<ServiceRegistryItem> DbFetchAsync(string name)
        {
            var context = new DynamoDBContext(_dynamoLazy.Value);
            var dynConfig = new DynamoDBOperationConfig
            {
                OverrideTableName = _tableName
            };
                
            var query = await context.QueryAsync<ServiceRegistryItem>(name).GetRemainingAsync();
            return Enumerable.FirstOrDefault<ServiceRegistryItem>(query);
        }
        
        
        private class ServiceRegistryItem
        {
            [DynamoDBHashKey("service")] public string Service { get; set; }

            [DynamoDBRangeKey("version")] public string Version { get; set; }

            [DynamoDBProperty("status")] public string Status { get; set; }

            [DynamoDBProperty("url")] public string Url { get; set; }
        
            [DynamoDBProperty("port")] public string Port { get; set; }

            [DynamoDBProperty("stage")] public string Stage { get; set; }

            [DynamoDBProperty("host")] public string Host { get; set; }

            public RegisteredService ToModel()
            {
                return new RegisteredService
                {
                    Name = Service,
                    Host = Host,
                    Url = Url
                };
            }
            
        }
    }
}