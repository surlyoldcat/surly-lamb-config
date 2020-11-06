using System;
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
                .WithAwsProfile("NADEV")
                .WithAwsRegion("us-west-2")
                .WithSettingsOverride(ConfigKeys.ServiceName, "cyber-source")
                .WithSettingsOverride(ConfigKeys.ServiceVersion, "2.0")
                .WithSettingsOverride(ConfigKeys.NetworkConfigTable, "network-develop-ConfigTable-20567PXQ6W9W")
                .ExportFinalConfigTo("configOutput.json")
                .Build();

            
#else
            var foo = new ConfigBuilder()
                            .EnableCaching()
                            .UseDynamoDB()
                            .Build();
#endif

            var configDoc = ConfigManager.Config;
            var region = configDoc.Settings[ConfigKeys.AwsRegion];
            
            Assert.False(string.IsNullOrEmpty(region));
        }
    }
}