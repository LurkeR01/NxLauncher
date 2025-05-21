using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using NxLauncher.Models;

namespace NxLauncher.Database.Repositories;

public class ScreenshotRepository : IScreenshotRepository
{
    public int AddScreenshot(Screenshot screenshot, Game game)
    {
        using var connection = Database.GetConnection();
        var command = new MySqlCommand("INSERT INTO screenshot (game_id, image_url) VALUES (@game_id, @image_url);" +
            "SELECT LAST_INSERT_ID();", connection);
        command.Parameters.AddWithValue("@game_id", game.Id);
        command.Parameters.AddWithValue("@image_url", screenshot.ImageUrl);

        var newId = Convert.ToInt32(command.ExecuteScalar());
        return newId;
    }
    
    public IEnumerable<Screenshot> GetAllGameScreenshots(int gameId)
    {
        var urls = new List<Screenshot>();
        using var connection = Database.GetConnection();
        var command = new MySqlCommand("SELECT * FROM screenshot WHERE game_id = @id", connection);
        command.Parameters.AddWithValue("@id", gameId);
        
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            urls.Add(new Screenshot
            {
                Id = reader.GetInt32("screenshot_id"),
                ImageUrl = reader.GetString("image_url")
            });
        }
        
        return urls;
    }

    public void DeleteScreenshot(int? screenshotId)
    {
        using var connection = Database.GetConnection();
        var command = new MySqlCommand("DELETE FROM screenshot WHERE screenshot_id = @id", connection);
        command.Parameters.AddWithValue("@id", screenshotId);
        command.ExecuteNonQuery();
    }
}