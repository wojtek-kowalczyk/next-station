namespace NS.Visuals.PathFollowing
{
    public class TrainManager : PathFollowingAgentManager<Train>
    {
        public int TrainsAlive => AgentsAlive;
    }
}
