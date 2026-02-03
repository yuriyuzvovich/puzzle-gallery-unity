using System;
using PuzzleGallery.Core;
using PuzzleGallery.Core.ServiceLocator;
using UnityObject = UnityEngine.Object;

namespace PuzzleGallery.Services.Logging
{
    public interface ILogsService : IService
    {
        bool IsEnabled(LogLevel level);

        void Log(LogLevel level, string message, UnityObject context = null);

        void LogOnce(string key, LogLevel level, string message, UnityObject context = null);

        void LogThrottled(string key, LogLevel level, float intervalSeconds, string message, UnityObject context = null);

        void LogException(Exception exception, string message = null, UnityObject context = null);
    }
}
