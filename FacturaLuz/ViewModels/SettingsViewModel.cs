using System;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using FacturaLuz.Contracts.Services;
using FacturaLuz.Helpers;

using Microsoft.UI.Xaml;

using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Security.Credentials;

namespace FacturaLuz.ViewModels;

public class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;

    private bool _isLoading = true; // ⬅️ evita gardar mentres cargamos

    public SettingsViewModel(IThemeSelectorService themeSelectorService)
    {
        _themeSelectorService = themeSelectorService;

        // Tema
        _elementTheme = _themeSelectorService.Theme;
        VersionDescription = GetVersionDescription();

        // Carga limpa (sen setters)
        _urlMedidor = LerValorConfiguracionComoCadea("urlMedidor");
        _potenciaContratada = LerValorConfiguracionComoDecimal("potenciaContratada");
        _termoFixoPeaxesTDCPunta = LerValorConfiguracionComoDecimal("termoFixoPeaxesTDCPunta");
        _termoFixoPeaxesTDCVal = LerValorConfiguracionComoDecimal("termoFixoPeaxesTDCVal");
        _marxeComercializacion = LerValorConfiguracionComoDecimal("marxeComercializacion");
        _descontoBonoSocial = LerValorConfiguracionComoDecimal("descontoBonoSocial");
        _limiteBonoSocial = LerValorConfiguracionComoDecimal("limiteBonoSocial");
        _impostoElectricidade = LerValorConfiguracionComoDecimal("impostoElectricidade");
        _alugueiroContador = LerValorConfiguracionComoDecimal("alugueiroContador");
        _ive = LerValorConfiguracionComoDecimal("ive");

        // Token totalmente illado
        _esiosApiKey = ReadApiKeyFromVault();

        _isLoading = false; // ⬅️ agora xa podemos gardar cando cambie algo
    }

    // -----------------------------
    // PROPIEDADES
    // -----------------------------

    private string _urlMedidor;
    public string UrlMedidor
    {
        get => _urlMedidor;
        set
        {
            if (SetProperty(ref _urlMedidor, value) && !_isLoading)
            {
                ApplicationData.Current.LocalSettings.Values["urlMedidor"] = value;
            }
        }
    }

    private string _esiosApiKey;
    public string EsiosApiKey
    {
        get => _esiosApiKey;
        set
        {
            if (SetProperty(ref _esiosApiKey, value) && !_isLoading)
            {
                SaveApiKeyToVault(value);
            }
        }
    }

    // -----------------------------
    // DECIMAIS
    // -----------------------------

    private decimal _potenciaContratada;
    public decimal PotenciaContratada
    {
        get => _potenciaContratada;
        set
        {
            if (SetProperty(ref _potenciaContratada, value) && !_isLoading)
            {
                ApplicationData.Current.LocalSettings.Values["potenciaContratada"] = value.ToString();
            }
        }
    }

    private decimal _termoFixoPeaxesTDCPunta;
    public decimal TermoFixoPeaxesTDCPunta
    {
        get => _termoFixoPeaxesTDCPunta;
        set
        {
            if (SetProperty(ref _termoFixoPeaxesTDCPunta, value) && !_isLoading)
            {
                ApplicationData.Current.LocalSettings.Values["termoFixoPeaxesTDCPunta"] = value.ToString();
            }
        }
    }

    private decimal _termoFixoPeaxesTDCVal;
    public decimal TermoFixoPeaxesTDCVal
    {
        get => _termoFixoPeaxesTDCVal;
        set
        {
            if (SetProperty(ref _termoFixoPeaxesTDCVal, value) && !_isLoading)
            {
                ApplicationData.Current.LocalSettings.Values["termoFixoPeaxesTDCVal"] = value.ToString();
            }
        }
    }

    private decimal _marxeComercializacion;
    public decimal MarxeComercializacion
    {
        get => _marxeComercializacion;
        set
        {
            if (SetProperty(ref _marxeComercializacion, value) && !_isLoading)
            {
                ApplicationData.Current.LocalSettings.Values["marxeComercializacion"] = value.ToString();
            }
        }
    }

    private decimal _descontoBonoSocial;
    public decimal DescontoBonoSocial
    {
        get => _descontoBonoSocial;
        set
        {
            if (SetProperty(ref _descontoBonoSocial, value) && !_isLoading)
            {
                ApplicationData.Current.LocalSettings.Values["descontoBonoSocial"] = value.ToString();
            }
        }
    }

    private decimal _limiteBonoSocial;
    public decimal LimiteBonoSocial
    {
        get => _limiteBonoSocial;
        set
        {
            if (SetProperty(ref _limiteBonoSocial, value) && !_isLoading)
            {
                ApplicationData.Current.LocalSettings.Values["limiteBonoSocial"] = value.ToString();
            }
        }
    }

    private decimal _impostoElectricidade;
    public decimal ImpostoElectricidade
    {
        get => _impostoElectricidade;
        set
        {
            if (SetProperty(ref _impostoElectricidade, value) && !_isLoading)
            {
                ApplicationData.Current.LocalSettings.Values["impostoElectricidade"] = value.ToString();
            }
        }
    }

    private decimal _alugueiroContador;
    public decimal AlugueiroContador
    {
        get => _alugueiroContador;
        set
        {
            if (SetProperty(ref _alugueiroContador, value) && !_isLoading)
            {
                ApplicationData.Current.LocalSettings.Values["alugueiroContador"] = value.ToString();
            }
        }
    }

    private decimal _ive;
    public decimal Ive
    {
        get => _ive;
        set
        {
            if (SetProperty(ref _ive, value) && !_isLoading)
            {
                ApplicationData.Current.LocalSettings.Values["ive"] = value.ToString();
            }
        }
    }


    // -----------------------------
    // TOKEN: CARGA / GARDADO
    // -----------------------------

    private static string ReadApiKeyFromVault()
    {
        try
        {
            var vault = new PasswordVault();
            var list = vault.FindAllByResource("FacturaLuz-ESIOS");

            if (list?.Count > 0)
            {
                var cred = list[0];
                cred.RetrievePassword();
                return cred.Password;
            }
        }
        catch
        {
        }

        return null;
    }

    private static void SaveApiKeyToVault(string key)
    {
        try
        {
            var vault = new PasswordVault();

            // eliminar existentes
            try
            {
                var existing = vault.FindAllByResource("FacturaLuz-ESIOS");
                foreach (var c in existing)
                {
                    vault.Remove(c);
                }
            }
            catch { }

            // gardar só se hai valor
            if (!string.IsNullOrEmpty(key))
            {
                vault.Add(new PasswordCredential("FacturaLuz-ESIOS", "esios", key));
            }
        }
        catch { }
    }


    // -----------------------------
    // OUTROS
    // -----------------------------

    private ElementTheme _elementTheme;
    public ElementTheme ElementTheme
    {
        get => _elementTheme;
        set => SetProperty(ref _elementTheme, value);
    }

    private string _versionDescription;
    public string VersionDescription
    {
        get => _versionDescription;
        set => SetProperty(ref _versionDescription, value);
    }

    public ICommand SwitchThemeCommand => new RelayCommand<ElementTheme>(
        async (param) =>
        {
            if (ElementTheme != param)
            {
                ElementTheme = param;
                await _themeSelectorService.SetThemeAsync(param);
            }
        });

    private static decimal LerValorConfiguracionComoDecimal(string clave)
    {
        return decimal.TryParse(ApplicationData.Current.LocalSettings.Values[clave] as string, out decimal valor)
            ? valor
            : 0;
    }

    private static string LerValorConfiguracionComoCadea(string clave)
    {
        return ApplicationData.Current.LocalSettings.Values[clave] as string;
    }

    private static string GetVersionDescription()
    {
        var appName = "AppDisplayName".GetLocalized();
        var version = Package.Current.Id.Version;
        return $"{appName} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}
