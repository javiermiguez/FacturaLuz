using System.Threading.Tasks;

namespace FacturaLuz.Activation;

public interface IActivationHandler
{
    bool CanHandle(object args);

    Task HandleAsync(object args);
}
