using System.Collections.Generic;
using System.Linq;

namespace Surly.LambConfig
{
   public class LambConfigDocument
    {
        public Dictionary<string, string> Settings { get; } = new Dictionary<string, string>();
        public List<ServiceRegistryEntry> ServiceRegistryEntries { get; } = new List<ServiceRegistryEntry>();


        public string ConfigSource
        {
            get
            {
                if (Settings.ContainsKey(SettingsKeys.ConfigurationSource))
                    return Settings[SettingsKeys.ConfigurationSource];
                else
                    return null;
            }
            set { Settings[SettingsKeys.ConfigurationSource] = value; }

        }

        public string AwsProfile
        {
            get
            {
                if (Settings.ContainsKey(SettingsKeys.AwsProfile))
                    return Settings[SettingsKeys.AwsProfile];
                else
                    return null;
            }
            set { Settings[SettingsKeys.AwsProfile] = value; }

        }
        
        public Dictionary<string, NetworkConfigItem> DynamoTables { get; set; }
        public Dictionary<string, LambdaConfigItem> Lambdas { get; set; }
        public Dictionary<string, ElasticSearchDomainConfigItem> ElasticSearchDomains { get; set; }
        public Dictionary<string, NetworkConfigItem> SNSTopics { get; set; }
        public Dictionary<string, NetworkConfigItem> APIs { get; set; }
        
    }
}