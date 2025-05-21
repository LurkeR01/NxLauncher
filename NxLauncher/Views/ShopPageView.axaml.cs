using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using NxLauncher.Database.Repositories;
using NxLauncher.ViewModels;

namespace NxLauncher.Views;

public partial class ShopPageView : UserControl
{
    public ShopPageView()
    {
        InitializeComponent();
    }
    
    private void OnRefreshButtonKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5 && DataContext is ShopPageViewModel vm)
        {
            vm.RefreshCommand.Execute(null);
        }
    }
}