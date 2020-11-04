using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Surly.LambConfig
{
    [JsonConverter(typeof(StringEnumConverter))]  
    public enum LambdaEventTypeEnum
    {
        Kinesis,
        SQS,
        S3Event,
        Schedule
    }

    [JsonConverter(typeof(StringEnumConverter))]  
    public enum ResourceMappingTypeEnum
    {
        DynamoTable,
        Lambda,
        LambdaEvent,
        SQS,
        API,
        SNSTopic,
        S3Bucket,
        Kinesis
    }
}