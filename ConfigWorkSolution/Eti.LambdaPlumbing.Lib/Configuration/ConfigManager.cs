using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Eti.LambdaPlumbing.Configuration.Core;

namespace Eti.LambdaPlumbing.Configuration
{
    /// <summary>
    /// Primary consumer class for accessing all Lambda-related config.
    /// This includes environment variables, and the data stored in the
    /// NetworkConfig and ServiceRegistry DynamoDB tables.
    /// </summary>
    /// <remarks>This depends on a 'distributed' cache table which breaks
    /// out the NetworkConfig data for this service (scope) into a straight
    /// set of key-value pairs. So it never queries/parses NetworkConfig directly.</remarks>
    public static class ConfigManager
    {
        private static ImmutableDictionary<string, string> _settings = ImmutableDictionary<string, string>.Empty;
        private static CloudResourceMapService _cloudResources;
        private static ServiceRegistryService _services;
        
        static ConfigManager()
        {
            //init with the default options
            InitConfig(new ConfigManagerOptions());
        }
        
        public static void InitConfig(ConfigManagerOptions options)
        {
            Dictionary<string, string> enVars = ImportEnvironmentVariables();
            _settings = enVars.ToImmutableDictionary();
            _cloudResources = new CloudResourceMapService(_settings, options);
            _services = new ServiceRegistryService(_settings, options);
        }
        
        /// <summary>
        /// Environment variables and custom key-value pairs.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Setting(string key)
        {
            if (!_settings.ContainsKey(key))
                return null;
            return _settings[key];
        }

        /// <summary>
        /// Gets a value from the NetworkConfig data
        /// </summary>
        /// <param name="logicalId"></param>
        /// <returns></returns>
        public static async Task<string> LookupResourceAsync(string logicalId)
        {
            return await _cloudResources.GetPhysicalIdAsync(logicalId);
            
        }

        /// <summary>
        /// Looks up a service from ServiceRegistry
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static async Task<RegisteredService> LookupServiceAsync(string name)
        {
            return await _services.GetService(name);
        }
        
        /// <summary>
        /// Used to add or override environment variables or custom settings.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void OverrideSetting(string key, string value)
        {
            _settings = _settings.SetItem(key, value);
        }

        private static Dictionary<string, string> ImportEnvironmentVariables()
        {
            Func<string, string> EnVarOrEmpty = key =>
            {
                return Environment.GetEnvironmentVariable(key) ?? string.Empty;
            };
            
            Dictionary<string, string> envars = new Dictionary<string, string>(4);
            envars.Add(ConfigKeys.AwsProfile, EnVarOrEmpty(ConfigKeys.AwsProfile));
            envars.Add(ConfigKeys.AwsRegion, EnVarOrEmpty(ConfigKeys.AwsRegion));
            envars.Add(ConfigKeys.LogStream, EnVarOrEmpty(ConfigKeys.LogStream));
            envars.Add(ConfigKeys.ResourcesTable, EnVarOrEmpty(ConfigKeys.ResourcesTable));
            envars.Add(ConfigKeys.Environment, EnVarOrEmpty(ConfigKeys.Environment));
            envars.Add(ConfigKeys.ServiceName, EnVarOrEmpty(ConfigKeys.ServiceName));
            envars.Add(ConfigKeys.ServiceVersion, EnVarOrEmpty(ConfigKeys.ServiceVersion));
            return envars;

        }

        
        
    }
}