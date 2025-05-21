// Файл: NxLauncher/Controls/Converters/AgeRatingToImagePathConverter.cs
using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging; // Для Bitmap
using Avalonia.Platform; // Для AssetLoader

namespace NxLauncher.Controls.Converters
{
    public class AgeRatingToImagePathConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string ageRatingString)
            {
                string imageName;
                switch (ageRatingString)
                {
                    case "3+":
                        imageName = "3+.png";
                        break;
                    case "7+":
                        imageName = "7+.png";
                        break;
                    case "12+":
                        imageName = "12+.png";
                        break;
                    case "16+":
                        imageName = "16+.png"; 
                        break;
                    case "18+":
                        imageName = "18+.png";
                        break;
                    default:
                        return null;
                }

                try
                {
                    var uri = new Uri($"avares://NxLauncher/Assets/Images/AgeRatings/{imageName}", UriKind.Absolute);
                    return new Bitmap(AssetLoader.Open(uri));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading age rating image '{imageName}': {ex.Message}");
                    return null;
                }
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}