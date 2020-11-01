namespace Surly.LambConfig.ConfigProviders
{
    internal class LocalJsonFileProvider : IConfigProvider
    {
        public LocalJsonFileProvider(string filename)
        {
            
        }
        public LambConfigDocument LoadConfig()
        {
            throw new System.NotImplementedException();
        }
    }
}