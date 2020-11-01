namespace Surly.LambConfig.ConfigProviders
{
    internal class DynamoDBProvider : IConfigProvider
    {
        public DynamoDBProvider(string addDynamoParamsFromEnvVars)
        {
            //or maybe just pull env vars from in here?
        }
        
        public LambConfigDocument LoadConfig()
        {
            throw new System.NotImplementedException();
        }
    }
}