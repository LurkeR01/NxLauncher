using System;
using Avalonia.Controls.Templates;
using NxLauncher.ViewModels;
using Avalonia.Controls;
using NxLauncher.Views;
using NxLauncher.ViewModels;

namespace NxLauncher;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data == null) return null;
        
        var viewName = data.GetType().FullName!.Replace("ViewModel", "View", StringComparison.InvariantCulture);
        var viewType = Type.GetType(viewName);
        
        if (viewType == null) return null;
        
        var control = (Control)Activator.CreateInstance(viewType)!;
        control.DataContext = data;
        return control;
    }
    
    public bool Match(object data) => data is ViewModelBase;
}