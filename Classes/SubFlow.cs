namespace _2.semEksamenProjekt
{
    public class SubFlow
    {
        public int id;
        public int flowId;
        public int? parentId;  // bruges til mappe struktur, null = yderst
        public string heading;
        public string text;

        // gør at dropdown i NewSubFlowWindow viser heading
        public override string ToString()
        {
            return heading;
        }
    }
}
