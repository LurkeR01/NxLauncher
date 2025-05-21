using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace NxLauncher.Controls.Converters;

public class PriceToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int price)
        {
            return price switch
            {
                -1 => "Без ограничей",
                0 => "Бесплатно",
                < 900 => $"до {price}",
                900 => "до 900",
                > 900 => "Выше 900"
            };
        }
        return "Неизвестно";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}