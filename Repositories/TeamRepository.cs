using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace _2.semEksamenProjekt
{
    public class TeamRepository
    {
        string connectionString = $"Data Source={AppDomain.CurrentDomain.BaseDirectory}2.sem projekt.db";

        public List<Team> GetAllTeams()
        {
            List<Team> teams = new List<Team>();

            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            using SqliteCommand command = new SqliteCommand("SELECT TeamName, Year, Education, City FROM Team", connection);

            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                teams.Add(new Team
                {
                    teamName = reader.GetString(0),
                    year = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                    education = reader.IsDBNull(2) ? null : reader.GetString(2),
                    city = reader.IsDBNull(3) ? null : reader.GetString(3)
                });
            }

            return teams;
        }
        // henter alle hold som en bruger er medlem af
        public List<Team> GetTeamsByUsername(string username)
        {
            List<Team> teams = new List<Team>();

            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            using SqliteCommand command = new SqliteCommand(
                "SELECT t.TeamName, t.Year, t.Education, t.City FROM Team t " +
                "INNER JOIN Team_User tu ON t.TeamName = tu.TeamName " +
                "WHERE tu.Username = @username",connection);

            command.Parameters.AddWithValue("@username", username);

            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                teams.Add(new Team
                {
                    teamName = reader.GetString(0),
                    year = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                    education = reader.IsDBNull(2) ? null : reader.GetString(2),
                    city = reader.IsDBNull(3) ? null : reader.GetString(3)
                });
            }

            return teams;
        }
    }
}
