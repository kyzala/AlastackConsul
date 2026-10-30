namespace Alastack.Consul.Registration;

public interface IRegistrationService
{
    Task RegisterAsync(CancellationToken cancellationToken);

    Task DeregisterAsync(CancellationToken cancellationToken);
}
