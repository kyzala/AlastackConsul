using AspNetSample;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("consul.json", true, true);
builder.Configuration.AddConsulConfiguration();

builder.Services.AddConsul(builder.Configuration);

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

//app.UseHttpsRedirection();

app.UseHealthCheck("/health");

app.UseAuthorization();

app.MapControllers();

app.Run();
