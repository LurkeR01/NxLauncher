using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;

namespace NxLauncher.Controls;

public class WishlistGameCard : TemplatedControl
{
    public WishlistGameCard()
    {
        Console.WriteLine("WishlistGameCard created");
        this.AttachedToVisualTree += (sender, args) => 
        {
            Console.WriteLine($"WishlistGameCard attached to visual tree. Icon: {Icon}, Title: {Title}");
        };
    }
    
    public static readonly StyledProperty<string> IconProperty = AvaloniaProperty.Register<WishlistGameCard, string>(nameof(Icon));
    public string Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<WishlistGameCard, string>(nameof(Title));
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    
    public static readonly StyledProperty<string> PriceProperty = AvaloniaProperty.Register<WishlistGameCard, string>(nameof(Price));
    public string Price
    {
        get => GetValue(PriceProperty);
        set => SetValue(PriceProperty, value);
    }

    public static readonly StyledProperty<string> AgeRatingProperty = AvaloniaProperty.Register<WishlistGameCard, string>(nameof(AgeRating));
    public string AgeRating
    {
        get => GetValue(AgeRatingProperty);
        set => SetValue(AgeRatingProperty, value);
    }
    
    public static readonly StyledProperty<string> AgeRatingDescriptionProperty = AvaloniaProperty.Register<WishlistGameCard, string>(nameof(AgeRatingDescription), String.Empty);
    public string AgeRatingDescription
    {
        get => GetValue(AgeRatingDescriptionProperty);
        set => SetValue(AgeRatingDescriptionProperty, value);
    }
    
    public static readonly StyledProperty<IRelayCommand> AgeRatingCommandProperty = AvaloniaProperty.Register<WishlistGameCard, IRelayCommand>(nameof(AgeRatingCommand));
    public IRelayCommand AgeRatingCommand
    {
        get => GetValue(AgeRatingCommandProperty);
        set => SetValue(AgeRatingCommandProperty, value);
    }
    
    public static readonly StyledProperty<IRelayCommand> SelectGameCommandProperty = AvaloniaProperty.Register<WishlistGameCard, IRelayCommand>(nameof(SelectGameCommand));
    public IRelayCommand SelectGameCommand
    {
        get => GetValue(SelectGameCommandProperty);
        set => SetValue(SelectGameCommandProperty, value);
    }
    
    public static readonly StyledProperty<int> SelectGameIdProperty = AvaloniaProperty.Register<WishlistGameCard, int>(nameof(SelectGameId));
    public int SelectGameId
    {
        get => GetValue(SelectGameIdProperty);
        set => SetValue(SelectGameIdProperty, value);
    }
}