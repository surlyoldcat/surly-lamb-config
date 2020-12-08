using System;
using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Eti.LambdaPlumbing.Configuration;
using Newtonsoft.Json;

namespace Eti.LambdaPlumbing.Logging
{
    /// <summary>
    /// Just a sketch of how we could build log events. For production-quality code,
    /// we'd fill in the (JSON) formatting details and customize how the context
    /// data is reported.
    /// </summary>
    public class LogPayloadBuilder
    {
        
        private readonly Dictionary<string, string> _payload;

        public LogPayloadBuilder(string logMessage)
        {
            _payload = new Dictionary<string,string>();
            _payload.Add("serviceName", ConfigManager.Setting(ConfigKeys.ServiceName));
            _payload.Add("serviceVersion", ConfigManager.Setting(ConfigKeys.ServiceVersion));
            _payload.Add("timestamp", DateTimeOffset.Now.ToString("G"));
            _payload.Add("message", logMessage);
        }
        
        public string BuildJson()
        {
            
            return JsonConvert.SerializeObject(_payload);
        }

        public LogPayloadBuilder WithException(Exception ex)
        {
            return this;
        }

        public LogPayloadBuilder WithAPIGRequest(APIGatewayProxyRequest request)
        {
            return this;
        }

        public LogPayloadBuilder WithLambdaContext(ILambdaContext context)
        {
            return this;
        }

        public LogPayloadBuilder WithValue(string key, object value)
        {
            return this;
        }

    }
}