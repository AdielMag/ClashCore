using System;

namespace App.InternalDomains.AppLifetimeService
{
    public interface IAppLifetimeService
    {
        event EventHandler<ApplicationPauseEventArgs> OnApplicationPause;
        event EventHandler<ApplicationQuitEventArgs> OnApplicationQuit;
        event EventHandler<ApplicationResumeEventArgs> OnApplicationResume;
        
        bool IsPaused { get; }
    }
}