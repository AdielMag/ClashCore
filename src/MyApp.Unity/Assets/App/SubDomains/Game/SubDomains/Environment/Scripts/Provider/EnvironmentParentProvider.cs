using App.SubDomains.Game.SubDomains.Environment.Scripts.Interface;

using UnityEngine;

namespace App.SubDomains.Game.SubDomains.Environment.Scripts.Provider
{
    public class EnvironmentParentProvider : MonoBehaviour, IEnvironmentParentProvider
    {
        public Transform EnvironmentParent => transform;
    }
}