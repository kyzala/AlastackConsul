using Alastack.Consul.Registration;
using Microsoft.Extensions.Options;

namespace Alastack.Consul.Tests
{
    public class ServiceInstanceStoreTests
    {
        [Fact]
        public async Task Resolves_Configured_Instances()
        {
            var store = CreateStore(CreateOptions(new ServiceInstance { Address = new Uri("http://localhost:5000") }));

            var instance = Assert.Single(await store.GetInstancesAsync(CancellationToken.None));

            Assert.Equal("AspNetSample@localhost:5000", instance.Id);
            Assert.Equal(new Uri("http://localhost:5000"), instance.Address);
            Assert.Equal("service:AspNetSample@localhost:5000", instance.HealthCheck.CheckId);
            Assert.Equal("http://localhost:5000/health", instance.HealthCheck.Endpoint.ToString());
        }

        [Fact]
        public async Task Merges_Provider_Addresses_Not_Already_Configured()
        {
            var store = CreateStore(
                CreateOptions(new ServiceInstance { Address = new Uri("http://localhost:5000") }),
                "http://localhost:5001");

            var instances = await store.GetInstancesAsync(CancellationToken.None);

            Assert.Equal(2, instances.Count);
            var merged = instances.Single(instance => instance.Address == new Uri("http://localhost:5001"));
            Assert.Equal("AspNetSample@localhost:5001", merged.Id);
            Assert.Equal("service:AspNetSample@localhost:5001", merged.HealthCheck.CheckId);
        }

        [Fact]
        public async Task Skips_Provider_Addresses_Already_Configured()
        {
            var store = CreateStore(
                CreateOptions(new ServiceInstance { Address = new Uri("http://localhost:5000") }),
                "http://localhost:5000");

            Assert.Single(await store.GetInstancesAsync(CancellationToken.None));
        }

        [Fact]
        public async Task Memoizes_Resolved_Instances()
        {
            var store = CreateStore(
                CreateOptions(new ServiceInstance { Address = new Uri("http://localhost:5000") }),
                "http://localhost:5001");

            var first = await store.GetInstancesAsync(CancellationToken.None);
            var second = await store.GetInstancesAsync(CancellationToken.None);

            Assert.Same(first, second);
        }

        [Fact]
        public async Task Keeps_Null_Id_When_Consul_RegistrationId_Policy_Is_Used()
        {
            var options = CreateOptions(new ServiceInstance { Address = new Uri("http://localhost:5000") });
            options.Metadata = new Dictionary<string, string>
            {
                [RegistrationPolicies.RegistrationIdNullPolicy] = RegistrationIdNullPolicy.Consul
            };
            var store = new ServiceInstanceStore(Options.Create(options), Array.Empty<IServerAddressesProvider>());

            var instance = Assert.Single(await store.GetInstancesAsync(CancellationToken.None));

            Assert.Null(instance.Id);
        }

        private static ConsulRegistrationOptions CreateOptions(params ServiceInstance[] instances)
        {
            var options = new ConsulRegistrationOptions { Name = "AspNetSample" };
            foreach (var instance in instances)
            {
                options.Instances.Add(instance);
            }

            return options;
        }

        private static ServiceInstanceStore CreateStore(ConsulRegistrationOptions options, params string[] addresses)
        {
            var providers = addresses
                .Select(address => (IServerAddressesProvider)new FakeAddressesProvider(address))
                .ToArray();
            return new ServiceInstanceStore(Options.Create(options), providers);
        }

        private sealed class FakeAddressesProvider(string address) : IServerAddressesProvider
        {
            public Task<IReadOnlyCollection<string>> GetServerAddressesAsync(CancellationToken cancellationToken)
                => Task.FromResult<IReadOnlyCollection<string>>(new[] { address });
        }
    }
}
