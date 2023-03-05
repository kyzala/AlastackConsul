# Alastack.Consul

Alastack.Consul supports Configuration and Service Discovery with Hashicorp Consul.

[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/kyzala/AlastackConsul/dotnet.yml?branch=main)](https://github.com/kyzala/AlastackConsul/actions/workflows/dotnet.yml)[![GitHub](https://img.shields.io/github/license/kyzala/AlastackConsul)](LICENSE)

## Getting started

### Install package from the .NET CLI

Client：

```
dotnet add package Alastack.Consul
```

### Hmac Authentication

The following code snippet demonstrates using Configuration and Service Discovery with Hashicorp Consul:

```csharp
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