using System;
using PuzzleGallery.Core;
using PuzzleGallery.Core.ServiceLocator;
using UnityEngine;
using UnityDebug = UnityEngine.Debug;
using UnityObject = UnityEngine.Object;

namespace PuzzleGallery.Services.Logging
{
    public static class Logs
    {
        public static bool IsEnabled(LogLevel level)
        {
            return TryGetService(out var service)
                ? service.IsEnabled(level)
                : IsFallbackEnabled(level);
        }

        public static void Debug(string message, UnityObject context = null)
        {
            Log(LogLevel.Debug, message, context);
        }

        public static void Info(string message, UnityObject context = null)
        {
            Log(LogLevel.Info, message, context);
        }

        public static void Warning(string message, UnityObject context = null)
        {
            Log(LogLevel.Warning, message, context);
        }

        public static void Error(string message, UnityObject context = null)
        {
            Log(LogLevel.Error, message, context);
        }

        public static void Exception(Exception exception, string message = null, UnityObject context = null)
        {
            if (TryGetService(out var service))
            {
                service.LogException(exception, message, context);
                return;
            }

            LogExceptionFallback(exception, message, context);
        }

        public static void Log(LogLevel level, string message, UnityObject context = null)
        {
            if (TryGetService(out var service))
            {
                service.Log(level, message, context);
                return;
            }

            LogFallback(level, message, context);
        }

        public static void LogOnce(string key, LogLevel level, string message, UnityObject context = null)
        {
            if (TryGetService(out var service))
            {
                service.LogOnce(key, level, message, context);
                return;
            }

            LogFallback(level, message, context);
        }

        public static void LogThrottled(string key, LogLevel level, float intervalSeconds, string message, UnityObject context = null)
        {
            if (TryGetService(out var service))
            {
                service.LogThrottled(key, level, intervalSeconds, message, context);
                return;
            }

            LogFallback(level, message, context);
        }

        private static bool TryGetService(out ILogsService service)
        {
            return ServiceLocator.Instance.TryGet(out service);
        }

        private static bool IsFallbackEnabled(LogLevel level)
        {
            if (UnityDebug.isDebugBuild || Application.isEditor)
            {
                return level >= LogLevel.Debug;
            }

            return level >= LogLevel.Warning;
        }

        private static void LogFallback(LogLevel level, string message, UnityObject context)
        {
            if (!IsFallbackEnabled(level))
            {
                return;
            }

            message ??= string.Empty;
            var logType = level switch
            {
                LogLevel.Warning => LogType.Warning,
                LogLevel.Error => LogType.Error,
                LogLevel.Exception => LogType.Exception,
                _ => LogType.Log
            };

            if (context != null)
            {
                UnityDebug.unityLogger.Log(logType, (object)message, context);
            }
            else
            {
                UnityDebug.unityLogger.Log(logType, (object)message);
            }
        }

        private static void LogExceptionFallback(Exception exception, string message, UnityObject context)
        {
            if (!IsFallbackEnabled(LogLevel.Exception))
            {
                return;
            }

            if (exception == null)
            {
                LogFallback(LogLevel.Exception, message ?? "Unknown exception", context);
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
    }
}
