using System.Collections.Generic;
using NxLauncher.Models;

namespace NxLauncher.Database.Repositories;

public interface IWishlistRepository
{
    IEnumerable<GameCard> GetAllGameCardsByUserId(int userId);
    IEnumerable<GameCard> GetByFilter(FilterOptions options, User user);
    void AddGame(Game game, User user);
    bool IsInWishlist(Game game, User user);
    void RemoveGame(int gameId, int userId);
}