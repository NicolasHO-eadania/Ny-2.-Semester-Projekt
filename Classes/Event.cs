using System;
using System.Collections.Generic;

namespace _2.semEksamenProjekt
{
    public class Event
    {
        public int id;
        public string title;
        public string description;
        public List<string> rooms;
        public DateTime start;
        public DateTime end;
        public Flow flowLink;
        public List<User> teachers;
        public string city;
        public List<Team> teams;
        public List<string> tags;

        // tilføjer event til event overviewet
        public void AddEvent(EventOverview overview)
        {
            overview.AllEvents.Add(this);
        }

        // fjerner event fra event overviewet
        public void DeleteEvent(EventOverview overview)
        {
            overview.AllEvents.Remove(this);
        }

        // erstatter et eksisterende event i overviewet med dette events opdaterede værdier
        public void EditEvent(EventOverview overview, Event oldEvent)
        {
            int index = overview.AllEvents.IndexOf(oldEvent);
            if (index >= 0)
                overview.AllEvents[index] = this;
        }
    }
}
