using Sample.Common;

namespace WorkerServiceSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.Configure<ConfigOptions>(context.Configuration.GetSection("ConfigOptions"));
                })
                .ConfigureAppConfiguration(builder => 
                {
                    builder.AddConsulConfiguration("ConsulOptions");
                })
                .Build();

            host.Run();
        }
    }
}