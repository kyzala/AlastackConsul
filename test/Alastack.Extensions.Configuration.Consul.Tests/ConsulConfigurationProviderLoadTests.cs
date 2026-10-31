using System.Net;
using System.Reflection;
using System.Text;
using Alastack.Extensions.Configuration.Consul;
using Consul;
using Xunit;

namespace Alastack.Extensions.Configuration.Consul.Tests
{
    public class ConsulConfigurationProviderLoadTests
    {
        private const string Key = "apps/sample.json";

        [Fact]
        public void Load_Applies_Value()
        {
            var provider = CreateProvider((_, _) => Result("""{"Section":{"Name":"value"}}"""));

            provider.Load();

            Assert.True(provider.TryGet("Section:Name", out var value));
            Assert.Equal("value", value);
        }

        [Fact]
        public void Load_Uses_Explicit_Format_Over_Extension()
        {
            var provider = CreateProvider(
                (_, _) => Result("""{"Section":{"Name":"value"}}"""),
                source =>
                {
                    source.Path = "config.txt";
                    source.Format = ConsulConfigurationFormat.Json;
                });

            provider.Load();

            Assert.True(provider.TryGet("Section:Name", out var value));
            Assert.Equal("value", value);
        }

        [Fact]
        public void Load_Throws_When_Key_Is_Missing()
        {
            var provider = CreateProvider((_, _) => NotFound());

            var exception = Assert.Throws<InvalidOperationException>(provider.Load);

            Assert.Contains(Key, exception.Message);
        }

        [Fact]
        public void Load_Leaves_Data_Empty_When_Key_Is_Missing_And_Optional()
        {
            var provider = CreateProvider((_, _) => NotFound(), source => source.Optional = true);

            provider.Load();

            Assert.False(provider.TryGet("Section:Name", out _));
        }

        [Fact]
        public void Load_Ignores_Exception_When_Configured()
        {
            var provider = CreateProvider(
                (_, _) => throw new InvalidOperationException("boom"),
                source => source.OnLoadException = context => context.Ignore = true);

            provider.Load();

            Assert.False(provider.TryGet("Section:Name", out _));
        }

        [Fact]
        public void Load_Rethrows_Exception_When_Not_Ignored()
        {
            var provider = CreateProvider((_, _) => throw new InvalidOperationException("boom"));

            Assert.Throws<InvalidOperationException>(provider.Load);
        }

        [Fact]
        public async Task Watch_Reloads_Value_And_Notifies_Change_Token()
        {
            var calls = 0;
            var watchStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var parked = new TaskCompletionSource<QueryResult<KVPair>>();
            QueryOptions? watchOptions = null;

            var source = new ConsulConfigurationSource
            {
                Path = Key,
                ReloadOnChange = true,
                PollingWaitTime = TimeSpan.FromSeconds(1)
            };
            source.ClientFactory = () => CreateClient(async (key, options) =>
            {
                switch (Interlocked.Increment(ref calls))
                {
                    case 1:
                        return Result(key, """{"Section":{"Name":"first"}}""");
                    case 2:
                        watchOptions = options;
                        watchStarted.TrySetResult();
                        await release.Task;
                        return Result(key, """{"Section":{"Name":"second"}}""", lastIndex: 2);
                    default:
                        return await parked.Task;
                }
            });

            var provider = new ConsulConfigurationProvider(source, ConsulConfigurationFormat.Json);
            var reloaded = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using var registration = provider.GetReloadToken()
                .RegisterChangeCallback(_ => reloaded.TrySetResult(), null);

            provider.Load();
            await watchStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            release.TrySetResult();
            await reloaded.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            Assert.True(provider.TryGet("Section:Name", out var value));
            Assert.Equal("second", value);
            Assert.Equal(1UL, watchOptions!.WaitIndex);
            Assert.Equal(TimeSpan.FromSeconds(1), watchOptions.WaitTime);
        }

        [Fact]
        public async Task Watch_Does_Not_Start_When_ReloadOnChange_Is_False()
        {
            var calls = 0;
            var source = new ConsulConfigurationSource { Path = Key };
            source.ClientFactory = () => CreateClient((key, _) =>
            {
                Interlocked.Increment(ref calls);
                return Task.FromResult(Result(key, """{"Section":{"Name":"value"}}"""));
            });

            var provider = new ConsulConfigurationProvider(source, ConsulConfigurationFormat.Json);

            provider.Load();
            await Task.Delay(200, TestContext.Current.CancellationToken);

            Assert.Equal(1, calls);
        }

        private static ConsulConfigurationProvider CreateProvider(
            Func<string, QueryOptions, Task<QueryResult<KVPair>>> get,
            Action<ConsulConfigurationSource>? configure = null)
        {
            var source = new ConsulConfigurationSource { Path = Key };
            configure?.Invoke(source);
            source.ClientFactory = () => CreateClient(get);

            var format = source.Format == ConsulConfigurationFormat.Auto
                ? ConsulConfigurationSource.ResolveFormat(source.Path)
                : source.Format;

            return new ConsulConfigurationProvider(source, format);
        }

        private static QueryResult<KVPair> Result(string key, string json, ulong lastIndex = 1)
            => new()
            {
                StatusCode = HttpStatusCode.OK,
                LastIndex = lastIndex,
                Response = new KVPair(key) { Value = Encoding.UTF8.GetBytes(json) }
            };

        private static Task<QueryResult<KVPair>> Result(string json)
            => Task.FromResult(Result(Key, json));

        private static Task<QueryResult<KVPair>> NotFound()
            => Task.FromResult(new QueryResult<KVPair> { StatusCode = HttpStatusCode.NotFound });

        private static IConsulClient CreateClient(Func<string, QueryOptions, Task<QueryResult<KVPair>>> get)
        {
            var kvEndpointType = typeof(IConsulClient)
                .GetProperty(nameof(IConsulClient.KV))!
                .PropertyType;

            var kv = DispatchProxy.Create(kvEndpointType, typeof(StubProxy));
            var kvProxy = (StubProxy)kv;
            kvProxy.Get = get;

            var client = DispatchProxy.Create<IConsulClient, StubProxy>();
            ((StubProxy)(object)client).KV = kv;

            return client;
        }

        /// <summary>
        /// Minimal <see cref="DispatchProxy"/> based stub so the test only has to provide the
        /// members that <see cref="ConsulConfigurationProvider"/> actually uses.
        /// </summary>
        public class StubProxy : DispatchProxy
        {
            public object? KV { get; set; }

            public Func<string, QueryOptions, Task<QueryResult<KVPair>>>? Get { get; set; }

            protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
            {
                switch (targetMethod?.Name)
                {
                    case "get_KV":
                        return KV;
                    case "Dispose":
                        return null;
                    case "Get" when args is { Length: 3 }:
                        return Get!((string)args[0]!, (QueryOptions)args[1]!);
                    default:
                        throw new NotImplementedException(targetMethod?.Name);
                }
            }
        }
    }
}
