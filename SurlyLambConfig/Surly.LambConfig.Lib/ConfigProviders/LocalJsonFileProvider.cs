namespace Surly.LambConfig.ConfigProviders
{
    internal class LocalJsonFileProvider : IConfigProvider
    {
        private readonly string _fileName;
        private readonly LambConfigDocument _configDoc;
        
        public LocalJsonFileProvider(string filename, LambConfigDocument configDoc)
        {
            _fileName = filename;
            _configDoc = configDoc;
        }
        
        public void UpdateConfig()
        {
            throw new System.NotImplementedException();
        }
    }
}