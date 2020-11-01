using System;
using Surly.LambConfig;

namespace Surly.LambConfig
{
    public class ConfigBuilder
    {
        private LambConfigDocument Config { get; set; }
        private bool EnableCache { get; set; }
        
        public ConfigBuilder()
        {
            EnableCache = false;
        }
        
        public LambConfigDocument Build()
        {
            //TODO add a delegate to allow consumer to validate config?   
            LambConfiguration.InitConfig(Config);
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
            EnableCache = true;
            return this;
        }

        private static void ValidateConfig(LambConfigDocument doc)
        {
            throw new NotImplementedException("Config is invalid because robots");
        }
        
        
    }
}