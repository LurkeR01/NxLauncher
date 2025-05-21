using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace NxLauncher.Controls.Converters;

public class EqualityToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Equals(value, parameter);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? parameter : BindingOperations.DoNothing;
    }
}