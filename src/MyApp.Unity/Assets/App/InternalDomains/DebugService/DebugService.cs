namespace App.InternalDomains.DebugService
{
    public class DebugService : IDebugService
    {
        void IDebugService.Log(string message)
        {
            UnityEngine.Debug.Log(message);
        }
        
        void IDebugService.LogWarning(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        }
        
        void IDebugService.LogError(string message)
        {
            UnityEngine.Debug.LogError(message);
        }
    }
}