using CommunityToolkit.Mvvm.Input;

namespace CosmosDbExplorer.Contracts.ViewModels
{
    public interface IHaveOpenCommand
    {
        RelayCommand OpenCommand { get; }
    }
}
