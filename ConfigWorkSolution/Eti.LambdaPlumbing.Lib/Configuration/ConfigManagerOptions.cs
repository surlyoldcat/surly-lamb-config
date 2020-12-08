namespace Eti.LambdaPlumbing.Configuration
{
    public class ConfigManagerOptions
    {

        public double CacheExpirationMinutes { get; set; } = 15.0d;
        public bool UseCache { get; set; } = true;
    }
}