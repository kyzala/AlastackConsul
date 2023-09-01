using Microsoft.Extensions.Configuration;
using Sample.Common;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConsoleSample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();

            builder.AddJsonFile("appsettings.json", false, true);
            builder.AddConsulConfiguration();
            var configuration = builder.Build();

            var options = configuration.GetSection("ConfigOptions").Get<ConfigOptions>();

            var jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            var str = JsonSerializer.Serialize(options, jsonSerializerOptions);

            Console.WriteLine("Output ConfigOptions:\n");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(str);

            Console.ReadLine();
        }
    }
}