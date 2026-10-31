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
        public void Build_Requires_Path()
        {
            var source = new ConsulConfigurationSource { Path = "" };

            Assert.Throws<InvalidOperationException>(() => source.Build(new ConfigurationBuilder()));
        }
    }
}
