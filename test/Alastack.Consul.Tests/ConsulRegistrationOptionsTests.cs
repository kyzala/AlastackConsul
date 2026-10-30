using Alastack.Consul.Registration;
using Microsoft.Extensions.Configuration;

namespace Alastack.Consul.Tests
{
    public class ConsulRegistrationOptionsTests
    {
        [Fact]
        public void Test_Custom_ConfigValues()
        {
            var options = BuildOptions("consul_test.json", "Consul1");
            Assert.NotNull(options);

            new ConsulRegistrationPostConfigureOptions().PostConfigure(null, options!);

            // Agent

            Assert.Equal("dc1", options!.Agent.Datacenter);
            Assert.Equal(new Uri("http://localhost:8500"), options.Agent.Address);
            Assert.Equal("045ee828-2fa4-45b7-b01d-d9fb3937da54", options.Agent.Token);
            Assert.Equal(TimeSpan.FromSeconds(5), options.Agent.WaitTime);

            // Registration

            foreach (var instance in options.Instances)
            {
                Assert.Equal("aspnetsample1", instance!.Id);
                Assert.Equal("AspNetSample", options.Name);
                Assert.Equal("1.0.0", options.Version);
                Assert.Equal(new Uri("http://localhost:5000"), instance.Address);
                Assert.Equal(2, instance.Tags!.Length);
                Assert.True(instance.EnableTagOverride);
                Assert.Equal(2, options.Metadata!.Count);

                Assert.NotNull(instance.HealthCheck);
                Assert.Equal("AspNetSample_HealthCheck1", instance.HealthCheck.CheckId);
                Assert.Equal("AspNetSample_HealthCheck", instance.HealthCheck.Name);
                Assert.Equal(TimeSpan.FromMinutes(1), instance.HealthCheck.DeregisterCriticalServiceAfter);
                Assert.Equal(TimeSpan.FromSeconds(30), instance.HealthCheck.Interval);
                Assert.Equal(new Uri("http://localhost:5000/health"), instance.HealthCheck.Endpoint);
                Assert.Equal(TimeSpan.FromSeconds(20), instance.HealthCheck.Timeout);
            }
        }

        [Fact]
        public void Test_Default_ConfigValues()
        {
            var options = BuildOptions("consul_test.json", "Consul2");
            Assert.NotNull(options);

            new ConsulRegistrationPostConfigureOptions().PostConfigure(null, options!);

            // Agent

            Assert.Null(options!.Agent.Datacenter);
            Assert.Equal(new Uri("http://localhost:8500"), options.Agent.Address);
            Assert.Null(options.Agent.Token);
            Assert.Null(options.Agent.WaitTime);

            // Registration

            foreach (var instance in options.Instances)
            {
                Assert.Equal("AspNetSample@localhost:5000", instance.Id);
                Assert.Equal("AspNetSample", options.Name);
                Assert.Equal("1.0.0", options.Version);
                Assert.Equal(new Uri("http://localhost:5000"), instance.Address);
                Assert.Null(instance.Tags);
                Assert.False(instance.EnableTagOverride);
                Assert.Null(options.Metadata);

                Assert.NotNull(instance.HealthCheck);
                Assert.StartsWith("service:AspNetSample@localhost:5000", instance.HealthCheck.CheckId);
                Assert.Equal($"Service '{instance.Id}' check", instance.HealthCheck.Name);
                Assert.NotNull(instance.HealthCheck.DeregisterCriticalServiceAfter);
                Assert.Equal(TimeSpan.FromSeconds(15), instance.HealthCheck.Interval);
                Assert.Equal("http://localhost:5000/health", instance.HealthCheck.Endpoint.ToString());
                Assert.Equal(TimeSpan.FromSeconds(10), instance.HealthCheck.Timeout);
            }
        }

        [Fact]
        public void Test_Registration_Instances_Default_To_Empty()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Consul:Agent:Address"] = "http://localhost:8500",
                    ["Consul:Registration:Name"] = "test"
                })
                .Build();

            var root = configuration.GetSection("Consul");
            var options = root.GetSection("Registration").Get<ConsulRegistrationOptions>();

            Assert.NotNull(options);
            Assert.Empty(options!.Instances);
        }

        private static ConsulRegistrationOptions? BuildOptions(string path, string key)
        {
            var configuration = new ConfigurationBuilder().AddJsonFile(path, false, false).Build();
            var root = configuration.GetSection(key);
            var options = root.GetSection("Registration").Get<ConsulRegistrationOptions>();
            if (options != null)
            {
                root.GetSection("Agent").Bind(options.Agent);
            }
            return options;
        }
    }
}
