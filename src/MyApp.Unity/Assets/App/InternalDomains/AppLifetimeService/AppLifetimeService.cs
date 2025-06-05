using System;

using App.InternalDomains.DebugService;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

using VContainer;
using VContainer.Unity;

namespace App.InternalDomains.AppLifetimeService
{
    public class AppLifetimeService : IAppLifetimeService, IInitializable, IDisposable
    {
        private bool _isPaused;
        private DateTime? _pauseStartTime;

        public event EventHandler<ApplicationPauseEventArgs> OnApplicationPause;
        public event EventHandler<ApplicationQuitEventArgs> OnApplicationQuit;
        public event EventHandler<ApplicationResumeEventArgs> OnApplicationResume;
        
        public bool IsPaused => _isPaused;

        [Inject] private readonly IDebugService _debugService;
        
        public void Initialize()
        {
            Application.focusChanged += HandleFocusChanged;
            Application.quitting += HandleApplicationQuit;
            
#if UNITY_EDITOR
            EditorApplication.focusChanged += HandleFocusChanged;
            EditorApplication.quitting += HandleApplicationQuit;
#endif
            
            
            _debugService.Log("AppLifeTimeManager initialized");
        }

        public void Dispose()
        {
            Application.focusChanged -= HandleFocusChanged;
            Application.quitting -= HandleApplicationQuit;
            
#if UNITY_EDITOR
            EditorApplication.focusChanged -= HandleFocusChanged;
            EditorApplication.quitting -= HandleApplicationQuit;
#endif
            OnApplicationPause = null;
            OnApplicationQuit = null;
            OnApplicationResume = null;

            _debugService.Log("AppLifeTimeManager disposed");
        }

        private void HandleFocusChanged(bool hasFocus)
        {
            switch (hasFocus)
            {
                case false when !_isPaused:
                {
                    _isPaused = true;
                    _pauseStartTime = DateTime.UtcNow;
                
                    var pauseArgs = new ApplicationPauseEventArgs(_isPaused);
                    OnApplicationPause?.Invoke(this, pauseArgs);
                
                    _debugService.Log($"Application paused at: {pauseArgs.Timestamp}");
                    break;
                }

                case true when _isPaused:
                {
                    _isPaused = false;
                
                    var pauseDuration = TimeSpan.Zero;
                    if (_pauseStartTime.HasValue)
                    {
                        pauseDuration = DateTime.UtcNow - _pauseStartTime.Value;
                    }
                
                    var resumeArgs = new ApplicationResumeEventArgs(pauseDuration);
                    OnApplicationResume?.Invoke(this, resumeArgs);
                
                    _pauseStartTime = null;
                
                    _debugService.Log($"Application resumed at: {resumeArgs.Timestamp}. Pause duration: {pauseDuration.TotalSeconds:F2} seconds");
                    break;
                }
            }
        }

        private void HandleApplicationQuit()
        {
            var quitArgs = new ApplicationQuitEventArgs();
            OnApplicationQuit?.Invoke(this, quitArgs);
            
            _debugService.Log($"Application quitting at: {quitArgs.Timestamp}");
        }
    }
    
    public class ApplicationPauseEventArgs : EventArgs
    {
        public DateTime Timestamp { get; }
        public bool IsPaused { get; }

        public ApplicationPauseEventArgs(bool isPaused)
        {
            Timestamp = DateTime.UtcNow;
            IsPaused = isPaused;
        }
    }

    public class ApplicationQuitEventArgs : EventArgs
    {
        public DateTime Timestamp { get; }

        public ApplicationQuitEventArgs()
        {
            Timestamp = DateTime.UtcNow;
        }
    }

    public class ApplicationResumeEventArgs : EventArgs
    {
        public DateTime Timestamp { get; }
        public TimeSpan PauseDuration { get; }

        public ApplicationResumeEventArgs(TimeSpan pauseDuration)
        {
            Timestamp = DateTime.UtcNow;
            PauseDuration = pauseDuration;
        }
    }
}