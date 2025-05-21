using System.Collections;
using System.Collections.Generic;
using NxLauncher.Controls;
using NxLauncher.Models;
using GameCard = NxLauncher.Models.GameCard;

namespace NxLauncher.Database.Repositories;

public interface IGameRepository
{
    IEnumerable<Game> GetAll();
    IEnumerable<GameCard> GetAllGameCards();
    IEnumerable<GameCard> GetByFilter(FilterOptions options);
    Game GetById(int id);
    bool AddGame(Game game, IEnumerable<int> genreIds, IEnumerable<string> screenshotUrls);
    bool DeleteGame(int? gameId);
    void Update(Game game);
    void UpdateGameGenres(int gameId, List<int> genreIds);
}