using App.InternalDomains.DebugService.InternalDomains.DebugsLogsView;
using App.InternalDomains.DebugService.InternalDomains.DebugsLogsView.Scripts;

using VContainer;

namespace App.InternalDomains.DebugService
{
    public class DebugService : IDebugService
    {
        [Inject] private readonly IDebugLogsView _debugLogsView;
        
        void IDebugService.Log(string message)
        {
            _debugLogsView.Log(message);
            UnityEngine.Debug.Log(message);
        }
        
        void IDebugService.LogWarning(string message)
        {
            _debugLogsView.LogWarning(message);
            UnityEngine.Debug.LogWarning(message);
        }
        
        void IDebugService.LogError(string message)
        {
            _debugLogsView.LogError(message);
            UnityEngine.Debug.LogError(message);
        }
    }
}