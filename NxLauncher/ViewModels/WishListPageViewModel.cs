using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NxLauncher.Data;
using NxLauncher.Database.Repositories;
using NxLauncher.Models;

namespace NxLauncher.ViewModels;

public partial class WishListPageViewModel : PageViewModel
{
    private readonly IGenreRepository _genreRepository;
    private readonly MainViewModel _mainViewModel;
    private readonly IWishlistRepository _wishlistRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IGameRepository _gameRepository;
    
    [ObservableProperty] private SortOptionItem _selectedSortOption;
    [ObservableProperty] private string _statusMessage;
    
    private Timer? _filterDebounceTimer;
    private readonly TimeSpan _debounceTime = TimeSpan.FromMilliseconds(300);
    
    public IRelayCommand<int> SelectGameCommand { get; }
    public IRelayCommand OpenRatingCommand { get; } = new RelayCommand(() =>
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://www.globalratings.com/ratingsguide.aspx#generic",
            UseShellExecute = true
        });
    });
    
    public ObservableCollection<Genre> AvailableGenres { get; }
    public ObservableCollection<AgeRating> AvailableAgeRatings { get; set; }
    public ObservableCollection<SortOptionItem> SortOptions { get; }
    public ObservableCollection<GameCardItemViewModel> WishlistGames { get; set; }
    
    public WishListPageViewModel(IGenreRepository genreRepository,
        MainViewModel mainViewModel,
        IWishlistRepository wishlistRepository,
        ICartRepository cartRepository,
        IGameRepository gameRepository)
    {
        PageName = ApplicationPageNames.WishList;
        
        _genreRepository = genreRepository;
        _mainViewModel = mainViewModel;
        _wishlistRepository = wishlistRepository;
        _cartRepository = cartRepository;
        _gameRepository = gameRepository;
        
        AvailableGenres = new ObservableCollection<Genre>(_genreRepository.GetAll());
        WishlistGames = new ObservableCollection<GameCardItemViewModel>();
        AvailableAgeRatings = new ObservableCollection<AgeRating>
        {
            new() { Name = "3+"},
            new() { Name = "7+"},
            new() { Name = "12+"},
            new() { Name = "16+"},
            new() { Name = "18+"},
        };
        foreach (var ar in AvailableAgeRatings) ar.PropertyChanged += FilterPropertyChanged;
        
        SortOptions = new ObservableCollection<SortOptionItem>
        {
            new() { DisplayName = "Недавно добавленные", Value = SortOption.RecentlyAdded },
            new() { DisplayName = "По алфавиту", Value = SortOption.Alphabetical },
            new() { DisplayName = "Цена: по возрастанию", Value = SortOption.PriceAscending},
            new() { DisplayName = "Цена: по убыванию", Value = SortOption.PriceDescending}
        };
        SelectedSortOption = SortOptions[0];
        
        SelectGameCommand = new RelayCommand<int>(ExecuteSelectGame);
        
        _mainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
        foreach (var genre in AvailableGenres)
        {
            genre.PropertyChanged += Genre_PropertyChanged;
        }
        
        _filterDebounceTimer = new Timer(FilterTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        LoadGameInfo();
    }

    private void LoadGameInfo()
    {
        if (_mainViewModel.CurrentUser == null)
        {
            WishlistGames.Clear();
            StatusMessage = "Войдите в аккаунт для просмотра списка.";
            return;
        }

        StatusMessage = "Загрузка...";
        OnPropertyChanged(nameof(StatusMessage));

        try
        {
            var addedGameCards = _wishlistRepository.GetAllGameCardsByUserId(_mainViewModel.CurrentUser.UserId);
            var gamesList = addedGameCards.ToList();
            
            if (gamesList.Count == 0)
            {
                StatusMessage = "Список пуст";
                return;
            }
            
            WishlistGames.Clear();

            foreach (var gameCard in gamesList)
            {
                if (gameCard != null)
                {
                    var isInCart = _cartRepository.IsInCart(_gameRepository.GetById(gameCard.Id), _mainViewModel.CurrentUser);
                    var viewModel = new GameCardItemViewModel(gameCard, SelectGameCommand)
                    {
                        IsInCart = isInCart
                    };
                    WishlistGames.Add(viewModel);
                }
            }

            StatusMessage = WishlistGames.Any() ? null : "Список пуст";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading wishlist: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            StatusMessage = "Не удалось загрузить список.";
            WishlistGames.Clear(); 
        }
    }
    
    private void MainViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.CurrentUser))
        {
            LoadGameInfo();
        }
    }
    
    private void FilterPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Genre.IsSelected) || e.PropertyName == nameof(AgeRating.IsSelected))
        {
            RequestFilterUpdate();
        }
    }
    
    private void Genre_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Genre.IsSelected))
        {
            RequestFilterUpdate();
        }
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
    
    private void ExecuteSelectGame(int gameId)
    {
        if (_mainViewModel.ShowGameDetailsCommand.CanExecute(gameId))
        {
            _mainViewModel.ShowGameDetailsCommand.Execute(gameId);
        }
    }
    
    private void ApplyFilters()
    {
        if (_mainViewModel.CurrentUser == null)
        {
            WishlistGames.Clear();
            WishlistGames.Clear();
            return;
        }
        
        var selectedGenres = AvailableGenres.Where(g => g.IsSelected).ToList();
        var selectedAgeRatings = AvailableAgeRatings.Where(g => g.IsSelected).ToList();

        var options = new FilterOptions
        {
            Genres = selectedGenres,
            AgeRatings = selectedAgeRatings,
        };
            
        var filteredGames = _wishlistRepository.GetByFilter(options, _mainViewModel.CurrentUser).ToList();

        switch (SelectedSortOption.Value)
        {
            case SortOption.RecentlyAdded:
                filteredGames = filteredGames.OrderByDescending(g => g.AddedDate).ToList();
                break;
            case SortOption.Alphabetical:
                filteredGames = filteredGames.OrderBy(g => g.Title).ToList();
                break;
            case SortOption.PriceDescending:
                filteredGames = filteredGames.OrderByDescending(g => g.Price).ToList();
                break;
            case SortOption.PriceAscending:
                filteredGames = filteredGames.OrderBy(g => g.Price).ToList();
                break;
        }
            
        WishlistGames.Clear();
        foreach (var game in filteredGames)
        {
            WishlistGames.Add(new GameCardItemViewModel(game, SelectGameCommand));
        }
    }

    [RelayCommand]
    private void RemoveFromWishlist(int gameId)
    {
        if (_mainViewModel.CurrentUser == null) return;
        
        var gameVm = WishlistGames.FirstOrDefault(g => g.Id == gameId);
        if (gameVm == null) return;

        try
        {
            _wishlistRepository.RemoveGame(gameId, _mainViewModel.CurrentUser.UserId);
            WishlistGames.Remove(gameVm);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting game from wishlist: {ex.Message}");
            StatusMessage = ex.Message;
        }
    }

    [RelayCommand]
    private void MoveToCart(int gameId)
    {
        if (_mainViewModel.CurrentUser == null) return;
        
        var gameVm = WishlistGames.FirstOrDefault(g => g.Id == gameId);
        if (gameVm == null) return;

        if (gameVm.IsInCart)
        {
            _mainViewModel.GoToCartPageCommand.Execute(null);
        }
        else
        {
            try
            {
                _cartRepository.AddGame(_gameRepository.GetById(gameId), _mainViewModel.CurrentUser);
                gameVm.IsInCart = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding game to cart: {ex.Message}");
                StatusMessage = ex.Message;
            }
        }
    }
}