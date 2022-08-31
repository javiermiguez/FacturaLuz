using System;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using FacturaLuz.Contracts.Services;
using FacturaLuz.Helpers;

using Microsoft.UI.Xaml;

using Windows.ApplicationModel;
using Windows.Storage;

namespace FacturaLuz.ViewModels;

public class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;
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

    private string _urlMedidor;

    public string UrlMedidor
    {
        get => _urlMedidor;
        set
        {
            if (SetProperty(ref _urlMedidor, value))
            {
                ApplicationData.Current.LocalSettings.Values["urlMedidor"] = value;
            }
        }
    }

    private decimal _potenciaContratada;

    public decimal PotenciaContratada
    {
        get => _potenciaContratada;
        set
        {
            if (SetProperty(ref _potenciaContratada, value))
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
            if (SetProperty(ref _termoFixoPeaxesTDCPunta, value))
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
            if (SetProperty(ref _termoFixoPeaxesTDCVal, value))
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
            if (SetProperty(ref _marxeComercializacion, value))
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
            if (SetProperty(ref _descontoBonoSocial, value))
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
            if (SetProperty(ref _limiteBonoSocial, value))
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
            if (SetProperty(ref _impostoElectricidade, value))
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
            if (SetProperty(ref _alugueiroContador, value))
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
            if (SetProperty(ref _ive, value))
            {
                ApplicationData.Current.LocalSettings.Values["ive"] = value.ToString();
            }
        }
    }

    private ICommand _switchThemeCommand;

    public ICommand SwitchThemeCommand
    {
        get
        {
            if (_switchThemeCommand == null)
            {
                _switchThemeCommand = new RelayCommand<ElementTheme>(
                    async (param) =>
                    {
                        if (ElementTheme != param)
                        {
                            ElementTheme = param;
                            await _themeSelectorService.SetThemeAsync(param);
                        }
                    });
            }

            return _switchThemeCommand;
        }
    }

    public SettingsViewModel(IThemeSelectorService themeSelectorService)
    {
        _themeSelectorService = themeSelectorService;
        _elementTheme = _themeSelectorService.Theme;
        VersionDescription = GetVersionDescription();

        UrlMedidor = LerValorConfiguracionComoCadea("urlMedidor");
        PotenciaContratada = LerValorConfiguracionComoDecimal("potenciaContratada");
        TermoFixoPeaxesTDCPunta = LerValorConfiguracionComoDecimal("termoFixoPeaxesTDCPunta");
        TermoFixoPeaxesTDCVal = LerValorConfiguracionComoDecimal("termoFixoPeaxesTDCVal");
        MarxeComercializacion = LerValorConfiguracionComoDecimal("marxeComercializacion");
        DescontoBonoSocial = LerValorConfiguracionComoDecimal("descontoBonoSocial");
        LimiteBonoSocial = LerValorConfiguracionComoDecimal("limiteBonoSocial");
        ImpostoElectricidade = LerValorConfiguracionComoDecimal("impostoElectricidade");
        AlugueiroContador = LerValorConfiguracionComoDecimal("alugueiroContador");
        Ive = LerValorConfiguracionComoDecimal("ive");
    }

    private static decimal LerValorConfiguracionComoDecimal(string clave)
    {
        return decimal.TryParse((string)ApplicationData.Current.LocalSettings.Values[clave], out decimal valor) ? valor : 0;
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