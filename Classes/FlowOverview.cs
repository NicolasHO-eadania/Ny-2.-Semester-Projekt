using System.Collections.Generic;

namespace _2.semEksamenProjekt
{
    public class FlowOverview
    {
        public List<Flow> AllFlows;

        public FlowOverview()
        {
            AllFlows = new List<Flow>();
        }

        // insertion sort. sorterer AllFlows alfabetisk efter titel
        public void SortByTitle()
        {
            for (int i = 1; i < AllFlows.Count; i++)
            {
                Flow val = AllFlows[i];
                int pointer = i;

                while (pointer > 0 && string.Compare(val.Title, AllFlows[pointer - 1].Title) < 0)
                {
                    AllFlows[pointer] = AllFlows[pointer - 1];
                    pointer--;
                }

                AllFlows[pointer] = val;
            }
        }
    }
}
