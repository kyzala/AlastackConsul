namespace AspNetSample;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseHealthCheck(this IApplicationBuilder app, PathString path)
    {
        app.Map(path, builder => builder.Use(
            (HttpContext context, RequestDelegate next) =>
            {
                context.Response.StatusCode = 200;
                return context.Response.WriteAsync("healthy");
            }));
        return app;
    }

    public static IApplicationBuilder UseHealthCheck(this IApplicationBuilder app, PathString path, Func<IServiceProvider, bool> healthCheckFunc)
    {
        if (healthCheckFunc == null)
        {
            healthCheckFunc = serviceProvider => true;
        }
        return UseHealthCheck(app, path, serviceProvider => Task.FromResult(healthCheckFunc(serviceProvider)));
    }

    public static IApplicationBuilder UseHealthCheck(this IApplicationBuilder app, PathString path, Func<IServiceProvider, Task<bool>> healthCheckFunc)
    {
        if (healthCheckFunc == null)
        {
            healthCheckFunc = serviceProvider => Task.FromResult(true);
        }
        app.Map(path, builder => builder.Use(
            async (HttpContext context, RequestDelegate next) =>
            {
                var healthCheckResult = "unhealthy";
                try
                {
                    var isHealthy = await healthCheckFunc.Invoke(context.RequestServices);
                    if (isHealthy)
                    {
                        context.Response.StatusCode = StatusCodes.Status200OK;
                        healthCheckResult = "healthy";
                    }
                    else
                    {
                        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                    }
                }
                catch (Exception ex)
                {
                    var logger = context.RequestServices.GetService<ILoggerFactory>()!.CreateLogger("HealthCheck");
                    logger.LogError(ex, "Executing HealthCheck Error.");
                    context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                }
                finally
                {
                    await context.Response.WriteAsync(healthCheckResult);
                }
            }));
        return app;
    }
}
