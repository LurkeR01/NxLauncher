using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using NxLauncher.Models;

namespace NxLauncher.Database.Repositories;

public class WishlistRepository : IWishlistRepository
{
    public IEnumerable<GameCard> GetAllGameCardsByUserId(int userId)
    {
        var gameCards = new ObservableCollection<GameCard>();
        using var connection = Database.GetConnection();
        var command = new MySqlCommand("""
                                       SELECT g.game_id, g.game_name, g.game_price, g.game_image, g.game_release_date, g.game_age_rating, w.added_at
                                       FROM wishlist w
                                       JOIN game g ON w.game_id = g.game_id
                                       WHERE w.user_id = @currentUserId;
                                       """, connection);
        command.Parameters.AddWithValue("@currentUserId", userId);
        
        Console.WriteLine($"Executing SQL: {command.CommandText}");
        Console.WriteLine($"Parameters: @currentUserId = {userId}");
        
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var gameCard = new GameCard
            {
                Id = reader.GetInt32("game_id"),
                Title = reader.GetString("game_name"),
                Price = reader.GetDecimal("game_price"),
                IconUrl = reader.GetString("game_image"),
                AddedDate = reader.GetDateTime("added_at"),
                AgeRating = reader.GetString("game_age_rating"),
            };
            Console.WriteLine($"Read game card: {gameCard.Title}");
            gameCards.Add(gameCard);
        }
        
        return gameCards;
    }
    
    public IEnumerable<GameCard> GetByFilter(FilterOptions options, User user)
    {
        var gameCards = new List<GameCard>();
        using var connection = Database.GetConnection();

        var queryBuilder = new StringBuilder("SELECT DISTINCT g.game_id, g.game_name, g.game_price, g.game_image, g.game_release_date, g.game_age_rating, w.added_at" +
                                             " FROM game g" +
                                             " JOIN wishlist w ON w.game_id = g.game_id");
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
            queryBuilder.Append(" AND w.user_id = @userId");
        }
        
        var command = new MySqlCommand(queryBuilder.ToString(), connection);
        if (parameters.Any())
        {
            command.Parameters.AddRange(parameters.ToArray());
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
                AddedDate = reader.GetDateTime("added_at"),
                AgeRating = reader.GetString("game_age_rating"),
            });
        }

        return gameCards;
    }

    public void AddGame(Game game, User user)
    {
        using var connection = Database.GetConnection();
        var command = new MySqlCommand("INSERT INTO wishlist (game_id, user_id, added_at)"
                                       + "VALUES (@gameId, @userId, @added_at);", connection);
        
        command.Parameters.AddWithValue("@gameId", game.Id);
        command.Parameters.AddWithValue("@userId", user.UserId);
        command.Parameters.AddWithValue("@added_at", DateTimeOffset.Now);
        
        command.ExecuteNonQuery();
    }

    public bool IsInWishlist(Game game, User user)
    {
        using var connection = Database.GetConnection();
        var command = new MySqlCommand(
            "SELECT COUNT(*) FROM wishlist WHERE user_id = @userId AND game_id = @gameId",
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
            Console.WriteLine($"Error checking wishlist status: {ex.Message}");
            return false;
        }
    }

    public void RemoveGame(int gameId, int userId)
    {
        using var connection = Database.GetConnection();
        var command = new MySqlCommand("DELETE FROM wishlist WHERE user_id = @user_id AND game_id = @game_id", connection);
        command.Parameters.AddWithValue("@user_id", userId);
        command.Parameters.AddWithValue("@game_id", gameId);
        
        using var reader = command.ExecuteReader();
    }
}