using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MySql.Data.MySqlClient;
using NxLauncher.Models;

namespace NxLauncher.Database.Repositories;

public class CartRepository : ICartRepository
{
    public IEnumerable<GameCard> GetAllGameCardsByUserId(int userId)
    {
        var gameCards = new ObservableCollection<GameCard>();
        using var connection = Database.GetConnection();
        var command = new MySqlCommand("""
                                       SELECT g.game_id, g.game_name, g.game_price, g.game_image, g.game_release_date, g.game_age_rating, c.added_at
                                       FROM cart c
                                       JOIN game g ON c.game_id = g.game_id
                                       WHERE c.user_id = @currentUserId;
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
    
    public void RemoveGame(int gameId, int userId)
    {
        using var connection = Database.GetConnection();
        var command = new MySqlCommand("DELETE FROM cart WHERE user_id = @user_id AND game_id = @game_id", connection);
        command.Parameters.AddWithValue("@user_id", userId);
        command.Parameters.AddWithValue("@game_id", gameId);
        
        using var reader = command.ExecuteReader();
    }
    
    public void AddGame(Game game, User user)
    {
        using var connection = Database.GetConnection();
        var command = new MySqlCommand("INSERT INTO cart (game_id, user_id, added_at)"
                                       + "VALUES (@gameId, @userId, @added_at);", connection);
        
        command.Parameters.AddWithValue("@gameId", game.Id);
        command.Parameters.AddWithValue("@userId", user.UserId);
        command.Parameters.AddWithValue("@added_at", DateTimeOffset.Now);
        
        command.ExecuteNonQuery();
    }
    
    public bool IsInCart(Game game, User user)
    {
        using var connection = Database.GetConnection();
        var command = new MySqlCommand(
            "SELECT COUNT(*) FROM cart WHERE user_id = @userId AND game_id = @gameId",
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
}