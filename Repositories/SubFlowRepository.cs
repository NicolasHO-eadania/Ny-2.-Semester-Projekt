using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace _2.semEksamenProjekt
{
    public class SubFlowRepository
    {
        string connectionString = $"Data Source={AppDomain.CurrentDomain.BaseDirectory}2.sem projekt.db";

        // Read: henter alle subflows for et bestemt flow
        public List<SubFlow> GetSubFlowsByFlowId(int flowId)
        {
            List<SubFlow> subFlows = new List<SubFlow>();

            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            using SqliteCommand command = new SqliteCommand("SELECT Id, FlowId, ParentId, Heading, Text FROM SubFlow WHERE FlowId = @flowId", connection);

            command.Parameters.AddWithValue("@flowId", flowId);

            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                subFlows.Add(new SubFlow
                {
                    id = reader.GetInt32(0),
                    flowId = reader.GetInt32(1),
                    parentId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                    heading = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    text = reader.IsDBNull(4) ? "" : reader.GetString(4),
                });
            }

            return subFlows;
        }

        // Create: indsæt et nyt subflow
        public void NewSubFlow(SubFlow sf)
        {
            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            using SqliteCommand command = new SqliteCommand("INSERT INTO SubFlow (FlowId, ParentId, Heading, Text) VALUES (@flowId, @parentId, @heading, @text)", connection);

            command.Parameters.AddWithValue("@flowId", sf.flowId);
            command.Parameters.AddWithValue("@parentId", sf.parentId.HasValue ? (object)sf.parentId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@heading", sf.heading ?? "");
            command.Parameters.AddWithValue("@text", sf.text ?? "");

            command.ExecuteNonQuery();
        }

        // Update: opdater et eksisterende subflow
        public void UpdateSubFlow(SubFlow sf)
        {
            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            using SqliteCommand command = new SqliteCommand("UPDATE SubFlow SET ParentId = @parentId, Heading = @heading, Text = @text WHERE Id = @id", connection);

            command.Parameters.AddWithValue("@id", sf.id);
            command.Parameters.AddWithValue("@parentId", sf.parentId.HasValue ? (object)sf.parentId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@heading", sf.heading ?? "");
            command.Parameters.AddWithValue("@text", sf.text ?? "");

            command.ExecuteNonQuery();
        }

        // Delete: slet et subflow og alle dets børn rekursivt
        public void DeleteSubFlow(int id)
        {
            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            DeleteRecursive(connection, id);
        }

        // sletter subflowet og alle underliggende subflows
        void DeleteRecursive(SqliteConnection connection, int id)
        {
            // find og slet børn først
            using SqliteCommand findChildren = new SqliteCommand("SELECT Id FROM SubFlow WHERE ParentId = @id", connection);

            findChildren.Parameters.AddWithValue("@id", id);

            List<int> childIds = new List<int>();
            using (SqliteDataReader reader = findChildren.ExecuteReader())
            {
                while (reader.Read())
                {
                    childIds.Add(reader.GetInt32(0));
                }
            }
           
            foreach (int childId in childIds)
            {
                DeleteRecursive(connection, childId);
            }

            // slet selve subflowet
            using SqliteCommand deleteCommand = new SqliteCommand("DELETE FROM SubFlow WHERE Id = @id", connection);

            deleteCommand.Parameters.AddWithValue("@id", id);
            deleteCommand.ExecuteNonQuery();
        }
    }
}
