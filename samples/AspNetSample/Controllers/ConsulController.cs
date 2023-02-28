using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sample.Configuration;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Xml.Linq;
using Alastack.Consul;

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
