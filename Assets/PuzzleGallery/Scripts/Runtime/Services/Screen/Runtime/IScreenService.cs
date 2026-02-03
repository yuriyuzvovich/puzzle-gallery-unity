using System;
using PuzzleGallery.Core.Devices;
using PuzzleGallery.Core.ServiceLocator;
using UnityEngine;

namespace PuzzleGallery.Services.Screen.Runtime
{
    public interface IScreenService : IService
    {
        AppDeviceType AppDeviceType { get; }

        ScreenOrientation CurrentOrientation { get; }

        int GridColumnCount { get; }

        event Action<ScreenOrientation> OnOrientationChanged;

        event Action<AppDeviceType> OnAppDeviceTypeChanged;

        bool IsTablet { get; }

        bool IsPhone { get; }
    }
}
