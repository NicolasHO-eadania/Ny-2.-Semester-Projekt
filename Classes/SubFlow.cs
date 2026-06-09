namespace _2.semEksamenProjekt
{
    public class SubFlow
    {
        public int    id;
        public int    flowId;
        public int?   parentId;  // null = topniveau, ellers Id på forælderen
        public string heading;
        public string text;
        public string file;
    }
}
