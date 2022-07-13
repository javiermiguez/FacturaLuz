using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Windows.Storage;

namespace FacturaLuz.ViewModels;

public class MainViewModel : ObservableRecipient
{
    public MainViewModel()
    {
    }

    public static async Task<string> ObterConsumos()
    {
        string response;

        var url = (ApplicationData.Current.LocalSettings.Values["urlMedidor"] as string);

        if (url == null)
        {
            response = "\nA URL do medidor non está gardada na configuración.";
        }
        else
        {
            if (!url.EndsWith("/"))
                url += "/";

            url += "emeter/1/em_data.csv";

            try
            {
                response = await App.httpClient.GetStringAsync(url);
            }
            catch (HttpRequestException e)
            {
                response = "\nProduciuse un erro:\n" + e.Message;
            }
        }

        return response;
    }
}
