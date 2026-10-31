using Alastack.Consul.Configuration;
using Alastack.Extensions.Configuration.Consul;
using Microsoft.Extensions.Configuration;

namespace Alastack.Consul.Tests
{
    public class ConfigurationBuilderExtensionsTests
    {
        [Fact]
        public void AddConsulConfiguration_Throws_On_Null_Builder()
        {
            Assert.Throws<ArgumentNullException>(() => ((IConfigurationBuilder)null!).AddConsulConfiguration());
        }

        [Fact]
        public void AddConsulConfiguration_Throws_On_Null_Options()
        {
            Assert.Throws<ArgumentNullException>(
                () => new ConfigurationBuilder().AddConsulConfiguration((ConsulConfigurationOptions)null!));
        }

        [Fact]
        public void AddConsulConfiguration_Throws_When_Configuration_Section_Is_Missing()
        {
            var configuration = new ConfigurationBuilder().Build();

            Assert.Throws<InvalidOperationException>(() => new ConfigurationBuilder().AddConsulConfiguration(configuration));
        }

        [Fact]
        public void AddConsulConfiguration_Returns_Builder_When_Configuration_Section_Is_Missing_And_Optional()
        {
            var configuration = new ConfigurationBuilder().Build();
            var builder = new ConfigurationBuilder();

            builder.AddConsulConfiguration(configuration, optional: true);

            Assert.Empty(builder.Sources);
        }

        [Fact]
        public void AddConsulConfiguration_Does_Not_Add_Sources_When_Sets_Is_Empty()
        {
            var builder = new ConfigurationBuilder();

            builder.AddConsulConfiguration(new ConsulConfigurationOptions { Sets = new List<ConfigurationSet>() });

            Assert.Empty(builder.Sources);
        }

        [Fact]
        public void AddConsulConfiguration_Builds_Expected_Kv_Paths_And_Source_Settings()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Consul:Agent:Address"] = "http://localhost:8500",
                    ["Consul:Configuration:PathBase"] = "/apps/sample",
                    ["Consul:Configuration:Namespace"] = "ns1",
                    ["Consul:Configuration:Sets:0:Id"] = "appsettings.json",
                    ["Consul:Configuration:Sets:1:Id"] = "config.ini",
                    ["Consul:Configuration:Sets:1:Group"] = "group1",
                    ["Consul:Configuration:Sets:1:Optional"] = "true",
                    ["Consul:Configuration:Sets:1:ReloadOnChange"] = "true",
                    ["Consul:Configuration:Sets:1:PollingWaitTime"] = "00:00:10",
                    ["Consul:Configuration:Sets:1:Format"] = "Ini"
                })
                .Build();

            var builder = new ConfigurationBuilder().AddConsulConfiguration(configuration);
            var sources = builder.Sources.OfType<ConsulConfigurationSource>().ToList();

            Assert.Equal(2, sources.Count);

            Assert.Equal("apps/sample/ns1/default/appsettings.json", sources[0].Path);
            Assert.False(sources[0].Optional);
            Assert.False(sources[0].ReloadOnChange);
            Assert.Equal(TimeSpan.FromSeconds(5), sources[0].PollingWaitTime);
            Assert.Equal(ConsulConfigurationFormat.Auto, sources[0].Format);

            Assert.Equal("apps/sample/ns1/group1/config.ini", sources[1].Path);
            Assert.True(sources[1].Optional);
            Assert.True(sources[1].ReloadOnChange);
            Assert.Equal(TimeSpan.FromSeconds(10), sources[1].PollingWaitTime);
            Assert.Equal(ConsulConfigurationFormat.Ini, sources[1].Format);
            Assert.NotNull(sources[1].ClientFactory);
            Assert.NotNull(sources[1].OnLoadException);
        }

        [Fact]
        public void AddConsulConfiguration_Maps_IgnoreException_To_OnLoadException()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Consul:Agent:Address"] = "http://localhost:8500",
                    ["Consul:Configuration:PathBase"] = "apps/sample",
                    ["Consul:Configuration:Sets:0:Id"] = "config.json",
                    ["Consul:Configuration:Sets:0:IgnoreException"] = "true"
                })
                .Build();

            var builder = new ConfigurationBuilder().AddConsulConfiguration(configuration);
            var source = Assert.Single(builder.Sources.OfType<ConsulConfigurationSource>());
            var context = new ConsulLoadExceptionContext(new InvalidOperationException("load failed"));

            source.OnLoadException!(context);

            Assert.True(context.Ignore);
        }
    }
}
