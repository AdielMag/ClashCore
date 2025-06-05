using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Cysharp.Threading.Tasks;

using TMPro;

using UnityEngine;

namespace App.InternalDomains.DebugService.InternalDomains.DebugsLogsView.Scripts
{
    public class DebugLogsView : MonoBehaviour , IDebugLogsView
    {
        private const float _kLOGLifetime = 15f;

        [SerializeField] private TextMeshProUGUI logText;
        
        private readonly List<LogEntry> _logEntries = new ();
        private int _logCounter = 0;

        private CancellationTokenSource _cancellationTokenSource = new ();
        
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void OnDestroy()
        {
            _cancellationTokenSource.Cancel();
        }

        public void Log(string message)
        {
            AddLog(message, LogType.Log);
        }

        public void LogWarning(string message)
        {
            AddLog(message, LogType.Warning);
        }

        public void LogError(string message)
        {
            AddLog(message, LogType.Error);
        }

        private void AddLog(string message, LogType logType)
        {
            _logCounter++;
            _logEntries.Add(new LogEntry(_logCounter, message, Time.time, logType));
            RemoveLogAfterDelay(_logCounter).Forget();
            UpdateLogText();
        }

        private async UniTask RemoveLogAfterDelay(int logNumber)
        {
            var token = _cancellationTokenSource.Token;
            await UniTask.Delay(TimeSpan.FromSeconds(_kLOGLifetime),cancellationToken: token);
            _logEntries.RemoveAll(entry => entry.LogNumber == logNumber && Time.time - entry.Timestamp >= _kLOGLifetime);
            UpdateLogText();
        }

        private void UpdateLogText()
        {
            var sb = new StringBuilder();
            foreach (var entry in _logEntries)
            {
                sb.AppendLine(FormatLogEntry(entry));
            }
            logText.text = sb.ToString();
        }

        private string FormatLogEntry(LogEntry entry)
        {
            var prefix = entry.LogType switch
            {
                LogType.Warning => "<color=yellow>[WARNING]</color> ",
                LogType.Error => "<color=red>[ERROR]</color> ",
                _ => ""
            };
            return $"{entry.LogNumber}. {prefix}{entry.Message}";
        }

        private class LogEntry
        {
            public int LogNumber { get; }
            public string Message { get; }
            public float Timestamp { get; }
            public LogType LogType { get; }

            public LogEntry(int logNumber, string message, float timestamp, LogType logType)
            {
                LogNumber = logNumber;
                Message = message;
                Timestamp = timestamp;
                LogType = logType;
            }
        }

        private enum LogType
        {
            Log,
            Warning,
            Error
        }
    }
}