using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NxLauncher.Models;

public partial class Genre : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected;
    
    public int Id { get; set; }
    public required string Name { get; set; }
}