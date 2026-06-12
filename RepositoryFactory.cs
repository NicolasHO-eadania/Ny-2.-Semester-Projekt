namespace _2.semEksamenProjekt
{
    // implementation af IRepositoryFactory.
    // opretter repositories
    public class RepositoryFactory : IRepositoryFactory
    {
        public EventRepository CreateEventRepository() => new EventRepository();
        public FlowRepository CreateFlowRepository() => new FlowRepository();
        public SubFlowRepository CreateSubFlowRepository() => new SubFlowRepository();
        public UserRepository CreateUserRepository() => new UserRepository();
        public TeamRepository CreateTeamRepository() => new TeamRepository();
        public TagRepository CreateTagRepository() => new TagRepository();
    }
}
