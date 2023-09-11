using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Alastack.Consul.Tests
{
    public class ConsulOptionsTests
    {
        [Fact]
        public void Test_Custom_ConfigValues()
        {
            var options = BuildConsulOptions("consul_test.json", "Consul1");
            Assert.NotNull(options);

            new ConsulPostConfigureOptions().PostConfigure(null, options!);

            // Agent

            Assert.Equal("dc1", options!.Agent.Datacenter);
            Assert.Equal(new Uri("http://127.0.0.1:8500"), options.Agent.Address);
            Assert.Equal("045ee828-2fa4-45b7-b01d-d9fb3937da54", options.Agent.Token);
            Assert.Equal(TimeSpan.FromSeconds(5), options.Agent.WaitTime);

            //Configuration

            Assert.NotNull(options.Configuration);
            Assert.Equal("/alastack/aspnetsample", options.Configuration!.PathBase);
            Assert.Equal("public1", options.Configuration.Namespace);

            Assert.Equal(2, options.Configuration.Sets!.Count);
            Assert.Equal("appsettings.json", options.Configuration.Sets[0].Id);
            Assert.Equal("default", options.Configuration.Sets[0].Group);
            Assert.False(options.Configuration.Sets[0].Optional);
            Assert.False(options.Configuration.Sets[0].ReloadOnChange);
            Assert.False(options.Configuration.Sets[0].IgnoreException);
            Assert.Equal(TimeSpan.FromSeconds(5), options.Configuration.Sets[0].PollingWaitTime);
            Assert.Null(options.Configuration.Sets[0].Description);

            Assert.Equal("config.json", options.Configuration.Sets[1].Id);
            Assert.Equal("group1", options.Configuration.Sets[1].Group);
            Assert.True(options.Configuration.Sets[1].Optional);
            Assert.True(options.Configuration.Sets[1].ReloadOnChange);
            Assert.True(options.Configuration.Sets[1].IgnoreException);
            Assert.Equal(TimeSpan.FromSeconds(10), options.Configuration.Sets[1].PollingWaitTime);
            Assert.Equal("some description", options.Configuration.Sets[1].Description);

            Assert.Equal(2, options.Configuration.Metadata!.Count);
            Assert.Equal("MyValue1", options.Configuration.Metadata["MyKey1"]);
            Assert.Equal("MyValue2", options.Configuration.Metadata["MyKey2"]);

            // Registration

            Assert.NotNull(options.Registration);
            foreach (var instance in options.Registration.Instances) 
            {
                Assert.Equal("aspnetsample1", instance!.Id);
                Assert.Equal("AspNetSample", options.Registration.Name);
                Assert.Equal("1.0.0", options.Registration.Version);
                Assert.Equal(new Uri("http://127.0.0.1:5000"), instance.Address);
                Assert.Equal(2, instance.Tags!.Length);
                Assert.True(instance.EnableTagOverride);
                Assert.Equal(2, options.Registration.Metadata!.Count);

                Assert.NotNull(instance.HealthCheck);
                Assert.Equal("AspNetSample_HealthCheck1", instance.HealthCheck.CheckId);
                Assert.Equal("AspNetSample_HealthCheck", instance.HealthCheck.Name);
                Assert.Equal(TimeSpan.FromMinutes(1), instance.HealthCheck.DeregisterCriticalServiceAfter);
                Assert.Equal(TimeSpan.FromSeconds(30), instance.HealthCheck.Interval);
                Assert.Equal(new Uri("http://127.0.0.1:5000/health"), instance.HealthCheck.Health);
                Assert.Equal(TimeSpan.FromSeconds(20), instance.HealthCheck.Timeout);
            }
            
        }

        [Fact]
        public void Test_Default_ConfigValues()
        {
            var options = BuildConsulOptions("consul_test.json", "Consul2");
            Assert.NotNull(options);

            new ConsulPostConfigureOptions().PostConfigure(null, options!);

            // Agent

            Assert.Null(options!.Agent.Datacenter);
            Assert.Equal(new Uri("http://127.0.0.1:8500"), options.Agent.Address);
            Assert.Null(options.Agent.Token);
            Assert.Null(options.Agent.WaitTime);

            //Configuration

            Assert.NotNull(options.Configuration);
            Assert.Equal("/alastack/aspnetsample", options.Configuration!.PathBase);
            Assert.Equal("public", options.Configuration.Namespace);

            Assert.Null(options.Configuration.Sets);

            Assert.Null(options.Configuration.Metadata);

            // Registration

            Assert.NotNull(options.Registration);
            foreach (var instance in options.Registration.Instances) 
            {
                Assert.Equal("AspNetSample@127.0.0.1:5000", instance.Id);
                Assert.Equal("AspNetSample", options.Registration.Name);
                Assert.Equal("1.0.0", options.Registration.Version);
                Assert.Equal(new Uri("http://127.0.0.1:5000"), instance.Address);
                Assert.Null(instance.Tags);
                Assert.False(instance.EnableTagOverride);
                Assert.Null(options.Registration.Metadata);

                Assert.NotNull(instance.HealthCheck);
                Assert.StartsWith("service:AspNetSample@127.0.0.1:5000", instance.HealthCheck.CheckId);
                Assert.Equal("Service 'AspNetSample' check", instance.HealthCheck.Name);
                Assert.Null(instance.HealthCheck.DeregisterCriticalServiceAfter);
                Assert.Equal(TimeSpan.FromSeconds(15), instance.HealthCheck.Interval);
                Assert.Equal("http://127.0.0.1:5000/health", instance.HealthCheck.Health.ToString());
                Assert.Equal(TimeSpan.FromSeconds(10), instance.HealthCheck.Timeout);
            }
               
        }

        private static ConsulOptions? BuildConsulOptions(string path, string key) 
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile(path,false,false);
            var configuration = builder.Build();
            var options = configuration.GetSection(key).Get<ConsulOptions>();
            return options;
        }
    }
}