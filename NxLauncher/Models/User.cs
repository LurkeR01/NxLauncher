using CommunityToolkit.Mvvm.ComponentModel;

namespace NxLauncher.Models;

public enum UserRole
{
    User,
    Admin
}

public partial class User : ObservableObject
{
    [ObservableProperty]
    private int _userId;
    
    [ObservableProperty]
    private string _userName;

    [ObservableProperty] 
    private string _passwordHash;

    [ObservableProperty] 
    private int _age;
    
    [ObservableProperty]
    private UserRole _role;
    
    [ObservableProperty]
    private string _email;
}