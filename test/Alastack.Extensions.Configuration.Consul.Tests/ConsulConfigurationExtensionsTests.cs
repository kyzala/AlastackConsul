using Alastack.Extensions.Configuration.Consul;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Alastack.Extensions.Configuration.Consul.Tests
{
    public class ConsulConfigurationExtensionsTests
    {
        [Fact]
        public void AddConsul_Throws_On_Null_Builder()
        {
            Assert.Throws<ArgumentNullException>(() => ((IConfigurationBuilder)null!).AddConsul("config.json"));
        }

        [Fact]
        public void AddConsul_Adds_Source_And_Applies_Configure()
        {
            var builder = new ConfigurationBuilder()
                .AddConsul("apps/sample.json", source =>
                {
                    source.Optional = true;
                    source.ReloadOnChange = true;
                    source.PollingWaitTime = TimeSpan.FromSeconds(1);
                    source.Format = ConsulConfigurationFormat.Ini;
                });

            var source = Assert.Single(builder.Sources.OfType<ConsulConfigurationSource>());

            Assert.Equal("apps/sample.json", source.Path);
            Assert.True(source.Optional);
            Assert.True(source.ReloadOnChange);
            Assert.Equal(TimeSpan.FromSeconds(1), source.PollingWaitTime);
            Assert.Equal(ConsulConfigurationFormat.Ini, source.Format);
        }

        [Fact]
        public void AddConsul_Uses_Source_Defaults()
        {
            var builder = new ConfigurationBuilder().AddConsul("config.json");

            var source = Assert.Single(builder.Sources.OfType<ConsulConfigurationSource>());

            Assert.Equal(ConsulConfigurationFormat.Auto, source.Format);
            Assert.False(source.Optional);
            Assert.False(source.ReloadOnChange);
            Assert.Equal(TimeSpan.FromSeconds(5), source.PollingWaitTime);
        }
    }
}
