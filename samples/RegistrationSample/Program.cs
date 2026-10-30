using RegistrationSample;

var builder = WebApplication.CreateBuilder(args);

// Add Consul Configuration
builder.Configuration.AddConsulConfiguration();
// Add Consul Registration
builder.Services.AddConsulRegistration(builder.Configuration).AddConsulAspNetCore();
builder.Services.AddControllers();

var app = builder.Build();

app.UseHealthCheck("/health");
app.UseAuthorization();
app.MapControllers();

Console.WriteLine(Environment.Version);

app.Run();
