using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MySql.Data.MySqlClient;
using NxLauncher.Models;

namespace NxLauncher.Database.Repositories;

public class DeveloperRepository : IDeveloperRepository
{
    public IEnumerable<Developer> GetAll()
    {
        var developers = new ObservableCollection<Developer>();
        
        using var connection = Database.GetConnection();
        var command = new MySqlCommand("SELECT * FROM developer", connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            developers.Add( new Developer
            {
                DeveloperId = reader.GetInt32("developer_id"),
                DeveloperName = reader.GetString("developer_name")
            });
        }
        
        return developers;
    }
    
    public Developer GetById(int id)
    {
        using var connection = Database.GetConnection();
        var command = new MySqlCommand("SELECT * FROM developer WHERE developer_id = @id", connection);
        command.Parameters.AddWithValue("@id", id);
        
        using var reader = command.ExecuteReader();
        reader.Read();
        return new Developer
        {
            DeveloperId = reader.GetInt32("developer_id"),
            DeveloperName = reader.GetString("developer_name")
        };
    }

    public int GetIdByName(string name)
    {
        using var connection = Database.GetConnection();
        var command = new MySqlCommand("SELECT id FROM developer WHERE name = @name", connection);
        command.Parameters.AddWithValue("@name", name);
        
        using var reader = command.ExecuteReader();
        reader.Read();
        return reader.GetInt32("id");
    }

    public void Add(Developer developer)
    {
        using var connection = Database.GetConnection();
        var command = new MySqlCommand("INSERT INTO developer (developer_name) VALUES (@developer_name); SELECT LAST_INSERT_ID();", connection);
        command.Parameters.AddWithValue("@developer_name", developer.DeveloperName);
        
        developer.DeveloperId = Convert.ToInt32(command.ExecuteScalar());
    }
}