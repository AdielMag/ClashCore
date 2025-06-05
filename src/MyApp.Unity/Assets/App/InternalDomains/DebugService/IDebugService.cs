namespace App.InternalDomains.DebugService
{
    public interface IDebugService
    {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}