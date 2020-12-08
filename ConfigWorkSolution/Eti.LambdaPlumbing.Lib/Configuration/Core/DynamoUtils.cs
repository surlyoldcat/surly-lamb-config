using System;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;

namespace Eti.LambdaPlumbing.Configuration.Core
{
    internal static class DynamoUtils
    {
        public static AmazonDynamoDBClient GetDynamoClient(string region, string profile)
        {
            var regEndpoint = RegionEndpoint.GetBySystemName(region);
            if (!string.IsNullOrEmpty(profile))
            {
                var credentials = GetAwsCredentials(profile);
                return new AmazonDynamoDBClient(credentials, regEndpoint);
            }
            return new AmazonDynamoDBClient(regEndpoint);
        }
        
        public static AWSCredentials GetAwsCredentials(string profile)
        {
            var chain = new CredentialProfileStoreChain();
            AWSCredentials credentials;
            if (!chain.TryGetAWSCredentials(profile, out credentials))
                throw new ApplicationException($"Failed to get credentials for profile: {profile}");

            return credentials;
        }
    }
}