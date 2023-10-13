namespace Alastack.Consul.Registration
{
    public interface IRegistrationService
    {
        Task ServiceRegister(CancellationToken cancellationToken);

        Task ServiceDeregister(CancellationToken cancellationToken);
    }
}
