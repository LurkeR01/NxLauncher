using System;
using CommunityToolkit.Mvvm.ComponentModel;
using NxLauncher.Database.Repositories;
using NxLauncher.Models;

namespace NxLauncher.Services;

public partial class AuthenticationService : ObservableObject
{
    private readonly IUserRepository _userRepository;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoggedIn))]
    [NotifyPropertyChangedFor(nameof(IsAdmin))]
    private User? _currentUser;
    
    public bool IsLoggedIn => CurrentUser != null;
    public bool IsAdmin => CurrentUser?.Role == UserRole.Admin;

    public AuthenticationService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public bool Login(string username, string password)
    {
        var user = _userRepository.GetByUserName(username);
        if (user == null)
        {
            Console.WriteLine($"Пользователь '{username}' не найден.");
            return false;
        }
        
        if(!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
           Console.WriteLine($"Неверный пароль для пользователя '{username}'.");
           return false;
        }
        
        CurrentUser = user;
        Console.WriteLine($"Пользователь '{username}' успешно вошел. Роль: {CurrentUser.Role}");
        return true; 
    }
    
    public void Logout()
    {
        Console.WriteLine($"Пользователь '{CurrentUser?.UserName}' вышел.");
        CurrentUser = null;
    }
}