using FacturaLuz.Helpers;
using FacturaLuz.ViewModels;
using Microsoft.UI.Xaml.Controls;

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

    private void TextBox_GotFocus(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (AxudaToggleSwitch.IsOn)
        {
            var target = (TextBox)sender;

            if (!AxudaTeachingTip.IsOpen || AxudaTeachingTip.Target != target)
            {
                AxudaTeachingTip.Target = target;
                AxudaTeachingTip.Title = ResourceExtensions.GetLocalized("Settings_" + target.Name + "/Header");
                AxudaTeachingTip.Subtitle = ResourceExtensions.GetLocalized("Settings_" + target.Name + "_" + AxudaTeachingTip.Name + "/Subtitle");

                AxudaTeachingTip.IsOpen = true;
            }
        }
    }
}
