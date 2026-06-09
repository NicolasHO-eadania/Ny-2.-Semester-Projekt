using System.Collections.Generic;
using System;
using Microsoft.Data.Sqlite;

namespace _2.semEksamenProjekt
{
    public class FlowRepository
    {
        string connectionString = $"Data Source={AppDomain.CurrentDomain.BaseDirectory}2.sem projekt.db";

        // Read: hent alle flows fra databasen
        public List<Flow> GetAllFlows()
        {
            List<Flow> flows = new List<Flow>();

            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            using SqliteCommand command = new SqliteCommand(
                "SELECT Id, Title, Image FROM Flow",
                connection);

            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Flow flow = new Flow
                {
                    id    = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    image = reader.IsDBNull(2) ? null : reader.GetString(2)
                };

                flows.Add(flow);
            }

            return flows;
        }
    }
}
