using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace _2.semEksamenProjekt
{
    public class SubFlowRepository
    {
        string connectionString = $"Data Source={AppDomain.CurrentDomain.BaseDirectory}2.sem projekt.db";

        // henter alle subflows for et bestemt flow
        public List<SubFlow> GetSubFlowsByFlowId(int flowId)
        {
            List<SubFlow> subFlows = new List<SubFlow>();

            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            using SqliteCommand command = new SqliteCommand(
                "SELECT Id, FlowId, ParentId, Heading, Text, File FROM SubFlow WHERE FlowId = @flowId",
                connection);

            command.Parameters.AddWithValue("@flowId", flowId);

            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                subFlows.Add(new SubFlow
                {
                    id       = reader.GetInt32(0),
                    flowId   = reader.GetInt32(1),
                    parentId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                    heading  = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    text     = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    file     = reader.IsDBNull(5) ? null : reader.GetString(5)
                });
            }

            return subFlows;
        }
    }
}
