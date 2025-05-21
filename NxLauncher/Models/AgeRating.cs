using CommunityToolkit.Mvvm.ComponentModel;

namespace NxLauncher.Models;

public partial class AgeRating : ObservableObject
{
    [ObservableProperty] private bool _isSelected;
    
    public required string Name { get; set; }
}