using App.SubDomains.Game.SubDomains.PlayerController.Scripts.@interface;

namespace App.SubDomains.Game.SubDomains.PlayersManager
{
    public interface ILocalPlayerProvider
    {
        IPlayerController LocalPlayerController { get; }
    }
}