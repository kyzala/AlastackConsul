# Alastack.Consul

Alastack.Consul supports Configuration and Service Discovery with Hashicorp Consul.

[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/kyzala/AlastackConsul/dotnet.yml?branch=main)](https://github.com/kyzala/AlastackConsul/actions/workflows/dotnet.yml)[![GitHub](https://img.shields.io/github/license/kyzala/AlastackConsul)](LICENSE)[![Nuget](https://img.shields.io/nuget/v/Alastack.Consul)](https://www.nuget.org/packages/Alastack.Consul)

## Getting started

### Install package from the .NET CLI

Client：

```
dotnet add package Alastack.Consul
```

### Configuration and Registration

The following code snippet demonstrates using Configuration and Service Discovery with Hashicorp Consul:

```c#
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("consul.json", true, true);
// Add Consul Configuration
builder.Configuration.AddConsulConfiguration();
// Add Consul Registration
builder.Services.AddConsul(builder.Configuration);
builder.Services.AddControllers();

var app = builder.Build();
// Use HealthCheck Endpoint
app.UseHealthCheck("/health");
app.UseAuthorization();
app.MapControllers();

app.Run();
```

## All Config properties

```JSON
{
  "Consul": {
    "Agent": {
      "Datacenter": "dc1", // Default to "dc1"
      "Address": "http://127.0.0.1:8500",
      "Token": "045ee828-2fa4-45b7-b01d-d9fb3937da54", // Default to null
      "WaitTime": "00:00:05" // Default to null
    },
    "Configuration": {
      "PathBase": "alastack/aspnetsample",
      "Namespace": "ns.default", // Default to "ns.default"
      "Sets": [
        {
          "Id": "appsettings.json",
          "Group": "gp.default" // Default to "gp.default"
        },
        {
          "Id": "config.json",
          "Group": "gp.group1"
        },
        {
          "Id": "config_other.json",
          "Group": "gp.group1",
          "Optional": false,
          "ReloadOnChange": false,
          "PollingWaitTime": "00:00:10",
          "IgnoreException": true,
          "Description": "description"
        }
      ],
      "Metadata": {
        "MyKey1": "MyValue1",
        "MyKey2": "MyValue2"
      }
    },
    "Registration": {
      "Id": "aspnetsample1", // Default to $"{registration.Name}:{registration.Version}#{registration!.Address.Host}:{registration!.Address.Port}"
      "Name": "AspNetSample",
      "Version": "1.0.0", // Default to "1.0.0"
      "Address": "http://127.0.0.1:5000",
      "Tags": [ "apiserver", "test" ],
      "EnableTagOverride": true, // Default to false
      "HealthCheck": {
        "CheckId": "AspNetSample_HealthCheck1", // Default to $"{Registration.Name}_hk_{Guid.NewGuid():n}"
        "Name": "AspNetSample_HealthCheck", // Default to $"{Registration.Name}_hk"
        "DeregisterCriticalServiceAfter": "00:01:00", // Default to null
        "Interval": "00:00:15", // Default to "00:00:15"
        "Health": "http://127.0.0.1:5000/health",
        "Timeout": "00:00:10" // Default to "00:00:10"
      },
      "Metadata": {
        "RegistrationIdPolicy": "Default", // or "ByConsul"
        "HealthCheckIdPolicy": "Default", // or "ByConsul"
        "HealthCheckNamePolicy": "Default", // or "ByConsul"
        "MyKey1": "MyValue1"
      }
    }
  }
}
```

### Configuration in Consul Key/Value store

Using [Winton.Extensions.Configuration.Consul](https://github.com/wintoncode/Winton.Extensions.Configuration.Consul) to provide Consul Configuration source.

**Path key:** 
`{Configuration.PathBase} / {Configuration.Namespace} / {Set.Group} / {Set.Id}`

## Samples

Visit the [samples](https://github.com/kyzala/AlastackConsul/tree/main/samples) folder.