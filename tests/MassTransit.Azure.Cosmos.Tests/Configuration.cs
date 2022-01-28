namespace MassTransit.Azure.Cosmos.Tests
{
    using System;
    using NUnit.Framework;
    using Cosmos.Configuration;


    public static class Configuration
    {
        public static string EndpointUri =>
            TestContext.Parameters.Exists("CosmosEndpoint")
                ? TestContext.Parameters.Get("CosmosEndpoint")
                : Environment.GetEnvironmentVariable("MT_COSMOS_ENDPOINT")
                ?? EmulatorConstants.EndpointUri;

        public static string Key =>
            TestContext.Parameters.Exists("CosmosKey")
                ? TestContext.Parameters.Get("CosmosKey")
                : Environment.GetEnvironmentVariable("MT_COSMOS_KEY")
                ?? EmulatorConstants.Key;
    }
}
