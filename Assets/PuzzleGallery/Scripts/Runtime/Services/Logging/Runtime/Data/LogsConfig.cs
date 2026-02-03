using UnityEngine;

namespace PuzzleGallery.Services.Logging.Data
{
    [CreateAssetMenu(fileName = "LogsConfig", menuName = "PuzzleGallery/Logs Config")]
    public sealed class LogsConfig : ScriptableObject
    {
        [Header("Level Filtering")]
        [SerializeField] private LogLevel _minimumLevelDevelopment = LogLevel.Debug;
        [SerializeField] private LogLevel _minimumLevelRelease = LogLevel.Warning;

        [Header("Stack Trace")]
        [SerializeField] private StackTraceLogType _logStackTrace = StackTraceLogType.None;
        [SerializeField] private StackTraceLogType _warningStackTrace = StackTraceLogType.ScriptOnly;
        [SerializeField] private StackTraceLogType _errorStackTrace = StackTraceLogType.Full;
        [SerializeField] private StackTraceLogType _exceptionStackTrace = StackTraceLogType.Full;

        [Header("Log Once")]
        [SerializeField] private bool _enableLogOnceCache = true;
        [SerializeField] private int _logOnceCapacity = 256;

        [Header("Throttling")]
        [SerializeField] private bool _enableThrottling = true;
        [SerializeField] private float _defaultThrottleIntervalSeconds = 2f;
        [SerializeField] private int _throttleCapacity = 256;

        [Header("File Logging")]
        [SerializeField] private bool _enableFileLoggingInDevelopment = false;
        [SerializeField] private bool _enableFileLoggingInRelease = false;
        [SerializeField] private int _maxFileSizeKB = 512;
        [SerializeField] private int _maxFiles = 3;
        [SerializeField] private string _filePrefix = "player_log";

        public LogLevel MinimumLevelDevelopment => _minimumLevelDevelopment;
        public LogLevel MinimumLevelRelease => _minimumLevelRelease;
        public StackTraceLogType LogStackTrace => _logStackTrace;
        public StackTraceLogType WarningStackTrace => _warningStackTrace;
        public StackTraceLogType ErrorStackTrace => _errorStackTrace;
        public StackTraceLogType ExceptionStackTrace => _exceptionStackTrace;
        public bool EnableLogOnceCache => _enableLogOnceCache;
        public int LogOnceCapacity => _logOnceCapacity;
        public bool EnableThrottling => _enableThrottling;
        public float DefaultThrottleIntervalSeconds => _defaultThrottleIntervalSeconds;
        public int ThrottleCapacity => _throttleCapacity;
        public bool EnableFileLoggingInDevelopment => _enableFileLoggingInDevelopment;
        public bool EnableFileLoggingInRelease => _enableFileLoggingInRelease;
        public int MaxFileSizeKB => _maxFileSizeKB;
        public int MaxFiles => _maxFiles;
        public string FilePrefix => _filePrefix;
    }
}
