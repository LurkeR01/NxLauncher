using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media.Imaging;

namespace NxLauncher.Controls;

public class GameCard : TemplatedControl
{
    public static readonly StyledProperty<string> IconProperty = AvaloniaProperty.Register<GameCard, string>(nameof(Icon));
    
    public string Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<GameCard, string>(nameof(Title));
    
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    
    public static readonly StyledProperty<string> PriceProperty = AvaloniaProperty.Register<GameCard, string>(nameof(Price));
    
    public string Price
    {
        get => GetValue(PriceProperty);
        set => SetValue(PriceProperty, value);
    }
}