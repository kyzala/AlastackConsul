using System.IO;
using System.Text;
using Alastack.Extensions.Configuration.Consul;
using Xunit;

namespace Alastack.Extensions.Configuration.Consul.Tests
{
    public class ConsulConfigurationProviderTests
    {
        [Fact]
        public void Parse_Json_Flattens_Sections()
        {
            var data = Parse(ConsulConfigurationFormat.Json, """{"Section":{"Name":"value"},"Key":"1"}""");

            Assert.Equal("value", data["Section:Name"]);
            Assert.Equal("1", data["Key"]);
        }

        [Fact]
        public void Parse_Ini_Flattens_Sections()
        {
            var data = Parse(ConsulConfigurationFormat.Ini, "[Section]\nName=value\n");

            Assert.Equal("value", data["Section:Name"]);
        }

        [Fact]
        public void Parse_Xml_Flattens_Elements()
        {
            var data = Parse(ConsulConfigurationFormat.Xml, "<configuration><Section><Name>value</Name></Section></configuration>");

            Assert.Equal("value", data["Section:Name"]);
        }

        [Fact]
        public void Parse_Unsupported_Format_Throws()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{}"));

            Assert.Throws<InvalidOperationException>(() => ConsulConfigurationProvider.Parse(ConsulConfigurationFormat.Auto, stream));
        }

        private static IDictionary<string, string?> Parse(ConsulConfigurationFormat format, string content)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            return ConsulConfigurationProvider.Parse(format, stream);
        }
    }
}
