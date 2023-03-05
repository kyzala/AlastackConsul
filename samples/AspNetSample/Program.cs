using AspNetSample;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("consul.json", true, true);
// Add Consul Configuration
builder.Configuration.AddConsulConfiguration();
// Add Consul 
builder.Services.AddConsul(builder.Configuration);
builder.Services.AddControllers();

var app = builder.Build();

app.UseHealthCheck("/health");
app.UseAuthorization();
app.MapControllers();

app.Run();
