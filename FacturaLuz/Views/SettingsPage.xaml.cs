using FacturaLuz.ViewModels;

using Microsoft.UI.Xaml.Controls;
using Windows.Storage;

namespace FacturaLuz.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel
    {
        get;
    }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
    }
}
