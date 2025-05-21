using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NxLauncher.Models;

namespace NxLauncher.ViewModels;

public partial class GameCardItemViewModel : ViewModelBase
{
    public GameCard GameCard { get; }
    public IRelayCommand<int> SelectGameCommand { get; } 

    public GameCardItemViewModel(GameCard gameCard, IRelayCommand<int> selectGameCommand)
    {
        GameCard = gameCard ?? throw new ArgumentNullException(nameof(gameCard));
        SelectGameCommand = selectGameCommand ?? throw new ArgumentNullException(nameof(selectGameCommand));
    }

    public int Id => GameCard.Id;
    public string Title => GameCard.Title;
    public string IconUrl => GameCard.IconUrl;
    public decimal Price => GameCard.Price;
    public DateTime PurchaseDate => GameCard.PurchaseDate;
    public bool IsFavorite => GameCard.IsFavorite;
    public string AgeRating => GameCard.AgeRating;
    public string AgeRatingDescription => GetAgeRatingDescription(GameCard.AgeRating);
    
    [ObservableProperty]
    public bool _isInCart;
    public string ButtonText => IsInCart ? "Посмотреть в корзине" : "Добавить в корзину";

    partial void OnIsInCartChanged(bool value) => OnPropertyChanged(nameof(ButtonText));

    private string GetAgeRatingDescription(string ageRating)
    {
        return ageRating switch
        {
            "3+" => "Для всех возрастов",
            "7+" => "Для детей от 7 лет",
            "12+" => "Для подростков от 12 лет",
            "16+" => "Для лиц старше 16 лет",
            "18+" => "Для взрослых",
            _ => "Возрастное ограничение"
        };
    }
}