# Alastack.Consul

Alastack.Consul supports Configuration and Service Discovery with Hashicorp Consul.

[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/kyzala/AlastackConsul/dotnet.yml?branch=main)](https://github.com/kyzala/AlastackConsul/actions/workflows/dotnet.yml)[![GitHub](https://img.shields.io/github/license/kyzala/AlastackConsul)](LICENSE)[![Nuget](https://img.shields.io/nuget/v/Alastack.Consul)](https://www.nuget.org/packages/Alastack.Consul)

## Getting started

### Install package from the .NET CLI

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

// Add Consul Naming
builder.Services.AddConsulNaming(builder.Configuration);
builder.Services.AddControllers();

var app = builder.Build();
// Use HealthCheck Endpoint
app.UseHealthCheck("/health");
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### Dynamic Registration for AspNetCore

```
dotnet add package Alastack.Consul.AspNetCore
```

code snippet:

```c#
// builder.Services.AddConsulNaming(builder.Configuration);
builder.Services.AddConsulNaming(builder.Configuration).AddAspNetCore();
```

```JSON
{
  "Consul": {
    "Agent": {
      "Datacenter": "dc1",
      "Address": "http://localhost:8500",
      "WaitTime": "00:00:05"
    },
    "Registration": {
      "Name": "DynamicInstanceSample",
      "Version": "1.0.0",
      "Metadata": {
      }
    }
  }
}
```

## All Config properties

```JSON
{
  "Consul": {
    "Agent": {
      "Datacenter": "dc1", // Default to "dc1"
      "Address": "http://localhost:8500",
      "Token": "045ee828-2fa4-45b7-b01d-d9fb3937da54", // Default to null
      "WaitTime": "00:00:05" // Default to null
    },
    "Configuration": {
      "PathBase": "/dotnet/aspnetsample",
      "Namespace": "public1", // Default to "public"
      "Sets": [
        {
          "Id": "appsettings.json"
          //"Group": "default"
        },
        {
          "Id": "config.json",
          "Group": "group1" // Default to "default"
        },
        {
          "Id": "config_other.json",
          "Group": "group1",
          "Optional": true, // Default to false
          "ReloadOnChange": true, // Default to false
          "PollingWaitTime": "00:00:10", // Default to 5s
          "IgnoreException": true, // Default to false
          "Description": "description" // Default to null
        }
      ],
      "Metadata": {
        "MyKey1": "MyValue1",
        "MyKey2": "MyValue2"
      }
    },
    "Registration": {
      //"IsEnabled": true, // Default to "true"
      "Name": "AspNetSample",
      "Version": "1.0.0", // Default to "1.0.0"
      "Instances": [
        {
          //"Id": "aspnetsample1", // Default to $"{registration.Name}@{registration.Address.Host}:{registration.Address.Port}"
          "Address": "http://localhost:5000",
          "Tags": [ "apiserver", "test", "http" ],
          "EnableTagOverride": true, // Default to false
          "HealthCheck": {
            //"CheckId": "AspNetSample_HealthCheck1", // Default to $"service:{options.Registration.Id}"
            //"Name": "AspNetSample_HealthCheck", // Default to $"Service '{options.Registration.Name}' check"
            "DeregisterCriticalServiceAfter": "00:01:00", // Default to null
            "Interval": "00:00:15", // Default to "00:00:15"
            "Health": "/health", // or absolute uri: http://127.0.0.1:5000/health
            "Timeout": "00:00:10" // Default to "00:00:10"
          }
        },
        {
          "Address": "https://localhost:5001",
          "Tags": [ "apiserver", "test", "https" ],
          "EnableTagOverride": true // Default to false
        }
      ],
      "HealthCheckDefault": {
        "DeregisterCriticalServiceAfter": "00:01:00",
        "Interval": "00:00:15",
        "Health": "/health",
        "Timeout": "00:00:10"
      },
      "Metadata": {
        //"RegistrationIdNullPolicy": "Default" // or "Consul"
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