using Cysharp.Threading.Tasks;

using MagicOnion;

namespace App.InternalDomains.NetworkService
{
    public interface INetworkService
    {
        void CreateMatchChannel(string url);
        
        UniTask<TService> GetService<TService>() where TService : IService<TService>;
        UniTask<THub> ConnectHub<THub, THubReceiver>(THubReceiver receiver)
            where THub : IStreamingHub<THub, THubReceiver>;
    }
}