using System;
using System.Collections.Generic;

namespace _2.semEksamenProjekt
{
    public class Event
    {
        public int id;
        public int? flowId; // tilknyttet flow (null = intet flow)
        public string title;
        public string description;
        public List<string> rooms;
        public DateTime start;
        public DateTime end;
        public List<User> teachers;
        public string city;
        public List<Team> teams;
        public List<string> tags;
    }
}
