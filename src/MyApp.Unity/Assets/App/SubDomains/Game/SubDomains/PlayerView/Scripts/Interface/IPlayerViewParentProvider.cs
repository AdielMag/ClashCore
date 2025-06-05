using UnityEngine;

namespace App.SubDomains.Game.SubDomains.PlayerView.Scripts.Interface
{
    public interface IPlayerViewParentProvider
    {
        Transform PlayerViewParent { get; }
    }
}