using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace _2.semEksamenProjekt
{
    public class UserRepository
    {
        string connectionString = $"Data Source={AppDomain.CurrentDomain.BaseDirectory}2.sem projekt.db";
        // finder en bruger på brugernavn og adgangskode
        public User GetUserByCredentials(string username, string password)
        {
            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            using SqliteCommand command = new SqliteCommand("SELECT Username, Password, Role FROM User WHERE Username = @username AND Password = @password", connection);

            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", password);

            using SqliteDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    username = reader.GetString(0),
                    password = reader.GetString(1),
                    role = reader.IsDBNull(2) ? null : reader.GetString(2)
                };
            }

            return null;
        }
        public List<User> GetUsersByRole(string role)
        {
            // samme som GetAllUsers men med WHERE Role = @role
            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            using SqliteCommand command = new SqliteCommand("SELECT Username, Password, Role FROM User WHERE Role = @role", connection);

            command.Parameters.AddWithValue("@role", role);

            using SqliteDataReader reader = command.ExecuteReader();
            List<User> users = new List<User>();
            while (reader.Read())
            {
                users.Add(new User
                {
                    username = reader.GetString(0),
                    password = reader.IsDBNull(1) ? null : reader.GetString(1),
                    role = reader.IsDBNull(2) ? null : reader.GetString(2)
                });
            }
            return users;
        }
    }
}
