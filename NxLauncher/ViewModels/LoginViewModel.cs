using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NxLauncher.Services;

namespace NxLauncher.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly AuthenticationService _authService;
    private readonly Action _closeAction;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string? _username;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string? _password;
    
    [ObservableProperty]
    private string? _errorMessage;

    public LoginViewModel(AuthenticationService authService, Action closeAction)
    {
        _authService = authService;
        _closeAction = closeAction;
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private void Login()
    {
        ErrorMessage = null;
        bool success = _authService.Login(Username ?? string.Empty, Password ?? string.Empty);

        if (success)
            _closeAction?.Invoke();
        else
            ErrorMessage = "Неверное имя пользователя или пароль.";
    }

    private bool CanLogin()
    {
        return !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
    }
    
    [RelayCommand]
    private void Cancel()
    {
        _closeAction?.Invoke(); 
    }
}