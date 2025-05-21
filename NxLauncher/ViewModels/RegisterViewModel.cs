using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NxLauncher.Database.Repositories;
using NxLauncher.Models;
using NxLauncher.Services;

namespace NxLauncher.ViewModels;

public partial class RegisterViewModel : ViewModelBase
{
    private readonly IUserRepository _userRepository;
    private readonly Action _closeAction;
    private readonly AuthenticationService _authService;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    private string? _username;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    private string? _email;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    private string? _age;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    private string? _password;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    private string? _confirmPassword;

    [ObservableProperty]
    private string? _errorMessage;

    public RegisterViewModel(IUserRepository userRepository, Action closeAction, AuthenticationService authservice)
    {
        _userRepository = userRepository;
        _closeAction = closeAction;
        _authService = authservice;
    }
    
    private bool CanRegister()
    {
        if (string.IsNullOrWhiteSpace(Username) ||
            string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Password) ||
            string.IsNullOrWhiteSpace(Age))
        {
            return false;
        }

        if (!int.TryParse(Age, out int age))
        {
            return false;
        }

        return age > 0;
    }

    [RelayCommand(CanExecute = nameof(CanRegister))]
    private void Register()
    {
        ErrorMessage = null;

        if (_userRepository.GetByUserName(_username) != null)
        {
            ErrorMessage = "Пользователь с таким именем уже существует.";
            return;
        }
        
        string passwordHash;
        try
        {
            passwordHash = BCrypt.Net.BCrypt.HashPassword(Password);
        }
        catch (Exception ex)
        {
            ErrorMessage = "Ошибка хэширования пароля.";
            Console.WriteLine($"Password Hashing Error: {ex}");
            return;
        }

        var newUser = new User
        {
            UserName = Username!,
            Email = Email!,
            Age = int.Parse(Age),
            PasswordHash = passwordHash,
            Role = UserRole.User
        };

        try
        {
            _userRepository.AddUser(newUser);
            _closeAction.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка регистрации: {ex.Message}";
            Console.WriteLine($"Registration Error: {ex}");
        }
        
        _authService.Login(Username, Password);
    }
    
    [RelayCommand]
    private void Cancel()
    {
        _closeAction?.Invoke();
    }
}