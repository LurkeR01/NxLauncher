using System;
using MySql.Data.MySqlClient;
using NxLauncher.Models;

namespace NxLauncher.Database.Repositories;

public class UserRepository : IUserRepository
{
    public User? GetByUserName(string userName)
    {
        User? user = null;
        using var connection = Database.GetConnection();

        var command = new MySqlCommand("SELECT * FROM user WHERE user_name = @username LIMIT 1", connection);
        command.Parameters.AddWithValue("@username", userName);
        
        try
        {
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                user = new User
                {
                    UserId = reader.GetInt32("user_id"),
                    UserName = reader.GetString("user_name"),
                    Email = reader.GetString("user_mail"),
                    PasswordHash = reader.GetString("user_password"), 
                    Age = reader.GetInt32("user_age"),
                    Role = Enum.Parse<UserRole>(reader.GetString("user_role"), true)
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении пользователя: {ex.Message}");
        }
        return user;
    }

    public void AddUser(User user)
    {
        using var connection = Database.GetConnection();
        var command = new MySqlCommand("INSERT INTO user (user_name, user_mail, user_password, user_age, user_role) VALUES (@user_name, @user_mail, @user_password, @user_age, @user_role)", connection);
        command.Parameters.AddWithValue("@user_name", user.UserName);
        command.Parameters.AddWithValue("@user_mail", user.Email);
        command.Parameters.AddWithValue("@user_password", user.PasswordHash);
        command.Parameters.AddWithValue("@user_age", user.Age);
        command.Parameters.AddWithValue("@user_role", user.Role.ToString());
        
        try
        {
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при добавлении пользователя: {ex.Message}");
            throw;
        }
    }
}