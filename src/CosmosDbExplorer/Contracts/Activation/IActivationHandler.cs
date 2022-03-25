using System.Threading.Tasks;

namespace CosmosDbExplorer.Contracts.Activation
{
    public interface IActivationHandler
    {
        bool CanHandle();

        Task HandleAsync();
    }
}
