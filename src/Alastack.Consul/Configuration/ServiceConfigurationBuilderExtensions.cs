using Alastack.Consul;
using Winton.Extensions.Configuration.Consul;

namespace Microsoft.Extensions.Configuration
{
    public static class ServiceConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddConsulConfiguration(this IConfigurationBuilder builder, string key = "Consul")
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddConsulConfiguration(builder.Build(), key);
            return builder;
        }

        public static IConfigurationBuilder AddConsulConfiguration(this IConfigurationBuilder builder, IConfiguration configuration, string key = "Consul")
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var consulOptions = configuration.GetSection(key).Get<ConsulOptions>();
            return builder.AddConsulConfiguration(consulOptions!);
        }

        public static IConfigurationBuilder AddConsulConfiguration(this IConfigurationBuilder builder, ConsulOptions consulOptions)
        {
            if (consulOptions == null)
            {
                throw new ArgumentNullException(nameof(ConsulOptions));
            }
            if (consulOptions.Application.Configuration?.Items != null)
            {
                var serviceConfiguration = consulOptions.Application.Configuration;
                var pathPrefix = $"{consulOptions.Application.Namespace}/{serviceConfiguration.Path}";
                foreach (var configurationItem in serviceConfiguration.Items)
                {
                    var pathKey = $"{pathPrefix}/{configurationItem.Name}";
                    builder.AddConsul(pathKey, options =>
                    {
                        options.Optional = configurationItem.Optional;
                        options.ReloadOnChange = configurationItem.ReloadOnChange;
                        options.PollWaitTime = configurationItem.PollingWaitTime ?? serviceConfiguration.PollingWaitTime;
                        options.ConsulConfigurationOptions =
                            config =>
                            {
                                config.Address = consulOptions.Agent.Address;//new Uri($"{agent.Address}:{agent.}");
                                config.Token = consulOptions.Agent.Token;
                                config.Datacenter = consulOptions.Agent.Datacenter;
                                config.WaitTime = consulOptions.Agent.WaitTime;
                            };
                        options.OnLoadException = exceptionContext => { exceptionContext.Ignore = configurationItem.IgnoreException ?? serviceConfiguration.IgnoreException; };
                    });
                }
            }

            return builder;
        }

        //private static void AddConsulConfiguration(this IConfigurationBuilder builder, ConsulOptions consulOptions, ConfigurationSetting configurationSetting, string configurationPath) 
        //{
        //    if (consulOptions.Agent == null)
        //    {
        //        throw new ArgumentNullException(nameof(consulOptions.Agent));
        //    }
        //    if (configurationSetting == null)
        //    {
        //        throw new ArgumentNullException(nameof(configurationSetting));
        //    }
        //    if (configurationPath == null)
        //    {
        //        throw new ArgumentNullException(nameof(configurationPath));
        //    }

        //    var pathKey = $"{configurationPath}/{configurationSetting.Name}";
        //    builder.AddConsul(pathKey, options =>
        //    {
        //        options.Optional = configurationSetting.Optional;
        //        options.ReloadOnChange = configurationSetting.ReloadOnChange;
        //        options.PollWaitTime = ;
        //        options.ConsulConfigurationOptions =
        //            config =>
        //            {
        //                config.Address = agent.Address;//new Uri($"{agent.Address}:{agent.}");
        //                config.Token = agent.Token;
        //                config.Datacenter = agent.Datacenter;
        //                config.WaitTime = agent.WaitTime;
        //            };
        //        options.OnLoadException = exceptionContext => { exceptionContext.Ignore = configurationSetting.IgnoreException; };
        //    });
        //}
    }
}
