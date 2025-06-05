using App.SubDomains.Game.SubDomains.Environment.Scripts.Interface;

using Cysharp.Threading.Tasks;

using UnityEngine;

using VContainer;
using VContainer.Unity;

namespace App.SubDomains.Game.SubDomains.Environment.Scripts.Manager
{
    public class EnvironmentManager : IEnvironmentManager
    {
        [Inject] private readonly IEnvironmentParentProvider _environmentParent;
        [Inject] private readonly IObjectResolver _objectResolver;
        
        public UniTask LoadEnvironment(string levelName)
        {
            var levelPrefab = Resources.Load<GameObject>($"Prefabs/Environments/{levelName}");
            _objectResolver.Instantiate(levelPrefab, _environmentParent.EnvironmentParent);
            return UniTask.CompletedTask;
        }
    }
}