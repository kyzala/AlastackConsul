namespace Alastack.Consul.Registration;

/// <summary>
/// The default <see cref="IServerAddressesProvider"/> that buffers the server listen addresses pushed by the hosting layer.
/// </summary>
public sealed class DefaultServerAddressesProvider : IServerAddressesProvider
{
    private readonly object _gate = new();
    private readonly List<string> _addresses = new();
    private readonly TaskCompletionSource _configuredSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private bool _configured;

    /// <summary>
    /// Gets or sets a value indicating whether the hosting layer has finished pushing server addresses.
    /// Setting the value to <see langword="true"/> releases the pending <see cref="GetServerAddressesAsync"/> calls.
    /// </summary>
    public bool Configured
    {
        get
        {
            lock (_gate)
            {
                return _configured;
            }
        }

        set
        {
            if (!value)
            {
                return;
            }

            lock (_gate)
            {
                if (_configured)
                {
                    return;
                }

                _configured = true;
                _configuredSignal.TrySetResult();
            }
        }
    }

    /// <summary>
    /// Adds a server listen address.
    /// </summary>
    /// <param name="address">The server listen address.</param>
    public void AddServerAddress(string address)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(address);

        lock (_gate)
        {
            _addresses.Add(address);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<string>> GetServerAddressesAsync(CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            if (_configured)
            {
                return _addresses.ToArray();
            }
        }

        try
        {
            await _configuredSignal.Task
                .WaitAsync(RegistrationDefaults.ServerAddressesTimeout, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            throw new TimeoutException("Server addresses were not configured within the timeout.");
        }

        lock (_gate)
        {
            return _addresses.ToArray();
        }
    }
}
