using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using MySql.Data.MySqlClient;
using NxLauncher.Database.Repositories;
using NxLauncher.Models;

namespace NxLauncher.ViewModels;

public partial class GamePageViewModel : ViewModelBase
{
    private readonly  IGameRepository _gameRepository;
    private readonly MainViewModel _mainViewModel;
    private readonly IDeveloperRepository _developerRepository;
    private readonly IGenreRepository _genreRepository;
    private readonly IScreenshotRepository _screenshotRepository;
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly IWishlistRepository _wishlistRepository;
    private readonly ICartRepository _cartRepository;
    
    [ObservableProperty] private Game? _selectedGame;
    [ObservableProperty] private Screenshot? _selectedScreenshot; 
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private bool _isAddingScreenshot;
    [ObservableProperty] private string? _editedName;
    [ObservableProperty] private string? _editedPrice;
    [ObservableProperty] private string? _editedDescription;
    [ObservableProperty] private string? _editedAgeRating;
    [ObservableProperty] private Developer? _editedDeveloper;
    [ObservableProperty] private DateTimeOffset? _editedReleaseDate;
    [ObservableProperty] private string? _editedImageUrl;
    [ObservableProperty] private string? _editedScreenshotUrl;
    [ObservableProperty] private bool _isAddingNewGenre;
    [ObservableProperty] private string? _newGenreName;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isAdmin;
    [ObservableProperty] private bool _canPurchaseOrInteract;
    [ObservableProperty] private bool _isInWishlist;
    [ObservableProperty] private bool _isInCart;
    
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(CanPurchaseOrInteract))] 
    private bool _isPurchased;

    public string WishlistButtonText => IsInWishlist ? "В списке желаемого" : "В список желаемого";
    public string CartButtonText => IsInCart ? "Посмотреть в корзине" : "Добавить в корзину";
    
    public bool IsCurrentUserAdmin => _mainViewModel.IsAdmin;
    
    public IRelayCommand GoBackCommand { get; }
    public Developer SelectedGameDeveloper { get; set; }
    public ObservableCollection<Genre> GameGenres { get; set; } = new();
    public ObservableCollection<Genre> AllGenres { get; set; }
    public ObservableCollection<Developer> AllDevelopers { get; set; }
    public ObservableCollection<Screenshot> Screenshots { get; } = new();
    
    public List<string> AgeRatings { get; } = new List<string> { "3+", "7+", "12+", "16+", "18+" };
    
    public GamePageViewModel(int gameId, 
        IGameRepository gameRepository, 
        MainViewModel mainViewModel, 
        IDeveloperRepository developerRepository,
        IGenreRepository genreRepository,
        IScreenshotRepository screenshotRepository,
        IPurchaseRepository purchaseRepository,
        IWishlistRepository wishlistRepository,
        ICartRepository cartRepository)
    {
        _gameRepository = gameRepository;
        _mainViewModel = mainViewModel;
        _developerRepository = developerRepository;
        _genreRepository = genreRepository;
        _screenshotRepository = screenshotRepository;
        _purchaseRepository = purchaseRepository;
        _wishlistRepository = wishlistRepository;
        _cartRepository = cartRepository;
        
        _mainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
        IsAdmin = mainViewModel.IsAdmin;
        
        AllGenres = new ObservableCollection<Genre>(_genreRepository.GetAll());
        AllDevelopers = new ObservableCollection<Developer>(_developerRepository.GetAll());
        LoadGameDetails(gameId);

        SelectedScreenshot = Screenshots.FirstOrDefault();

        GoBackCommand = new RelayCommand(NavigateBack);
    }
    
    private void MainViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.CurrentUser) || e.PropertyName == nameof(MainViewModel.IsAdmin))
        {
            CheckPurchaseStatus(); 
            CheckWishlistStatus();
            CheckCartStatus();
            PurchaseGameCommand.NotifyCanExecuteChanged(); 
            OnPropertyChanged(nameof(CanPurchaseOrInteract)); 
            OnPropertyChanged(nameof(CanExecuteWishlistOrCart));
        }
    }
    
    private void CheckPurchaseStatus()
    {
        if (_mainViewModel.IsLoggedIn && SelectedGame != null && _mainViewModel.CurrentUser != null)
        {
            try
            {
                IsPurchased = _purchaseRepository.IsPurchased(SelectedGame, _mainViewModel.CurrentUser);
                Console.WriteLine($"Game {SelectedGame.Id} purchased status for user {_mainViewModel.CurrentUser.UserId}: {IsPurchased}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking purchase status: {ex.Message}");
                IsPurchased = false; 
            }
        }
        else
        {
            IsPurchased = false; 
        }
        UpdateCanPurchaseOrInteract();
    }
    
    private void UpdateCanPurchaseOrInteract()
    {
        CanPurchaseOrInteract = _mainViewModel.IsLoggedIn && !IsCurrentUserAdmin && !IsPurchased;
    }
    
    partial void OnIsPurchasedChanged(bool value)
    {
        UpdateCanPurchaseOrInteract(); 
        PurchaseGameCommand.NotifyCanExecuteChanged(); 
    }

    private void LoadGameDetails(int gameId)
    {
        SelectedGame = _gameRepository.GetById(gameId);
        if (SelectedGame != null)
        {
            SelectedGameDeveloper = _developerRepository.GetById(SelectedGame.DeveloperId);
            
            AllGenres.Clear();
            foreach (var genre in _genreRepository.GetAll()) AllGenres.Add(genre);

            AllDevelopers.Clear();
            foreach (var dev in _developerRepository.GetAll()) AllDevelopers.Add(dev);

            GameGenres.Clear();
            var genres = _genreRepository.GetAllGameGenres(gameId);
            foreach(var genre in genres) GameGenres.Add(genre);

            Screenshots.Clear();
            var screenshots = _screenshotRepository.GetAllGameScreenshots(gameId);
            foreach(var shot in screenshots) Screenshots.Add(shot);
            
            SelectedScreenshot = Screenshots.FirstOrDefault();
            CheckPurchaseStatus();
            CheckWishlistStatus();
            CheckCartStatus();
        }
        else
        {
            GameGenres.Clear();
            Screenshots.Clear();
            IsPurchased = false;
            UpdateCanPurchaseOrInteract();
            SelectedScreenshot = null;
        }
    }
    
    private void CheckWishlistStatus()
    {
        if (_mainViewModel.IsLoggedIn && SelectedGame != null && _mainViewModel.CurrentUser != null)
        {
            IsInWishlist = _wishlistRepository.IsInWishlist(SelectedGame, _mainViewModel.CurrentUser);
        }
        else
        {
            IsInWishlist = false;
        }
    }

    private void CheckCartStatus()
    {
        if (_mainViewModel.IsLoggedIn && SelectedGame != null && _mainViewModel.CurrentUser != null)
        {
            IsInCart = _cartRepository.IsInCart(SelectedGame, _mainViewModel.CurrentUser);
        }
        else
        {
            IsInCart = false;
        }
    }
    
    private bool CanExecutePurchaseGame()
    {
        return _mainViewModel.IsLoggedIn && !IsCurrentUserAdmin && SelectedGame != null && !IsPurchased;
    }

    [RelayCommand(CanExecute = nameof(CanExecutePurchaseGame))]
    private void PurchaseGame()
    {
        if (!CanExecutePurchaseGame() || SelectedGame == null || _mainViewModel.CurrentUser == null) return;

        if (_mainViewModel.CurrentUser.Age < Int32.Parse(SelectedGame.AgeRating.Remove(SelectedGame.AgeRating.Length-1)))
        {
            ErrorMessage = "Ваш возраст ниже возрастного ограничения игры";
            return;
        }
        
        Console.WriteLine($"Attempting purchase of game {SelectedGame.Id} for user {_mainViewModel.CurrentUser.UserId}");
        try
        {
            _purchaseRepository.PurchaseGame(SelectedGame, _mainViewModel.CurrentUser);
            IsPurchased = true;
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Error purchasing game: {ex.Message}");
        }
    }
    
    [RelayCommand(CanExecute = nameof(CanMoveNext))]
    private void NextScreenshot()
    {
        if (SelectedScreenshot == null || Screenshots.Count < 2) return;

        int index = Screenshots.IndexOf(SelectedScreenshot);
        if (index < Screenshots.Count - 1)
        {
            SelectedScreenshot = Screenshots[index + 1];
        }
    }

    [RelayCommand(CanExecute = nameof(CanMovePrevious))]
    private void PreviousScreenshot()
    {
        if (SelectedScreenshot == null || Screenshots.Count < 2) return;

        int index = Screenshots.IndexOf(SelectedScreenshot);
        if (index > 0)
        {
            SelectedScreenshot = Screenshots[index - 1];
        }
    }

    [RelayCommand]
    public void GoToLibrary()
    {
        _mainViewModel.GoToLibraryPageCommand.Execute(null);
    }
    
    [RelayCommand(CanExecute = nameof(CanExecuteWishlistOrCart))]
    private void AddToWishlist()
    {
        if (!CanExecuteWishlistOrCart() || SelectedGame == null || _mainViewModel.CurrentUser == null) return;

        if (IsInWishlist)
        {
            _mainViewModel.GoToWishListPageCommand.Execute(null);
        }
        else
        {
            if (IsPurchased)
            {
                ErrorMessage = "Игра добавлена в список желаемого";
                return;
            }

            try
            {
                _wishlistRepository.AddGame(SelectedGame, _mainViewModel.CurrentUser);
                IsInWishlist = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteWishlistOrCart))]
    private void AddToCart()
    {
        if (!CanExecuteWishlistOrCart() || SelectedGame == null || _mainViewModel.CurrentUser == null) return;

        if (IsInCart)
        {
            _mainViewModel.GoToCartPageCommand.Execute(null);
        }
        else
        {
            if (IsPurchased)
            {
                ErrorMessage = "Игра уже куплена";
                return;
            }

            try
            {
                _cartRepository.AddGame(SelectedGame, _mainViewModel.CurrentUser);
                IsInCart = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }

    private bool CanExecuteWishlistOrCart() => 
        _mainViewModel.IsLoggedIn && !IsCurrentUserAdmin && SelectedGame != null;

    partial void OnIsInWishlistChanged(bool value) => 
        OnPropertyChanged(nameof(WishlistButtonText));
    
    partial void OnIsInCartChanged(bool value) =>
        OnPropertyChanged(nameof(CartButtonText));
    
    private bool CanMoveNext() =>
        SelectedScreenshot != null && Screenshots.IndexOf(SelectedScreenshot) < Screenshots.Count - 1;

    private bool CanMovePrevious() =>
        SelectedScreenshot != null && Screenshots.IndexOf(SelectedScreenshot) > 0;
    
    
    private void NavigateBack()
    {
         _mainViewModel.CloseDetailsCommand.Execute(null);
    }
    
    partial void OnSelectedScreenshotChanged(Screenshot? value)
    {
        NextScreenshotCommand.NotifyCanExecuteChanged();
        PreviousScreenshotCommand.NotifyCanExecuteChanged();
    }
    
    public void Dispose()
    {
        _mainViewModel.PropertyChanged -= MainViewModel_PropertyChanged;
    }
    
    // --- Admin Panel ---
    
    [RelayCommand]
    public void ToggleEditMode() => IsEditing = !IsEditing;

    [RelayCommand]
    public void ToggleAddingScreenshot() => IsAddingScreenshot = !IsAddingScreenshot;
    
    [RelayCommand]
    private void ToggleAddGenre() => IsAddingNewGenre = !IsAddingNewGenre;
    
    partial void OnIsEditingChanged(bool value)
    {
        if (value)
        {
            EditedName = SelectedGame.Name;
            EditedDescription = SelectedGame.Description;
            EditedAgeRating = SelectedGame.AgeRating;
            EditedPrice = SelectedGame.Price.ToString("F2");
            EditedImageUrl = SelectedGame.ImageUrl;
            
            AllDevelopers.Clear();
            foreach (var dev in _developerRepository.GetAll()) AllDevelopers.Add(dev);
            
            AllGenres.Clear();
            foreach (var genre in _genreRepository.GetAll()) 
            {
                var editGenre = new Genre 
                { 
                    Id = genre.Id,
                    Name = genre.Name,
                    IsSelected = GameGenres.Any(g => g.Id == genre.Id)
                };
                AllGenres.Add(editGenre);
            }
            
            EditedDeveloper = AllDevelopers.FirstOrDefault(d => d.DeveloperId == SelectedGame?.DeveloperId);
        }
    }
    
    [RelayCommand]
    public void SaveChanges()
    {
        SelectedGame.Name = EditedName;
        SelectedGame.Description = EditedDescription;
        SelectedGame.AgeRating = EditedAgeRating;
        SelectedGame.Price = Decimal.Parse(EditedPrice);
        SelectedGame.DeveloperId = EditedDeveloper.DeveloperId;
        SelectedGame.ImageUrl = EditedImageUrl;
        
        var selectedGenreIds = AllGenres.Where(g => g.IsSelected).Select(g => g.Id).ToList();
        _gameRepository.UpdateGameGenres(SelectedGame.Id, selectedGenreIds);
    
        _gameRepository.Update(SelectedGame);
        
        LoadGameDetails(SelectedGame.Id);
        IsEditing = false;
    }

    [RelayCommand]
    public void DeleteCurrentScreenshot()
    {
        if (SelectedScreenshot != null)
        {
            _screenshotRepository.DeleteScreenshot(SelectedScreenshot.Id);
            Screenshots.Remove(SelectedScreenshot);
            SelectedScreenshot = Screenshots.FirstOrDefault();
        }
    }
    
    [RelayCommand]
    public void AddScreenshot()
    {
        var newScreenshot = new Screenshot { ImageUrl = EditedScreenshotUrl };
        _screenshotRepository.AddScreenshot(newScreenshot, SelectedGame);
        Screenshots.Add(newScreenshot);
        SelectedScreenshot = Screenshots.FirstOrDefault();
        EditedScreenshotUrl = string.Empty;
    }

    [RelayCommand]
    public async void DeleteGame()
    {
        var box = MessageBoxManager.GetMessageBoxStandard("Подтверждение", "Вы уверены, что хотите удалить игру?",
            ButtonEnum.YesNo);
        var result = await box.ShowAsync();
    
        if (result == ButtonResult.Yes)
        {
            _gameRepository.DeleteGame(SelectedGame.Id);
            
            GameGenres.Clear();
            Screenshots.Clear();
        
            SelectedGame = null;
            SelectedScreenshot = null;
        
            NavigateBack();
        }
    }
    
    [RelayCommand]
    private void AddNewGenre()
    {
        if (!string.IsNullOrWhiteSpace(NewGenreName))
        {
            ErrorMessage = null;
            
            try
            {
                var newGenre = new Genre { Name = NewGenreName };
                _genreRepository.Add(newGenre);
                AllGenres.Add(newGenre);
                NewGenreName = string.Empty;
                IsAddingNewGenre = false;
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                ErrorMessage = $"Жанр '{NewGenreName}' уже существует!";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
            }
        }
    }
}