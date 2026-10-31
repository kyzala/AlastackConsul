using System.Reflection;
using Alastack.Consul;
using Alastack.Consul.Registration;
using Consul;
using Microsoft.Extensions.Options;

namespace Alastack.Consul.Tests
{
    public class RegistrationServiceTests
    {
        [Fact]
        public async Task RegisterAsync_Maps_Resolved_Instance_To_AgentServiceRegistration()
        {
            var registrations = new List<AgentServiceRegistration>();
            var (client, proxy) = CreateClient(onRegister: registrations.Add);
            var options = CreateOptions(new ServiceInstance { Address = new Uri("http://localhost:5000") });
            var service = CreateService(options, client);

            await service.RegisterAsync(CancellationToken.None);

            var registration = Assert.Single(registrations);
            Assert.Equal("AspNetSample@localhost:5000", registration.ID);
            Assert.Equal("AspNetSample", registration.Name);
            Assert.Equal("localhost", registration.Address);
            Assert.Equal(5000, registration.Port);
            Assert.Null(registration.Tags);
            Assert.Null(registration.Meta);
            Assert.False(registration.EnableTagOverride);

            Assert.Equal("service:AspNetSample@localhost:5000", registration.Check.CheckID);
            Assert.Equal("Service 'AspNetSample@localhost:5000' check", registration.Check.Name);
            Assert.Equal(TimeSpan.FromSeconds(60), registration.Check.DeregisterCriticalServiceAfter);
            Assert.Equal(TimeSpan.FromSeconds(15), registration.Check.Interval);
            Assert.Equal("http://localhost:5000/health", registration.Check.HTTP);
            Assert.Equal(TimeSpan.FromSeconds(10), registration.Check.Timeout);

            Assert.True(proxy.Disposed);
        }

        [Fact]
        public async Task RegisterAsync_Maps_Configured_Instance_Values()
        {
            var registrations = new List<AgentServiceRegistration>();
            var (client, _) = CreateClient(onRegister: registrations.Add);
            var options = CreateOptions();
            options.Metadata = new Dictionary<string, string> { ["env"] = "test" };
            options.Instances.Clear();
            options.Instances.Add(new ServiceInstance
            {
                Id = "aspnetsample1",
                Address = new Uri("http://localhost:5000"),
                Tags = new[] { "apiserver", "http" },
                EnableTagOverride = true,
                HealthCheck = new HealthCheckOptions
                {
                    CheckId = "AspNetSample_HealthCheck1",
                    Name = "AspNetSample_HealthCheck",
                    DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1),
                    Interval = TimeSpan.FromSeconds(30),
                    Endpoint = new Uri("http://localhost:5000/health"),
                    Timeout = TimeSpan.FromSeconds(20)
                }
            });
            var service = CreateService(options, client);

            await service.RegisterAsync(CancellationToken.None);

            var registration = Assert.Single(registrations);
            Assert.Equal("aspnetsample1", registration.ID);
            Assert.Equal(new[] { "apiserver", "http" }, registration.Tags);
            Assert.Equal("test", registration.Meta!["env"]);
            Assert.True(registration.EnableTagOverride);
            Assert.Equal("AspNetSample_HealthCheck1", registration.Check.CheckID);
            Assert.Equal("AspNetSample_HealthCheck", registration.Check.Name);
            Assert.Equal(TimeSpan.FromMinutes(1), registration.Check.DeregisterCriticalServiceAfter);
            Assert.Equal(TimeSpan.FromSeconds(30), registration.Check.Interval);
            Assert.Equal("http://localhost:5000/health", registration.Check.HTTP);
            Assert.Equal(TimeSpan.FromSeconds(20), registration.Check.Timeout);
        }

        [Fact]
        public async Task RegisterAsync_Registers_All_Instances()
        {
            var registrations = new List<AgentServiceRegistration>();
            var (client, _) = CreateClient(onRegister: registrations.Add);
            var options = CreateOptions(
                new ServiceInstance { Address = new Uri("http://localhost:5000") },
                new ServiceInstance { Address = new Uri("http://localhost:5001") });
            var service = CreateService(options, client);

            await service.RegisterAsync(CancellationToken.None);

            Assert.Equal(2, registrations.Count);
            Assert.Equal(
                new[] { "AspNetSample@localhost:5000", "AspNetSample@localhost:5001" },
                registrations.Select(registration => registration.ID));
        }

        [Fact]
        public async Task RegisterAsync_Omits_Check_When_Health_Check_Is_Disabled()
        {
            var registrations = new List<AgentServiceRegistration>();
            var (client, _) = CreateClient(onRegister: registrations.Add);
            var options = CreateOptions(new ServiceInstance
            {
                Address = new Uri("http://localhost:5000"),
                HealthCheck = new HealthCheckOptions { Enabled = false }
            });
            var service = CreateService(options, client);

            await service.RegisterAsync(CancellationToken.None);

            Assert.Null(Assert.Single(registrations).Check);
        }

        [Fact]
        public async Task DeregisterAsync_Deregisters_All_Instance_Ids()
        {
            var deregisteredIds = new List<string>();
            var (client, proxy) = CreateClient(onDeregister: deregisteredIds.Add);
            var options = CreateOptions(
                new ServiceInstance { Address = new Uri("http://localhost:5000") },
                new ServiceInstance { Address = new Uri("http://localhost:5001") });
            var service = CreateService(options, client);

            await service.DeregisterAsync(CancellationToken.None);

            Assert.Equal(
                new[] { "AspNetSample@localhost:5000", "AspNetSample@localhost:5001" },
                deregisteredIds);
            Assert.True(proxy.Disposed);
        }

        [Fact]
        public async Task RegisterAsync_Throws_When_No_Instances_Are_Resolved()
        {
            var (client, _) = CreateClient();
            var options = CreateOptions();
            var service = CreateService(options, client);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.RegisterAsync(CancellationToken.None));

            Assert.Contains("No service instances were resolved", exception.Message);
        }

        [Fact]
        public async Task DeregisterAsync_Skips_Instances_Without_Id()
        {
            var deregisteredIds = new List<string>();
            var (client, _) = CreateClient(onDeregister: deregisteredIds.Add);
            var options = CreateOptions(new ServiceInstance { Address = new Uri("http://localhost:5000") });
            options.Metadata = new Dictionary<string, string>
            {
                [RegistrationPolicies.RegistrationIdNullPolicy] = RegistrationIdNullPolicy.Consul
            };
            var service = CreateService(options, client);

            await service.DeregisterAsync(CancellationToken.None);

            // A Consul assigned id cannot be addressed, so the request is skipped instead of failing.
            Assert.Empty(deregisteredIds);
        }

        [Fact]
        public async Task RegisterAsync_Does_Not_Publish_The_Id_Policy_As_Metadata()
        {
            var registrations = new List<AgentServiceRegistration>();
            var (client, _) = CreateClient(onRegister: registrations.Add);
            var options = CreateOptions(new ServiceInstance { Address = new Uri("http://localhost:5000") });
            options.Metadata = new Dictionary<string, string>
            {
                ["env"] = "test",
                [RegistrationPolicies.RegistrationIdNullPolicy] = RegistrationIdNullPolicy.Default
            };
            var service = CreateService(options, client);

            await service.RegisterAsync(CancellationToken.None);

            var registration = Assert.Single(registrations);
            Assert.Equal("test", registration.Meta!["env"]);
            Assert.False(registration.Meta!.ContainsKey(RegistrationPolicies.RegistrationIdNullPolicy));
        }

        private static RegistrationService CreateService(ConsulRegistrationOptions options, IConsulClient client)
        {
            var instanceStore = new ServiceInstanceStore(
                Options.Create(options),
                Array.Empty<IServerAddressesProvider>());
            return new RegistrationService(Options.Create(options), instanceStore, () => client);
        }

        private static ConsulRegistrationOptions CreateOptions(params ServiceInstance[] instances)
        {
            var options = new ConsulRegistrationOptions
            {
                Name = "AspNetSample",
                Agent = new AgentOptions { Address = new Uri("http://localhost:8500") }
            };
            foreach (var instance in instances)
            {
                options.Instances.Add(instance);
            }

            return options;
        }

        private static (IConsulClient Client, AgentStubProxy Proxy) CreateClient(
            Action<AgentServiceRegistration>? onRegister = null,
            Action<string>? onDeregister = null)
        {
            var agentType = typeof(IConsulClient)
                .GetProperty(nameof(IConsulClient.Agent))!
                .PropertyType;

            var agent = DispatchProxy.Create(agentType, typeof(AgentStubProxy));
            var agentProxy = (AgentStubProxy)agent;
            agentProxy.OnRegister = onRegister ?? (_ => { });
            agentProxy.OnDeregister = onDeregister ?? (_ => { });

            var client = DispatchProxy.Create<IConsulClient, AgentStubProxy>();
            var clientProxy = (AgentStubProxy)(object)client;
            clientProxy.Agent = agent;

            return (client, clientProxy);
        }

        /// <summary>
        /// Minimal <see cref="DispatchProxy"/> based stub so the test only has to provide the
        /// members that <see cref="RegistrationService"/> actually uses.
        /// </summary>
        public class AgentStubProxy : DispatchProxy
        {
            public object? Agent { get; set; }

            public Action<AgentServiceRegistration>? OnRegister { get; set; }

            public Action<string>? OnDeregister { get; set; }

            public bool Disposed { get; private set; }

            protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
            {
                switch (targetMethod?.Name)
                {
                    case "get_Agent":
                        return Agent;
                    case "Dispose":
                        Disposed = true;
                        return null;
                    case "ServiceRegister" when args is { Length: 2 }:
                        OnRegister!((AgentServiceRegistration)args[0]!);
                        return Task.FromResult(new WriteResult());
                    case "ServiceDeregister" when args is { Length: 2 }:
                        OnDeregister!((string)args[0]!);
                        return Task.FromResult(new WriteResult());
                    default:
                        throw new NotImplementedException(targetMethod?.Name);
                }
            }
        }
    }
}
