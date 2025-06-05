namespace App.InternalDomains.PlayersService
{
    public interface IPlayerIdProvider
    {
        string PlayerId { get; }
    }
}