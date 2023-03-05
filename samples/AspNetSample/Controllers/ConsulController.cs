using Alastack.Consul;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sample.Common;

namespace AspNetSample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsulController : ControllerBase
    {
        private readonly ILogger<ConsulController> _logger;
        private readonly IConfiguration _configuration;

        public ConsulController(ILogger<ConsulController> logger, IConfiguration configuration) 
        {
            _logger = logger;
            _configuration = configuration;
        }

        // api/Consul/ConsulConfig
        [HttpGet("ConsulConfig")]
        public ConsulOptions? GetConsulConfig()
        {
            return _configuration.GetSection("Consul").Get<ConsulOptions>();
        }

        // api/Consul/ConsulOptions
        [HttpGet("ConsulOptions")]
        public ConsulOptions? GetConfig([FromServices] IOptionsMonitor<ConsulOptions> optionsMonitor)
        {
            return optionsMonitor.CurrentValue;
        }

        // api/Consul/TestConfig/config1
        // api/Consul/TestConfig/config2
        [HttpGet("TestConfig/{name}")]
        public ConfigOptions? GetTestConfig(string name)
        {
            return _configuration.GetSection(name).Get<ConfigOptions>();
        }
    }
}
