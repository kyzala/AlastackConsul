using Alastack.Consul.Configuration;
using Alastack.Extensions.Configuration.Consul;
using Microsoft.Extensions.Configuration;

namespace Alastack.Consul.Tests
{
    public class ConsulConfigurationOptionsTests
    {
        [Fact]
        public void Test_Custom_ConfigValues()
        {
            var options = BuildOptions("consul_test.json", "Consul1");
            Assert.NotNull(options);

            // Agent

            Assert.Equal("dc1", options!.Agent.Datacenter);
            Assert.Equal(new Uri("http://localhost:8500"), options.Agent.Address);
            Assert.Equal("045ee828-2fa4-45b7-b01d-d9fb3937da54", options.Agent.Token);
            Assert.Equal(TimeSpan.FromSeconds(5), options.Agent.WaitTime);

            // Configuration

            Assert.Equal("/samples/aspnetsample", options.PathBase);
            Assert.Equal("public1", options.Namespace);

            Assert.Equal(2, options.Sets!.Count);
            Assert.Equal("appsettings.json", options.Sets[0].Id);
            Assert.Equal("default", options.Sets[0].Group);
            Assert.False(options.Sets[0].Optional);
            Assert.False(options.Sets[0].ReloadOnChange);
            Assert.False(options.Sets[0].IgnoreException);
            Assert.Equal(TimeSpan.FromSeconds(5), options.Sets[0].PollingWaitTime);
            Assert.Null(options.Sets[0].Description);
            Assert.Equal(ConsulConfigurationFormat.Auto, options.Sets[0].Format);

            Assert.Equal("config.json", options.Sets[1].Id);
            Assert.Equal("group1", options.Sets[1].Group);
            Assert.True(options.Sets[1].Optional);
            Assert.True(options.Sets[1].ReloadOnChange);
            Assert.True(options.Sets[1].IgnoreException);
            Assert.Equal(TimeSpan.FromSeconds(10), options.Sets[1].PollingWaitTime);
            Assert.Equal("some description", options.Sets[1].Description);
            Assert.Equal(ConsulConfigurationFormat.Json, options.Sets[1].Format);

            Assert.Equal(2, options.Metadata!.Count);
            Assert.Equal("MyValue1", options.Metadata["MyKey1"]);
            Assert.Equal("MyValue2", options.Metadata["MyKey2"]);
        }

        [Fact]
        public void Test_Default_ConfigValues()
        {
            var options = BuildOptions("consul_test.json", "Consul2");
            Assert.NotNull(options);

            // Agent

            Assert.Null(options!.Agent.Datacenter);
            Assert.Equal(new Uri("http://localhost:8500"), options.Agent.Address);
            Assert.Null(options.Agent.Token);
            Assert.Null(options.Agent.WaitTime);

            // Configuration

            Assert.Equal("/alastack/aspnetsample", options.PathBase);
            Assert.Equal("public", options.Namespace);
            Assert.Null(options.Sets);
            Assert.Null(options.Metadata);
        }

        [Fact]
        public void Test_AddConsulConfiguration_Requires_Agent()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Consul:Configuration:PathBase"] = "alastack/test",
                    ["Consul:Configuration:Sets:0:Id"] = "config.json"
                });

            Assert.Throws<InvalidOperationException>(() => builder.AddConsulConfiguration());
        }

        [Fact]
        public void Test_AddConsulConfiguration_Requires_PathBase()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Consul:Agent:Address"] = "http://localhost:8500",
                    ["Consul:Configuration:Sets:0:Id"] = "config.json"
                });

            Assert.Throws<InvalidOperationException>(() => builder.AddConsulConfiguration());
        }

        private static ConsulConfigurationOptions? BuildOptions(string path, string key)
        {
            var configuration = new ConfigurationBuilder().AddJsonFile(path, false, false).Build();
            var root = configuration.GetSection(key);
            var options = root.GetSection("Configuration").Get<ConsulConfigurationOptions>();
            if (options != null)
            {
                root.GetSection("Agent").Bind(options.Agent);
            }
            return options;
        }
    }
}
