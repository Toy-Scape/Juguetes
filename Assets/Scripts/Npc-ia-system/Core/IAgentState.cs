namespace Core
{
    public interface IAgentState
    {
        void Enter();
        void Update();
        void Exit();
    }
}