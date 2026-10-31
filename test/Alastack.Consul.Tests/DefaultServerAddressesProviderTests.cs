using Alastack.Consul.Registration;

namespace Alastack.Consul.Tests
{
    public class DefaultServerAddressesProviderTests
    {
        [Fact]
        public async Task Returns_Configured_Addresses()
        {
            var provider = new DefaultServerAddressesProvider();
            provider.AddServerAddress("http://127.0.0.1:5000");
            provider.AddServerAddress("https://127.0.0.1:5001");
            provider.Configured = true;

            var addresses = await provider.GetServerAddressesAsync(CancellationToken.None);

            Assert.Equal(new[] { "http://127.0.0.1:5000", "https://127.0.0.1:5001" }, addresses);
        }

        [Fact]
        public async Task Returns_Empty_When_Configured_Without_Addresses()
        {
            var provider = new DefaultServerAddressesProvider { Configured = true };

            Assert.Empty(await provider.GetServerAddressesAsync(CancellationToken.None));
        }

        [Fact]
        public async Task Waits_Until_Addresses_Are_Configured()
        {
            var provider = new DefaultServerAddressesProvider();
            _ = Task.Run(() =>
            {
                Thread.Sleep(100);
                provider.AddServerAddress("http://127.0.0.1:5000");
                provider.Configured = true;
            }, TestContext.Current.CancellationToken);

            var addresses = await provider.GetServerAddressesAsync(CancellationToken.None);

            Assert.Equal(new[] { "http://127.0.0.1:5000" }, addresses);
        }

        [Fact]
        public async Task Throws_When_Cancelled_Before_Configured()
        {
            var provider = new DefaultServerAddressesProvider();
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(
                () => provider.GetServerAddressesAsync(cancellationTokenSource.Token));
        }
    }
}
