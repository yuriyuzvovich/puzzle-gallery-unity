using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PuzzleGallery.Services.Logging.Data;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;
using UnityDebug = UnityEngine.Debug;
using UnityObject = UnityEngine.Object;

namespace PuzzleGallery.Services.Logging
{
    public sealed class LogsService : ILogsService
    {
        private const int DefaultLogOnceCapacity = 256;
        private const int DefaultThrottleCapacity = 256;
        private const int DefaultMaxFileSizeKB = 512;
        private const int DefaultMaxFiles = 3;
        private const float DefaultThrottleIntervalSeconds = 2f;
        private const string DefaultFilePrefix = "player_log";

        private readonly LogsConfig _config;
        private readonly object _lock = new object();
        private readonly Stopwatch _stopwatch;

        private LogLevel _minimumLevel;
        private bool _logOnceEnabled;
        private int _logOnceCapacity;
        private bool _throttlingEnabled;
        private float _defaultThrottleInterval;
        private int _throttleCapacity;

        private StackTraceLogType _logStackTrace;
        private StackTraceLogType _warningStackTrace;
        private StackTraceLogType _errorStackTrace;
        private StackTraceLogType _exceptionStackTrace;

        private bool _fileLoggingEnabled;
        private string _logDirectory;
        private string _currentLogPath;
        private int _maxFileSizeBytes;
        private int _maxFiles;
        private string _filePrefix;

        private HashSet<string> _logOnceKeys;
        private Queue<string> _logOnceOrder;
        private Dictionary<string, float> _throttleTimes;

        public LogsService(LogsConfig config)
        {
            _config = config;
            _stopwatch = Stopwatch.StartNew();
            ApplyConfig();
        }

        public void Initialize()
        {
            ApplyStackTraceSettings();

            if (_fileLoggingEnabled)
            {
                TryInitializeFileLogging();
            }
        }

        public void Dispose()
        {
            if (_fileLoggingEnabled)
            {
                Application.logMessageReceived -= HandleLogMessageReceived;
            }

            lock (_lock)
            {
                _logOnceKeys?.Clear();
                _logOnceOrder?.Clear();
                _throttleTimes?.Clear();
            }
        }

        public bool IsEnabled(LogLevel level)
        {
            return level >= _minimumLevel;
        }

        public void Log(LogLevel level, string message, UnityObject context = null)
        {
            if (!IsEnabled(level))
            {
                return;
            }

            message ??= string.Empty;
            var logType = ToLogType(level);
            WriteLog(logType, message, context);
        }

        public void LogOnce(string key, LogLevel level, string message, UnityObject context = null)
        {
            if (!IsEnabled(level))
            {
                return;
            }

            if (!_logOnceEnabled || string.IsNullOrWhiteSpace(key))
            {
                Log(level, message, context);
                return;
            }

            bool shouldLog;
            lock (_lock)
            {
                if (_logOnceKeys == null)
                {
                    _logOnceKeys = new HashSet<string>(StringComparer.Ordinal);
                    _logOnceOrder = new Queue<string>();
                }

                if (_logOnceKeys.Contains(key))
                {
                    shouldLog = false;
                }
                else
                {
                    _logOnceKeys.Add(key);
                    _logOnceOrder.Enqueue(key);
                    TrimLogOnceCache();
                    shouldLog = true;
                }
            }

            if (shouldLog)
            {
                Log(level, message, context);
            }
        }

        public void LogThrottled(string key, LogLevel level, float intervalSeconds, string message, UnityObject context = null)
        {
            if (!IsEnabled(level))
            {
                return;
            }

            if (!_throttlingEnabled || string.IsNullOrWhiteSpace(key))
            {
                Log(level, message, context);
                return;
            }

            var interval = intervalSeconds > 0f ? intervalSeconds : _defaultThrottleInterval;
            if (interval <= 0f)
            {
                Log(level, message, context);
                return;
            }

            bool shouldLog;
            var now = (float)_stopwatch.Elapsed.TotalSeconds;

            lock (_lock)
            {
                if (_throttleTimes == null)
                {
                    _throttleTimes = new Dictionary<string, float>(StringComparer.Ordinal);
                }

                if (_throttleTimes.TryGetValue(key, out var lastTime) && now - lastTime < interval)
                {
                    shouldLog = false;
                }
                else
                {
                    _throttleTimes[key] = now;
                    TrimThrottleCache();
                    shouldLog = true;
                }
            }

            if (shouldLog)
            {
                Log(level, message, context);
            }
        }

        public void LogException(Exception exception, string message = null, UnityObject context = null)
        {
            if (!IsEnabled(LogLevel.Exception))
            {
                return;
            }

            if (exception == null)
            {
                Log(LogLevel.Exception, message ?? "Unknown exception", context);
                return;
            }

            var effectiveException = string.IsNullOrWhiteSpace(message)
                ? exception
                : new Exception(message, exception);

            if (context != null)
            {
                UnityDebug.LogException(effectiveException, context);
            }
            else
            {
                UnityDebug.LogException(effectiveException);
            }
        }

        private void ApplyConfig()
        {
            var isDevelopment = UnityDebug.isDebugBuild || Application.isEditor;

            var minDevelopment = _config != null ? _config.MinimumLevelDevelopment : LogLevel.Debug;
            var minRelease = _config != null ? _config.MinimumLevelRelease : LogLevel.Warning;
            _minimumLevel = isDevelopment ? minDevelopment : minRelease;

            _logOnceEnabled = _config == null || _config.EnableLogOnceCache;
            _logOnceCapacity = _config != null ? _config.LogOnceCapacity : DefaultLogOnceCapacity;
            _logOnceCapacity = Math.Max(0, _logOnceCapacity);

            _throttlingEnabled = _config == null || _config.EnableThrottling;
            _defaultThrottleInterval = _config != null ? _config.DefaultThrottleIntervalSeconds : DefaultThrottleIntervalSeconds;
            _throttleCapacity = _config != null ? _config.ThrottleCapacity : DefaultThrottleCapacity;
            _throttleCapacity = Math.Max(0, _throttleCapacity);

            _logStackTrace = _config != null ? _config.LogStackTrace : StackTraceLogType.None;
            _warningStackTrace = _config != null ? _config.WarningStackTrace : StackTraceLogType.ScriptOnly;
            _errorStackTrace = _config != null ? _config.ErrorStackTrace : StackTraceLogType.Full;
            _exceptionStackTrace = _config != null ? _config.ExceptionStackTrace : StackTraceLogType.Full;

            var enableFileLogging = false;
            if (_config != null)
            {
                enableFileLogging = isDevelopment
                    ? _config.EnableFileLoggingInDevelopment
                    : _config.EnableFileLoggingInRelease;
            }

            _fileLoggingEnabled = enableFileLogging;
            _maxFileSizeBytes = (_config != null ? _config.MaxFileSizeKB : DefaultMaxFileSizeKB) * 1024;
            _maxFiles = _config != null ? _config.MaxFiles : DefaultMaxFiles;
            _filePrefix = _config != null && !string.IsNullOrWhiteSpace(_config.FilePrefix)
                ? _config.FilePrefix.Trim()
                : DefaultFilePrefix;

            if (_maxFileSizeBytes <= 0 || _maxFiles <= 0)
            {
                _fileLoggingEnabled = false;
            }
        }

        private void ApplyStackTraceSettings()
        {
            Application.SetStackTraceLogType(LogType.Log, _logStackTrace);
            Application.SetStackTraceLogType(LogType.Warning, _warningStackTrace);
            Application.SetStackTraceLogType(LogType.Error, _errorStackTrace);
            Application.SetStackTraceLogType(LogType.Exception, _exceptionStackTrace);
        }

        private void TryInitializeFileLogging()
        {
            try
            {
                _logDirectory = Path.Combine(Application.persistentDataPath, "Logs");
                Directory.CreateDirectory(_logDirectory);
                _currentLogPath = GetLogFilePath(0);
                Application.logMessageReceived += HandleLogMessageReceived;
            }
            catch (Exception)
            {
                _fileLoggingEnabled = false;
            }
        }

        private void HandleLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (!_fileLoggingEnabled)
            {
                return;
            }

            try
            {
                var entry = BuildLogEntry(condition, stackTrace, type);
                var entryBytes = Encoding.UTF8.GetByteCount(entry) + Encoding.UTF8.GetByteCount(Environment.NewLine);
                RotateFilesIfNeeded(entryBytes);
                File.AppendAllText(_currentLogPath, entry + Environment.NewLine);
            }
            catch (Exception)
            {
            }
        }

        private string BuildLogEntry(string condition, string stackTrace, LogType type)
        {
            var timestamp = DateTime.UtcNow.ToString("o");
            if (string.IsNullOrWhiteSpace(stackTrace))
            {
                return $"{timestamp} [{type}] {condition}";
            }

            return $"{timestamp} [{type}] {condition}{Environment.NewLine}{stackTrace}";
        }

        private void RotateFilesIfNeeded(int newEntryBytes)
        {
            if (string.IsNullOrWhiteSpace(_currentLogPath))
            {
                return;
            }

            if (!File.Exists(_currentLogPath))
            {
                return;
            }

            var fileInfo = new FileInfo(_currentLogPath);
            if (fileInfo.Length + newEntryBytes <= _maxFileSizeBytes)
            {
                return;
            }

            if (_maxFiles <= 1)
            {
                File.Delete(_currentLogPath);
                return;
            }

            for (int i = _maxFiles - 1; i >= 1; i--)
            {
                var sourcePath = GetLogFilePath(i - 1);
                var destinationPath = GetLogFilePath(i);

                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }

                if (File.Exists(sourcePath))
                {
                    File.Move(sourcePath, destinationPath);
                }
            }
        }

        private string GetLogFilePath(int index)
        {
            return Path.Combine(_logDirectory, $"{_filePrefix}_{index}.log");
        }

        private void TrimLogOnceCache()
        {
            if (_logOnceCapacity <= 0)
            {
                _logOnceKeys.Clear();
                _logOnceOrder.Clear();
                return;
            }

            while (_logOnceKeys.Count > _logOnceCapacity && _logOnceOrder.Count > 0)
            {
                var oldest = _logOnceOrder.Dequeue();
                _logOnceKeys.Remove(oldest);
            }
        }

        private void TrimThrottleCache()
        {
            if (_throttleCapacity <= 0)
            {
                _throttleTimes.Clear();
                return;
            }

            if (_throttleTimes.Count > _throttleCapacity)
            {
                _throttleTimes.Clear();
            }
        }

        private static LogType ToLogType(LogLevel level)
        {
            return level switch
            {
                LogLevel.Warning => LogType.Warning,
                LogLevel.Error => LogType.Error,
                LogLevel.Exception => LogType.Exception,
                _ => LogType.Log
            };
        }

        private static void WriteLog(LogType type, string message, UnityObject context)
        {
            if (context != null)
            {
                UnityDebug.unityLogger.Log(type, (object)message, context);
            }
            else
            {
                UnityDebug.unityLogger.Log(type, (object)message);
            }
        }
    }
}
