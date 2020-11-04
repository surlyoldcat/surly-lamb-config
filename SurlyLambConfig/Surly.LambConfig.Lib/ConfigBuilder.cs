using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Newtonsoft.Json;
using Surly.LambConfig;
using Surly.LambConfig.ConfigProviders;

namespace Surly.LambConfig
{
    /// <summary>
    /// Config initialization class, intended to be called from startup code.
    /// </summary>
    public class ConfigBuilder
    {
        //this is internal to allow it to be accessed by extension methods, might be unnecessary
        internal readonly LambConfigDocument _configInternal;

        private bool _enableCache;
        private string _outputFile;

        private readonly List<IConfigProvider> _providers;
        
        public ConfigBuilder()
        {
            _providers = new List<IConfigProvider>(4);
            _configInternal = new LambConfigDocument();
            _enableCache = false;
            _outputFile = String.Empty;
        }
        
        public LambConfigDocument Build()
        {
            _configInternal.Settings[SettingsKeys.LogStream] =
                Environment.GetEnvironmentVariable(EnvironmentKeys.LogStream) ?? String.Empty;
            bool skipLoaders = false;
            if (_enableCache)
            {
                //deserialize from file...
                //if cache is expired, fall thru and let the loaders load it
                //if we got config from cache, skip loading
                //skipLoaders = true;
            }

            if (!skipLoaders)
            {
                foreach (var provider in _providers)
                {
                    provider.UpdateConfig();
                }
            }

            
            if (!string.IsNullOrEmpty(_outputFile))
            {
                string path = Path.Combine(Environment.CurrentDirectory, _outputFile);
                
                //write out to file
                WriteConfigToFile(path, _configInternal);
            }
            
            LambConfiguration.InitConfig(_configInternal);
            return LambConfiguration.Config;
        }

       
        
        public ConfigBuilder WithAwsProfile(string profileName)
        {
            _configInternal.Settings[SettingsKeys.AwsProfile] = profileName;
            return this;
        }

        public ConfigBuilder WithAwsRegion(string region)
        {
            _configInternal.Settings[SettingsKeys.Region] = region;
            return this;
        }
        
        public ConfigBuilder UseDynamoDB()
        {
            //USAGE- it's a lot easier to just use a "last one to set config wins" rule.
            //but ideally, a developer could just use pound-IF DEBUG directive
            //to set custom debug config. so, the DEBUG config would only have
            //local file config, and the normal (Release) build configuration
            //would have Dynamo and caching enabled.
            
            //string profile = _configInternal.Settings[SettingsKeys.AwsProfile];
            
            //TODO should check to ensure the profile has been set or defaulted
            
            
            _providers.Add(new DynamoDBProvider(_configInternal));

//            _configInternal.Settings[SettingsKeys.ConfigurationSource] = "dynamodb";
            return this;
        }

        public ConfigBuilder UseLocalFile(string jsonFile)
        {
            _providers.Add(new LocalJsonFileProvider(jsonFile, _configInternal));
            //_configInternal.Settings[SettingsKeys.ConfigurationSource] = "localfile";
            return this;
        }
        
        public ConfigBuilder WithSettingsOverride(string key, string value)
        {
            _configInternal.Settings[key] = value;
            return this;
        }
        
        public ConfigBuilder EnableCaching()
        {
            _enableCache = true;
            return this;
        }

        public ConfigBuilder ExportFinalConfigTo(string filename)
        {
            _outputFile = filename;
            return this;
        }

        private static void WriteConfigToFile(string fullPath, LambConfigDocument doc)
        {
            using (JsonWriter writer = new JsonTextWriter(File.CreateText(fullPath)))
            {
                writer.Formatting = Formatting.Indented;
                JsonSerializer ser = new JsonSerializer();
                ser.Serialize(writer, doc);
            }
           
        }
        
        private static void ValidateConfig(LambConfigDocument doc)
        {
            throw new NotImplementedException("Config is invalid because robots");
        }
        
        
    }
}