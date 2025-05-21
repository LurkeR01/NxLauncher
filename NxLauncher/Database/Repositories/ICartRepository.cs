using System.Collections;
using System.Collections.Generic;
using NxLauncher.Models;

namespace NxLauncher.Database.Repositories;

public interface ICartRepository
{
    IEnumerable<GameCard> GetAllGameCardsByUserId(int userId);
    void RemoveGame(int gameId, int userId);
    void AddGame(Game game, User user);
    bool IsInCart(Game game, User user);
}