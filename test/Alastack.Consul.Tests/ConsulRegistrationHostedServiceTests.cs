using Alastack.Consul.Registration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Alastack.Consul.Tests
{
    public class ConsulRegistrationHostedServiceTests
    {
        [Fact]
        public async Task Start_Registers_Service_Instances()
        {
            var service = new FakeRegistrationService();
            var hostedService = CreateHostedService(service);

            await hostedService.StartAsync(CancellationToken.None);
            await service.RegisterCalled.Task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

            Assert.Equal(1, service.RegisterCalls);
            Assert.Equal(0, service.DeregisterCalls);
        }

        [Fact]
        public async Task Start_Does_Not_Fault_When_Registration_Fails()
        {
            var service = new FakeRegistrationService { RegisterException = new HttpRequestException("Consul is down") };
            var hostedService = CreateHostedService(service);

            await hostedService.StartAsync(CancellationToken.None);
            await service.RegisterCalled.Task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

            // A faulted background task would make StopAsync rethrow the registration exception,
            // which in turn would stop the host when the default exception behavior is used.
            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task Start_Stops_Retrying_After_Max_Attempts()
        {
            var service = new FakeRegistrationService { RegisterException = new HttpRequestException("Consul is down") };
            var hostedService = CreateHostedService(service);

            await hostedService.StartAsync(CancellationToken.None);

            var cancellationToken = TestContext.Current.CancellationToken;
            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(1);
            while (service.RegisterCalls < ConsulRegistrationHostedService.MaxRegistrationAttempts && DateTime.UtcNow < deadline)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken);
            }

            Assert.Equal(ConsulRegistrationHostedService.MaxRegistrationAttempts, service.RegisterCalls);
        }

        [Fact]
        public async Task Start_Does_Not_Retry_Invalid_Configuration()
        {
            var service = new FakeRegistrationService
            {
                RegisterException = new OptionsValidationException(
                    "ConsulRegistrationOptions",
                    typeof(ConsulRegistrationOptions),
                    new[] { "Name is required." })
            };
            var hostedService = CreateHostedService(service);

            await hostedService.StartAsync(CancellationToken.None);
            await service.RegisterCalled.Task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

            // Give a retry a chance to happen before asserting that a permanent failure is not retried.
            await Task.Delay(TimeSpan.FromMilliseconds(100), TestContext.Current.CancellationToken);

            Assert.Equal(1, service.RegisterCalls);
        }

        [Fact]
        public async Task Start_Retries_Registration_When_It_Fails()
        {
            var service = new FakeRegistrationService { FailuresRemaining = 2 };
            var hostedService = CreateHostedService(service);

            await hostedService.StartAsync(CancellationToken.None);
            await service.RegisterSucceeded.Task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

            Assert.Equal(3, service.RegisterCalls);
            Assert.Equal(0, service.DeregisterCalls);
        }

        [Fact]
        public async Task Start_Skips_Registration_When_Already_Stopping()
        {
            var service = new FakeRegistrationService();
            var hostedService = CreateHostedService(service);
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await hostedService.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(TimeSpan.FromMilliseconds(100), TestContext.Current.CancellationToken);

            Assert.Equal(0, service.RegisterCalls);
        }

        [Fact]
        public async Task Stop_Deregisters_Even_When_Registration_Failed()
        {
            var service = new FakeRegistrationService { RegisterException = new HttpRequestException("Consul is down") };
            var hostedService = CreateHostedService(service);

            await hostedService.StartAsync(CancellationToken.None);
            await service.RegisterCalled.Task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
            await hostedService.StopAsync(CancellationToken.None);

            Assert.Equal(1, service.DeregisterCalls);
        }

        [Fact]
        public async Task Stop_Does_Not_Throw_When_Deregistration_Fails()
        {
            var service = new FakeRegistrationService { DeregisterException = new InvalidOperationException("Consul is down") };
            var hostedService = CreateHostedService(service);

            await hostedService.StartAsync(CancellationToken.None);
            await service.RegisterCalled.Task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

            // The shutdown token is usually cancelled, so deregistration must not surface the failure.
            await hostedService.StopAsync(CancellationToken.None);

            Assert.Equal(1, service.DeregisterCalls);
        }

        private static ConsulRegistrationHostedService CreateHostedService(IRegistrationService registrationService)
        {
            return new ConsulRegistrationHostedService(
                registrationService,
                NullLogger<ConsulRegistrationHostedService>.Instance,
                TimeSpan.FromMilliseconds(10));
        }

        private sealed class FakeRegistrationService : IRegistrationService
        {
            public TaskCompletionSource RegisterCalled { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

            public TaskCompletionSource RegisterSucceeded { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

            public int RegisterCalls { get; private set; }

            public int DeregisterCalls { get; private set; }

            public Exception? RegisterException { get; init; }

            public Exception? DeregisterException { get; init; }

            /// <summary>
            /// The number of initial registration attempts that fail before one succeeds.
            /// </summary>
            public int FailuresRemaining { get; set; }

            public Task RegisterAsync(CancellationToken cancellationToken)
            {
                RegisterCalls++;
                RegisterCalled.TrySetResult();

                if (RegisterException != null)
                {
                    return Task.FromException(RegisterException);
                }
                if (FailuresRemaining > 0)
                {
                    FailuresRemaining--;
                    return Task.FromException(new HttpRequestException("Consul is down"));
                }

                RegisterSucceeded.TrySetResult();
                return Task.CompletedTask;
            }

            public Task DeregisterAsync(CancellationToken cancellationToken)
            {
                DeregisterCalls++;
                return DeregisterException == null ? Task.CompletedTask : Task.FromException(DeregisterException);
            }
        }
    }
}
