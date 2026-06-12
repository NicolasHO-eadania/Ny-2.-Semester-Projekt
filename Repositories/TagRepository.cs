using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace _2.semEksamenProjekt
{
    public class TagRepository
    {
        string connectionString = $"Data Source={AppDomain.CurrentDomain.BaseDirectory}2.sem projekt.db";

        // henter tags der gælder for events (Event eller Both)
        public List<string> GetEventTags()
        {
            return GetTags("Event");
        }

        // henter tags der gælder for flows (Flow eller Both)
        public List<string> GetFlowTags()
        {
            return GetTags("Flow");
        }

        List<string> GetTags(string type)
        {
            List<string> tags = new List<string>();

            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            using SqliteCommand command = new SqliteCommand("SELECT Name FROM Tag WHERE Type = @type OR Type = 'Both' ORDER BY Name", connection);

            command.Parameters.AddWithValue("@type", type);

            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                tags.Add(reader.GetString(0));
            }

            return tags;
        }
    }
}
