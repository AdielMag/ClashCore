using App.Scripts.Command;
using App.SubDomains.Lobby.SubDomains.LobbyPlayButton;

using VContainer;
using VContainer.Unity;

namespace App.SubDomains.Lobby.Scripts
{
    public class LobbyLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            RegisterCommands(builder);
            
            builder.RegisterBuildCallback(resolver =>
            {
                var commandPool = resolver.Resolve<CommandPool<PlayCommand>>();
                commandPool.SetPoolSize(1);
            });
        }

        private void RegisterCommands(IContainerBuilder builder)
        {
            builder.Register<PlayCommand>(Lifetime.Scoped);
            builder.Register<CommandPool<PlayCommand>>(Lifetime.Singleton);
        }
    }
}