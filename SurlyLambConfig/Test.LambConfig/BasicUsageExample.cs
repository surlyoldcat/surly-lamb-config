using System;
using System.Runtime.InteropServices;
using Surly.LambConfig;
using Xunit;

namespace Test.LambConfig
{
    public class BasicUsageExample
    {
        [Fact]
        public void Test1()
        {
#if DEBUG
            
            var foo = new ConfigBuilder()
                .WithAwsProfile("NALAB")
                .UseLocalFile("bigoldconfigfile.json")
                .Build();

            
#else
            var foo = new ConfigBuilder()
                            .WithDefaultAwsProfile()
                            .EnableCaching()
                            .UseDynamoDB()
                            .Build();
#endif

            var configDoc = LambConfiguration.Config;
            var region = configDoc.Settings[SettingsKeys.Region];
            var source = configDoc.Settings[SettingsKeys.ConfigurationSource];
            
            Assert.Equal("localfile", source);
        }
    }
}