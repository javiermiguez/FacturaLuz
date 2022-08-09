using System;
using System.Collections.Generic;
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

    //public class Consumo
    //{
    //    public DateTime DataHora
    //    {
    //        get; set;
    //    }
    //    public float WatiosHora
    //    {
    //        get; set;
    //    }
    //}

    //public static async Task ObterConsumosAsync(DateTimeOffset dataInicio, DateTimeOffset dataFin, Progress<double> progress, string response)
    //{
    //    dataInicio = new DateTimeOffset(dataInicio.Year, dataInicio.Month, dataInicio.Day, 0, 0, 0, TimeSpan.Zero);
    //    dataFin = new DateTimeOffset(dataFin.Year, dataFin.Month, dataFin.Day, 23, 59, 59, TimeSpan.Zero);

    //    var filePath = ApplicationData.Current.LocalFolder.Path + "\\em_data.csv";
    //    var response = "\n- Comprobando se existe o arquivo de consumos: " + filePath;
    //    bool necesarioDescargarArquivo;

    //    if (File.Exists(filePath))
    //    {
    //        response += "\n- Arquivo atopado. Comprobando se contén as datas requiridas...";

    //        var consumos = FiltrarConsumos(dataInicio, dataFin, filePath);

    //        var existeDataInicio = consumos.Any(c => c.DataHora.Date == dataInicio.Date && c.DataHora.Hour == 0);
    //        var existeDataFin = consumos.Any(c => c.DataHora.Date == dataFin.Date && c.DataHora.Hour == 23);

    //        if (existeDataInicio && existeDataFin)
    //        {
    //            response += "\n- Atopados " + consumos.Count().ToString() + " rexistros entre as datas indicadas.";
    //            necesarioDescargarArquivo = false;
    //        }
    //        else
    //        {
    //            response += "\n- Datas non atopadas. Procedendo coa descarga do arquivo...";
    //            necesarioDescargarArquivo= true;
    //        }
    //    }
    //    else
    //    {
    //        response += "\n- Arquivo non atopado. Procedendo coa descarga...";
    //        necesarioDescargarArquivo = true;
    //    }

    //    if (necesarioDescargarArquivo)
    //    {
    //        var url = (ApplicationData.Current.LocalSettings.Values["urlMedidor"] as string);

    //        if (url == null)
    //        {
    //            response += "\n- Atopouse un problema: A URL do medidor non está gardada na configuración.";
    //        }
    //        else
    //        {
    //            if (!url.EndsWith("/"))
    //                url += "/";

    //            url += "emeter/1/em_data.csv";

    //            var cancellationToken = new CancellationTokenSource();

    //            try
    //            {
    //                await DescargarArquivoAsync(url, filePath, progress, cancellationToken.Token);
    //                response += "\n- Descarga completada.";
    //            }
    //            catch (HttpRequestException e)
    //            {
    //                response = "\n- Produciuse un erro durante a descarga: " + e.Message;
    //            }
    //        }
    //    }

    //    return response;
    //}

    //private static List<Consumo> FiltrarConsumos(DateTimeOffset dataInicio, DateTimeOffset dataFin, string filePath)
    //{
    //    var csvLines = File.ReadLines(filePath)
    //            .Skip(1)
    //            .Where(s => s != "" && !s.StartsWith("D"))
    //            .Select(s => s.Split(new[] { ',' }))
    //            .Select(l => new Consumo
    //            {
    //                DataHora = DateTime.Parse(l[0]),
    //                WatiosHora = float.Parse(l[1])
    //            })
    //            .Where(l => DateTimeOffset.Compare(l.DataHora, dataInicio) >= 0 && DateTimeOffset.Compare(l.DataHora, dataFin) <= 0).ToList();

    //    return csvLines;
    //}

    //public static async Task DescargarArquivoAsync(string url, string filePath, IProgress<double> progress, CancellationToken token)
    //{
    //    var response = await App.httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);

    //    if (!response.IsSuccessStatusCode)
    //    {
    //        throw new Exception(string.Format("The request returned with HTTP status code {0}", response.StatusCode));
    //    }

    //    var total = response.Content.Headers.ContentLength.HasValue ? response.Content.Headers.ContentLength.Value : -1L;
    //    var canReportProgress = total != -1 && progress != null;
        
    //    using var stream = await response.Content.ReadAsStreamAsync();
    //    var totalRead = 0L;
    //    var buffer = new byte[4096];
    //    var isMoreToRead = true;

    //    do
    //    {
    //        token.ThrowIfCancellationRequested();

    //        var read = await stream.ReadAsync(buffer, 0, buffer.Length, token);

    //        if (read == 0)
    //        {
    //            isMoreToRead = false;
    //        }
    //        else
    //        {
    //            var data = new byte[read];
    //            buffer.ToList().CopyTo(0, data, 0, read);

    //            var fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write);
    //            fileStream.Write(data);
    //            fileStream.Close();
    //            fileStream.Dispose();

    //            totalRead += read;

    //            if (canReportProgress)
    //            {
    //                progress.Report((totalRead * 1d) / (total * 1d) * 100);
    //            }
    //        }
    //    } while (isMoreToRead);
    //}
}
