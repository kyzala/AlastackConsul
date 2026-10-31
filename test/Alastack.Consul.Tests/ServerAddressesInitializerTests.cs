using Alastack.Consul.AspNetCore;
using Alastack.Consul.Registration;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;

namespace Alastack.Consul.Tests
{
    public class ServerAddressesInitializerTests
    {
        [Fact]
        public async Task Publishes_Server_Addresses_Only_After_The_Host_Has_Started()
        {
            var provider = new DefaultServerAddressesProvider();
            var lifetime = new FakeApplicationLifetime();
            var addresses = new FakeServerAddressesFeature();
            addresses.Addresses.Add("http://localhost:5000");
            addresses.Addresses.Add("http://0.0.0.0:5001");

            var server = new FakeServer();
            server.Features.Set<IServerAddressesFeature>(addresses);

            var initializer = new ServerAddressesInitializer(server, provider, lifetime);
            await initializer.StartAsync(TestContext.Current.CancellationToken);

            // The server only fills its addresses while it starts, so nothing is published yet.
            Assert.False(provider.Configured);

            lifetime.NotifyStarted();

            Assert.True(provider.Configured);
            var resolved = await provider.GetServerAddressesAsync(TestContext.Current.CancellationToken);
            Assert.Equal(new[] { "http://localhost:5000", "http://0.0.0.0:5001" }, resolved);
        }

        [Fact]
        public async Task Publishes_No_Addresses_When_The_Server_Does_Not_Expose_Them()
        {
            var provider = new DefaultServerAddressesProvider();
            var lifetime = new FakeApplicationLifetime();
            var server = new FakeServer();

            var initializer = new ServerAddressesInitializer(server, provider, lifetime);
            await initializer.StartAsync(TestContext.Current.CancellationToken);
            lifetime.NotifyStarted();

            Assert.True(provider.Configured);
            Assert.Empty(await provider.GetServerAddressesAsync(TestContext.Current.CancellationToken));
        }

        private sealed class FakeApplicationLifetime : IHostApplicationLifetime
        {
            private readonly CancellationTokenSource _started = new();
            private readonly CancellationTokenSource _stopping = new();
            private readonly CancellationTokenSource _stopped = new();

            public CancellationToken ApplicationStarted => _started.Token;

            public CancellationToken ApplicationStopping => _stopping.Token;

            public CancellationToken ApplicationStopped => _stopped.Token;

            public void StopApplication() => _stopping.Cancel();

            public void NotifyStarted() => _started.Cancel();
        }

        private sealed class FakeServer : IServer
        {
            public IFeatureCollection Features { get; } = new FeatureCollection();

            public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
                where TContext : notnull
                => Task.CompletedTask;

            public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

            public void Dispose()
            {
            }
        }

        private sealed class FakeServerAddressesFeature : IServerAddressesFeature
        {
            public ICollection<string> Addresses { get; } = new List<string>();

            public bool PreferHostingUrls { get; set; }
        }
    }
}
