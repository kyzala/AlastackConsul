using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sample.Configuration;

namespace AspNetSample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        private readonly ILogger<ConfigController> _logger;
        private readonly IConfiguration _configuration;

        public ConfigController(ILogger<ConfigController> logger, IConfiguration configuration) 
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("raw")]
        public IConfiguration GetConfig()
        {
            _logger.LogInformation(_configuration.ToString());

            return _configuration;

        }

        [HttpGet("{name}")]
        public ConfigOptions? GetConfigOptions(string name)
        {
            _logger.LogInformation(_configuration.GetSection(name).ToString());

            return _configuration.GetSection(name).Get<ConfigOptions>();
        }
    }
}
