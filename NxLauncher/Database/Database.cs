using System;
using MySql.Data.MySqlClient;

namespace NxLauncher.Database;

public class Database
{
    private const string ConnectionString = 
        "Server=localhost;" + 
        "Port=3306;" + 
        "Database=NxLauncher;" +
        "user ID=app_user;" +
        "password=appuser;" +
        "SslMode=Preferred;";
    
    public static MySqlConnection GetConnection()
     {
         try
         {
             var connection = new MySqlConnection(ConnectionString);
             connection.Open();
             return connection;
         }
         catch (Exception ex)
         {
             throw new Exception("Помилка: " + ex.Message, ex);
         }
     }
    
    public static void TestConnection()
    {
        try
        {
            using var connection = GetConnection();
            Console.WriteLine("Подключение успешно! Статус: " + connection.State);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка подключения: " + ex.Message);
        }
    }
}