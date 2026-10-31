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
        public void AddConsulConfiguration_Throws_When_Sets_Is_Empty_And_Not_Optional()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Consul:Agent:Address"] = "http://localhost:8500",
                    ["Consul:Configuration:PathBase"] = "apps/sample"
                })
                .Build();

            var exception = Assert.Throws<InvalidOperationException>(
                () => new ConfigurationBuilder().AddConsulConfiguration(configuration));

            Assert.Contains("Configuration:Sets", exception.Message);
        }

        [Fact]
        public void AddConsulConfiguration_Returns_Builder_When_Sets_Is_Empty_And_Optional()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Consul:Agent:Address"] = "http://localhost:8500",
                    ["Consul:Configuration:PathBase"] = "apps/sample"
                })
                .Build();
            var builder = new ConfigurationBuilder();

            builder.AddConsulConfiguration(configuration, optional: true);

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
            // The client is supplied by a factory and may be shared, so the provider does not own it.
            Assert.False(sources[1].DisposeClient);
            Assert.NotNull(sources[1].OnLoadException);
        }

        [Fact]
        public void AddConsulConfiguration_Falls_Back_To_Default_Namespace_And_Group()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Consul:Agent:Address"] = "http://localhost:8500",
                    ["Consul:Configuration:PathBase"] = "/apps/sample",
                    ["Consul:Configuration:Namespace"] = "",
                    ["Consul:Configuration:Sets:0:Id"] = "appsettings.json",
                    ["Consul:Configuration:Sets:0:Group"] = ""
                })
                .Build();

            var builder = new ConfigurationBuilder().AddConsulConfiguration(configuration);
            var source = Assert.Single(builder.Sources.OfType<ConsulConfigurationSource>());

            Assert.Equal("apps/sample/public/default/appsettings.json", source.Path);
        }

        [Fact]
        public void AddConsulConfiguration_Throws_When_PollingWaitTime_Is_Not_Positive()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Consul:Agent:Address"] = "http://localhost:8500",
                    ["Consul:Configuration:PathBase"] = "apps/sample",
                    ["Consul:Configuration:Sets:0:Id"] = "config.json",
                    ["Consul:Configuration:Sets:0:PollingWaitTime"] = "00:00:00"
                })
                .Build();

            Assert.Throws<InvalidOperationException>(
                () => new ConfigurationBuilder().AddConsulConfiguration(configuration));
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

        [Fact]
        public void AddConsulConfiguration_Normalizes_Path_Segments()
        {
            var options = CreateOptions();
            options.PathBase = "/apps/sample/";
            options.Namespace = "/ns1/";
            options.Sets = new List<ConfigurationSet>
            {
                new() { Id = "/appsettings.json", Group = "default/" }
            };

            var builder = new ConfigurationBuilder().AddConsulConfiguration(options);
            var source = Assert.Single(builder.Sources.OfType<ConsulConfigurationSource>());

            Assert.Equal("apps/sample/ns1/default/appsettings.json", source.Path);
        }

        [Fact]
        public void AddConsulConfiguration_Throws_When_PathBase_Is_Missing()
        {
            var options = CreateOptions();
            options.PathBase = " ";
            options.Sets = new List<ConfigurationSet> { new() { Id = "appsettings.json" } };

            Assert.Throws<InvalidOperationException>(() => new ConfigurationBuilder().AddConsulConfiguration(options));
        }

        [Fact]
        public void AddConsulConfiguration_Throws_When_Agent_Is_Missing()
        {
            var options = CreateOptions();
            options.Agent = new AgentOptions();
            options.Sets = new List<ConfigurationSet> { new() { Id = "appsettings.json" } };

            Assert.Throws<InvalidOperationException>(() => new ConfigurationBuilder().AddConsulConfiguration(options));
        }

        [Fact]
        public void AddConsulConfiguration_Throws_When_Set_Id_Is_Missing()
        {
            var options = CreateOptions();
            options.Sets = new List<ConfigurationSet> { new() { Id = " " } };

            Assert.Throws<InvalidOperationException>(() => new ConfigurationBuilder().AddConsulConfiguration(options));
        }

        [Fact]
        public void AddConsulConfiguration_Throws_On_Unsupported_Extension_When_Format_Is_Auto()
        {
            var options = CreateOptions();
            options.Sets = new List<ConfigurationSet> { new() { Id = "config.yaml" } };

            // The format is resolved while reading the configuration instead of when the root is built.
            Assert.Throws<InvalidOperationException>(() => new ConfigurationBuilder().AddConsulConfiguration(options));
        }

        [Fact]
        public void AddConsulConfiguration_Does_Not_Add_Duplicate_Sources()
        {
            var options = CreateOptions();
            options.Sets = new List<ConfigurationSet> { new() { Id = "appsettings.json" } };
            var builder = new ConfigurationBuilder();

            builder.AddConsulConfiguration(options);
            builder.AddConsulConfiguration(options);

            Assert.Single(builder.Sources.OfType<ConsulConfigurationSource>());
        }

        private static ConsulConfigurationOptions CreateOptions()
        {
            return new ConsulConfigurationOptions
            {
                Agent = new AgentOptions { Address = new Uri("http://localhost:8500") },
                PathBase = "apps/sample",
                Sets = new List<ConfigurationSet>()
            };
        }
    }
}
