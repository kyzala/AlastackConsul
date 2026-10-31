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

        [Fact]
        public async Task Uses_Address_Based_Check_Id_When_Id_Is_Left_To_Consul()
        {
            var options = CreateOptions(new ServiceInstance { Address = new Uri("http://localhost:5000") });
            options.Metadata = new Dictionary<string, string>
            {
                [RegistrationPolicies.RegistrationIdNullPolicy] = RegistrationIdNullPolicy.Consul
            };
            var store = new ServiceInstanceStore(Options.Create(options), Array.Empty<IServerAddressesProvider>());

            var instance = Assert.Single(await store.GetInstancesAsync(CancellationToken.None));

            Assert.Null(instance.Id);
            Assert.Equal("service:localhost:5000", instance.HealthCheck.CheckId);
            Assert.Equal("service:localhost:5000", instance.HealthCheck.Name);
        }

        [Fact]
        public async Task Throws_When_A_Provider_Address_Is_Not_Absolute()
        {
            var store = CreateStore(CreateOptions(), "http://");

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => store.GetInstancesAsync(CancellationToken.None));

            Assert.Contains("not a valid absolute URI", exception.Message);
        }

        [Fact]
        public async Task Throws_When_Instance_Id_Is_Duplicated()
        {
            var store = CreateStore(CreateOptions(
                new ServiceInstance { Id = "duplicate", Address = new Uri("http://localhost:5000") },
                new ServiceInstance { Id = "duplicate", Address = new Uri("http://localhost:5001") }));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => store.GetInstancesAsync(CancellationToken.None));

            Assert.Contains("duplicate", exception.Message);
        }

        [Fact]
        public async Task Does_Not_Add_Provider_Addresses_When_Disabled()
        {
            var options = CreateOptions(new ServiceInstance { Address = new Uri("http://localhost:5000") });
            options.IncludeServerAddresses = false;
            var store = CreateStore(options, "http://localhost:5001");

            var instance = Assert.Single(await store.GetInstancesAsync(CancellationToken.None));

            Assert.Equal(new Uri("http://localhost:5000"), instance.Address);
        }

        [Fact]
        public async Task Skips_Provider_Address_With_The_Same_Host_And_Port()
        {
            var store = CreateStore(
                CreateOptions(new ServiceInstance { Address = new Uri("http://localhost:5000") }),
                "https://localhost:5000");

            // Both addresses would generate the same instance id, so only the configured one is kept.
            Assert.Single(await store.GetInstancesAsync(CancellationToken.None));
        }

        [Fact]
        public async Task Skips_Wildcard_Provider_Addresses()
        {
            var store = CreateStore(CreateOptions(), "http://0.0.0.0:5000", "http://localhost:5000");

            var instance = Assert.Single(await store.GetInstancesAsync(CancellationToken.None));

            Assert.Equal("localhost", instance.Address.Host);
            Assert.Equal(5000, instance.Address.Port);
        }

        [Fact]
        public async Task Throws_When_Only_Wildcard_Provider_Addresses_Are_Available()
        {
            var store = CreateStore(CreateOptions(), "http://0.0.0.0:5000");

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => store.GetInstancesAsync(CancellationToken.None));

            Assert.Contains("wildcard", exception.Message);
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
