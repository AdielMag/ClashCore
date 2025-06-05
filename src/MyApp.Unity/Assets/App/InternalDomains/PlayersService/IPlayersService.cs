using Cysharp.Threading.Tasks;

namespace App.InternalDomains.PlayersService
{
    public interface IPlayersService
    {
        UniTask Login();
    }
}