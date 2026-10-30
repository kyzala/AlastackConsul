using Alastack.Consul;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace RegistrationSample.Controllers;

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

    [HttpGet("Configuration")]
    public object GetConfiguration()
    {
        var configAsDict = _configuration.AsEnumerable().ToDictionary(c => c.Key, c => c.Value);
        var json = JsonSerializer.Serialize(configAsDict);

        return json;
    }
}
