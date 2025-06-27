using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace App.Scripts.Command
{
    public abstract class CommandBase : IDisposable
    {
        private bool _isDisposed;

        public virtual void Reset() {}

        public void Dispose()
        {
            InternalDispose();
            GC.SuppressFinalize(this);
        }

        protected virtual void InternalDispose()
        {
            if (_isDisposed)
            {
                return;
            }
            
            _isDisposed = true;
        }
    }

    public abstract class Command : CommandBase, ICommand
    {
        public Command() {}
        
        public abstract UniTask ExecuteAsync(CancellationToken cancellationToken = default);
    }
    
    public abstract class Command<T> : CommandBase, ICommand<T>
    {
        public abstract UniTask<T> ExecuteAsync(CancellationToken cancellationToken = default);
    }
}