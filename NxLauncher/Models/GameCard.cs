using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NxLauncher.Models;

public partial class GameCard : ObservableObject
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string IconUrl { get; set; }
    public decimal Price { get; set; }
    public DateTime ReleaseDate { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string AgeRating { get; set; }
    public DateTime AddedDate { get; set; }

    [ObservableProperty] 
    private bool _isFavorite;
}