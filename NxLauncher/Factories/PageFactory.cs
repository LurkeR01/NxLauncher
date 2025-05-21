using System;
using NxLauncher.Data;
using NxLauncher.ViewModels;

namespace NxLauncher.Factories;

public class PageFactory(Func<ApplicationPageNames, PageViewModel> factory)
{
    public PageViewModel GetPageViewModel(ApplicationPageNames pageName) => factory.Invoke(pageName);

}