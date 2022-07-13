using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FacturaLuz.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;

namespace FacturaLuz.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }

    private void finDatePicker_Opened(object sender, object e)
    {
        if (!finDatePicker.Date.HasValue)
        {
            finDatePicker.Date = DateTimeOffset.Now;
        }
    }

    private void inicioDatePicker_Opened(object sender, object e)
    {
        if (!inicioDatePicker.Date.HasValue)
        {
            inicioDatePicker.Date = DateTimeOffset.Now.AddMonths(-1);
        }
    }

    private async void CalcularButton_ClickAsync(object sender, RoutedEventArgs e)
    {
        string result = await MainViewModel.ObterConsumos();
        infoRun.Text = result;
    }
}