using PuzzleGallery.Core.Devices;
using UnityEngine;

namespace PuzzleGallery.Services.Screen.Runtime
{
    [CreateAssetMenu(fileName = "MenuLayoutConfig", menuName = "PuzzleGallery/Menu Layout Config")]
    public sealed class MenuLayoutConfig : ScriptableObject
    {
        [Header("Phone")]
        [SerializeField] private MenuLayoutProfile _phone;

        [Header("Tablet")]
        [SerializeField] private MenuLayoutProfile _tablet;

        public MenuLayoutProfile Phone => _phone;
        public MenuLayoutProfile Tablet => _tablet;

        public MenuLayoutProfile GetProfile(AppDeviceType deviceType)
        {
            return deviceType == AppDeviceType.Tablet ? _tablet : _phone;
        }
    }

    [System.Serializable]
    public struct MenuLayoutProfile
    {
        [Range(0f, 1f)]
        public float CarouselHeightRatio;

        [Range(0f, 1f)]
        public float TabBarHeightRatio;
    }
}
