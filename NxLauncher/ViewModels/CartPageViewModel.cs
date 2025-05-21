using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MySql.Data.MySqlClient;
using NxLauncher.Data;
using NxLauncher.Database.Repositories;

namespace NxLauncher.ViewModels;

public partial class CartPageViewModel : PageViewModel
{
    private readonly ICartRepository _cartRepository;
    private readonly MainViewModel _mainViewModel;
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IGameRepository _gameRepository;
    
    [ObservableProperty] private string _statusMessage;
    [ObservableProperty] private decimal _totalPrice;
    
    public IRelayCommand<int> SelectGameCommand { get; }
    public IRelayCommand OpenRatingCommand { get; } = new RelayCommand(() =>
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://www.globalratings.com/ratingsguide.aspx#generic",
            UseShellExecute = true
        });
    });
    
    public ObservableCollection<GameCardItemViewModel> CartGames { get; set; }
    
    public CartPageViewModel(ICartRepository cartRepository,
        MainViewModel mainViewModel,
        IWishlistRepository wishlistRepository,
        IGameRepository gameRepository)
    {
        PageName = ApplicationPageNames.Cart;
        
        _cartRepository = cartRepository;
        _mainViewModel = mainViewModel;
        _wishlistRepository = wishlistRepository;
        _gameRepository = gameRepository;
        
        CartGames = new ObservableCollection<GameCardItemViewModel>();

        SelectGameCommand = new RelayCommand<int>(ExecuteSelectGame);

        _mainViewModel.PropertyChanged += MainViewModel_PropertyChanged;

        LoadGameInfo();
    }

    private void LoadGameInfo()
    {
        if (_mainViewModel.CurrentUser == null)
        {
            CartGames.Clear();
            StatusMessage = "Войдите в аккаунт для просмотра списка.";
            return;
        }

        StatusMessage = "Загрузка...";
        OnPropertyChanged(nameof(StatusMessage));

        try
        {
            var addedGameCards = _cartRepository.GetAllGameCardsByUserId(_mainViewModel.CurrentUser.UserId);
            var gamesList = addedGameCards.ToList();
            
            if (gamesList.Count == 0)
            {
                StatusMessage = "Список пуст";
                return;
            }
            
            CartGames.Clear();

            foreach (var gameCard in gamesList)
            {
                if (gameCard != null)
                {
                    var viewModel = new GameCardItemViewModel(gameCard, SelectGameCommand);
                    CartGames.Add(viewModel);
                    TotalPrice += gameCard.Price;
                }
            }

            StatusMessage = CartGames.Any() ? null : "Список пуст";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading cart: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            StatusMessage = "Не удалось загрузить список.";
            CartGames.Clear(); 
        }
    }
    
    private void MainViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.CurrentUser))
        {
            LoadGameInfo();
        }
    }

    [RelayCommand]
    private void RemoveFromCart(int gameId)
    {
        if (_mainViewModel.CurrentUser == null) return;
        
        var gameVm = CartGames.FirstOrDefault(g => g.Id == gameId);
        if (gameVm == null) return;

        try
        {
            _cartRepository.RemoveGame(gameId, _mainViewModel.CurrentUser.UserId);
            CartGames.Remove(gameVm);
            TotalPrice -= gameVm.Price;
            OnPropertyChanged(nameof(TotalPrice));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting game from wishlist: {ex.Message}");
            StatusMessage = ex.Message;
        }
    }

    [RelayCommand]
    private void MoveToWishlist(int gameId)
    {
        if (_mainViewModel.CurrentUser == null) return;
        
        RemoveFromCart(gameId);

        try
        {
            _wishlistRepository.AddGame(_gameRepository.GetById(gameId), _mainViewModel.CurrentUser);
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            RemoveFromCart(gameId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding game to wishlist: {ex.Message}");
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
}