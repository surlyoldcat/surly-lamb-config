using System.Collections.Generic;
using System.Linq;
using Eti.LambdaPlumbing.Configuration.Core;
using Newtonsoft.Json.Linq;

namespace Eti.LambdaPlumbing.Configuration.ConfigExtractor
{
    internal class NetworkConfigParser
    {
        internal static readonly string PHYSICAL = "physicalid";
        internal static readonly string LOGICAL = "logicalid";
        private readonly JObject _root;
        private readonly string _serviceName;
        
        public NetworkConfigParser(NetworkConfigDynamoItem item)
        {
            _root = JObject.Parse(item.ConfigJson);
            _serviceName = item.ServiceName;
        }
        
        public IEnumerable<CloudResourceMapping> Parse()
        {
            //note, this version does not drill down into Lambdas to infer event source resources (S3 buckets, SQS queues, etc)
            var resources = new List<CloudResourceMapping>();
            
            resources.AddRange(ParseTokens("lambda"));
            resources.AddRange(ParseTokens("dynamo_tables"));
            resources.AddRange(ParseTokens("sqs"));
            resources.AddRange(ParseTokens("kinesis"));
            resources.AddRange(ParseTokens("sns_topics_published"));
            resources.AddRange(ParseTokens("s3"));

            resources.AddRange(ParseElasticSearchDomains());
            resources.AddRange(ParseAPIs());
            return resources;

        }

        private IEnumerable<CloudResourceMapping> ParseTokens(string tokenName)
        {
            var toks = (JArray) _root[tokenName] ?? new JArray();
            return toks.Select(t => CreateMapping(t, _serviceName));
        }
        
     
        private IEnumerable<CloudResourceMapping> ParseElasticSearchDomains()
        {
            /*
            "elastic_search_domains": [
			{
				"domain": "cybersource",
				"service_name": "eties",
				"service_version": "1.0",
				"endpoint": "vpc-cybersource-bvf4lahv576thuc75lvmhwksrq.us-west-2.es.amazonaws.com"
			}
		    ],

             */
            //TODO this might not be sufficient info for what Cyber needs
            JArray esDomains = (JArray) _root["elastic_search_domains"] ?? new JArray();
            return esDomains.Select(tok => new CloudResourceMapping
            {
                LogicalId = tok["domain"].ToString(),
                PhysicalId = $"https://{tok["endpoint"]}",
                Scope = _serviceName
            });
           
        }
        
        private IEnumerable<CloudResourceMapping> ParseAPIs()
        {
            JArray apis = (JArray) _root["dependency"]["api"] ?? new JArray();
            return apis.Select(tok => CreateMapping(tok, _serviceName));

        }
        
     
        private static CloudResourceMapping CreateMapping(JToken tok, string scope)
        {
            
           return new CloudResourceMapping
            {
                LogicalId = tok[LOGICAL].ToString(),
                PhysicalId = GetPhysical(tok),
                Scope = scope
            };
        }

        private static string GetPhysical(JToken token)
        {
            JToken jt = token[PHYSICAL];
            if (null != jt)
            {
                if (jt.Type == JTokenType.Array)
                {
                    return jt[0].ToString();
                }
                else
                {
                    return jt.ToString();
                }
                
            }

            return string.Empty;
        }
    }
}