using App.SubDomains.Game.SubDomains.MovementAnimator.Scripts;

using UnityEngine;

namespace App.SubDomains.Game.SubDomains.PlayerView.Scripts.View
{
    public class PlayerView : Transformable.Transformable
    {
        [SerializeField] private PlayerVisualController playerVisualController;

        public PlayerVisualController PlayerVisualController => playerVisualController;
    }
}