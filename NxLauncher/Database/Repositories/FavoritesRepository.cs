using System.Collections.Generic;
using System.Collections.ObjectModel;
using MySql.Data.MySqlClient;

namespace NxLauncher.Database.Repositories;

public class FavoritesRepository : IFavoritesRepository
{
    public IEnumerable<int> GetUserFavoriteGameIds(int userId)
    {
        var favoriteGameIds = new ObservableCollection<int>();
        
        using var connection = Database.GetConnection();
        var command = new MySqlCommand("SELECT game_id FROM favorites WHERE user_id = @id", connection);
        command.Parameters.AddWithValue("@id", userId);
        
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            favoriteGameIds.Add(reader.GetInt32("game_id"));
        }
        
        return favoriteGameIds;
    }

    public void AddFavoriteGame(int userId, int gameId)
    {
        using var connection = Database.GetConnection();
        var command = new MySqlCommand("INSERT INTO favorites (user_id, game_id) VALUES (@user_id, @game_id)", connection);
        command.Parameters.AddWithValue("@user_id", userId);
        command.Parameters.AddWithValue("@game_id", gameId);
        
        using var reader = command.ExecuteReader();
    }

    public void RemoveFavoriteGame(int userId, int gameId)
    {
        using var connection = Database.GetConnection();
        var command = new MySqlCommand("DELETE FROM favorites WHERE user_id = @user_id AND game_id = @game_id", connection);
        command.Parameters.AddWithValue("@user_id", userId);
        command.Parameters.AddWithValue("@game_id", gameId);
        
        using var reader = command.ExecuteReader();
    }
}