using Shared.Data;

namespace Shared.Hubs
{
    public interface IGameHubReceiver
    {
        void OnJoin(TransformData transformData);
        void OnLeave(TransformData transformData);
        void OnMove(TransformData transformData);
    }
}