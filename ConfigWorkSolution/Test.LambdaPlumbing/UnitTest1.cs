using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eti.LambdaPlumbing.Configuration;
using Eti.LambdaPlumbing.Configuration.ConfigExtractor;
using Xunit;

namespace Test.LambdaPlumbing
{
    public class UnitTest1
    {
        private static readonly string REGION = "us-west-2";
        
        [Fact]
        public void UpdateCyberMappings()
        {
            Task t = UpdateMappings("cyber-source", "2.0");
            t.Wait();

        }

        [Fact]
        public void UpdateEtiMappings()
        {
            Task t = UpdateMappings("eti", "2.0");
            t.Wait();

        }
        
        private async Task UpdateMappings(string serviceName, string serviceVersion)
        {
            var extractParms = new ExtractManager.NetworkConfigExtractParameters
            {
                AwsProfile = "NADEV",
                AwsRegion = REGION,
                ServiceName = serviceName,
                ServiceVersion = serviceVersion,
                TableName = "network-develop-ConfigTable-20567PXQ6W9W"
            };
            IEnumerable<CloudResourceMapping> mappings = await ExtractManager.ExtractNetworkConfig(extractParms);

            var importParms = new ExtractManager.ExtractManagerParameters
            {
                AwsProfile = "NADEV",
                AwsRegion = REGION,
                TableName = "rkt-eti-resource-mappings"
            };
            await ExtractManager.UpdateMappingsTable(importParms, mappings);
        }
        
    }
}