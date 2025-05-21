using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using MySql.Data.MySqlClient;
using NxLauncher.Models;

namespace NxLauncher.Database.Repositories;

public class GenreRepository : IGenreRepository
{
    public IEnumerable<Genre> GetAll()
    {
        var genres = new ObservableCollection<Genre>();
        
        using var connection = Database.GetConnection();
        var command = new MySqlCommand("SELECT * FROM genre", connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            genres.Add( new Genre
            {
                Id = reader.GetInt32("genre_id"),
                Name = reader.GetString("genre_name"),
            });
        }
        
        return genres;
    }

    public Genre GetById(int id)
    {
        using var connection = Database.GetConnection();
        var command = new MySqlCommand("SELECT * FROM genre WHERE id = @id", connection);
        command.Parameters.AddWithValue("@id", id);
        
        using var reader = command.ExecuteReader();
        reader.Read();
        return new Genre
        {
            Id = reader.GetInt32("genre_id"),
            Name = reader.GetString("genre_name")
        };
    }

    public IEnumerable<Genre> GetAllGameGenres(int gameId)
    {
        var genres = new ObservableCollection<Genre>();
        
        using var connection = Database.GetConnection();
        var command = new MySqlCommand("""
                                       SELECT gr.genre_name, gr.genre_id
                                       FROM genre gr
                                       JOIN game_genre gg ON gr.genre_id = gg.genre_id
                                       JOIN game g ON gg.game_id = g.game_id
                                       WHERE g.game_id = @game_id
                                       """, connection);
        
        command.Parameters.AddWithValue("@game_id", gameId);
        
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            genres.Add(new Genre
            {
                Id = reader.GetInt32("genre_id"),
                Name = reader.GetString("genre_name")
            });
        }
        
        return genres;
    }

    public void Add(Genre genre)
    {
        using var connection = Database.GetConnection();
        var command = new MySqlCommand("INSERT INTO genre (genre_name) VALUES (@genre_name);SELECT LAST_INSERT_ID();", connection);
        command.Parameters.AddWithValue("@genre_name", genre.Name);
        
        genre.Id = Convert.ToInt32(command.ExecuteScalar());
    }
}