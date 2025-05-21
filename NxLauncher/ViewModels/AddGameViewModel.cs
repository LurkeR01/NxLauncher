using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MySql.Data.MySqlClient;
using NxLauncher.Database.Repositories;
using NxLauncher.Models;

namespace NxLauncher.ViewModels;

public partial class AddGameViewModel : ViewModelBase
{
    private readonly Action _closeAction;
    private readonly IDeveloperRepository _developerRepository;
    private readonly IGameRepository _gameRepository;
    private readonly IGenreRepository _genreRepository;
    private readonly IScreenshotRepository _screenshotRepository;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddGameCommand))]
    private string? _gameName;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddGameCommand))]
    private string? _gamePrice;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddGameCommand))]
    private Developer? _gameDeveloper;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddGameCommand))]
    private string? _gameRating;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddGameCommand))]
    private string? _gameImage;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddGameCommand))]
    private string? _gameDescription;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddGameCommand))]
    private DateTimeOffset? _gameReleaseDate = DateTimeOffset.Now;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddGameCommand))]
    private string? _gameAgeRating;
    
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isAddingNewGenre;
    [ObservableProperty] private bool _isAddingNewDeveloper;
    [ObservableProperty] private string? _newGenreName;
    [ObservableProperty] private string? _newDeveloperName;
    [ObservableProperty] private string? _screenshotUrl;
    
    public ObservableCollection<Developer> AllDevelopers { get; set; }
    public ObservableCollection<Genre> SelectedGenres { get; set; }
    public ObservableCollection<string> ScreenshotUrls { get; set; }
    
    public List<string> Ratings { get; } = Enumerable.Range(1, 10).Select(i => i.ToString()).ToList();
    public List<string> AgeRatings { get; } = new List<string> { "3+", "7+", "12+", "16+", "18+" };

    public AddGameViewModel(IDeveloperRepository developerRepository, 
        Action closeAction, 
        IGameRepository gameRepository, 
        IGenreRepository genreRepository, 
        IScreenshotRepository screenshotRepository)
    {
        _developerRepository = developerRepository;
        _closeAction = closeAction;
        _gameRepository = gameRepository;
        _genreRepository = genreRepository;
        _screenshotRepository = screenshotRepository;
        
        AllDevelopers = new ObservableCollection<Developer>(_developerRepository.GetAll());
        SelectedGenres = new ObservableCollection<Genre>(_genreRepository.GetAll());
        ScreenshotUrls = new ObservableCollection<string>();
    }

    private bool CanAddGame()
    {
        return !string.IsNullOrWhiteSpace(GameName)
               && !string.IsNullOrWhiteSpace(GamePrice) && decimal.TryParse(GamePrice, out _) 
               && GameDeveloper != null 
               && !string.IsNullOrWhiteSpace(GameRating) && decimal.TryParse(GameRating, out _) 
               && !string.IsNullOrWhiteSpace(GameImage) 
               && !string.IsNullOrWhiteSpace(GameDescription)
               && !string.IsNullOrWhiteSpace(GameAgeRating)
               && GameReleaseDate.HasValue;
    }
    

    [RelayCommand(CanExecute = nameof(CanAddGame))]
    private void AddGame()
    {
        ErrorMessage = null;

        if (!decimal.TryParse(GamePrice, out decimal price) || price < 0)
        {
            ErrorMessage = "Неверный формат цены.";
            return;
        }
        if (GameDeveloper == null) 
        {
            ErrorMessage = "Выберите разработчика.";
            return;
        }
        if (!GameReleaseDate.HasValue)
        {
            ErrorMessage = "Выберите дату выхода.";
            return;
        }

        
        var selectedGenreIds = SelectedGenres 
            .Where(g => g.IsSelected) 
            .Select(g => g.Id)
            .ToList();
        
        if (!selectedGenreIds.Any())
        {
            ErrorMessage = "Выберите хотя бы один жанр.";
            return;
        }
        
        Game game = new Game
        {
            Name = GameName,
            AgeRating = GameAgeRating,
            Description = GameDescription,
            DeveloperId = GameDeveloper.DeveloperId,
            ImageUrl = GameImage,
            Price = decimal.Parse(GamePrice),
            ReleaseDate = GameReleaseDate,
            Rating = decimal.Parse(GameRating)
        };

        List<int> genreIds = SelectedGenres
            .Where(g => g.IsSelected)
            .Select(g => g.Id)
            .ToList(); 
        try
        {
            _gameRepository.AddGame(game, genreIds, ScreenshotUrls);
            GameName = string.Empty;
            GameDeveloper = null;
            GamePrice = string.Empty;
            GameAgeRating = string.Empty;
            GameImage = string.Empty;
            GameDescription = string.Empty;
        
            _closeAction?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            Console.WriteLine($"Adding game error: {ex}");
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _closeAction?.Invoke();
    }

    [RelayCommand]
    private void ToggleAddDeveloper() => IsAddingNewDeveloper = !IsAddingNewDeveloper;

    [RelayCommand]
    private void ToggleAddGenre() => IsAddingNewGenre = !IsAddingNewGenre;
    
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
                SelectedGenres.Add(newGenre);
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
    
    [RelayCommand]
    private void AddNewDeveloper()
    {
        ErrorMessage = null;
        
        if (!string.IsNullOrWhiteSpace(NewDeveloperName))
        {
            try
            {
                var newDeveloper = new Developer { DeveloperName = NewDeveloperName };
                _developerRepository.Add(newDeveloper);
                AllDevelopers.Add(newDeveloper);
                NewDeveloperName = string.Empty;
                IsAddingNewDeveloper = false;
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                ErrorMessage = $"Разработчик '{NewDeveloperName}' уже существует!";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
            }
        }
    }

    [RelayCommand]
    private void AddScreenshot()
    {
        ErrorMessage = null;
        
        try
        {
            ScreenshotUrls.Add(_screenshotUrl);
            ScreenshotUrl = string.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка: {ex.Message}";
        }
    }
}