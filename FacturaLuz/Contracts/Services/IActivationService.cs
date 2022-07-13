using System.Threading.Tasks;

namespace FacturaLuz.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
