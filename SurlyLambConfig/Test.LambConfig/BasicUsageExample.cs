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
                .UseDynamoDB()
                .WithSettingsOverride(SettingsKeys.ServiceName, "cyber-source")
                .WithSettingsOverride(SettingsKeys.ServiceVersion, "2.0")
                .WithSettingsOverride(SettingsKeys.NetworkConfigTable, "network-develop-ConfigTable-20567PXQ6W9W")
                .WithSettingsOverride(SettingsKeys.ServiceRegistryTable, "network-develop-ServiceRegistry-15JZPD2909JLR")
                .ExportFinalConfigTo("configOutput.json")
                .Build();

            
#else
            var foo = new ConfigBuilder()
                            .EnableCaching()
                            .UseDynamoDB()
                            .Build();
#endif

            var configDoc = LambConfiguration.Config;
            var region = configDoc.Settings[SettingsKeys.Region];
            var source = configDoc.Settings[SettingsKeys.ConfigurationSource];
            
            Assert.False(string.IsNullOrEmpty(region));
        }
    }
}