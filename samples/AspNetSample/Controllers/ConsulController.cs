using Alastack.Consul;
using Alastack.Consul.Registration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sample.Common;
using System.Text.Json;

namespace AspNetSample.Controllers;

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

    [HttpGet("Configuration")]
    public object GetConfiguration()
    {
        //var configAsList = _configuration.AsEnumerable().ToList();
        //var json = JsonSerializer.Serialize(configAsList);

        var configAsDict = _configuration.AsEnumerable().ToDictionary(c => c.Key, c => c.Value);
        var json = JsonSerializer.Serialize(configAsDict);

        return json;
    }

    // api/Consul/ConsulConfig
    [HttpGet("ConsulConfig")]
    public ConsulRegistrationOptions? GetConsulConfig()
    {
        var options = _configuration.GetSection("Consul:Registration").Get<ConsulRegistrationOptions>();
        if (options != null)
        {
            _configuration.GetSection("Consul:Agent").Bind(options.Agent);
        }
        return options;
    }

    // api/Consul/ConsulOptions
    [HttpGet("ConsulOptions")]
    public ConsulRegistrationOptions? GetConfig([FromServices] IOptionsMonitor<ConsulRegistrationOptions> optionsMonitor)
    {
        return optionsMonitor.CurrentValue;
    }
}
