using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;
using Surly.LambConfig.ConfigProviders.ProviderModel;

namespace Surly.LambConfig.ConfigProviders
{
    internal static class NetworkConfigTableParser
    {
        internal static readonly string PHYSICAL = "physicalid";
        internal static readonly string LOGICAL = "logicalid";
        
        public static AwsResourceComposite Parse(string networkConfigJson)
        {
            JObject root = JObject.Parse(networkConfigJson);
            AwsResourceComposite result = new AwsResourceComposite();
            
            result.DynamoTables = ParseTables(root).ToList();
            result.APIs = ParseAPIs(root).ToList();
            result.ElasticSearchDomains = ParseElasticSearchDomains(root).ToList();
            result.SNSTopics = ParseSnsTopics(root).ToList();
            result.S3Buckets = ParseS3Buckets(root).ToList();
            result.KinesisStreams = ParseKinesis(root).ToList();
            result.SQSs = ParseSQS(root).ToList();

            var lambdaResults = ParseLambdas(root);
            result.Lambdas = lambdaResults.Lambdas;
            result.S3Buckets.AddRange(lambdaResults.S3Buckets);
            result.SQSs.AddRange(lambdaResults.SQSs);
            result.KinesisStreams.AddRange(lambdaResults.KinesisStreams);
            
            //TODO may have to add other Lambda event sources in the future...
            return result;
        }
        
        private static AwsResourceComposite ParseLambdas(JObject configRoot)
        {
            Func<JToken, LambdaEventTypeEnum> parseEvType = tok =>
            {
                switch (tok.ToString().ToLower())
                {
                    case "sqs":
                        return LambdaEventTypeEnum.SQS;
                        break;
                    case "s3event":
                        return LambdaEventTypeEnum.S3Event;
                        break;
                    case "kinesis":
                        return LambdaEventTypeEnum.Kinesis;
                        break;
                    case "schedule":
                        return LambdaEventTypeEnum.Schedule;
                        break;
                    default:
                        throw new ArgumentException($"Unrecognized lambda event type: {tok}");
                }
            };
            
            var result = new AwsResourceComposite();
            var lambdaTokens = (JArray) configRoot["lambda"] ?? new JArray();
            foreach (JToken lambdaToken in lambdaTokens)
            {
                var lammap = CreateMapping<LambdaMapping>(lambdaToken, ResourceMappingTypeEnum.Lambda);
                
                if (null != lambdaToken["events"])
                {
                    JToken eventsToken = lambdaToken["events"];
                    var eventType = parseEvType(eventsToken);
                    var eventMapping =
                        CreateMapping<LambdaEventMapping>(eventsToken, ResourceMappingTypeEnum.LambdaEvent);
                    eventMapping.EventType = eventType;
                    lammap.Events = new List<LambdaEventMapping> {eventMapping};

                    switch (eventType)
                    {
                        case LambdaEventTypeEnum.Kinesis:
                            result.KinesisStreams.Add(CreateMapping(eventsToken, ResourceMappingTypeEnum.Kinesis));
                            break;
                        case LambdaEventTypeEnum.Schedule:
                            break;
                        case LambdaEventTypeEnum.S3Event:
                            result.S3Buckets.Add(CreateMapping(eventsToken, ResourceMappingTypeEnum.S3Bucket, true));
                            break;
                        case LambdaEventTypeEnum.SQS:
                            result.SQSs.Add(CreateMapping(eventsToken, ResourceMappingTypeEnum.SQS));
                            break;
                        default:
                            throw new ApplicationException($"Unexpected Lambda event type: {eventType}");
                    }
                }
                else
                {
                    lammap.Events = Enumerable.Empty<LambdaEventMapping>();
                }

                result.Lambdas.Add(lammap);
            }

            return result;
           
            
        }


        private static IEnumerable<ResourceMapping> ParseTables(JObject configRoot)
        {
            JArray tables = (JArray) configRoot["dynamo_tables"] ?? new JArray();
            return tables.Select(tok => CreateMapping(tok, ResourceMappingTypeEnum.DynamoTable));

        }

        private static IEnumerable<ResourceMapping> ParseSQS(JObject configRoot)
        {
            JArray tables = (JArray) configRoot["sqs"] ?? new JArray();
            return tables.Select(tok => CreateMapping(tok, ResourceMappingTypeEnum.SQS));
        }
        
        private static IEnumerable<ResourceMapping> ParseKinesis(JObject configRoot)
        {
            JArray tables = (JArray) configRoot["kinesis"] ?? new JArray();
            return tables.Select(tok => CreateMapping(tok, ResourceMappingTypeEnum.Kinesis));
        }

        
        private static IEnumerable<ResourceMapping> ParseSnsTopics(JObject configRoot)
        {
            JArray topics = (JArray) configRoot["sns_topics_published"] ?? new JArray();
            return topics.Select(tok => CreateMapping(tok, ResourceMappingTypeEnum.SNSTopic));
        }
        
        private static IEnumerable<ElasticSearchDomainConfigItem> ParseElasticSearchDomains(JObject configRoot)
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
            JArray esDomains = (JArray) configRoot["elastic_search_domains"] ?? new JArray();
            return esDomains.Select(tok => new ElasticSearchDomainConfigItem
            {
                Domain = tok["domain"].ToString(),
                Endpoint = $"https://{tok["endpoint"]}",
                ServiceName = tok["service_name"]?.ToString(),
                ServiceVersion = tok["service_version"]?.ToString()
            });
           
        }
        
        private static IEnumerable<ResourceMapping> ParseAPIs(JObject configRoot)
        {
            JArray apis = (JArray) configRoot["dependency"]["api"] ?? new JArray();
            return apis.Select(tok => CreateMapping(tok, ResourceMappingTypeEnum.API, true));

        }
        
        private static IEnumerable<ResourceMapping> ParseS3Buckets(JObject configRoot)
        {
            
            JArray buckets = (JArray) configRoot["s3"] ?? new JArray();
            return buckets.Select(tok => CreateMapping(tok, ResourceMappingTypeEnum.S3Bucket, true));

        }

        private static T CreateMapping<T>(JToken tok, ResourceMappingTypeEnum resourceType, bool htmlDecode = false)
            where T : ResourceMapping
        {
            ResourceMapping rm = CreateMapping(tok, resourceType, htmlDecode);
            return (T) rm;
        }
        
        
        private static ResourceMapping CreateMapping(JToken tok, ResourceMappingTypeEnum resourceType, bool htmlDecode = false)
        {
            var physicalId = tok[PHYSICAL]?.ToString();
            if (htmlDecode && !string.IsNullOrEmpty(physicalId))
                physicalId = HttpUtility.HtmlDecode(physicalId);
            
            return new ResourceMapping
            {
                LogicalId = tok[LOGICAL].ToString(),
                PhysicalId =  physicalId,
                ResourceType = resourceType
            };
        }
    }
    
}