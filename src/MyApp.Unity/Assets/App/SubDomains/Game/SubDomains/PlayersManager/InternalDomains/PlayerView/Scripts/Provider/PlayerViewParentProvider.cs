using App.SubDomains.Game.SubDomains.PlayerView.Scripts.Interface;

using UnityEngine;

namespace App.SubDomains.Game.SubDomains.PlayerView.Scripts.Provider
{
    public class PlayerViewParentProvider : MonoBehaviour, IPlayerViewParentProvider
    {
        public Transform PlayerViewParent => transform;
    }
}