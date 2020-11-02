using System;
using Surly.LambConfig;

namespace Surly.LambConfig
{
    public class ConfigBuilder
    {
        //this is internal to allow it to be accessed by extension methods, might be unnecessary
        internal readonly LambConfigDocument _configInternal;

        private bool _enableCache;
        
        public ConfigBuilder()
        {
            _configInternal = new LambConfigDocument();
            _enableCache = false;
        }
        
        public LambConfigDocument Build()
        {
            LambConfiguration.InitConfig(_configInternal);
            return LambConfiguration.Config;
        }

        public ConfigBuilder WithDefaultAwsProfile()
        {
            return this;
        }
        
        public ConfigBuilder WithAwsProfile(string profileName)
        {
            return this;
        }

        public ConfigBuilder UseDynamoDB()
        {
            //USAGE- it's a lot easier to just use a "last one to set config wins" rule.
            //but ideally, a developer could just use pound-IF DEBUG directive
            //to set custom debug config. so, the DEBUG config would only have
            //local file config, and the normal (Release) build configuration
            //would have Dynamo and caching enabled.
            return this;
        }

        public ConfigBuilder UseLocalFile(string jsonFile)
        {
            return this;
        }
        
        public ConfigBuilder EnableCaching()
        {
            _enableCache = true;
            return this;
        }

        private static void ValidateConfig(LambConfigDocument doc)
        {
            throw new NotImplementedException("Config is invalid because robots");
        }
        
        
    }
}