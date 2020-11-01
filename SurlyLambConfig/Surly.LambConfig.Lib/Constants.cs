namespace Surly.LambConfig
{
    public static class SettingsKeys
    {
        public const string Region = "region";
        public const string ConfigurationSource = "configsource";
        public const string AwsProfile = "awsprofile";
        
       
    }
    
    internal static class EnvironmentKeys
    {
        public const string AwsRegion = "AWS_REGION";
        public const string NetworkConfigTable = "NETWORK_CONFIG_TABLE";
        public const string ServiceName = "SERVICE_NAME";
        public const string ServiceVersion = "SERVICE_VERSION";
    }
}