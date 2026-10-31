using Alastack.Consul;
using Alastack.Consul.Registration;
using Microsoft.Extensions.Options;

namespace Alastack.Consul.Tests
{
    public class ConsulRegistrationValidateOptionsTests
    {
        private readonly ConsulRegistrationValidateOptions _validator = new();

        [Fact]
        public void Validate_Fails_When_Agent_Is_Null()
        {
            var result = _validator.Validate(null, new ConsulRegistrationOptions { Agent = null! });

            Assert.True(result.Failed);
            Assert.Equal("Agent.Address is required.", result.FailureMessage);
        }

        [Fact]
        public void Validate_Fails_When_Agent_Address_Is_Missing()
        {
            var result = _validator.Validate(null, new ConsulRegistrationOptions());

            Assert.True(result.Failed);
            Assert.Equal("Agent.Address is required.", result.FailureMessage);
        }

        [Fact]
        public void Validate_Succeeds_When_Agent_Address_Is_Set()
        {
            var options = new ConsulRegistrationOptions
            {
                Agent = new AgentOptions { Address = new Uri("http://localhost:8500") }
            };

            Assert.True(_validator.Validate(null, options).Succeeded);
        }
    }
}
