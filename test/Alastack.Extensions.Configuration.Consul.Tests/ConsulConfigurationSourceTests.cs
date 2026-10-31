using Alastack.Extensions.Configuration.Consul;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Alastack.Extensions.Configuration.Consul.Tests
{
    public class ConsulConfigurationSourceTests
    {
        [Theory]
        [InlineData("config.json", ConsulConfigurationFormat.Json)]
        [InlineData("config.JSON", ConsulConfigurationFormat.Json)]
        [InlineData("group/config.ini", ConsulConfigurationFormat.Ini)]
        [InlineData("config.xml", ConsulConfigurationFormat.Xml)]
        public void Format_Is_Resolved_From_Extension(string path, ConsulConfigurationFormat expected)
        {
            Assert.Equal(expected, ConsulConfigurationSource.ResolveFormat(path));
        }

        [Theory]
        [InlineData("config.yaml")]
        [InlineData("config")]
        [InlineData("")]
        public void Unsupported_Extension_Throws(string path)
        {
            Assert.Throws<InvalidOperationException>(() => ConsulConfigurationSource.ResolveFormat(path));
        }

        [Fact]
        public void Defaults_Are_Applied()
        {
            var source = new ConsulConfigurationSource();

            Assert.Equal(ConsulConfigurationFormat.Auto, source.Format);
            Assert.False(source.Optional);
            Assert.False(source.ReloadOnChange);
            Assert.Equal(TimeSpan.FromSeconds(5), source.PollingWaitTime);
            Assert.Null(source.ClientFactory);
            Assert.Null(source.OnLoadException);
        }

        [Fact]
        public void Build_Uses_Explicit_Format_Over_Extension()
        {
            var source = new ConsulConfigurationSource { Path = "config.txt", Format = ConsulConfigurationFormat.Xml };

            Assert.IsType<ConsulConfigurationProvider>(source.Build(new ConfigurationBuilder()));
        }

        [Theory]
        [InlineData("config.json")]
        [InlineData("config.ini")]
        [InlineData("config.xml")]
        public void Build_Resolves_Auto_Format_From_Extension(string path)
        {
            var source = new ConsulConfigurationSource { Path = path };

            Assert.IsType<ConsulConfigurationProvider>(source.Build(new ConfigurationBuilder()));
        }

        [Fact]
        public void Build_Throws_For_Unknown_Extension_When_Format_Is_Auto()
        {
            var source = new ConsulConfigurationSource { Path = "config.txt" };

            Assert.Throws<InvalidOperationException>(() => source.Build(new ConfigurationBuilder()));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public void Build_Requires_Path(string path)
        {
            var source = new ConsulConfigurationSource { Path = path };

            Assert.Throws<InvalidOperationException>(() => source.Build(new ConfigurationBuilder()));
        }
    }
}
