using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using NxLauncher.Models;

namespace NxLauncher.Database.Repositories;

public class GameRepository :  IGameRepository
{
    public IEnumerable<Game> GetAll()
    {
        var games = new ObservableCollection<Game>();
        
        using var connection = Database.GetConnection();
        var command = new MySqlCommand("SELECT * FROM game", connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            games.Add( new Game
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

    public IEnumerable<GameCard> GetAllGameCards()
    {
        var gameCards = new ObservableCollection<GameCard>();
        using var connection = Database.GetConnection();
        var command = new MySqlCommand("SELECT game_id, game_name, game_price, game_image, game_release_date, game_age_rating FROM game", connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            gameCards.Add( new GameCard
            {
                Id = reader.GetInt32("game_id"),
                Title = reader.GetString("game_name"),
                Price = reader.GetDecimal("game_price"),
                IconUrl = reader.GetString("game_image"),
                AgeRating = reader.GetString("game_age_rating"),
            });
        }
        
        return gameCards;
    }

    public Game GetById(int id)
    {
        using var connection = Database.GetConnection();
        var command = new MySqlCommand("SELECT * FROM game WHERE game_id = @id", connection);
        command.Parameters.AddWithValue("@id", id);
        
        try
        {
            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                throw new KeyNotFoundException($"Игра с ID {id} не найдена");
            }
            
            return new Game
            {
                Id = reader.GetInt32("game_id"),
                Name = reader.GetString("game_name"),
                Price = reader.GetDecimal("game_price"),
                DeveloperId = reader.GetInt32("developer_id"),
                Rating = reader.GetDecimal("game_rating"),
                AgeRating = reader.GetString("game_age_rating"),
                ImageUrl = reader.GetString("game_image"),
                Description = reader.GetString("game_description"),
                ReleaseDate = reader.GetDateTime("game_release_date")
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении игры: {ex.Message}");
            throw;
        }
    }

    public IEnumerable<GameCard> GetByFilter(FilterOptions options)
    {
        var gameCards = new List<GameCard>();
        using var connection = Database.GetConnection();

        var queryBuilder = new StringBuilder("SELECT DISTINCT g.game_id, g.game_name, g.game_price, g.game_image, g.game_release_date, g.game_age_rating FROM game g");
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
            conditions.Add($"g.game_age_rating IN ({string.Join(", ", paramNames)})");
        }

        if (options.PriceFilterItem != null)
        {
            if (options.PriceFilterItem.Value > 0)
            {
                conditions.Add("g.game_price <= @maxPrice");
                parameters.Add(new MySqlParameter("@maxPrice", options.PriceFilterItem.Value.Value));
            }
            else if (options.PriceFilterItem.Value == 0)
            {
                conditions.Add("g.game_price = 0");
            }
        }
       
        if (!string.IsNullOrEmpty(options.SearchQuery))
        {
            conditions.Add("game_name LIKE @searchQuery OR game_description LIKE @searchQuery");
            parameters.Add(new MySqlParameter("@searchQuery", $"%{options.SearchQuery}%"));
        }
        

        if (conditions.Any())
        {
            queryBuilder.Append(" WHERE ");
            queryBuilder.Append(string.Join(" AND ", conditions));
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
                AgeRating = reader.GetString("game_age_rating"),
            });
        }

        return gameCards;
    }

    public bool DeleteGame(int? gameId)
    {
        using var connection = Database.GetConnection();
        using var transaction = connection.BeginTransaction();
    
        try
        {
            using var cmdScreenshots = new MySqlCommand(
                "DELETE FROM screenshots WHERE game_id = @gameId", 
                connection, 
                transaction);
            cmdScreenshots.Parameters.AddWithValue("@gameId", gameId);
            cmdScreenshots.ExecuteNonQuery();
            
            using var cmdGenres = new MySqlCommand(
                "DELETE FROM game_genre WHERE game_id = @gameId", 
                connection, 
                transaction);
            cmdGenres.Parameters.AddWithValue("@gameId", gameId);
            cmdGenres.ExecuteNonQuery();
            
            using var cmdGame = new MySqlCommand(
                "DELETE FROM game WHERE game_id = @gameId", 
                connection, 
                transaction);
            cmdGame.Parameters.AddWithValue("@gameId", gameId);
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
    
    public bool AddGame(Game game, IEnumerable<int> genreIds, IEnumerable<string> screenshotUrls)
    {
        using var connection = Database.GetConnection();
        using var transaction = connection.BeginTransaction();
        
        try
        {
            var gameId = InsertGame(game, connection, transaction);
            
            if (genreIds.Any())
            {
                InsertGameGenres(gameId, genreIds, connection, transaction);
            }

            if (screenshotUrls.Any())
            {
                InsertScreenshots(gameId, screenshotUrls, connection, transaction);
            }

            transaction.Commit();
            return true;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Console.WriteLine($"Ошибка: {ex.Message}");
            return false;
        }
    }
    
    private int InsertGame(Game game, MySqlConnection connection, MySqlTransaction transaction)
    {
        const string sql = @"INSERT INTO game 
            (game_name, game_price, developer_id, game_rating, game_age_rating, game_image, game_description, game_release_date)
            VALUES (@name, @price, @devId, @rating, @ageRating, @image, @desc, @releaseDate);
            SELECT LAST_INSERT_ID();";

        using var command = new MySqlCommand(sql, connection, transaction);
        
        command.Parameters.AddWithValue("@name", game.Name);
        command.Parameters.AddWithValue("@price", game.Price);
        command.Parameters.AddWithValue("@devId", game.DeveloperId);
        command.Parameters.AddWithValue("@rating", game.Rating);
        command.Parameters.AddWithValue("@ageRating", game.AgeRating);
        command.Parameters.AddWithValue("@image", game.ImageUrl);
        command.Parameters.AddWithValue("@desc", game.Description);
        command.Parameters.AddWithValue("@releaseDate", game.ReleaseDate?.DateTime ?? DateTime.MinValue);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    private void InsertGameGenres(
        int gameId, 
        IEnumerable<int> genreIds,
        MySqlConnection connection, 
        MySqlTransaction transaction)
    {
        var sql = new StringBuilder("INSERT INTO game_genre (game_id, genre_id) VALUES ");
        var parameters = new List<MySqlParameter>();
        
        int i = 0;
        foreach (var genreId in genreIds)
        {
            sql.Append($"(@gameId{i}, @genreId{i}),");
            parameters.Add(new MySqlParameter($"@gameId{i}", gameId));
            parameters.Add(new MySqlParameter($"@genreId{i}", genreId));
            i++;
        }
        sql.Length--;

        using var command = new MySqlCommand(sql.ToString(), connection, transaction);
        command.Parameters.AddRange(parameters.ToArray());
        command.ExecuteNonQuery();
    }

    private void InsertScreenshots(
        int gameId,
        IEnumerable<string> screenshotUrls,
        MySqlConnection connection,
        MySqlTransaction transaction)
    {
        var sql = new StringBuilder("INSERT INTO screenshot (game_id, image_url) VALUES ");
        var parameters = new List<MySqlParameter>();
        
        int i = 0;
        foreach (var url in screenshotUrls)
        {
            sql.Append($"(@gameId{i}, @url{i}),");
            parameters.Add(new MySqlParameter($"@gameId{i}", gameId));
            parameters.Add(new MySqlParameter($"@url{i}", url));
            i++;
        }
        sql.Length--;

        using var command = new MySqlCommand(sql.ToString(), connection, transaction);
        command.Parameters.AddRange(parameters.ToArray());
        command.ExecuteNonQuery();
    }

    public void Update(Game game)
    {
        using var connection = Database.GetConnection();
        var command = new MySqlCommand(
            @"UPDATE game SET 
                game_name = @name,
                game_description = @desc,
                game_price = @price,
                game_release_date = @releaseDate,
                developer_id = @devId,
                game_image = @image
              WHERE game_id = @id", connection);
        
        command.Parameters.AddWithValue("@name", game.Name);
        command.Parameters.AddWithValue("@desc", game.Description);
        command.Parameters.AddWithValue("@price", game.Price);
        command.Parameters.AddWithValue("@releaseDate", game.ReleaseDate);
        command.Parameters.AddWithValue("@devId", game.DeveloperId);
        command.Parameters.AddWithValue("@image", game.ImageUrl);
        command.Parameters.AddWithValue("@id", game.Id);

        command.ExecuteNonQuery();
    }
    
    public void UpdateGameGenres(int gameId, List<int> genreIds)
    {
        using var connection = Database.GetConnection();
        using var transaction = connection.BeginTransaction();
    
        try
        {
            var deleteCommand = new MySqlCommand(
                "DELETE FROM game_genre WHERE game_id = @gameId", 
                connection, 
                transaction);
            
            deleteCommand.Parameters.AddWithValue("@gameId", gameId);
            deleteCommand.ExecuteNonQuery();

            if (genreIds.Any())
            {
                var sql = new StringBuilder(
                    "INSERT INTO game_genre (game_id, genre_id) VALUES ");
            
                for (int i = 0; i < genreIds.Count; i++)
                {
                    sql.Append($"(@gameId, @genreId{i}),");
                }
                sql.Length--;

                var insertCommand = new MySqlCommand(sql.ToString(), connection, transaction);
                insertCommand.Parameters.AddWithValue("@gameId", gameId);
            
                for (int i = 0; i < genreIds.Count; i++)
                {
                    insertCommand.Parameters.AddWithValue($"@genreId{i}", genreIds[i]);
                }
            
                insertCommand.ExecuteNonQuery();
            }
        
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}