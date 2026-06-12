using System;
using System.Collections.Generic;
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

            using SqliteCommand command = new SqliteCommand("SELECT Id, Title, TeamName, Color FROM Flow", connection);

            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Flow flow = new Flow
                {
                    id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    team = reader.IsDBNull(2) ? null : new Team { teamName = reader.GetString(2) },
                    color = reader.IsDBNull(3) ? null : reader.GetString(3)
                };

                flow.tags = GetTags(connection, flow.id);
                flow.teachers = GetTeachers(connection, flow.id);

                flows.Add(flow);
            }

            return flows;
        }

        // Read: hent et enkelt flow baserert på id
        public Flow GetFlowById(int id)
        {
            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            using SqliteCommand command = new SqliteCommand("SELECT Id, Title, TeamName, Color FROM Flow WHERE Id = @id", connection);

            command.Parameters.AddWithValue("@id", id);

            using SqliteDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                Flow flow = new Flow
                {
                    id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    team  = reader.IsDBNull(2) ? null : new Team { teamName = reader.GetString(2) },
                    color = reader.IsDBNull(3) ? null : reader.GetString(3)
                };

                flow.tags     = GetTags(connection, flow.id);
                flow.teachers = GetTeachers(connection, flow.id);

                return flow;
            }

            return null;
        }

        // Create: indsæt et nyt flow i databasen
        public void NewFlow(Flow flow)
        {
            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            using SqliteCommand command = new SqliteCommand("INSERT INTO Flow (Title, TeamName, Color) VALUES (@title, @teamName, @color); SELECT last_insert_rowid();", connection);

            command.Parameters.AddWithValue("@title", flow.Title ?? "");
            command.Parameters.AddWithValue("@teamName", flow.team != null ? (object)flow.team.teamName : DBNull.Value);
            command.Parameters.AddWithValue("@color", flow.color != null ? (object)flow.color : DBNull.Value);

            flow.id = Convert.ToInt32(command.ExecuteScalar());

            InsertTags(connection, flow);
            InsertTeachers(connection, flow);
        }

        // Update: opdater et eksisterende flow
        public void UpdateFlow(Flow flow)
        {
            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            using SqliteCommand command = new SqliteCommand("UPDATE Flow SET Title = @title, TeamName = @teamName, Color = @color WHERE Id = @id", connection);

            command.Parameters.AddWithValue("@id", flow.id);
            command.Parameters.AddWithValue("@title", flow.Title ?? "");
            command.Parameters.AddWithValue("@teamName", flow.team != null ? (object)flow.team.teamName : DBNull.Value);
            command.Parameters.AddWithValue("@color", flow.color != null ? (object)flow.color : DBNull.Value);

            command.ExecuteNonQuery();

            DeleteRelatedRows(connection, flow.id);
            InsertTags(connection, flow);
            InsertTeachers(connection, flow);
        }

        // Delete: slet et flow og relaterede rækker
        public void DeleteFlow(int id)
        {
            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            DeleteRelatedRows(connection, id);

            using SqliteCommand command = new SqliteCommand("DELETE FROM Flow WHERE Id = @id", connection);

            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }

        void InsertTags(SqliteConnection connection, Flow flow)
        {
            if (flow.tags == null) return;

            foreach (string tag in flow.tags)
            {
                using SqliteCommand command = new SqliteCommand("INSERT INTO Flow_Tags (FlowId, Tag) VALUES (@id, @tag)", connection);

                command.Parameters.AddWithValue("@id", flow.id);
                command.Parameters.AddWithValue("@tag", tag);
                command.ExecuteNonQuery();
            }
        }

        void InsertTeachers(SqliteConnection connection, Flow flow)
        {
            if (flow.teachers == null) return;

            foreach (User teacher in flow.teachers)
            {
                using SqliteCommand command = new SqliteCommand("INSERT INTO Flow_User (FlowId, Username) VALUES (@id, @username)", connection);

                command.Parameters.AddWithValue("@id", flow.id);
                command.Parameters.AddWithValue("@username", teacher.username);
                command.ExecuteNonQuery();
            }
        }

        List<string> GetTags(SqliteConnection connection, int flowId)
        {
            List<string> tags = new List<string>();

            using SqliteCommand command = new SqliteCommand("SELECT Tag FROM Flow_Tags WHERE FlowId = @id", connection);

            command.Parameters.AddWithValue("@id", flowId);

            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                tags.Add(reader.GetString(0));
            }

            return tags;
        }

        List<User> GetTeachers(SqliteConnection connection, int flowId)
        {
            List<User> teachers = new List<User>();

            using SqliteCommand command = new SqliteCommand("SELECT Username FROM Flow_User WHERE FlowId = @id", connection);

            command.Parameters.AddWithValue("@id", flowId);

            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                teachers.Add(new User { username = reader.GetString(0) });
            }

            return teachers;
        }

        void DeleteRelatedRows(SqliteConnection connection, int flowId)
        {
            foreach (string table in new[] { "Flow_Tags", "Flow_User", "SubFlow" })
            {
                using SqliteCommand command = new SqliteCommand($"DELETE FROM {table} WHERE FlowId = @id", connection);

                command.Parameters.AddWithValue("@id", flowId);
                command.ExecuteNonQuery();
            }
        }
    }
}
