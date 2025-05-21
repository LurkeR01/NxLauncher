using CommunityToolkit.Mvvm.ComponentModel;
using NxLauncher.Data;

namespace NxLauncher.ViewModels;

public partial class PageViewModel : ViewModelBase
{
    [ObservableProperty]
    private ApplicationPageNames _pageName;
}