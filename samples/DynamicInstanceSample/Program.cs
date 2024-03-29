using DynamicInstanceSample;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("consul.json", false, true);
// Add Consul Configuration
builder.Configuration.AddConsulConfiguration();
// Add Consul Naming
builder.Services.AddConsulNaming(builder.Configuration).AddAspNetCore();
builder.Services.AddControllers();

var app = builder.Build();

app.UseHealthCheck("/health");
app.UseAuthorization();
app.MapControllers();

Console.WriteLine(Environment.Version);

app.Run();
