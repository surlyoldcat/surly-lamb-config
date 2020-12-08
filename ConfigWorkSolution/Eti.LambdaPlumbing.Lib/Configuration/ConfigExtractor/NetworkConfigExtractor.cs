using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Eti.LambdaPlumbing.Configuration.Core;

namespace Eti.LambdaPlumbing.Configuration.ConfigExtractor
{
    internal class NetworkConfigExtractor
    {
        private readonly string _serviceName;
        private readonly string _serviceVersion;
        private readonly string _sourceTable;
        private readonly DynamoDBContext _context;
        
        public NetworkConfigExtractor(AmazonDynamoDBClient dynamo, 
            string sourceTable,
            string serviceName,
            string serviceVersion) 
        {
            _sourceTable = sourceTable;
            _serviceName = serviceName;
            _serviceVersion = serviceVersion;
            _context = new DynamoDBContext(dynamo);    
        }

        public async Task<IEnumerable<CloudResourceMapping>> Extract()
        {
            var config = new DynamoDBOperationConfig
            {
                OverrideTableName = _sourceTable
            };
            var item = await _context.LoadAsync<NetworkConfigDynamoItem>(_serviceName, _serviceVersion, config, CancellationToken.None);
           
            string scope = item.ServiceName;

            var parser = new NetworkConfigParser(item);
            var parsedMappings = parser.Parse();
           
            return parsedMappings;
        }
    }
}