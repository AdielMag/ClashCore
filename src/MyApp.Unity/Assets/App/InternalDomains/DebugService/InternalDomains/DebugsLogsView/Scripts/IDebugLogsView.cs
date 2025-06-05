namespace App.InternalDomains.DebugService.InternalDomains.DebugsLogsView.Scripts
{
    public interface IDebugLogsView
    {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}