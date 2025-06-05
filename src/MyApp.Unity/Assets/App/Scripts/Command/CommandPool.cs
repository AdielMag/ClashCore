using System;
using System.Collections.Generic;
using VContainer;
using App.InternalDomains.DebugService;

namespace App.Scripts.Command
{
    public abstract class CommandPool<TCommand> where TCommand : ICommand, new()
    {
        [Inject] private IDebugService _debugService;
        
        private readonly List<TCommand> _pool;
        private readonly int _maxPoolSize;
        private readonly object _lockObject = new();

        protected CommandPool(int maxPoolSize)
        {
            _maxPoolSize = maxPoolSize;
            _pool = new List<TCommand>(maxPoolSize);
        }

        public TCommand Get()
        {
            lock (_lockObject)
            {
                if (_pool.Count > 0)
                {
                    var lastIndex = _pool.Count - 1;
                    var command = _pool[lastIndex];
                    _pool.RemoveAt(lastIndex);
                    return command;
                }
            }

            _debugService.Log($"Create new command: {typeof(TCommand).Name}");
            return new TCommand();
        }

        public void Return(TCommand command)
        {
            lock (_lockObject)
            {
                if (_pool.Count < _maxPoolSize)
                {
                    _debugService.Log($"Return command: {typeof(TCommand).Name}");
                    _pool.Add(command);
                }
                else
                {
                    _debugService.Log(
                        "The pool is full. Dispose the command. Can't return the command. Disposing the command.");
                    DisposeCommand(command);
                }
            }
        }

        protected virtual void DisposeCommand(TCommand command)
        {
            if (command is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}