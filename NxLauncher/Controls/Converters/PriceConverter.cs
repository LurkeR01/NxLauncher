using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace NxLauncher.Controls.Converters;

public class PriceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal price)
        {
            return price == 0 ? "Бесплатно" : $"{price} грн.";
        }
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}