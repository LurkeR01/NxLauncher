using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using NxLauncher.Models;

namespace NxLauncher.Database.Repositories;

public class PurchaseRepository : IPurchaseRepository
{
    public IEnumerable<Game> GetGamesByUserId(int userId)
    {
        List<Game> games = new();
        
        using var connection = Database.GetConnection();
        var command = new MySqlCommand("""
                                       SELECT g.game_id, g.game_name, g.game_price, g.developer_id, game_rating, game_image, game_descriprion, game_release_date, game_age_rating, p.purchase_date
                                       FROM purchase p
                                       JOIN game g ON p.game_id = g.game_id
                                       WHERE p.user_id = @currentUserId;
                                       """, connection);
        command.Parameters.AddWithValue("@currentUserId", userId);
        
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            games.Add(new Game
            {
                Id = reader.GetInt32("game_id"),
                Name = reader.GetString("game_name"),
                Price = reader.GetDecimal("game_price"),
                DeveloperId = reader.GetInt32("developer_id"),
                Rating = reader.GetDecimal("game_rating"),
                AgeRating = reader.GetString("game_age_rating"),
                ImageUrl = reader.GetString("game_image"),
                Description = reader.GetString("game_description"),
                ReleaseDate = reader.GetDateTime("game_release_date"),
            });
        }
        
        return games;
    }
    
    public IEnumerable<GameCard> GetGameCardsByUserId(int userId)
    {
        var gameCards = new ObservableCollection<GameCard>();
        using var connection = Database.GetConnection();
        var command = new MySqlCommand("""
                                       SELECT g.game_id, g.game_name, g.game_price, g.game_image, g.game_release_date, p.purchase_date
                                       FROM purchase p
                                       JOIN game g ON p.game_id = g.game_id
                                       WHERE p.user_id = @currentUserId;
                                       """, connection);
        command.Parameters.AddWithValue("@currentUserId", userId);
        
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            gameCards.Add( new GameCard
            {
                Id = reader.GetInt32("game_id"),
                Title = reader.GetString("game_name"),
                Price = reader.GetDecimal("game_price"),
                IconUrl = reader.GetString("game_image"),
                PurchaseDate = reader.GetDateTime("purchase_date")
            });
        }
        
        return gameCards;
    }

    public void PurchaseGame(Game game, User user)
    {
        using var connection = Database.GetConnection();
        var command = new MySqlCommand("INSERT INTO purchase (game_id, purchase_date, user_id, purchase_price) "
        + "VALUES (@gameId, @purchaseDate, @userId, @purchasePrice);", connection);
        
        command.Parameters.AddWithValue("@gameId", game.Id);
        command.Parameters.AddWithValue("@purchaseDate", DateTimeOffset.Now);
        command.Parameters.AddWithValue("@userID", user.UserId);
        command.Parameters.AddWithValue("@purchasePrice", game.Price);
        
        command.ExecuteNonQuery();
    }

    public bool IsPurchased(Game game, User user)
    {
        using var connection = Database.GetConnection();
        var command = new MySqlCommand(
            "SELECT COUNT(*) FROM purchase WHERE user_id = @userId AND game_id = @gameId",
            connection);
        command.Parameters.AddWithValue("@userId", user.UserId);
        command.Parameters.AddWithValue("@gameId", game.Id);

        try
        {
            long count = (long)command.ExecuteScalar();
            return count > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking purchase status: {ex.Message}");
            return false;
        }
    }

    public IEnumerable<GameCard> GetByFilter(FilterOptions options, User user)
    {
        var gameCards = new List<GameCard>();
        using var connection = Database.GetConnection();

        var queryBuilder = new StringBuilder("SELECT DISTINCT g.game_id, g.game_name, g.game_price, g.game_image, g.game_release_date, p.purchase_date" +
                                             " FROM game g" +
                                             " JOIN purchase p ON p.game_id = g.game_id");
        var parameters = new List<MySqlParameter>();
        var conditions = new List<string>();
        
        if (options.Genres.Any())
        {
            queryBuilder.Append(" JOIN game_genre gg ON g.game_id = gg.game_id");
            queryBuilder.Append(" JOIN genre gr ON gg.genre_id = gr.genre_id");

            var genreNames = options.Genres.Select(g => g.Name).ToList();
            var paramNames = new List<string>();
            for (int i = 0; i < genreNames.Count; i++)
            {
                var paramName = $"@genre{i}";
                paramNames.Add(paramName);
                parameters.Add(new MySqlParameter(paramName, genreNames[i]));
            }
            parameters.Add(new MySqlParameter("@userId", user.UserId));
            conditions.Add($"gr.genre_name IN ({string.Join(", ", paramNames)})");
        }
        
        if (options.AgeRatings.Any())
        {
            var ageRatingNames = options.AgeRatings.Select(a => a.Name).ToList();
            var paramNames = new List<string>();
            for (int i = 0; i < ageRatingNames.Count; i++)
            {
                var paramName = $"@ageRating{i}";
                paramNames.Add(paramName);
                parameters.Add(new MySqlParameter(paramName, ageRatingNames[i]));
            }
            parameters.Add(new MySqlParameter("@userId", user.UserId));
            conditions.Add($"g.game_age_rating IN ({string.Join(", ", paramNames)})");
        }
        
        if (conditions.Any())
        {
            queryBuilder.Append(" WHERE ");
            queryBuilder.Append(string.Join(" AND ", conditions));
            queryBuilder.Append(" AND p.user_id = @userId");
        }
        
        var command = new MySqlCommand(queryBuilder.ToString(), connection);
        if (parameters.Any())
        {
            command.Parameters.AddRange(parameters.ToArray());
        }
        
        Console.WriteLine($"Executing SQL: {command.CommandText}"); 
        foreach (MySqlParameter p in command.Parameters) { 
            Console.WriteLine($"Parameter: {p.ParameterName} = {p.Value}");
        }

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            gameCards.Add(new GameCard
            {
                Id = reader.GetInt32("game_id"),
                Title = reader.GetString("game_name"),
                Price = reader.GetDecimal("game_price"),
                IconUrl = reader.GetString("game_image"),
                ReleaseDate = reader.GetDateTime("game_release_date"),
                PurchaseDate = reader.GetDateTime("purchase_date")
            });
        }

        return gameCards;
    }

    public bool DeletePurchasedGame(int userId, int gameId)
    {
        using var connection = Database.GetConnection();
        using var transaction = connection.BeginTransaction();
    
        try
        {
            using var cmdFavorites = new MySqlCommand(
                "DELETE FROM favorites WHERE game_id = @gameId AND user_id = @userId", 
                connection, 
                transaction);
            cmdFavorites.Parameters.AddWithValue("@gameId", gameId);
            cmdFavorites.Parameters.AddWithValue("@userId", userId);
            cmdFavorites.ExecuteNonQuery();

            using var cmdGame = new MySqlCommand(
                "DELETE FROM purchase WHERE game_id = @gameId AND user_id = @userId", 
                connection, 
                transaction);
            cmdGame.Parameters.AddWithValue("@gameId", gameId);
            cmdGame.Parameters.AddWithValue("@userId", userId);
            int rowsAffected = cmdGame.ExecuteNonQuery();

            transaction.Commit();
            return rowsAffected > 0;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}