namespace Surly.LambConfig.ConfigProviders
{
    internal interface IConfigCacheProvider : IConfigProvider
    {
        void WriteConfig(LambConfigDocument doc);
    }
    
    internal class FileCacheProvider : IConfigCacheProvider
    {
        //why not use BSON for a little more efficiency
        public LambConfigDocument LoadConfig()
        {
            throw new System.NotImplementedException();
        }

        public void WriteConfig(LambConfigDocument doc)
        {
            throw new System.NotImplementedException();
        }
    }
}