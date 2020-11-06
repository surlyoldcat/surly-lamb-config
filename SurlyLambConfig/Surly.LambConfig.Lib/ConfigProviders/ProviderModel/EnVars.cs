using System;
using System.Collections.Generic;

namespace Surly.LambConfig.ConfigProviders.ProviderModel
{
    internal static class EnVars
    {
        
        public static Dictionary<string, string> ImportEnvironmentVariables()
        {
            var dict = new Dictionary<string, string>();
            dict[ConfigKeys.AwsRegion] = EnvironmentVariableOrDefault(ConfigKeys.AwsRegion, string.Empty);
            dict[ConfigKeys.AwsProfile] = EnvironmentVariableOrDefault(ConfigKeys.AwsProfile, string.Empty);
            dict[ConfigKeys.LogStream] = EnvironmentVariableOrDefault(ConfigKeys.LogStream, string.Empty);
            dict[ConfigKeys.RegistryTable] = EnvironmentVariableOrDefault(ConfigKeys.RegistryTable, string.Empty);
            dict[ConfigKeys.ServiceName] = EnvironmentVariableOrDefault(ConfigKeys.ServiceName, string.Empty);
            dict[ConfigKeys.ServiceVersion] = EnvironmentVariableOrDefault(ConfigKeys.ServiceVersion, string.Empty);
            dict[ConfigKeys.NetworkConfigTable] = EnvironmentVariableOrDefault(ConfigKeys.NetworkConfigTable, string.Empty);
            
            return dict;
        }

        private static string EnvironmentVariableOrDefault(string key, string defaultVal)
        {
            string value = Environment.GetEnvironmentVariable(key);
            return string.IsNullOrEmpty(value) ? default : value;

        }
    }
}