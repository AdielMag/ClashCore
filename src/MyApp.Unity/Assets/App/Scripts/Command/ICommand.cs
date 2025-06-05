using System.Threading;

using Cysharp.Threading.Tasks;

namespace App.Scripts.Command
{
    public interface ICommand
    {
        UniTask ExecuteAsync(CancellationToken cancellationToken = default);
    }
    
    public interface ICommand<T>
    {
        UniTask<T> ExecuteAsync(CancellationToken cancellationToken = default);
    }
}