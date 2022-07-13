using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Windows.Storage;

namespace FacturaLuz.ViewModels;

public class MainViewModel : ObservableRecipient
{
    public MainViewModel()
    {
    }

    public static async Task<string> ObterConsumosAsync()
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

            var progress = new Progress<double>();
            progress.ProgressChanged += (sender, value) => System.Console.Write("\r%{0:N0}", value);

            var cancellationToken = new CancellationTokenSource();

            try
            {
                await DownloadFileAsync(url, progress, cancellationToken.Token, "em_data.csv");
                response = "\nDescargado o arquivo de consumos.";
            }
            catch (HttpRequestException e)
            {
                response = "\nProduciuse un erro: " + e.Message;
            }
        }

        return response;
    }

    public static async Task DownloadFileAsync(string url, IProgress<double> progress, CancellationToken token, string filename)
    {
        var response = await App.httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(string.Format("The request returned with HTTP status code {0}", response.StatusCode));
        }

        var total = response.Content.Headers.ContentLength.HasValue ? response.Content.Headers.ContentLength.Value : -1L;
        var canReportProgress = total != -1 && progress != null;

        using var stream = await response.Content.ReadAsStreamAsync();
        var totalRead = 0L;
        var buffer = new byte[4096];
        var isMoreToRead = true;

        do
        {
            token.ThrowIfCancellationRequested();

            var read = await stream.ReadAsync(buffer, 0, buffer.Length, token);

            if (read == 0)
            {
                isMoreToRead = false;
            }
            else
            {
                var data = new byte[read];
                buffer.ToList().CopyTo(0, data, 0, read);

                // TODO: put here the code to write the file to disk
                File.WriteAllBytes(ApplicationData.Current.LocalFolder.Path + "\\" + filename, buffer);
                
                totalRead += read;

                if (canReportProgress)
                {
                    progress.Report((totalRead * 1d) / (total * 1d) * 100);
                }
            }
        } while (isMoreToRead);
    }
}
