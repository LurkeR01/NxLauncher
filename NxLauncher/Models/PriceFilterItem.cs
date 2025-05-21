using CommunityToolkit.Mvvm.ComponentModel;

namespace NxLauncher.Models;

public partial class PriceFilterItem : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected;
    public int? Value { get; }
    public string DisplayText { get; }
    
    public PriceFilterItem(int? value, string displayText)
    {
        Value = value;
        DisplayText = displayText;
    }
}