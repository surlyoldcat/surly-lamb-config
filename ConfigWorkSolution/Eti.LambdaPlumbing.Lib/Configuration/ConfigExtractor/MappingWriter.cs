using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Eti.LambdaPlumbing.Configuration.Core;

namespace Eti.LambdaPlumbing.Configuration.ConfigExtractor
{
    internal class MappingWriter
    {
        private readonly string _table;
        private readonly DynamoDBContext _context;
        
        public MappingWriter(AmazonDynamoDBClient dynamo, string table)
        {
            _table = table;
            _context = new DynamoDBContext(dynamo);
        }

        public async Task WriteAsync(IEnumerable<CloudResourceMapping> mappings)
        {
            //todo yes, this is a very crappy and simplistic implementation
            foreach (var mapping in mappings)
            {
                await _context.SaveAsync(mapping, new DynamoDBOperationConfig
                    {
                        OverrideTableName = _table
                    },
                    CancellationToken.None);
            }
        }
        
    }
}