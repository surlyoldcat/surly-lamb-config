namespace Surly.LambConfig.ConfigProviders
{
    internal interface IConfigCacheProvider : IConfigProvider
    {
        void WriteConfig();
    }
    
    internal class FileCacheProvider : IConfigCacheProvider
    {
        //why not use BSON for a little more efficiency
        public void UpdateConfig()
        {
            throw new System.NotImplementedException();
        }

        public void WriteConfig()
        {
            
            throw new System.NotImplementedException();
        }
    }
}