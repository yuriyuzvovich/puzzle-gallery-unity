using System;
using Cysharp.Threading.Tasks;
using PuzzleGallery.Core.Devices;
using PuzzleGallery.Features.MainMenu;
using PuzzleGallery.Services.Logging;
using UnityEngine;

namespace PuzzleGallery.Services.Screen.Runtime
{
    public sealed class ScreenService : IScreenService
    {
        private readonly ScreenConfig _config;
        private ScreenOrientation _lastOrientation;
        private AppDeviceType _deviceType;
        private bool _isMonitoring;

        public AppDeviceType AppDeviceType => _deviceType;
        public ScreenOrientation CurrentOrientation => UnityEngine.Screen.orientation;
        public bool IsTablet => _deviceType == AppDeviceType.Tablet;
        public bool IsPhone => _deviceType == AppDeviceType.Phone;

        public int GridColumnCount => _deviceType == AppDeviceType.Tablet
            ? _config.TabletColumns
            : _config.PhoneColumns;

        public event Action<ScreenOrientation> OnOrientationChanged;
        public event Action<AppDeviceType> OnAppDeviceTypeChanged;

        public ScreenService(ScreenConfig config)
        {
            _config = config;
        }

        public void Initialize()
        {
            DetectAppDeviceType();
            _lastOrientation = UnityEngine.Screen.orientation;
            _isMonitoring = true;
            MonitorOrientationAsync().Forget();
        }

        public void Dispose()
        {
            _isMonitoring = false;
            OnOrientationChanged = null;
            OnAppDeviceTypeChanged = null;
        }

        private void DetectAppDeviceType()
        {
            float dpi = UnityEngine.Screen.dpi;
            if (dpi <= 0)
            {
                dpi = 160f;
            }

            float widthInches = UnityEngine.Screen.width / dpi;
            float heightInches = UnityEngine.Screen.height / dpi;
            float diagonalInches = Mathf.Sqrt(widthInches * widthInches + heightInches * heightInches);

            var newAppDeviceType = diagonalInches >= _config.TabletMinDiagonalInches
                ? AppDeviceType.Tablet
                : AppDeviceType.Phone;

            if (_deviceType != newAppDeviceType)
            {
                _deviceType = newAppDeviceType;
                OnAppDeviceTypeChanged?.Invoke(_deviceType);
                Logs.Info($"Device detected as {_deviceType} (diagonal: {diagonalInches:F1} inches)");
            }
        }

        private async UniTaskVoid MonitorOrientationAsync()
        {
            while (_isMonitoring)
            {
                await UniTask.Yield();

                if (UnityEngine.Screen.orientation != _lastOrientation)
                {
                    _lastOrientation = UnityEngine.Screen.orientation;
                    OnOrientationChanged?.Invoke(_lastOrientation);
                    Logs.Info($"Orientation changed to {_lastOrientation}");
                }
            }
        }
    }
}
