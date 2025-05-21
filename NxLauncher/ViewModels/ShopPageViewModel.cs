using System;
using CommunityToolkit.Mvvm.ComponentModel;
using NxLauncher.Database.Repositories;
using NxLauncher.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;

namespace NxLauncher.ViewModels
{
    public partial class ShopPageViewModel : PageViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IGameRepository _gameRepository;
        private readonly IGenreRepository _genreRepository;
        private readonly DeveloperRepository _developerRepository;
        
        [ObservableProperty] private string? _searchQuery;
        [ObservableProperty] private SortOptionItem _selectedSortOption;
        [ObservableProperty] private bool _isAdmin;
        
        private Timer? _filterDebounceTimer;
        private readonly TimeSpan _debounceTime = TimeSpan.FromMilliseconds(300);
        
        public ObservableCollection<PriceFilterItem> PriceFilters { get; }
        public ObservableCollection<Genre> AvailableGenres { get; }
        public ObservableCollection<GameCardItemViewModel> FilteredGameCards { get; set; }
        public ObservableCollection<SortOptionItem> SortOptions { get; }
        public ObservableCollection<AgeRating> AvailableAgeRatings { get; set; }
        
        public IRelayCommand<int> SelectGameCommand { get; }
        public IRelayCommand AddGameCommand { get; }
        public IRelayCommand RefreshCommand { get; }

        public ShopPageViewModel(IGenreRepository genreRepository, IGameRepository gameRepository, MainViewModel mainViewModel)
        {
            _gameRepository = gameRepository;
            _genreRepository = genreRepository;
            _mainViewModel = mainViewModel;
            
            IsAdmin = _mainViewModel.IsAdmin;
            _mainViewModel.PropertyChanged += MainViewModel_PropertyChanged;

            AvailableGenres = new ObservableCollection<Genre>(_genreRepository.GetAll());
            PriceFilters = new()
            {
                new PriceFilterItem(null, "Любая цена"),
                new PriceFilterItem(0, "Бесплатные"),
                new PriceFilterItem(300, "До 300"),
                new PriceFilterItem(600, "До 600"),
                new PriceFilterItem(1000, "До 1000")
            };
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
                new() { DisplayName = "Все", Value = SortOption.All },
                new() { DisplayName = "Новинка", Value = SortOption.Newest },
                new() { DisplayName = "По алфавиту", Value = SortOption.Alphabetical },
                new() { DisplayName = "Цена: по убыванию", Value = SortOption.PriceDescending },
                new() { DisplayName = "Цена: по возрастанию", Value = SortOption.PriceAscending }
            };
            SelectedSortOption = SortOptions[0];

            foreach (var genre in AvailableGenres)
            {
                genre.PropertyChanged += Genre_PropertyChanged;
            }

            foreach (var item in PriceFilters)
            {
                item.PropertyChanged += Price_PropertyChanged;
            }
            
            FilteredGameCards = new ObservableCollection<GameCardItemViewModel>();
            SelectGameCommand = new RelayCommand<int>(ExecuteSelectGame);   
            AddGameCommand = new RelayCommand(RequestAddGameOverlay);
            RefreshCommand = new RelayCommand(Refresh);
            
            LoadAndWrapInitialGames();
            
            _filterDebounceTimer = new Timer(FilterTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }
        
        private void FilterPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Genre.IsSelected) || e.PropertyName == nameof(AgeRating.IsSelected))
            {
                RequestFilterUpdate();
            }
        }

        private void Refresh()
        {
            LoadAndWrapInitialGames();
        }
        
        private void LoadAndWrapInitialGames()
        {
            var gameCards = _gameRepository.GetAllGameCards();
            FilteredGameCards.Clear();
            foreach (var card in gameCards)
            {
                FilteredGameCards.Add(new GameCardItemViewModel(card, SelectGameCommand));
            }
        }

        private void RequestAddGameOverlay()
        {
            if (_mainViewModel.ShowAddGameOverlayCommand.CanExecute(null))
            {
                _mainViewModel.ShowAddGameOverlayCommand.Execute(null);
            }
        }
        
        private void ExecuteSelectGame(int gameId)
        {
            if (_mainViewModel.ShowGameDetailsCommand.CanExecute(gameId))
            {
                _mainViewModel.ShowGameDetailsCommand.Execute(gameId);
            }
        }
        
        private void Genre_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Genre.IsSelected))
            {
                RequestFilterUpdate(); 
            }
        }
        
        private void Price_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PriceFilterItem.IsSelected))
            {
                RequestFilterUpdate(); 
            }
        }
        
        partial void OnSearchQueryChanged(string? value)
        {
            RequestFilterUpdate(); 
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
                Console.WriteLine($"Applying filters at {DateTime.Now:HH:mm:ss.fff}");
                ApplyFilters();
            });
        }
        

        private void ApplyFilters()
        {
            var selectedGenres = AvailableGenres.Where(g => g.IsSelected).ToList();
            var selectedPriceFilterItem = PriceFilters.FirstOrDefault(g => g.IsSelected);
            var selectedAgeRatings = AvailableAgeRatings.Where(g => g.IsSelected).ToList();

            var options = new FilterOptions
            {
                PriceFilterItem = selectedPriceFilterItem,
                Genres = selectedGenres,
                SearchQuery = SearchQuery,
                AgeRatings = selectedAgeRatings,
            };
            
            var filteredGames = _gameRepository.GetByFilter(options).ToList();

            switch (SelectedSortOption.Value)
            {
                case SortOption.Newest:
                    filteredGames = filteredGames.OrderByDescending(g => g.ReleaseDate).ToList();
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
            
            FilteredGameCards.Clear();
            foreach (var game in filteredGames)
            {
                FilteredGameCards.Add(new GameCardItemViewModel(game, SelectGameCommand));
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
                foreach (var item in PriceFilters)
                {
                    item.PropertyChanged -= Price_PropertyChanged;
                }
            }
        }

        ~ShopPageViewModel()
        {
            Dispose(false);
        }

        private void MainViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.IsAdmin))
            {
                IsAdmin = _mainViewModel.IsAdmin;
            }
        }
    }
}
