using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FacturaLuz.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Text.Json;
using Windows.Storage;
using System.Globalization;

namespace FacturaLuz.Views;

public sealed partial class MainPage : Page
{
    private class Consumo
    {
        public DateTime DataHora
        {
            get; set;
        }
        public decimal WatiosHora
        {
            get; set;
        }
    }

    private class Prezo
    {
        public DateTime DataHora
        {
            get; set;
        }
        public float PrezoWatioHora
        {
            get; set;
        }
    }

    private List<Consumo> Consumos;

    private List<Prezo> Prezos;

    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }

    private void abrirCartafolLocalButton_Click(object sender, RoutedEventArgs e)
    {
        Process.Start("explorer.exe", ApplicationData.Current.LocalFolder.Path);
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

    private async void calcularButton_Click(object sender, RoutedEventArgs e)
    {
        infoRun.Text = "";

        if (Validar())
        {
            progresoProgressRing.Visibility = Visibility.Visible;

            infoRun.Text = "\n-------------------- INICIANDO PROCESO --------------------";

            var dataInicio = new DateTime(inicioDatePicker.Date.Value.Year, inicioDatePicker.Date.Value.Month, inicioDatePicker.Date.Value.Day, 0, 0, 0);
            var dataFin = new DateTime(inicioDatePicker.Date.Value.Year, inicioDatePicker.Date.Value.Month, inicioDatePicker.Date.Value.Day, 23, 59, 59);

            await ObterConsumosAsync(dataInicio, dataFin);
            await ObterPrezosAsync(dataInicio, dataFin);

            CalcularFactura();
        }
    }

    private bool Validar()
    {
        if (!inicioDatePicker.Date.HasValue || !finDatePicker.Date.HasValue)
        {
            validacionRun.Text = "Selecciona as datas de inicio e fin.";
            return false;
        }
        else if (inicioDatePicker.Date > finDatePicker.Date)
        {
            validacionRun.Text = "A data de inicio ten que ser anterior ou igual á de fin.";
            return false;
        }
        else if (finDatePicker.Date > DateTime.Now)
        {
            validacionRun.Text = "A data de fin ten que ser menor ou igual á de hoxe.";
            return false;
        }

        validacionRun.Text = "";
        return true;
    }

    private async Task ObterConsumosAsync(DateTime dataInicio, DateTime dataFin)
    {
        var rutaLocal = ApplicationData.Current.LocalFolder.Path + "\\em_data.csv";

        infoRun.Text += "\n- Comprobando se existe o arquivo de consumos: " + rutaLocal;

        bool necesarioDescargarArquivo;

        if (File.Exists(rutaLocal))
        {
            infoRun.Text += "\n- Arquivo atopado. Comprobando se contén as datas requiridas...";

            FiltrarConsumos(dataInicio, dataFin, rutaLocal);

            var existeDataInicio = Consumos.Any(c => c.DataHora.Date == dataInicio.Date && c.DataHora.Hour == 0);
            var existeDataFin = Consumos.Any(c => c.DataHora.Date == dataFin.Date && c.DataHora.Hour == 23);

            if (existeDataInicio && existeDataFin)
            {
                infoRun.Text += "\n- Atopados " + Consumos.Count.ToString() + " rexistros entre as datas indicadas.";
                necesarioDescargarArquivo = false;
            }
            else
            {
                infoRun.Text += "\n- Datas non atopadas.";
                File.Delete(rutaLocal);
                infoRun.Text += "\n- Arquivo eliminado. Procedendo coa descarga do arquivo...";
                necesarioDescargarArquivo = true;
            }
        }
        else
        {
            infoRun.Text += "\n- Arquivo non atopado. Procedendo coa descarga...";
            necesarioDescargarArquivo = true;
        }

        if (necesarioDescargarArquivo)
        {
            var url = (ApplicationData.Current.LocalSettings.Values["urlMedidor"] as string);

            if (url == null)
            {
                infoRun.Text += "\n- Atopouse un problema: A URL do medidor non está gardada na configuración.";
            }
            else
            {
                if (!url.EndsWith("/"))
                {
                    url += "/";
                }

                url += "emeter/1/em_data.csv";

                var cancellationToken = new CancellationTokenSource();

                try
                {
                    App.httpClient.DefaultRequestHeaders.Clear();
                    await DescargarArquivoAsync(url, rutaLocal, cancellationToken.Token);
                    infoRun.Text += "\n- Descarga completada.";
                }
                catch (HttpRequestException e)
                {
                    infoRun.Text = "\n- Produciuse un erro durante a descarga: " + e.Message;
                }
            }

            FiltrarConsumos(dataInicio, dataFin, rutaLocal);
        }
    }

    private async Task ObterPrezosAsync(DateTime dataInicio, DateTime dataFin)
    {
        var rutaLocal = ApplicationData.Current.LocalFolder.Path + "\\1001.json";

        infoRun.Text += "\n\n- Comprobando se existe o arquivo de prezos: " + rutaLocal;

        bool necesarioDescargarArquivo;

        if (File.Exists(rutaLocal))
        {
            infoRun.Text += "\n- Arquivo atopado. Comprobando se contén as datas requiridas...";

            FiltrarPrezos(dataInicio, dataFin, rutaLocal);

            var existeDataInicio = Prezos.Any(p => p.DataHora.Date == dataInicio.Date && p.DataHora.Hour == 0);
            var existeDataFin = Prezos.Any(p => p.DataHora.Date == dataFin.Date && p.DataHora.Hour == 23);

            if (existeDataInicio && existeDataFin)
            {
                infoRun.Text += "\n- Atopados " + Prezos.Count.ToString() + " rexistros entre as datas indicadas.";
                necesarioDescargarArquivo = false;
            }
            else
            {
                infoRun.Text += "\n- Datas non atopadas.";
                File.Delete(rutaLocal);
                infoRun.Text += "\n- Arquivo eliminado. Procedendo coa descarga do arquivo...";
                necesarioDescargarArquivo = true;
            }
        }
        else
        {
            infoRun.Text += "\n- Arquivo non atopado. Procedendo coa descarga...";
            necesarioDescargarArquivo = true;
        }

        if (necesarioDescargarArquivo)
        {
            var url = "https://api.esios.ree.es/indicators/1001?"
                + "start_date=" + dataInicio.Date.Year + "-" + dataInicio.Date.Month + "-" + dataInicio.Date.Day + "T00:00:00&"
                + "end_date=" + dataFin.Date.Year + "-" + dataFin.Date.Month + "-" + dataFin.Date.Day + "T23:59:59&geo_ids[]=8741";

            var cancellationToken = new CancellationTokenSource();

            try
            {
                App.httpClient.DefaultRequestHeaders.Clear();
                App.httpClient.DefaultRequestHeaders.Add("Host", "api.esios.ree.es");
                App.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                App.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.esios-api-v2+json"));
                App.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", @"token=""***REMOVED***""");
                App.httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                await DescargarArquivoAsync(url, rutaLocal, cancellationToken.Token);
                infoRun.Text += "\n- Descarga completada.";
            }
            catch (HttpRequestException e)
            {
                infoRun.Text = "\n- Produciuse un erro durante a descarga: " + e.Message;
            }

            FiltrarPrezos(dataInicio, dataFin, rutaLocal);
        }
    }

    private void FiltrarConsumos(DateTime dataInicio, DateTime dataFin, string rutaLocal)
    {
        Consumos = File.ReadLines(rutaLocal)
                .Skip(1)
                .Select(s => s.Split(new[] { ',' }))
                .Select(c => new Consumo
                {
                    DataHora = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(c[0]), TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time")),
                    WatiosHora = decimal.Parse(c[1], System.Globalization.NumberStyles.AllowDecimalPoint, new CultureInfo("en-US"))
                })
                .Where(c => c.DataHora >= dataInicio && c.DataHora <= dataFin)
                .OrderBy(c => c.DataHora)
                .ToList();
    }

    private void FiltrarPrezos(DateTimeOffset dataInicio, DateTimeOffset dataFin, string rutaLocal)
    {
        Prezos = new List<Prezo>();

        using StreamReader streamReader = new StreamReader(rutaLocal);

        var json = streamReader.ReadToEnd();
        //Prezos = JsonSerializer.Deserialize<List<Prezo>>(json);
        Console.WriteLine("Ola");
    }

    private void CalcularFactura()
    {
        //Product product = await response.Content.ReadAsAsync > Product > ();
        //Console.WriteLine("{0}\t${1}\t{2}", product.Name, product.Price, product.Category);
    }

    private async Task DescargarArquivoAsync(string url, string rutaLocal, CancellationToken token)
    {
        var response = await App.httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(string.Format("The request returned with HTTP status code {0}", response.StatusCode));
        }

        var total = response.Content.Headers.ContentLength.HasValue ? response.Content.Headers.ContentLength.Value : -1L;
        var canReportPercentProgress = total != -1;

        using var stream = await response.Content.ReadAsStreamAsync();
        var totalRead = 0L;
        var buffer = new byte[4096];
        var isMoreToRead = true;

        if (!canReportPercentProgress)
        {
            progresoProgressRing.IsIndeterminate = true;
            progresoProgressRing.IsActive = true;
        }

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

                var fileStream = new FileStream(rutaLocal, FileMode.Append, FileAccess.Write);
                fileStream.Write(data);
                fileStream.Close();
                fileStream.Dispose();

                totalRead += read;

                if (canReportPercentProgress)
                {
                    progresoProgressRing.Value = (totalRead * 1d) / (total * 1d) * 100;
                }
                else
                {
                    descargaRun.Text = "\n" + totalRead.ToString() + " bytes descargados";
                }
            }
        } while (isMoreToRead);

        if (!canReportPercentProgress)
        {
            infoRun.Text += descargaRun.Text;
            descargaRun.Text = "";
            progresoProgressRing.IsActive = false;
        }
    }
}