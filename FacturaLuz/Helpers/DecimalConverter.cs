using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

namespace FacturaLuz.Helpers;
public class DecimalConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        //Para amosar o valor na caixa de texto, convírtese a cadea e cámbianse os puntos por comas.
        return value.ToString().Replace(".", ",");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        //Para pasar o valor á propiedade correspondente do ViewModel (x:Bind ViewModel.[Propiedade]), convírtese a cadea, cámbianselle as comas por puntos e convírtese a decimal.
        return decimal.Parse(value.ToString().Replace(",", "."), System.Globalization.NumberStyles.AllowDecimalPoint, new CultureInfo("en-US"));
    }
}