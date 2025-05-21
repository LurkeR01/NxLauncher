using System.Collections.Generic;
using Org.BouncyCastle.Utilities;

namespace NxLauncher.Database.Repositories;

public interface IFavoritesRepository
{
    IEnumerable<int> GetUserFavoriteGameIds(int userId);
    void AddFavoriteGame(int userId, int gameId);
    void RemoveFavoriteGame(int userId, int gameId);
}