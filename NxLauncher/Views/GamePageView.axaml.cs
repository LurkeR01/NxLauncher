using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using NxLauncher.ViewModels;

namespace NxLauncher.Views;

public partial class GamePageView : UserControl
{
    public GamePageView()
    {
        InitializeComponent();
    }
    
    private void OnScreenshotTextBoxKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is GamePageViewModel vm)
        {
            vm.AddScreenshotCommand.Execute(null);
        }
    }
    
    private void OnGenreTextBoxKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is GamePageViewModel vm)
        {
            vm.AddNewGenreCommand.Execute(null);
        }
    }
}