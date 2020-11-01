namespace Surly.LambConfig.ConfigProviders
{
    public interface IConfigProvider
    {
        LambConfigDocument LoadConfig();
    }
}