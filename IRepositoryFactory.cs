namespace _2.semEksamenProjekt
{
    // Abstract Factory interface definerer hvilke repositories der kan oprettes.
    public interface IRepositoryFactory
    {
        EventRepository CreateEventRepository();
        FlowRepository CreateFlowRepository();
        SubFlowRepository CreateSubFlowRepository();
        UserRepository CreateUserRepository();
        TeamRepository CreateTeamRepository();
        TagRepository CreateTagRepository();
    }
}
