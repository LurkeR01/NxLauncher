using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace NxLauncher.Controls.Converters
{
    public class BoolToFavoriteStringConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isFavorite)
            {
                return isFavorite ? "Убрать из избранного" : "Добавить в избранное";
            }
            return string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}