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
            var options = CreateValidOptions();

            Assert.True(_validator.Validate(null, options).Succeeded);
        }

        [Fact]
        public void Validate_Fails_When_Name_Is_Missing()
        {
            var options = CreateValidOptions();
            options.Name = " ";

            var result = _validator.Validate(null, options);

            Assert.True(result.Failed);
            Assert.Equal("Name is required.", result.FailureMessage);
        }

        [Fact]
        public void Validate_Fails_When_Instance_Address_Is_Missing()
        {
            var options = CreateValidOptions();
            options.Instances.Add(new ServiceInstance());

            var result = _validator.Validate(null, options);

            Assert.True(result.Failed);
            Assert.Equal("Instances.Address is required.", result.FailureMessage);
        }

        [Fact]
        public void Validate_Fails_When_Health_Check_Is_Enabled_Without_Endpoint()
        {
            var options = CreateValidOptions();
            options.DefaultHealthCheck.Endpoint = null!;

            var result = _validator.Validate(null, options);

            Assert.True(result.Failed);
            Assert.Equal(
                "DefaultHealthCheck.Endpoint is required when the health check is enabled.",
                result.FailureMessage);
        }

        [Fact]
        public void Validate_Succeeds_When_Health_Check_Is_Disabled_Without_Endpoint()
        {
            var options = CreateValidOptions();
            options.DefaultHealthCheck.Enabled = false;
            options.DefaultHealthCheck.Endpoint = null!;

            Assert.True(_validator.Validate(null, options).Succeeded);
        }

        private static ConsulRegistrationOptions CreateValidOptions()
        {
            return new ConsulRegistrationOptions
            {
                Name = "AspNetSample",
                Agent = new AgentOptions { Address = new Uri("http://localhost:8500") }
            };
        }
    }
}
