using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using FacturaLuz.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using System.Globalization;
using Newtonsoft.Json.Linq;
using FacturaLuz.Helpers;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Microsoft.UI.Xaml.Documents;

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
        public decimal PrezoWatioHora
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

    private void AbrirCartafolLocal_Click(object sender, RoutedEventArgs e)
    {
        Process.Start("explorer.exe", ApplicationData.Current.LocalFolder.Path);
    }

    private void DataFin_Opened(object sender, object e)
    {
        if (!DataFin.Date.HasValue)
        {
            if (DataInicio.Date.HasValue)
            {
                DataFin.Date = DataInicio.Date.Value.AddDays(31);
            }
            else
            {
                DataFin.Date = DateTimeOffset.Now.AddDays(-1);
            }
        }
    }

    private void DataInicio_Opened(object sender, object e)
    {
        if (!DataInicio.Date.HasValue)
        {
            if (DataFin.Date.HasValue)
            {
                DataInicio.Date = DataFin.Date.Value.AddDays(-31);
            }
            else
            { 
                DataInicio.Date = DateTimeOffset.Now.AddMonths(-1).AddDays(-2);
            }
        }
    }

    private async void Calcular_Click(object sender, RoutedEventArgs e)
    {
        LogInfo.Text = "";
        ValidacionInfo.Text = "";
        GridFactura.Visibility = Visibility.Collapsed;
        SeparadorFacturaLog.Visibility = Visibility.Collapsed;

        if (!DataInicio.Date.HasValue || !DataFin.Date.HasValue)
        {
            ValidacionInfo.Text = ResourceExtensions.GetLocalized("Txt_SeleccionaInicioFin");
            return;
        }
        var dataInicio = new DateTime(DataInicio.Date.Value.Year, DataInicio.Date.Value.Month, DataInicio.Date.Value.Day, 0, 0, 0).AddDays(1); // A data de inicio exclúese da facturación
        var dataFin = new DateTime(DataFin.Date.Value.Year, DataFin.Date.Value.Month, DataFin.Date.Value.Day, 23, 59, 59);

        if (dataInicio >= dataFin)
        {
            ValidacionInfo.Text = ResourceExtensions.GetLocalized("Txt_InicioAnteriorFin");
            return;
        }
        else if (dataFin > DateTime.Now)
        {
            ValidacionInfo.Text = ResourceExtensions.GetLocalized("Txt_FinAnteriorHoxe");
            return;
        }

        LogInfo.Text = ResourceExtensions.GetLocalized("Txt_InicioLog");
        Progreso.Visibility = Visibility.Visible;

        await ObterConsumosAsync(dataInicio, dataFin);
        await ObterPrezosAsync(dataInicio, dataFin);
        //await Task.Run(() => CalcularFactura(dataInicio, dataFin));
        CalcularFactura(dataInicio, dataFin);

        GridFactura.Visibility = Visibility.Visible;
        SeparadorFacturaLog.Visibility = Visibility.Visible;
        
    }

    private async Task ObterConsumosAsync(DateTime dataInicio, DateTime dataFin)
    {
        var rutaLocal = ApplicationData.Current.LocalFolder.Path + "\\em_data.csv";

        LogInfo.Text += string.Format(ResourceExtensions.GetLocalized("Txt_ExisteArquivoConsumos"), rutaLocal);

        bool necesarioDescargarArquivo;

        if (File.Exists(rutaLocal))
        {
            LogInfo.Text += ResourceExtensions.GetLocalized("Txt_ArquivoAtopadoBuscandoDatas");

            FiltrarConsumos(dataInicio, dataFin, rutaLocal);

            var existeDataInicio = Consumos.Any(c => c.DataHora.Date == dataInicio.Date && c.DataHora.Hour == 0);
            var existeDataFin = Consumos.Any(c => c.DataHora.Date == dataFin.Date && c.DataHora.Hour == 23);

            if (existeDataInicio && existeDataFin)
            {
                LogInfo.Text += string.Format(ResourceExtensions.GetLocalized("Txt_NRexistrosEntreDatas"), Consumos.Count);
                necesarioDescargarArquivo = false;
            }
            else
            {
                LogInfo.Text += ResourceExtensions.GetLocalized("Txt_DatasNonAtopadas");
                File.Delete(rutaLocal);
                LogInfo.Text += ResourceExtensions.GetLocalized("Txt_ArquivoEliminadoDescargando");
                necesarioDescargarArquivo = true;
            }
        }
        else
        {
            LogInfo.Text += ResourceExtensions.GetLocalized("Txt_ArquivoNonAtopadoDescargando");
            necesarioDescargarArquivo = true;
        }

        if (necesarioDescargarArquivo)
        {
            var url = (ApplicationData.Current.LocalSettings.Values["urlMedidor"] as string);

            if (url == null)
            {
                LogInfo.Text += ResourceExtensions.GetLocalized("Txt_URLMedidorNonGardada");
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
                    LogInfo.Text += ResourceExtensions.GetLocalized("Txt_DescargaCompletada");
                }
                catch (HttpRequestException e)
                {
                    LogInfo.Text = string.Format(ResourceExtensions.GetLocalized("Txt_ErroDescarga"), e.Message);
                }
            }

            FiltrarConsumos(dataInicio, dataFin, rutaLocal);
        }
    }

    private async Task ObterPrezosAsync(DateTime dataInicio, DateTime dataFin)
    {
        var rutaLocalPrezosTotais = ApplicationData.Current.LocalFolder.Path + "\\1001.json";
        var rutaLocalPrezosPeaxes = ApplicationData.Current.LocalFolder.Path + "\\1876.json";
        var rutaLocalPeriodos = ApplicationData.Current.LocalFolder.Path + "\\1002.json";

        LogInfo.Text += string.Format(ResourceExtensions.GetLocalized("Txt_ExistenArquivosPrezos"), rutaLocalPrezosTotais, rutaLocalPrezosPeaxes, rutaLocalPeriodos);

        bool necesarioDescargarArquivos;

        if (File.Exists(rutaLocalPrezosTotais) && File.Exists(rutaLocalPrezosPeaxes) && File.Exists(rutaLocalPeriodos))
        {
            LogInfo.Text += ResourceExtensions.GetLocalized("Txt_ArquivosAtopadosBuscandoDatas");

            FiltrarPrezos(dataInicio, dataFin, rutaLocalPrezosTotais);

            var existeDataInicio = Prezos.Any(p => p.DataHora.Date == dataInicio.Date && p.DataHora.Hour == 0);
            var existeDataFin = Prezos.Any(p => p.DataHora.Date == dataFin.Date && p.DataHora.Hour == 23);

            if (existeDataInicio && existeDataFin)
            {
                LogInfo.Text += string.Format(ResourceExtensions.GetLocalized("Txt_NRexistrosEntreDatas"), Prezos.Count);
                necesarioDescargarArquivos = false;
            }
            else
            {
                LogInfo.Text += ResourceExtensions.GetLocalized("Txt_DatasNonAtopadas");
                File.Delete(rutaLocalPrezosTotais);
                LogInfo.Text += ResourceExtensions.GetLocalized("Txt_ArquivoEliminadoDescargando");
                necesarioDescargarArquivos = true;
            }
        }
        else
        {
            LogInfo.Text += ResourceExtensions.GetLocalized("Txt_ArquivosNonAtopadosDescargando");
            necesarioDescargarArquivos = true;
        }

        if (necesarioDescargarArquivos)
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

                await DescargarArquivoAsync(url, rutaLocalPrezosTotais, cancellationToken.Token);
                LogInfo.Text += ResourceExtensions.GetLocalized("Txt_DescargaCompletada");
            }
            catch (HttpRequestException e)
            {
                LogInfo.Text = string.Format(ResourceExtensions.GetLocalized("Txt_ErroDescarga"), e.Message);
            }

            FiltrarPrezos(dataInicio, dataFin, rutaLocalPrezosTotais);
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
                .GroupBy(c => new
                                {
                                    Data = c.DataHora.Date,
                                    Hora = c.DataHora.Hour
                                })
                .Select(c => new Consumo
                {
                    DataHora = c.Key.Data.AddHours(c.Key.Hora),
                    WatiosHora = c.Sum(c => c.WatiosHora)
                })
                .OrderBy(c => c.DataHora)
                .ToList();
    }

    private void FiltrarPrezos(DateTime dataInicio, DateTime dataFin, string rutaLocal)
    {
        using var streamReader = new StreamReader(rutaLocal);
        var jObject = JObject.Parse(streamReader.ReadToEnd());
        
        Prezos = jObject["indicator"]["values"]
                .Select(p => new Prezo
                    {
                        DataHora = (DateTime)p["datetime"],
                        PrezoWatioHora = decimal.Parse((string)p["value"], System.Globalization.NumberStyles.AllowDecimalPoint, new CultureInfo("en-US")) / 1000000
                    })
                .Where(p => p.DataHora >= dataInicio && p.DataHora <= dataFin)
                .OrderBy(p => p.DataHora)
                .ToList();
    }

    private void CalcularFactura(DateTime dataInicio, DateTime dataFin)
    {
        var config = App.GetService<SettingsViewModel>();

        decimal diasFactura = (dataFin - dataInicio).Days + 1; // + 1 porque a hora de dataFin son as 23:59:59, así que non conta ese día como enteiro e hai que engadilo
        decimal diasAnoDataFin = DateTime.IsLeapYear(dataFin.Year) ? 366 : 365;
        var termoFixoPeaxesTDCPuntaResultado = config.PotenciaContratada * config.TermoFixoPeaxesTDCPunta * (diasFactura / diasAnoDataFin);
        var termoFixoPeaxesTDCValResultado = config.PotenciaContratada * config.TermoFixoPeaxesTDCVal * (diasFactura / diasAnoDataFin);
        var termoFixoMarxeComercializacionResultado = config.PotenciaContratada * config.MarxeComercializacion * (diasFactura / diasAnoDataFin);
        var termoFixoResultado = termoFixoPeaxesTDCPuntaResultado + termoFixoPeaxesTDCValResultado + termoFixoMarxeComercializacionResultado;

        var ConsumosPrezos = Consumos.Join(Prezos,
                                            c => c.DataHora,
                                            p => p.DataHora,
                                            (Consumo, Prezo) => new { Consumo.DataHora, Consumo.WatiosHora, Prezo.PrezoWatioHora }
                                          )
                                          .ToList();

        var termoVariableResultado = ConsumosPrezos.Sum(cp => cp.WatiosHora * cp.PrezoWatioHora);
        var kWhConsumidosPunta = (decimal)0; //ConsumoPrezos.Where(cp => cp.DataHora IN XXX).Sum(cp => cp.WatiosHora);
        var kWhConsumidosChan = (decimal)0;
        var kWhConsumidosVal = (decimal)0;
        var termoVariablePeaxesTDCPuntaResultado = kWhConsumidosPunta * 1;
        var termoVariablePeaxesTDCChanResultado = kWhConsumidosChan * 1;
        var termoVariablePeaxesTDCValResultado = kWhConsumidosVal * 1;
        var numeroPrezosDiferentes = ConsumosPrezos.Count();
        var custeEnerxiaResultado = termoVariableResultado - termoVariablePeaxesTDCPuntaResultado - termoVariablePeaxesTDCChanResultado - termoVariablePeaxesTDCValResultado;

        var bonoSocialTermoFixoResultado = termoFixoResultado * config.DescontoBonoSocial / 100;
        var kWhConsumidos = ConsumosPrezos.Sum(cp => cp.WatiosHora) / 1000;
        var kWhBonificables = (config.LimiteBonoSocial / diasAnoDataFin) * diasFactura;
        var bonoSocialTermoVariableResultado = termoVariableResultado * (kWhBonificables / kWhConsumidos) * (config.DescontoBonoSocial / 100);
        var bonoSocialResultado = bonoSocialTermoFixoResultado + bonoSocialTermoVariableResultado;

        var impostoElectricidadeBaseImpoñible = termoFixoResultado + termoVariableResultado - bonoSocialResultado;
        var impostoElectricidadeResultado = impostoElectricidadeBaseImpoñible * config.ImpostoElectricidade / 100;
        var alugueiroContadorResultado = diasFactura * config.AlugueiroContador;
        var iveBaseImpoñible = impostoElectricidadeBaseImpoñible + impostoElectricidadeResultado + alugueiroContadorResultado;
        var iveResultado = iveBaseImpoñible * config.Ive / 100;

        var totalResultado = iveBaseImpoñible + iveResultado;

        EscribirFragmentoFactura(Titulo_Texto, dataInicio.AddDays(-1).ToString("dd/MM/yyyy"), dataFin.ToString("dd/MM/yyyy"));

        EscribirFragmentoFactura(TermoFixo_Texto);
        EscribirFragmentoFactura(TermoFixo_Resultado, Math.Round(termoFixoResultado, 2));
        EscribirFragmentoFactura(TermoFixoPeaxesTDC_Texto);
        EscribirFragmentoFactura(TermoFixoPunta_Texto);
        EscribirFragmentoFactura(TermoFixoPunta_Calculo, config.PotenciaContratada, config.TermoFixoPeaxesTDCPunta, diasFactura, diasAnoDataFin);
        EscribirFragmentoFactura(TermoFixoPunta_Resultado, Math.Round(termoFixoPeaxesTDCPuntaResultado, 2));
        EscribirFragmentoFactura(TermoFixoVal_Texto);
        EscribirFragmentoFactura(TermoFixoVal_Calculo, config.PotenciaContratada, config.TermoFixoPeaxesTDCVal, diasFactura, diasAnoDataFin);
        EscribirFragmentoFactura(TermoFixoVal_Resultado, Math.Round(termoFixoPeaxesTDCValResultado, 2));
        EscribirFragmentoFactura(TermoFixoMarxe_Texto);
        EscribirFragmentoFactura(TermoFixoMarxe_Calculo, config.PotenciaContratada, config.MarxeComercializacion, diasFactura, diasAnoDataFin);
        EscribirFragmentoFactura(TermoFixoMarxe_Resultado, Math.Round(termoFixoMarxeComercializacionResultado, 2));

        EscribirFragmentoFactura(TermoVariable_Texto);
        EscribirFragmentoFactura(TermoVariable_Resultado, Math.Round(termoVariableResultado, 2));
        EscribirFragmentoFactura(TermoVariablePeaxesTDC_Texto);
        EscribirFragmentoFactura(TermoVariablePunta_Texto);
        EscribirFragmentoFactura(TermoVariablePunta_Calculo, "?", "?");
        EscribirFragmentoFactura(TermoVariablePunta_Resultado, "?"); //Math.Round(termoVariablePeaxesTDCPuntaResultado, 2));
        EscribirFragmentoFactura(TermoVariableChan_Texto);
        EscribirFragmentoFactura(TermoVariableChan_Calculo, "?", "?");
        EscribirFragmentoFactura(TermoVariableChan_Resultado, "?"); //Math.Round(termoVariablePeaxesTDCChanResultado, 2));
        EscribirFragmentoFactura(TermoVariableVal_Texto);
        EscribirFragmentoFactura(TermoVariableVal_Calculo, "?", "?");
        EscribirFragmentoFactura(TermoVariableVal_Resultado, "?"); //Math.Round(termoVariablePeaxesTDCValResultado, 2));
        EscribirFragmentoFactura(TermoVariableCusteEnerxia_Texto);
        EscribirFragmentoFactura(TermoVariableCusteEnerxia_Calculo, numeroPrezosDiferentes);
        EscribirFragmentoFactura(TermoVariableCusteEnerxia_Resultado, Math.Round(custeEnerxiaResultado, 2));

        EscribirFragmentoFactura(BonoSocial_Texto);
        EscribirFragmentoFactura(BonoSocial_Resultado, -Math.Round(bonoSocialResultado, 2));
        EscribirFragmentoFactura(BonoSocialTermoFixo_Texto);
        EscribirFragmentoFactura(BonoSocialTermoFixo_Calculo, Math.Round(termoFixoResultado, 2), config.DescontoBonoSocial);
        EscribirFragmentoFactura(BonoSocialTermoFixo_Resultado, -Math.Round(bonoSocialTermoFixoResultado, 2));
        EscribirFragmentoFactura(BonoSocialTermoVariable_Texto);
        EscribirFragmentoFactura(BonoSocialTermoVariable_Calculo, Math.Round(termoVariableResultado, 2), Math.Round(kWhBonificables, 0),
                                 Math.Round(kWhConsumidos, 0), config.DescontoBonoSocial);
        EscribirFragmentoFactura(BonoSocialTermoVariable_Resultado, -Math.Round(bonoSocialTermoVariableResultado, 2));

        EscribirFragmentoFactura(ImpostoElectricidade_Texto);
        EscribirFragmentoFactura(ImpostoElectricidade_Calculo, Math.Round(impostoElectricidadeBaseImpoñible, 2), config.ImpostoElectricidade);
        EscribirFragmentoFactura(ImpostoElectricidade_Resultado, Math.Round(impostoElectricidadeResultado, 2));

        EscribirFragmentoFactura(AlugueiroContador_Texto);
        EscribirFragmentoFactura(AlugueiroContador_Calculo, diasFactura, config.AlugueiroContador);
        EscribirFragmentoFactura(AlugueiroContador_Resultado, Math.Round(alugueiroContadorResultado, 2));

        EscribirFragmentoFactura(Ive_Texto);
        EscribirFragmentoFactura(Ive_Calculo, Math.Round(iveBaseImpoñible, 2), config.Ive);
        EscribirFragmentoFactura(Ive_Resultado, Math.Round(iveResultado, 2));

        EscribirFragmentoFactura(Total_Texto);
        EscribirFragmentoFactura(Total_Resultado, Math.Round(totalResultado, 2));
    }

    private void EscribirFragmentoFactura(Run seccionTexto, object arg0 = null, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null)
    {
        var resourceKey = "Main_Factura_" + (seccionTexto.Name.EndsWith("_Resultado") ? "ResultadoLiña" : seccionTexto.Name);

        seccionTexto.Text = string.Format(ResourceExtensions.GetLocalized(resourceKey), new object[] { arg0, arg1, arg2, arg3, arg4 });
    }

    private async Task DescargarArquivoAsync(string url, string rutaLocal, CancellationToken token)
    {
        var response = await App.httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(string.Format(ResourceExtensions.GetLocalized("Txt_EstadoPeticionErro"), response.StatusCode));
        }

        var total = response.Content.Headers.ContentLength.HasValue ? response.Content.Headers.ContentLength.Value : -1L;
        var canReportPercentProgress = total != -1;

        using var stream = await response.Content.ReadAsStreamAsync();
        var totalRead = 0L;
        var buffer = new byte[4096];
        var isMoreToRead = true;

        if (!canReportPercentProgress)
        {
            Progreso.IsIndeterminate = true;
            Progreso.IsActive = true;
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
                    Progreso.Value = (totalRead * 1d) / (total * 1d) * 100;
                }
                else
                {
                    DescargaInfo.Text = string.Format(ResourceExtensions.GetLocalized("Txt_BytesDescargados"), totalRead);
                }
            }
        } while (isMoreToRead);

        if (!canReportPercentProgress)
        {
            LogInfo.Text += DescargaInfo.Text;
            DescargaInfo.Text = "";
            Progreso.IsActive = false;
        }
    }
}