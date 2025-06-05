namespace Server.Hubs.GamingHub.Managers.ConnectionManager
{
    public interface IConnectionManager
    {
        void CancelDisconnectionTimer(string playerId);
        Task StartDisconnectionTimer(string playerId, Func<Task> onTimeout);
        void Leave();
        void Cleanup(string playerId);
    }
}