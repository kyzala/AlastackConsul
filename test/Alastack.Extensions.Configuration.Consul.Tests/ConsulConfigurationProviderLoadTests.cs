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
            var provider = CreateProvider((_, _, _) => Result("""{"Section":{"Name":"value"}}"""));

            provider.Load();

            Assert.True(provider.TryGet("Section:Name", out var value));
            Assert.Equal("value", value);
        }

        [Fact]
        public void Load_Uses_Explicit_Format_Over_Extension()
        {
            var provider = CreateProvider(
                (_, _, _) => Result("""{"Section":{"Name":"value"}}"""),
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
            var provider = CreateProvider((_, _, _) => NotFound());

            var exception = Assert.Throws<InvalidOperationException>(provider.Load);

            Assert.Contains(Key, exception.Message);
        }

        [Fact]
        public void Load_Leaves_Data_Empty_When_Key_Is_Missing_And_Optional()
        {
            var provider = CreateProvider((_, _, _) => NotFound(), source => source.Optional = true);

            provider.Load();

            Assert.False(provider.TryGet("Section:Name", out _));
        }

        [Fact]
        public void Load_Ignores_Exception_When_Configured()
        {
            var provider = CreateProvider(
                (_, _, _) => throw new InvalidOperationException("boom"),
                source => source.OnLoadException = context => context.Ignore = true);

            provider.Load();

            Assert.False(provider.TryGet("Section:Name", out _));
        }

        [Fact]
        public void Load_Rethrows_Exception_When_Not_Ignored()
        {
            var provider = CreateProvider((_, _, _) => throw new InvalidOperationException("boom"));

            Assert.Throws<InvalidOperationException>(provider.Load);
        }

        [Fact]
        public void Load_Throws_When_Consul_Returns_A_Server_Error()
        {
            var provider = CreateProvider((_, _, _) =>
                Task.FromResult(new QueryResult<KVPair> { StatusCode = HttpStatusCode.InternalServerError }));

            var exception = Assert.Throws<HttpRequestException>(provider.Load);

            Assert.Contains("500", exception.Message);
            Assert.Contains(Key, exception.Message);
        }

        [Fact]
        public void Load_After_Dispose_Does_Not_Query_Again()
        {
            var calls = 0;
            var provider = CreateProvider((key, _, _) =>
            {
                Interlocked.Increment(ref calls);
                return Task.FromResult(Result(key, """{"Section":{"Name":"value"}}"""));
            });

            provider.Load();
            provider.Dispose();
            provider.Load();

            Assert.Equal(1, calls);
        }

        [Fact]
        public async Task Watch_Reloads_Value_And_Notifies_Change_Token()
        {
            var calls = 0;
            var watchStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            QueryOptions? watchOptions = null;

            var source = new ConsulConfigurationSource
            {
                Path = Key,
                ReloadOnChange = true,
                PollingWaitTime = TimeSpan.FromSeconds(1)
            };
            source.ClientFactory = () => CreateClient(async (key, options, cancellationToken) =>
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
                        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                        return Result(key, "{}");
                }
            }, out _);

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

            provider.Dispose();
        }

        [Fact]
        public async Task Watch_Does_Not_Start_When_ReloadOnChange_Is_False()
        {
            var calls = 0;
            var source = new ConsulConfigurationSource { Path = Key };
            source.ClientFactory = () => CreateClient((key, _, _) =>
            {
                Interlocked.Increment(ref calls);
                return Task.FromResult(Result(key, """{"Section":{"Name":"value"}}"""));
            }, out _);

            var provider = new ConsulConfigurationProvider(source, ConsulConfigurationFormat.Json);

            provider.Load();
            await Task.Delay(200, TestContext.Current.CancellationToken);

            Assert.Equal(1, calls);
        }

        [Fact]
        public async Task Watch_Picks_Up_Optional_Key_Created_After_Initial_Miss()
        {
            var calls = 0;
            var watchStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            var source = new ConsulConfigurationSource
            {
                Path = Key,
                Optional = true,
                ReloadOnChange = true,
                PollingWaitTime = TimeSpan.FromSeconds(1)
            };
            source.ClientFactory = () => CreateClient(async (key, _, cancellationToken) =>
            {
                switch (Interlocked.Increment(ref calls))
                {
                    case 1:
                        return await NotFound();
                    case 2:
                        watchStarted.TrySetResult();
                        await release.Task;
                        return Result(key, """{"Section":{"Name":"late"}}""");
                    default:
                        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                        return Result(key, "{}");
                }
            }, out _);

            var provider = new ConsulConfigurationProvider(source, ConsulConfigurationFormat.Json);
            var reloaded = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using var registration = provider.GetReloadToken()
                .RegisterChangeCallback(_ => reloaded.TrySetResult(), null);

            provider.Load();
            Assert.False(provider.TryGet("Section:Name", out _));

            await watchStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            release.TrySetResult();
            await reloaded.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            Assert.True(provider.TryGet("Section:Name", out var value));
            Assert.Equal("late", value);

            provider.Dispose();
        }

        [Fact]
        public async Task Watch_Skips_Refresh_When_Index_Is_Unchanged()
        {
            var calls = 0;
            var watchPolled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            var source = new ConsulConfigurationSource
            {
                Path = Key,
                ReloadOnChange = true,
                PollingWaitTime = TimeSpan.FromSeconds(1)
            };
            source.ClientFactory = () => CreateClient(async (key, _, cancellationToken) =>
            {
                switch (Interlocked.Increment(ref calls))
                {
                    case 1:
                        return Result(key, """{"Section":{"Name":"first"}}""");
                    case 2:
                        // Simulates a blocking query that timed out with no change:
                        // same data, same LastIndex (Result defaults to lastIndex 1).
                        return Result(key, """{"Section":{"Name":"first"}}""");
                    default:
                        watchPolled.TrySetResult();
                        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                        return Result(key, "{}");
                }
            }, out _);

            var provider = new ConsulConfigurationProvider(source, ConsulConfigurationFormat.Json);
            var reloaded = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using var registration = provider.GetReloadToken()
                .RegisterChangeCallback(_ => reloaded.TrySetResult(), null);

            provider.Load();

            // Reaching call 3 proves the loop continued; the unchanged response must not reload.
            await watchPolled.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            Assert.False(reloaded.Task.IsCompleted);
            Assert.True(provider.TryGet("Section:Name", out var value));
            Assert.Equal("first", value);

            provider.Dispose();
        }

        [Fact]
        public async Task Watch_Reports_Server_Errors_Instead_Of_Spinning()
        {
            var calls = 0;
            var exceptions = 0;

            var source = new ConsulConfigurationSource
            {
                Path = Key,
                ReloadOnChange = true,
                PollingWaitTime = TimeSpan.FromMilliseconds(300)
            };
            source.OnLoadException = context =>
            {
                Interlocked.Increment(ref exceptions);
                context.Ignore = true;
            };
            source.ClientFactory = () => CreateClient((key, _, _) =>
                Interlocked.Increment(ref calls) == 1
                    ? Task.FromResult(Result(key, """{"Section":{"Name":"value"}}"""))
                    : Task.FromResult(new QueryResult<KVPair> { StatusCode = HttpStatusCode.InternalServerError }),
                out _);

            var provider = new ConsulConfigurationProvider(source, ConsulConfigurationFormat.Json);
            provider.Load();

            await Task.Delay(TimeSpan.FromMilliseconds(150), TestContext.Current.CancellationToken);

            // The 500 must be routed to OnLoadException once, then wait before retrying.
            Assert.Equal(2, calls);
            Assert.Equal(1, exceptions);
            Assert.True(provider.TryGet("Section:Name", out var value));
            Assert.Equal("value", value);

            provider.Dispose();
        }

        [Fact]
        public async Task Watch_Waits_When_Key_Is_Deleted()
        {
            var calls = 0;
            var deleted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            var source = new ConsulConfigurationSource
            {
                Path = Key,
                ReloadOnChange = true,
                PollingWaitTime = TimeSpan.FromMilliseconds(500)
            };
            source.ClientFactory = () => CreateClient((key, _, _) =>
            {
                if (Interlocked.Increment(ref calls) == 1)
                {
                    return Task.FromResult(Result(key, """{"Section":{"Name":"value"}}"""));
                }

                deleted.TrySetResult();
                return NotFound();
            }, out _);

            var provider = new ConsulConfigurationProvider(source, ConsulConfigurationFormat.Json);
            provider.Load();
            await deleted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            // Consul answers a 404 blocking query immediately; without the delay the loop would spin.
            await Task.Delay(TimeSpan.FromMilliseconds(200), TestContext.Current.CancellationToken);
            Assert.Equal(2, calls);

            Assert.True(provider.TryGet("Section:Name", out var value));
            Assert.Equal("value", value);

            provider.Dispose();
        }

        [Fact]
        public async Task Load_Starts_Watch_When_Initial_Failure_Is_Ignored()
        {
            var calls = 0;
            var source = new ConsulConfigurationSource
            {
                Path = Key,
                ReloadOnChange = true,
                PollingWaitTime = TimeSpan.FromMilliseconds(200)
            };
            source.OnLoadException = context => context.Ignore = true;
            source.ClientFactory = () => CreateClient(async (key, _, cancellationToken) =>
            {
                switch (Interlocked.Increment(ref calls))
                {
                    case 1:
                        throw new InvalidOperationException("boom");
                    case 2:
                        return Result(key, """{"Section":{"Name":"recovered"}}""");
                    default:
                        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                        return Result(key, "{}");
                }
            }, out _);

            var provider = new ConsulConfigurationProvider(source, ConsulConfigurationFormat.Json);
            var reloaded = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using var registration = provider.GetReloadToken()
                .RegisterChangeCallback(_ => reloaded.TrySetResult(), null);

            // The initial load throws, is ignored, and the provider must still start watching.
            provider.Load();

            await reloaded.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            Assert.True(provider.TryGet("Section:Name", out var value));
            Assert.Equal("recovered", value);

            provider.Dispose();
        }

        [Fact]
        public async Task Watch_Waits_PollingWaitTime_Before_Retrying_An_Ignored_Exception()
        {
            var calls = 0;
            var exceptions = 0;
            var source = new ConsulConfigurationSource
            {
                Path = Key,
                ReloadOnChange = true,
                PollingWaitTime = TimeSpan.FromMilliseconds(500)
            };
            source.OnLoadException = context =>
            {
                Interlocked.Increment(ref exceptions);
                context.Ignore = true;
            };
            source.ClientFactory = () => CreateClient((key, _, _) =>
            {
                if (Interlocked.Increment(ref calls) == 1)
                {
                    return Task.FromResult(Result(key, """{"Section":{"Name":"value"}}"""));
                }
                throw new InvalidOperationException("boom");
            }, out _);

            var provider = new ConsulConfigurationProvider(source, ConsulConfigurationFormat.Json);
            provider.Load();

            await Task.Delay(TimeSpan.FromMilliseconds(200), TestContext.Current.CancellationToken);

            Assert.Equal(2, calls);
            Assert.Equal(1, exceptions);

            provider.Dispose();
        }

        [Fact]
        public async Task Dispose_Stops_Watch_And_Keeps_The_Provided_Client()
        {
            var calls = 0;
            var watchStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var source = new ConsulConfigurationSource
            {
                Path = Key,
                ReloadOnChange = true,
                PollingWaitTime = TimeSpan.FromMilliseconds(200)
            };
            StubProxy clientProxy = null!;
            source.ClientFactory = () => CreateClient(async (key, _, cancellationToken) =>
            {
                if (Interlocked.Increment(ref calls) == 1)
                {
                    return Result(key, """{"Section":{"Name":"value"}}""");
                }
                watchStarted.TrySetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                return Result(key, "{}");
            }, out clientProxy);

            var provider = new ConsulConfigurationProvider(source, ConsulConfigurationFormat.Json);
            provider.Load();
            await watchStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            provider.Dispose();
            await Task.Delay(TimeSpan.FromMilliseconds(300), TestContext.Current.CancellationToken);

            Assert.Equal(2, calls);
            Assert.False(clientProxy.Disposed);
        }

        [Fact]
        public async Task Watch_Continues_When_Exception_Is_Not_Ignored()
        {
            var calls = 0;
            var watchRetried = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            var source = new ConsulConfigurationSource
            {
                Path = Key,
                ReloadOnChange = true,
                PollingWaitTime = TimeSpan.FromMilliseconds(200)
            };
            source.ClientFactory = () => CreateClient(async (key, _, cancellationToken) =>
            {
                switch (Interlocked.Increment(ref calls))
                {
                    case 1:
                        return Result(key, """{"Section":{"Name":"first"}}""");
                    case 2:
                        throw new InvalidOperationException("boom");
                    default:
                        watchRetried.TrySetResult();
                        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                        return Result(key, "{}");
                }
            }, out _);

            var provider = new ConsulConfigurationProvider(source, ConsulConfigurationFormat.Json);
            provider.Load();

            // Without an OnLoadException handler a failure must still not stop the watch loop.
            await watchRetried.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            Assert.Equal(3, calls);
            Assert.True(provider.TryGet("Section:Name", out var value));
            Assert.Equal("first", value);

            provider.Dispose();
        }

        [Fact]
        public async Task Watch_Clears_Data_When_Optional_Key_Is_Deleted()
        {
            var calls = 0;
            var source = new ConsulConfigurationSource
            {
                Path = Key,
                Optional = true,
                ReloadOnChange = true,
                PollingWaitTime = TimeSpan.FromMilliseconds(200)
            };
            source.ClientFactory = () => CreateClient(async (key, _, _) =>
                Interlocked.Increment(ref calls) == 1
                    ? Result(key, """{"Section":{"Name":"first"}}""")
                    : await NotFound(), out _);

            var provider = new ConsulConfigurationProvider(source, ConsulConfigurationFormat.Json);
            var reloaded = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using var registration = provider.GetReloadToken()
                .RegisterChangeCallback(_ => reloaded.TrySetResult(), null);

            provider.Load();
            Assert.True(provider.TryGet("Section:Name", out _));

            await reloaded.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            Assert.False(provider.TryGet("Section:Name", out _));

            provider.Dispose();
        }

        [Fact]
        public async Task Watch_Reports_Deleted_NonOptional_Key_Once()
        {
            var calls = 0;
            var exceptions = 0;

            var source = new ConsulConfigurationSource
            {
                Path = Key,
                ReloadOnChange = true,
                PollingWaitTime = TimeSpan.FromMilliseconds(200)
            };
            source.OnLoadException = context =>
            {
                Interlocked.Increment(ref exceptions);
                context.Ignore = true;
            };
            source.ClientFactory = () => CreateClient((key, _, _) =>
                Interlocked.Increment(ref calls) == 1
                    ? Task.FromResult(Result(key, """{"Section":{"Name":"value"}}"""))
                    : NotFound(), out _);

            var provider = new ConsulConfigurationProvider(source, ConsulConfigurationFormat.Json);
            provider.Load();

            // Two 404 polls: the deletion is reported once and the value of a required key is kept.
            await Task.Delay(TimeSpan.FromMilliseconds(500), TestContext.Current.CancellationToken);

            Assert.Equal(1, exceptions);
            Assert.True(provider.TryGet("Section:Name", out var value));
            Assert.Equal("value", value);

            provider.Dispose();
        }

        private static ConsulConfigurationProvider CreateProvider(
            Func<string, QueryOptions, CancellationToken, Task<QueryResult<KVPair>>> get,
            Action<ConsulConfigurationSource>? configure = null)
        {
            var source = new ConsulConfigurationSource { Path = Key };
            configure?.Invoke(source);
            source.ClientFactory = () => CreateClient(get, out _);

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

        private static IConsulClient CreateClient(
            Func<string, QueryOptions, CancellationToken, Task<QueryResult<KVPair>>> get,
            out StubProxy clientProxy)
        {
            var kvEndpointType = typeof(IConsulClient)
                .GetProperty(nameof(IConsulClient.KV))!
                .PropertyType;

            var kv = DispatchProxy.Create(kvEndpointType, typeof(StubProxy));
            var kvProxy = (StubProxy)kv;
            kvProxy.Get = get;

            var client = DispatchProxy.Create<IConsulClient, StubProxy>();
            clientProxy = (StubProxy)(object)client;
            clientProxy.KV = kv;

            return client;
        }

        /// <summary>
        /// Minimal <see cref="DispatchProxy"/> based stub so the test only has to provide the
        /// members that <see cref="ConsulConfigurationProvider"/> actually uses.
        /// </summary>
        public class StubProxy : DispatchProxy
        {
            public object? KV { get; set; }

            public Func<string, QueryOptions, CancellationToken, Task<QueryResult<KVPair>>>? Get { get; set; }

            public bool Disposed { get; private set; }

            protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
            {
                switch (targetMethod?.Name)
                {
                    case "get_KV":
                        return KV;
                    case "Dispose":
                        Disposed = true;
                        return null;
                    case "Get" when args is { Length: 3 }:
                        return Get!((string)args[0]!, (QueryOptions)args[1]!, (CancellationToken)args[2]!);
                    default:
                        throw new NotImplementedException(targetMethod?.Name);
                }
            }
        }
    }
}
