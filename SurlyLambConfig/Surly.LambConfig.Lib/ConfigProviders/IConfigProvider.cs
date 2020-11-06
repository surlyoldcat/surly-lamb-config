using Surly.LambConfig.ConfigProviders.ProviderModel;

namespace Surly.LambConfig.ConfigProviders
{
    public interface IConfigProvider
    {
        LambConfigDocument LoadConfig();
    }
}