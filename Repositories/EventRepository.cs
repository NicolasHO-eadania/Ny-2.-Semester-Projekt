using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace _2.semEksamenProjekt
{
    public class EventRepository
    {
        string connectionString = $"Data Source={AppDomain.CurrentDomain.BaseDirectory}2.sem projekt.db";

        // Create: nyt event i databasen
        public void NewEvent(Event ev)
        {
            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            using SqliteCommand command = new SqliteCommand("INSERT INTO Event (Title, Start, End, Description, City, FlowId) VALUES (@title, @start, @end, @description, @city, @flowId); SELECT last_insert_rowid();", connection);

            command.Parameters.AddWithValue("@title", ev.title);
            command.Parameters.AddWithValue("@start", ev.start.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@end", ev.end.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@description", ev.description ?? "");
            command.Parameters.AddWithValue("@city", ev.city ?? "");
            command.Parameters.AddWithValue("@flowId", ev.flowId.HasValue ? (object)ev.flowId.Value : DBNull.Value);

            ev.id = Convert.ToInt32(command.ExecuteScalar());

            InsertRooms(connection, ev);
            InsertTags(connection, ev);
            InsertTeachers(connection, ev);
            InsertTeams(connection, ev);
        }

        // Read: hent alle events fra databasen
        public List<Event> GetAllEvents()
        {
            List<Event> events = new List<Event>();
            
            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            using SqliteCommand command = new SqliteCommand("SELECT Id, Title, Start, End, Description, City, FlowId FROM Event", connection);

            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Event ev = new Event
                {
                    id = reader.GetInt32(0),
                    title = reader.GetString(1),
                    start = DateTime.Parse(reader.GetString(2)),
                    end = DateTime.Parse(reader.GetString(3)),
                    description = reader.IsDBNull(4) ? null : reader.GetString(4),
                    city = reader.IsDBNull(5) ? null : reader.GetString(5),
                    flowId = reader.IsDBNull(6) ? null : reader.GetInt32(6)
                };

                ev.rooms = GetRooms(connection, ev.id);
                ev.tags = GetTags(connection, ev.id);
                ev.teachers = GetTeachers(connection, ev.id);
                ev.teams = GetTeams(connection, ev.id);

                events.Add(ev);
            }

            return events;
        }

        // Update: opdater et eksisterende event i databasen
        public void UpdateEvent(Event ev)
        {
            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            using SqliteCommand command = new SqliteCommand("UPDATE Event SET Title = @title, Start = @start, End = @end, Description = @description, City = @city, FlowId = @flowId WHERE Id = @id", connection);

            command.Parameters.AddWithValue("@id", ev.id);
            command.Parameters.AddWithValue("@title", ev.title);
            command.Parameters.AddWithValue("@start", ev.start.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@end", ev.end.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@description", ev.description ?? "");
            command.Parameters.AddWithValue("@city", ev.city ?? "");
            command.Parameters.AddWithValue("@flowId", ev.flowId.HasValue ? (object)ev.flowId.Value : DBNull.Value);

            command.ExecuteNonQuery();

            DeleteRelatedRows(connection, ev.id);
            InsertRooms(connection, ev);
            InsertTags(connection, ev);
            InsertTeachers(connection, ev);
            InsertTeams(connection, ev);
        }

        // Delete: slet et event og relaterede rækker
        public void DeleteEvent(int id)
        {
            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            DeleteRelatedRows(connection, id);

            using SqliteCommand command = new SqliteCommand("DELETE FROM Event WHERE Id = @id", connection);

            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }

        // indsætter relaterede rækker i andre tabeller
        void InsertRooms(SqliteConnection connection, Event ev)
        {
            if (ev.rooms == null) return;

            foreach (string room in ev.rooms)
            {
                using SqliteCommand command = new SqliteCommand("INSERT INTO Event_Rooms (EventId, Room) VALUES (@id, @room)", connection);

                command.Parameters.AddWithValue("@id", ev.id);
                command.Parameters.AddWithValue("@room", room);
                command.ExecuteNonQuery();
            }
        }

        void InsertTags(SqliteConnection connection, Event ev)
        {
            if (ev.tags == null) return;

            foreach (string tag in ev.tags)
            {
                using SqliteCommand command = new SqliteCommand("INSERT INTO Event_Tags (EventId, Tag) VALUES (@id, @tag)", connection);

                command.Parameters.AddWithValue("@id", ev.id);
                command.Parameters.AddWithValue("@tag", tag);
                command.ExecuteNonQuery();
            }
        }

        void InsertTeachers(SqliteConnection connection, Event ev)
        {
            if (ev.teachers == null) return;

            foreach (User teacher in ev.teachers)
            {
                using SqliteCommand command = new SqliteCommand("INSERT INTO Event_Teachers (EventId, Username) VALUES (@id, @username)", connection);

                command.Parameters.AddWithValue("@id", ev.id);
                command.Parameters.AddWithValue("@username", teacher.username);
                command.ExecuteNonQuery();
            }
        }

        void InsertTeams(SqliteConnection connection, Event ev)
        {
            if (ev.teams == null) return;

            foreach (Team team in ev.teams)
            {
                using SqliteCommand command = new SqliteCommand("INSERT INTO Event_Teams (EventId, TeamName) VALUES (@id, @teamName)", connection);

                command.Parameters.AddWithValue("@id", ev.id);
                command.Parameters.AddWithValue("@teamName", team.teamName);
                command.ExecuteNonQuery();
            }
        }

        // læser relaterede rækker fra andre tabeller
        List<string> GetRooms(SqliteConnection connection, int eventId)
        {
            List<string> rooms = new List<string>();

            using SqliteCommand command = new SqliteCommand("SELECT Room FROM Event_Rooms WHERE EventId = @id", connection);

            command.Parameters.AddWithValue("@id", eventId);

            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                rooms.Add(reader.GetString(0));
            }

            return rooms;
        }

        List<string> GetTags(SqliteConnection connection, int eventId)
        {
            List<string> tags = new List<string>();

            using SqliteCommand command = new SqliteCommand("SELECT Tag FROM Event_Tags WHERE EventId = @id", connection);

            command.Parameters.AddWithValue("@id", eventId);

            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                tags.Add(reader.GetString(0));
            }

            return tags;
        }

        List<User> GetTeachers(SqliteConnection connection, int eventId)
        {
            List<User> teachers = new List<User>();

            using SqliteCommand command = new SqliteCommand("SELECT Username FROM Event_Teachers WHERE EventId = @id", connection);

            command.Parameters.AddWithValue("@id", eventId);

            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                teachers.Add(new User { username = reader.GetString(0) });
            }

            return teachers;
        }

        List<Team> GetTeams(SqliteConnection connection, int eventId)
        {
            List<Team> teams = new List<Team>();

            using SqliteCommand command = new SqliteCommand("SELECT TeamName FROM Event_Teams WHERE EventId = @id", connection);

            command.Parameters.AddWithValue("@id", eventId);

            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                teams.Add(new Team { teamName = reader.GetString(0) });
            }

            return teams;
        }

        // sletter relaterede rækker fra andre tabeller
        void DeleteRelatedRows(SqliteConnection connection, int eventId)
        {
            foreach (string table in new[] { "Event_Rooms", "Event_Tags", "Event_Teachers", "Event_Teams" })
            {
                using SqliteCommand command = new SqliteCommand($"DELETE FROM {table} WHERE EventId = @id", connection);

                command.Parameters.AddWithValue("@id", eventId);
                command.ExecuteNonQuery();
            }
        }
    }
}