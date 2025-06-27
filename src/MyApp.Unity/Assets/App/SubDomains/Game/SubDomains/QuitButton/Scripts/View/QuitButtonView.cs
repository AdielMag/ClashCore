using App.Scripts.Command;
using App.SubDomains.Game.Scripts.Command;

using Cysharp.Threading.Tasks;

using UnityEngine;

using VContainer;

namespace App.SubDomains.Game.SubDomains.QuitButton.Scripts
{
    public class QuitButtonView : MonoBehaviour
    {
        private CommandPool<QuitGameCommand> _quitCommandPool;
    
        [Inject]
        public void Inject(CommandPool<QuitGameCommand> commandPool)
        {
            _quitCommandPool = commandPool;
        }
        
        public void OnClick()
        {
            _quitCommandPool.Get().ExecuteAsync().Forget();
        }
    }
}