using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using NxLauncher.ViewModels;

namespace NxLauncher.Views;

public partial class AddGameView : UserControl
{
    public AddGameView()
    {
        InitializeComponent();
    }
    
    private void OnGenreTextBoxKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is AddGameViewModel vm)
        {
            vm.AddNewGenreCommand.Execute(null);
        }
    }
    
    private void OnDeveloperTextBoxKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is AddGameViewModel vm)
        {
            vm.AddNewDeveloperCommand.Execute(null);
        }
    }
    
    private void OnScreenshotTextBoxKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is AddGameViewModel vm)
        {
            vm.AddScreenshotCommand.Execute(null);
        }
    }
}