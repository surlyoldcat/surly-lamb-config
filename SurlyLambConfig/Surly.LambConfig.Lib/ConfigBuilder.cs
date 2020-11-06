using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Newtonsoft.Json;
using Surly.LambConfig.ConfigProviders;
using Surly.LambConfig.ConfigProviders.ProviderModel;

namespace Surly.LambConfig
{
    /// <summary>
    /// Config initialization class, intended to be called from startup code.
    /// </summary>
    public class ConfigBuilder
    {
        private const string KEY_CONFIGSOURCE = "configsource";
        private bool _enableCache;
        private string _outputFile;
        private Dictionary<string, string> _overrides;
        private readonly Dictionary<string, string> _environmentVars;
        private IConfigProvider _provider;
        
        public ConfigBuilder()
        {
            _enableCache = false;
            _outputFile = String.Empty;
            _overrides = new Dictionary<string, string>();
            _environmentVars = EnVars.ImportEnvironmentVariables();
            //set up DynamoDB as the default (because it makes life a lot easier)
            _overrides[KEY_CONFIGSOURCE] = "dynamodb";
            _provider = new DynamoDBProvider(_environmentVars);
        }
        
        public LambConfigDocument Build()
        {
            if (_enableCache)
            {
                //TODO TBD
            }
            
            foreach (var item in _overrides)
            {
                _environmentVars[item.Key] = item.Value;
            }

            var configDoc = _provider.LoadConfig();
            
            if (!string.IsNullOrEmpty(_outputFile))
            {
                //write out to file
                string path = Path.Combine(Environment.CurrentDirectory, _outputFile);
                LocalJsonFileProvider.WriteConfigToFile(configDoc, path);
            }
            
            ConfigManager.InitConfig(configDoc);
            return ConfigManager.Config;
        }

       
        
        public ConfigBuilder WithAwsProfile(string profileName)
        {
            _overrides[ConfigKeys.AwsProfile] = profileName;
            return this;
        }

        public ConfigBuilder WithAwsRegion(string region)
        {
            _overrides[ConfigKeys.AwsRegion] = region;
            return this;
        }

       
        public ConfigBuilder UseLocalConfigFile(string jsonFile)
        {
            
            _overrides[KEY_CONFIGSOURCE] = "localfile";
            _provider = new LocalJsonFileProvider(jsonFile);
            return this;
        }

    
        
        public ConfigBuilder WithSettingsOverride(string key, string value)
        {
            _overrides[key] = value;
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

       
        
        
    }
}