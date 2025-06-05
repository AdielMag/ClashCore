using MagicOnion;

namespace Shared.Services
{
    public interface IPlayersService : IService<IPlayersService>
    {
        UnaryResult<string> RegisterAsync();
        UnaryResult LoginAsync(string id);
    }
}