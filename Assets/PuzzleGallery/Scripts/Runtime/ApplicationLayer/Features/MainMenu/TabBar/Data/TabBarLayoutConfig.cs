using PuzzleGallery.Core.Devices;
using UnityEngine;

namespace PuzzleGallery.Features.MainMenu.Data
{
    [CreateAssetMenu(fileName = "TabBarLayoutConfig", menuName = "PuzzleGallery/TabBar Layout Config")]
    public sealed class TabBarLayoutConfig : ScriptableObject
    {
        [Header("Phone")]
        [SerializeField] private TabBarLayoutProfile _phone;

        [Header("Tablet")]
        [SerializeField] private TabBarLayoutProfile _tablet;

        public TabBarLayoutProfile Phone => _phone;
        public TabBarLayoutProfile Tablet => _tablet;

        public TabBarLayoutProfile GetProfile(AppDeviceType deviceType)
        {
            return deviceType == AppDeviceType.Tablet ? _tablet : _phone;
        }
    }
}
