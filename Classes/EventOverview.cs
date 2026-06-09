using System;
using System.Collections.Generic;

namespace _2.semEksamenProjekt
{
    public class EventOverview
    {
        public List<Event> AllEvents;

        // constructor
        public EventOverview()
        {
            AllEvents = new List<Event>();
        }

        public List<Event> FilterByTag(string tag)
        {
            List<Event> result = new List<Event>();

            foreach (Event e in AllEvents)
            {
                if (e.tags != null && e.tags.Contains(tag))
                    result.Add(e);
            }

            return result;
        }
    }
}
