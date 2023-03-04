using Alastack.Consul;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet("options")]
        public ConsulOptions? GetConfig()
        {
            return _configuration.GetSection("Consul").Get<ConsulOptions>();
        }

        [HttpGet("config/{name}")]
        public ConfigOptions? GetConfigOptions(string name)
        {
            return _configuration.GetSection(name).Get<ConfigOptions>();
        }
    }
}
