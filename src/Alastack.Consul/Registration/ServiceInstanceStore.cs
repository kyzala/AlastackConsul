using Microsoft.Extensions.Options;

namespace Alastack.Consul.Registration;

/// <summary>
/// Holds the final service instances to register to Consul, merged from the configured instances
/// and the addresses provided by <see cref="IServerAddressesProvider"/>.
/// </summary>
internal sealed class ServiceInstanceStore
{
    private readonly ConsulRegistrationOptions _registrationOptions;
    private readonly IEnumerable<IServerAddressesProvider> _addressesProviders;
    private readonly SemaphoreSlim _mutex = new(1, 1);
    private IReadOnlyList<ResolvedServiceInstance>? _instances;

    public ServiceInstanceStore(IOptions<ConsulRegistrationOptions> options, IEnumerable<IServerAddressesProvider> addressesProviders)
    {
        _registrationOptions = options.Value;
        _addressesProviders = addressesProviders;
    }

    /// <summary>
    /// Gets the merged service instances. The result is materialized once and then memoized,
    /// so that registration and deregistration always operate on the same collection.
    /// </summary>
    public async Task<IReadOnlyList<ResolvedServiceInstance>> GetInstancesAsync(CancellationToken cancellationToken)
    {
        if (_instances != null)
        {
            return _instances;
        }

        await _mutex.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_instances != null)
            {
                return _instances;
            }

            var instances = new List<ResolvedServiceInstance>();
            foreach (var instance in _registrationOptions.Instances)
            {
                instances.Add(ResolvedServiceInstance.Resolve(instance, _registrationOptions));
            }

            foreach (var provider in _addressesProviders)
            {
                var addresses = await provider.GetServerAddressesAsync(cancellationToken).ConfigureAwait(false);
                foreach (var address in addresses)
                {
                    if (!Uri.TryCreate(address, UriKind.Absolute, out var uri))
                    {
                        throw new InvalidOperationException($"The server address '{address}' is not a valid absolute URI.");
                    }

                    // A wildcard listen address cannot be reached by consumers, so it is not registered.
                    if (IsWildcardHost(uri.Host))
                    {
                        continue;
                    }

                    if (!instances.Any(i => i.Address == uri))
                    {
                        instances.Add(ResolvedServiceInstance.Resolve(new ServiceInstance { Address = uri }, _registrationOptions));
                    }
                }
            }

            if (instances.Count == 0)
            {
                throw new InvalidOperationException(
                    "No service instances were resolved. Configure Consul:Registration:Instances or listen on a " +
                    "concrete address; wildcard addresses such as 0.0.0.0 and [::] cannot be registered.");
            }

            var duplicateId = instances
                .Where(instance => !string.IsNullOrWhiteSpace(instance.Id))
                .GroupBy(instance => instance.Id!, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicateId != null)
            {
                // Consul silently replaces a service registered with an existing id.
                throw new InvalidOperationException($"The service instance id '{duplicateId.Key}' is configured more than once.");
            }

            _instances = instances;
            return instances;
        }
        finally
        {
            _mutex.Release();
        }
    }

    private static bool IsWildcardHost(string host)
    {
        return host is "0.0.0.0" or "::" or "[::]" or "0:0:0:0:0:0:0:0";
    }
}
