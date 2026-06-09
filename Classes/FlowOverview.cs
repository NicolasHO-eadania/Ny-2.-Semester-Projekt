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

        public List<Flow> FilterByTag(string tag)
        {
            List<Flow> result = new List<Flow>();

            foreach (Flow flow in AllFlows)
            {
                if (flow.tags != null && flow.tags.Contains(tag))
                    result.Add(flow);
            }

            return result;
        }
    }
}
