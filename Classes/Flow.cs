using System.Collections.Generic;

namespace _2.semEksamenProjekt
{
    public class Flow
    {
        public int id;
        public string Title;
        public string image;
        public List<string> tags;
        public List<User> teachers;
        public Team team;
        public List<SubFlowText> TextSubFlows;
        public List<SubFlowFile> FileSubFlows;

        public void AddFlow(FlowOverview overview)
        {
            overview.AllFlows.Add(this);
        }

        public void DeleteFlow(FlowOverview overview)
        {
            overview.AllFlows.Remove(this);
        }

        public void EditFlow(FlowOverview overview, Flow oldFlow)
        {
            int index = overview.AllFlows.IndexOf(oldFlow);
            if (index >= 0)
                overview.AllFlows[index] = this;
        }
    }
}
