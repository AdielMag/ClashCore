using App.Scripts.Command;

using UnityEngine;

using VContainer;

namespace App.SubDomains.Lobby.SubDomains.LobbyPlayButton
{
    public class LobbyPlayButton : MonoBehaviour
    {
        private CommandPool<PlayCommand> _playCommandPool;

        [Inject]
        public void Inject(CommandPool<PlayCommand> commandPool)
        {
            _playCommandPool = commandPool;
        }

        public void OnClick()
        {
            _playCommandPool.Get().ExecuteAsync();
        }
    }
}