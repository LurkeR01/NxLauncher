using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NxLauncher.Data;
using NxLauncher.Database.Repositories;
using NxLauncher.Factories;
using NxLauncher.Models;
using NxLauncher.Services;

namespace NxLauncher.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly PageFactory _pageFactory;
    private readonly IGameRepository _gameRepository;
    private readonly IDeveloperRepository _developerRepository;
    private readonly IGenreRepository _genreRepository;
    private readonly AuthenticationService _authService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IScreenshotRepository _screenshotRepository;
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly IWishlistRepository _wishlistRepository;
    private readonly ICartRepository _cartRepository;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShopPageIsActive))]
    [NotifyPropertyChangedFor(nameof(LibraryPageIsActive))]
    [NotifyPropertyChangedFor(nameof(CartPageIsActive))]
    [NotifyPropertyChangedFor(nameof(WishListPageIsActive))]
    private PageViewModel _currentPage;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOverlayVisible))]
    [NotifyPropertyChangedFor(nameof(IsMainContentInteractive))] 
    private bool _showOverlay;
    
    public bool IsOverlayVisible => ShowOverlay && OverlayViewModel != null;
    public bool IsMainContentInteractive => !IsOverlayVisible;
    
    [ObservableProperty]
    private string? _searchQuery;

    [ObservableProperty]
    private ViewModelBase? _detailPageViewModel;
    
    [ObservableProperty]
    private bool _isDetailViewVisible;
    
    [ObservableProperty]
    private bool _isGoBackButtonVisible;
    
    [ObservableProperty]
    private User? _currentUser;

    [ObservableProperty]
    private bool _isAdmin;

    [ObservableProperty]
    private bool _isLoggedIn;
    
    [ObservableProperty]
    private string _profileButtonText = "Войти";
    
    [ObservableProperty]
    private ViewModelBase? _overlayViewModel;
    
    
    public bool ShopPageIsActive => CurrentPage.PageName == ApplicationPageNames.Shop;
    public bool LibraryPageIsActive => CurrentPage.PageName == ApplicationPageNames.Library;
    public bool CartPageIsActive => CurrentPage.PageName == ApplicationPageNames.Cart;
    public bool WishListPageIsActive => CurrentPage.PageName == ApplicationPageNames.WishList;
    
    public IRelayCommand GoBackCommand { get; }
    public IRelayCommand LoginCommand { get; }
    public IRelayCommand RegisterCommand { get; }
    public IRelayCommand LogoutCommand { get; }
    public IRelayCommand ShowAddGameOverlayCommand  { get; }
    
    public MainViewModel(PageFactory pageFactory, 
        IGameRepository gameRepository, 
        IDeveloperRepository developerRepository, 
        IGenreRepository genreRepository, 
        AuthenticationService authService,
        IServiceProvider serviceProvider,
        IScreenshotRepository screenshotRepository,
        IPurchaseRepository purchaseRepository,
        IWishlistRepository wishlistRepository,
        ICartRepository cartRepository)
    {
        _pageFactory = pageFactory;
        _gameRepository = gameRepository; 
        _developerRepository = developerRepository;
        _genreRepository = genreRepository;
        _authService = authService; 
        _serviceProvider = serviceProvider;
        _screenshotRepository = screenshotRepository;
        _purchaseRepository = purchaseRepository;
        _wishlistRepository = wishlistRepository;
        _cartRepository = cartRepository;
        
        _authService.PropertyChanged += AuthService_PropertyChanged;
        
        GoBackCommand = new RelayCommand(NavigateBack);
        
        LoginCommand = new RelayCommand(ExecuteLogin, () => !IsLoggedIn);     
        RegisterCommand = new RelayCommand(ExecuteRegister, () => !IsLoggedIn);
        LogoutCommand = new RelayCommand(ExecuteLogout, () => IsLoggedIn);
        ShowAddGameOverlayCommand = new RelayCommand(ExecuteShowAddGameOverlay);
    }
    
    private void AuthService_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AuthenticationService.CurrentUser))
        {
            UpdateAuthState();
            LoginCommand.NotifyCanExecuteChanged();
            RegisterCommand.NotifyCanExecuteChanged();
            LogoutCommand.NotifyCanExecuteChanged();
            ShowAddGameOverlayCommand.NotifyCanExecuteChanged();
        }
    }

    private void UpdateAuthState()
    {
        CurrentUser = _authService.CurrentUser;
        IsLoggedIn = _authService.IsLoggedIn;
        IsAdmin = _authService.IsAdmin;
        ProfileButtonText = IsLoggedIn ? CurrentUser?.UserName ?? "Профиль" : "Войти";
        
        OnPropertyChanged(nameof(CurrentUser));
        OnPropertyChanged(nameof(IsLoggedIn));
        OnPropertyChanged(nameof(IsAdmin));
        OnPropertyChanged(nameof(ProfileButtonText));
    }

    private void ExecuteLogin()
    {
        OverlayViewModel = new LoginViewModel(
            _serviceProvider.GetRequiredService<AuthenticationService>(),
            CloseOverlay
            );
        ShowOverlay = true;
        OnPropertyChanged(nameof(IsOverlayVisible));
        OnPropertyChanged(nameof(IsMainContentInteractive));
    }

    private void ExecuteRegister()
    {
        OverlayViewModel = new RegisterViewModel(
            _serviceProvider.GetRequiredService<IUserRepository>(),
            CloseOverlay,
            _authService
        );
        ShowOverlay = true;
        OnPropertyChanged(nameof(IsOverlayVisible));
        OnPropertyChanged(nameof(IsMainContentInteractive));
    }

    private void ExecuteLogout()
    {
        _authService.Logout();
        if (ShowOverlay) CloseOverlay();
    }
    
    private void ExecuteShowAddGameOverlay()
    {
        OverlayViewModel = new AddGameViewModel(
            _developerRepository, 
            CloseOverlay,
            _gameRepository,
            _genreRepository,
            _screenshotRepository
        );
        ShowOverlay = true;
        OnPropertyChanged(nameof(IsOverlayVisible));
        OnPropertyChanged(nameof(IsMainContentInteractive));
    }
    

    private void CloseOverlay()
    {
        OverlayViewModel = null;
        ShowOverlay = false;
        OnPropertyChanged(nameof(IsOverlayVisible));
        OnPropertyChanged(nameof(IsMainContentInteractive));
    }
    
    public void InitializeNavigation()
    {
        Console.WriteLine("MainViewModel InitializeNavigation called.");
        GoToShopPage(); 
        Console.WriteLine("MainViewModel InitializeNavigation: Initial page set.");
    }
    
    partial void OnSearchQueryChanged(string? value)
    {
        CurrentPage = _pageFactory.GetPageViewModel(ApplicationPageNames.Shop);
        if (CurrentPage is ShopPageViewModel shopPageVM)
        {
            shopPageVM.SearchQuery = value;
        }
    }

    [RelayCommand]
    private void GoToShopPage()
    {
        CurrentPage = _pageFactory.GetPageViewModel(ApplicationPageNames.Shop);
        IsDetailViewVisible = false;
    }

    [RelayCommand]
    public void GoToLibraryPage()
    {
        if (CurrentUser != null)
        {
            CurrentPage = _pageFactory.GetPageViewModel(ApplicationPageNames.Library);
            IsDetailViewVisible = false;
        }
        else
        {
            ExecuteLogin();
        }
    }

    [RelayCommand]
    private void GoToCartPage()
    {
        if (CurrentUser != null)
        {
            CurrentPage = _pageFactory.GetPageViewModel(ApplicationPageNames.Cart);
            IsDetailViewVisible = false;
        }
        else
        {
            ExecuteLogin();
        }
    }

    [RelayCommand]
    private void GoToWishListPage()
    {
        if (CurrentUser != null)
        {
            CurrentPage = _pageFactory.GetPageViewModel(ApplicationPageNames.WishList);
            IsDetailViewVisible = false;
        }
        else
        {
            ExecuteLogin();
        }
    }


    [RelayCommand]
    private void ShowGameDetails(int gameId)
    {
        DetailPageViewModel = new GamePageViewModel(gameId, 
            _gameRepository,
            this,
            _developerRepository, 
            _genreRepository,
            _screenshotRepository,
            _purchaseRepository,
            _wishlistRepository,
            _cartRepository);
        IsDetailViewVisible = true;
        IsGoBackButtonVisible = true;
    }

    [RelayCommand]
    private void CloseDetails()
    {
        DetailPageViewModel = null;
        IsDetailViewVisible = false;
        IsGoBackButtonVisible = false;
    }
    
    
    private void NavigateBack()
    {
        CloseDetails();
    }
    
    partial void OnShowOverlayChanged(bool value)
    {
        OnPropertyChanged(nameof(IsOverlayVisible));
        OnPropertyChanged(nameof(IsMainContentInteractive));
    }
}