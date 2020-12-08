using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Eti.LambdaPlumbing.Configuration;
using Eti.LambdaPlumbing.Logging;
using Newtonsoft.Json;
using Serilog;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace HelloWorld
{

    public class Function
    {

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent,
            ILambdaContext context)
        {
            //note, if one wanted to alter the configuration defaults (caching, etc),
            //then put a call to ConfigManager.InitConfig() up here
            
            //note: the using/IDisposable pattern is important, because the logger only
            //flushes when disposed. this is the closest we can get to forcing any
            //buffered log events (for Kinesis) to be sent prior to the Lambda host
            //shutting down the main thread. without that flush, it's pretty likely
            //that events at the end of the execution cycle would be lost.
            using (var logger = new ELogWrapper())
            {
                logger.Information(new LogPayloadBuilder("Lambda starting")
                    .WithAPIGRequest(apigProxyEvent));
                try
                {
                    //grab a couple environment variables
                    string appName = ConfigManager.Setting(ConfigKeys.AppScope);
                    string serviceName = ConfigManager.Setting(ConfigKeys.ServiceName);
                    //query the Dynamo config tables for a couple values
                    string table = await ConfigManager.LookupResourceAsync("Enrollment");
                    var registeredService = await ConfigManager.LookupServiceAsync("eti3");

                    var retVal = new
                    {
                        TableName = table,
                        App = appName,
                        Service = serviceName,
                        SvcUrl = registeredService?.Url
                    };
                    logger.Debug(new LogPayloadBuilder("Yay!").WithValue("value", retVal));
                    
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = 200,
                        Body = JsonConvert.SerializeObject(retVal)
                    };
                }
                catch (Exception e)
                {
                    //log the error plus the full context
                    var payload = new LogPayloadBuilder("Failed to do something")
                        .WithException(e)
                        .WithLambdaContext(context)
                        .WithAPIGRequest(apigProxyEvent)
                        .BuildJson();
                    logger.Error(payload);

                    return new APIGatewayProxyResponse()
                    {
                        StatusCode = 500
                    };
                }
                
                //since it's inside the using block, this message should get logged. 
                logger.Information(new LogPayloadBuilder("Lambda completed"));
            }

        }
    }
}
