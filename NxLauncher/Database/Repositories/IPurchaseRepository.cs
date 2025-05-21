using System.Collections.Generic;
using NxLauncher.Models;
using Org.BouncyCastle.Utilities;

namespace NxLauncher.Database.Repositories;

public interface IPurchaseRepository
{
    IEnumerable<Game> GetGamesByUserId(int userId);
    IEnumerable<GameCard> GetGameCardsByUserId(int userId);
    void PurchaseGame(Game game, User user);
    bool IsPurchased(Game game, User user);
    IEnumerable<GameCard> GetByFilter(FilterOptions options, User user);
    bool DeletePurchasedGame(int userId, int gameId);
}