using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NxLauncher.Data;
using NxLauncher.Database.Repositories;
using NxLauncher.Models;

namespace NxLauncher.ViewModels;

public enum LibraryFilterMode
{
    All,
    Favorites
}

public partial class LibraryPageViewModel : PageViewModel
{
    private readonly IGenreRepository _genreRepository;
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly MainViewModel _mainViewModel;
    private readonly IFavoritesRepository _favoritesRepository;
    
    [ObservableProperty] private SortOptionItem _selectedSortOption; 
    [ObservableProperty] private LibraryFilterMode _currentLibraryFilter = LibraryFilterMode.All; // Фильтр Все/Избранное
    [ObservableProperty] private string? _statusMessage;
    
    private Timer? _filterDebounceTimer;
    private readonly TimeSpan _debounceTime = TimeSpan.FromMilliseconds(300);
    
    public IRelayCommand<int> SelectGameCommand { get; }
    public IRelayCommand<int> ToggleFavoriteCommand { get; } 
    public IRelayCommand<LibraryFilterMode> SetFilterCommand { get; }
    public IRelayCommand<int> DeletePurchasedGameCommand { get; }
    
    public ObservableCollection<Genre> AvailableGenres { get; }
    public ObservableCollection<AgeRating> AvailableAgeRatings { get; set; }
    public ObservableCollection<SortOptionItem> SortOptions { get; }
    public ObservableCollection<GameCardItemViewModel> PurchasedGames { get; }
    public IEnumerable<int> FavoriteGameIds { get; set; } = Enumerable.Empty<int>();
    
    public LibraryPageViewModel(IGenreRepository genreRepository,
        IPurchaseRepository purchaseRepository,
        MainViewModel mainViewModel,
        IFavoritesRepository favoritesRepository)
    {
        PageName = ApplicationPageNames.Library;
        
        _genreRepository = genreRepository;
        _purchaseRepository = purchaseRepository;
        _mainViewModel = mainViewModel;
        _favoritesRepository = favoritesRepository;
        
        AvailableGenres = new ObservableCollection<Genre>(_genreRepository.GetAll());
        PurchasedGames = new ObservableCollection<GameCardItemViewModel>();

        SortOptions = new ObservableCollection<SortOptionItem>
        {
            new() { DisplayName = "Недавние покупки", Value = SortOption.RecentlyAdded },
            new() { DisplayName = "По алфавиту от A до Z", Value = SortOption.AlphabetAscending },
            new() { DisplayName = "По алфавиту от Z до A", Value = SortOption.AlphabetDescending }
        };
        SelectedSortOption = SortOptions[0];
        
        AvailableAgeRatings = new ObservableCollection<AgeRating>
        {
            new() { Name = "3+"},
            new() { Name = "7+"},
            new() { Name = "12+"},
            new() { Name = "16+"},
            new() { Name = "18+"},
        };
        foreach (var ar in AvailableAgeRatings) ar.PropertyChanged += FilterPropertyChanged;
        
        SelectGameCommand = new RelayCommand<int>(ExecuteSelectGame);
        ToggleFavoriteCommand = new RelayCommand<int>(ExecuteToggleFavorite);
        SetFilterCommand = new RelayCommand<LibraryFilterMode>(ExecuteSetFilter);
        DeletePurchasedGameCommand = new RelayCommand<int>(ExecuteDeletePurchasedGame);

        _mainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
        foreach (var genre in AvailableGenres)
        {
            genre.PropertyChanged += Genre_PropertyChanged;
        }
        
        _filterDebounceTimer = new Timer(FilterTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        LoadGameInfo();
    }
    
    private void FilterPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Genre.IsSelected) || e.PropertyName == nameof(AgeRating.IsSelected))
        {
            RequestFilterUpdate();
        }
    }

    private void LoadGameInfo()
    {
        if (_mainViewModel.CurrentUser == null)
        {
            PurchasedGames.Clear();
            StatusMessage = "Войдите в аккаунт для просмотра библиотеки.";
            ApplyFilters(); 
            return;
        }

        StatusMessage = "Загрузка библиотеки...";
        OnPropertyChanged(nameof(StatusMessage));

        try
        {
            FavoriteGameIds = _favoritesRepository.GetUserFavoriteGameIds(_mainViewModel.CurrentUser.UserId);

            var purchasedGameCards = _purchaseRepository.GetGameCardsByUserId(_mainViewModel.CurrentUser.UserId);

            PurchasedGames.Clear();

            foreach (var gameCard in purchasedGameCards)
            {
                if (gameCard != null)
                    PurchasedGames.Add(new GameCardItemViewModel(gameCard, SelectGameCommand));
            }

            StatusMessage = PurchasedGames.Any() ? null : "Библиотека пуста";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading library: {ex.Message}");
            StatusMessage = "Не удалось загрузить библиотеку.";
            PurchasedGames.Clear(); 
        }
        finally
        {
            ApplyFilters(); 
        }
    }

    private void ExecuteDeletePurchasedGame(int gameId)
    {
        if (_mainViewModel.CurrentUser == null) return;

        var gameVm = PurchasedGames.FirstOrDefault(g => g.Id == gameId);
        if (gameVm == null) return;
        try
        {
            _purchaseRepository.DeletePurchasedGame(_mainViewModel.CurrentUser.UserId, gameId);
            PurchasedGames.Remove(gameVm);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting pruchased game: {ex.Message}");
            StatusMessage = ex.Message;
        }
    }
    
    private void ExecuteSelectGame(int gameId)
    {
        if (_mainViewModel.ShowGameDetailsCommand.CanExecute(gameId))
        {
            _mainViewModel.ShowGameDetailsCommand.Execute(gameId);
        }
    }
    
    private void ExecuteToggleFavorite(int gameId)
    {
        if (_mainViewModel.CurrentUser == null) return;

        var gameVm = PurchasedGames.FirstOrDefault(g => g.Id == gameId);
        if (gameVm == null) return;
        
        var newFavoriteState = !gameVm.IsFavorite;
        try
        {
            if (newFavoriteState)
            {
                _favoritesRepository.AddFavoriteGame(_mainViewModel.CurrentUser.UserId, gameId);
            }
            else
            {
                _favoritesRepository.RemoveFavoriteGame(_mainViewModel.CurrentUser.UserId, gameId);
            }
            FavoriteGameIds = newFavoriteState 
                ? FavoriteGameIds.Append(gameId)
                : FavoriteGameIds.Where(id => id != gameId);
            
            gameVm.GameCard.IsFavorite = newFavoriteState;
            
            if (CurrentLibraryFilter == LibraryFilterMode.Favorites && !newFavoriteState)
            {
                PurchasedGames.Remove(gameVm);
            }
            else 
            {
                var index = PurchasedGames.IndexOf(gameVm);
                if (index >= 0)
                {
                    PurchasedGames[index] = gameVm;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error toggling favorite: {ex.Message}");
            StatusMessage = "Ошибка изменения статуса избранного";
            OnPropertyChanged(nameof(StatusMessage));
            gameVm.GameCard.IsFavorite = !gameVm.IsFavorite;
        }
    }
    
    private void ExecuteSetFilter(LibraryFilterMode filter)
    {
        CurrentLibraryFilter = filter;
    }
    
    partial void OnCurrentLibraryFilterChanged(LibraryFilterMode value)
    {
        RequestFilterUpdate();
    }
    
    private void MainViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.CurrentUser))
        {
            LoadGameInfo();
        }
    }
    
    private void Genre_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Genre.IsSelected))
        {
            RequestFilterUpdate();
        }
    }
    
    partial void OnSelectedSortOptionChanged(SortOptionItem? value)
    {
        RequestFilterUpdate();
    }
    
    private void RequestFilterUpdate()
    {
        _filterDebounceTimer?.Change(_debounceTime, Timeout.InfiniteTimeSpan);
    }
    
    private void FilterTimerCallback(object? state)
    {
        Dispatcher.UIThread.Post(() =>
        {
            ApplyFilters();
        });
    }
    
    private void ApplyFilters()
    {
        var selectedGenres = AvailableGenres.Where(g => g.IsSelected).ToList();
        var selectedAgeRatings = AvailableAgeRatings.Where(g => g.IsSelected).ToList();
        var options = new FilterOptions
        {
            Genres = selectedGenres,
            AgeRatings = selectedAgeRatings,
        };
            
        var filteredGames = _purchaseRepository.GetByFilter(options, _mainViewModel.CurrentUser).ToList();

        foreach (var game in filteredGames)
        {
            if (FavoriteGameIds.Contains(game.Id))
                game.IsFavorite = true;
        }
    
        switch (SelectedSortOption.Value)
        {
            case SortOption.RecentlyAdded:
                filteredGames = filteredGames.OrderByDescending(g => g.PurchaseDate).ToList();
                break;
            case SortOption.AlphabetAscending:
                filteredGames = filteredGames.OrderBy(g => g.Title).ToList();
                break;
            case SortOption.AlphabetDescending:
                filteredGames = filteredGames.OrderByDescending(g => g.Title).ToList();
                break;
        }
        
        var filteredByFavorite = CurrentLibraryFilter == LibraryFilterMode.Favorites
            ? filteredGames.Where(g => g.IsFavorite)
            : filteredGames;
            
        PurchasedGames.Clear();
        foreach (var game in filteredByFavorite)
        {
            PurchasedGames.Add(new GameCardItemViewModel(game, SelectGameCommand));
        }
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _filterDebounceTimer?.Dispose();
            _filterDebounceTimer = null;

            foreach (var genre in AvailableGenres)
            {
                genre.PropertyChanged -= Genre_PropertyChanged;
            }
        }
    }

    ~LibraryPageViewModel()
    {
        Dispose(false);
    }
}