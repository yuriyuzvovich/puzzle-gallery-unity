using UnityEngine;

namespace PuzzleGallery.Features.MainMenu
{
    [CreateAssetMenu(fileName = "ScreenConfig", menuName = "PuzzleGallery/Screen Config")]
    public sealed class ScreenConfig : ScriptableObject
    {
        [Header("Device Detection")]
        [SerializeField] private float _tabletMinDiagonalInches = 7f;

        [Header("Grid Layout")]
        [SerializeField] private int _phoneColumns = 2;
        [SerializeField] private int _tabletColumns = 3;

        [Header("UI Scaling")]
        [SerializeField] private float _phoneReferenceWidth = 1080f;
        [SerializeField] private float _tabletReferenceWidth = 2048f;

        public float TabletMinDiagonalInches => _tabletMinDiagonalInches;
        public int PhoneColumns => _phoneColumns;
        public int TabletColumns => _tabletColumns;
        public float PhoneReferenceWidth => _phoneReferenceWidth;
        public float TabletReferenceWidth => _tabletReferenceWidth;
    }
}
