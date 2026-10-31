using Alastack.Extensions.Configuration.Consul;
using Consul;
using Xunit;

namespace Alastack.Extensions.Configuration.Consul.Tests
{
    public class ConsulClientFactoryTests
    {
        [Fact]
        public void GetClient_Returns_Same_Instance_For_Equal_Settings()
        {
            using var factory = new ConsulClientFactory();
            var settings = new ConsulClientSettings { Address = new Uri("http://localhost:8500") };

            var first = factory.GetClient(settings);
            var second = factory.GetClient(settings with { });

            Assert.Same(first, second);
        }

        [Fact]
        public void GetClient_Returns_Different_Instances_For_Different_Token()
        {
            using var factory = new ConsulClientFactory();

            var first = factory.GetClient(new ConsulClientSettings { Token = "token-a" });
            var second = factory.GetClient(new ConsulClientSettings { Token = "token-b" });

            Assert.NotSame(first, second);
        }

        [Fact]
        public void GetClient_Returns_Different_Instances_For_Different_Address()
        {
            using var factory = new ConsulClientFactory();

            var first = factory.GetClient(new ConsulClientSettings { Address = new Uri("http://localhost:8500") });
            var second = factory.GetClient(new ConsulClientSettings { Address = new Uri("http://localhost:8501") });

            Assert.NotSame(first, second);
        }

        [Fact]
        public void GetClient_Creates_One_Instance_When_Called_Concurrently()
        {
            using var factory = new ConsulClientFactory();
            var settings = new ConsulClientSettings();

            var clients = Enumerable.Range(0, 32)
                .AsParallel()
                .WithDegreeOfParallelism(8)
                .Select(_ => factory.GetClient(settings))
                .ToArray();

            Assert.All(clients, client => Assert.Same(clients[0], client));
        }

        [Fact]
        public void GetClient_Throws_After_Dispose()
        {
            var factory = new ConsulClientFactory();

            factory.GetClient(new ConsulClientSettings());
            factory.Dispose();

            Assert.Throws<ObjectDisposedException>(() => factory.GetClient(new ConsulClientSettings()));
        }

        [Fact]
        public void Dispose_Is_Idempotent()
        {
            var factory = new ConsulClientFactory();

            factory.GetClient(new ConsulClientSettings());
            factory.Dispose();
            factory.Dispose();
        }

        [Fact]
        public void ToString_Does_Not_Expose_The_Token()
        {
            var settings = new ConsulClientSettings { Token = "secret-token" };

            Assert.DoesNotContain("secret-token", settings.ToString());
        }

        [Fact]
        public void Default_Is_A_Shared_Factory()
        {
            var client = ConsulClientFactory.Default.GetClient(new ConsulClientSettings());
            var other = ConsulClientFactory.Default.GetClient(new ConsulClientSettings());

            Assert.Same(client, other);
        }
    }
}
